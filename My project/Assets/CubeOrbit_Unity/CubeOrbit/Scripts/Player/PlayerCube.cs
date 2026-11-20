using UnityEngine;

namespace CubeOrbit
{
    /// <summary>
    /// プレイヤーのキューブ。転がる・よじ登る・落ちる・押し出される・動く床に乗る、を担当。
    /// どこに動けるかの判定は MoveRules に任せ、ここでは動きのアニメーションと状態管理をする。
    /// </summary>
    [SelectionBase]
    [RequireComponent(typeof(CubeInput))]
    public class PlayerCube : MonoBehaviour
    {
        public static PlayerCube Instance { get; private set; }

        [Header("転がり")]
        [Tooltip("1マス転がるのにかかる時間（秒）")]
        [Min(0.03f)] public float rollDuration = 0.15f;
        [Tooltip("1段よじ登るのにかかる時間（秒）")]
        [Min(0.03f)] public float climbDuration = 0.26f;

        [Header("壁にぶつかった時")]
        [Min(0.01f)] public float bumpDuration = 0.14f;
        public float bumpDistance = 0.1f;

        [Header("落下")]
        [Tooltip("重力（マス/秒²）")]
        public float gravity = 42f;

        [Header("押し出された時")]
        [Min(0.01f)] public float pushDuration = 0.13f;

        [Header("出現")]
        public float firstSpawnHeight = 6f;
        public float firstSpawnDuration = 0.7f;
        public float respawnHeight = 3.5f;
        public float respawnDuration = 0.4f;

        [Header("ゴール演出")]
        public float winDuration = 1.4f;
        public float winRise = 2.2f;

        [Header("見た目")]
        [Tooltip("キューブの大きさ（1でマスぴったり）")]
        [Range(0.5f, 1f)] public float cubeScale = 0.96f;

        public enum State { Idle, Rolling, Bumping, Falling, Pushed, Dead, Spawning, Win }

        public State CurrentState { get { return state; } }
        public Vector3Int Cell { get; private set; }
        public bool IsRiding { get { return ride != null; } }
        public int MoveCount { get; private set; }

        State state = State.Idle;
        float t;
        CubeInput input;
        Renderer[] renderers;

        // 転がり
        Vector3Int rollFrom, rollTo, bumpDir;
        Vector3 pivot, axis;
        float angle, duration;
        // 落下
        float fallVel;
        int fallNotifiedY;
        // 押し出し
        Vector3Int pushFrom, pushTo;
        // 出現 / ゴール
        Vector3 spawnFrom;
        Vector3Int winBase;
        bool clearShown;
        // 動く床
        MovingPlatform ride;
        Vector3 rideOffset;

        IGridQuery Q { get { return GridWorld.Live; } }

        void Awake()
        {
            Instance = this;
            input = GetComponent<CubeInput>();
            renderers = GetComponentsInChildren<Renderer>();
            Cell = GridUtil.ToCell(transform.position);
            transform.localScale = Vector3.one * cubeScale;
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        // ================= 外から呼ぶもの =================

        /// <summary>cell に出現させる（GameManager が呼ぶ）</summary>
        public void Spawn(Vector3Int cell, bool first)
        {
            ride = null;
            Cell = cell;
            state = State.Spawning;
            t = 0f;
            duration = first ? firstSpawnDuration : respawnDuration;
            spawnFrom = (Vector3)cell + Vector3.up * (first ? firstSpawnHeight : respawnHeight);
            transform.position = spawnFrom;
            transform.rotation = Quaternion.identity;
            SetVisible(true);
            if (input != null) input.Clear();
        }

        /// <summary>押し出す壁が飛び出した瞬間に呼ばれる</summary>
        public void OnPusherFired(Pusher p)
        {
            if (state == State.Dead || state == State.Win || state == State.Spawning || state == State.Pushed) return;
            Vector3Int cur;
            if (TryGetLogicalCell(out cur) && cur == p.ExtendCell) StartPush(p);
        }

        public void StartWin()
        {
            if (state == State.Win) return;
            ride = null;
            state = State.Win;
            t = 0f;
            winBase = Cell;
            clearShown = false;
            SoundPlayer.Play(Sfx.Goal);
            BurstEffect.Spawn(transform.position, Color.white, 30, 6f);
        }

        public void Die()
        {
            if (state == State.Dead || state == State.Win) return;
            state = State.Dead;
            t = 0f;
            ride = null;
            SetVisible(false);
            SoundPlayer.Play(Sfx.Fall);
            if (GameManager.Instance != null) GameManager.Instance.OnPlayerDied();
        }

        // ================= 毎フレーム =================

        void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            switch (state)
            {
                case State.Idle: UpdateIdle(); break;
                case State.Rolling: UpdateRolling(dt); break;
                case State.Bumping: UpdateBumping(dt); break;
                case State.Falling: UpdateFalling(dt); break;
                case State.Pushed: UpdatePushed(dt); break;
                case State.Dead: UpdateDead(dt); break;
                case State.Spawning: UpdateSpawning(dt); break;
                case State.Win: UpdateWin(dt); break;
            }
        }

        void UpdateIdle()
        {
            // 動く床に乗っている
            if (ride != null)
            {
                transform.position = ride.CurrentPosition + rideOffset;
                Cell = GridUtil.ToCell(transform.position);
                if (ride.JustStopped)
                {
                    transform.position = Cell;
                    GridWorld.NotifyEnter(this, Cell, false);
                    if (state != State.Idle) return;
                }
                if (!ride.Paused) return; // 動いている間は操作できない
            }
            else
            {
                // 足場が消えた（崩れた床など）
                Vector3Int below = Cell + Vector3Int.down;
                if (!Q.Floor(below) && !Q.Block(below)) { StartFall(); return; }
            }

            Vector3Int dir;
            if (input != null && input.TryPeek(out dir))
            {
                if (TryMove(dir)) input.Consume();
            }
        }

        bool TryMove(Vector3Int d)
        {
            var r = MoveRules.Resolve(Q, Cell, d);
            if (r.type == MoveType.Wait) return false;

            ride = null;
            transform.position = Cell;
            transform.rotation = Quaternion.identity;
            t = 0f;

            if (r.type == MoveType.Bump)
            {
                state = State.Bumping;
                bumpDir = d;
                SoundPlayer.Play(Sfx.Bump);
                return true;
            }

            rollFrom = Cell;
            rollTo = r.to;
            axis = Vector3.Cross(Vector3.up, (Vector3)d);
            if (r.type == MoveType.Climb)
            {
                pivot = (Vector3)rollFrom + (Vector3)d * 0.5f + Vector3.up * 0.5f;
                angle = 180f;
                duration = climbDuration;
                SoundPlayer.Play(Sfx.Climb);
            }
            else
            {
                pivot = (Vector3)rollFrom + (Vector3)d * 0.5f + Vector3.down * 0.5f;
                angle = 90f;
                duration = rollDuration;
                SoundPlayer.Play(Sfx.Roll);
            }
            state = State.Rolling;
            MoveCount++;
            return true;
        }

        void UpdateRolling(float dt)
        {
            t += dt;
            float k = Mathf.Clamp01(t / duration);
            Quaternion q = Quaternion.AngleAxis(angle * GridUtil.EaseInOut(k), axis);
            transform.position = pivot + q * ((Vector3)rollFrom - pivot);
            transform.rotation = q;
            if (k >= 1f) Arrive(rollTo);
        }

        void UpdateBumping(float dt)
        {
            t += dt;
            float k = Mathf.Clamp01(t / bumpDuration);
            float s = Mathf.Sin(k * Mathf.PI) * bumpDistance;
            transform.position = (Vector3)Cell + (Vector3)bumpDir * s;
            transform.rotation = Quaternion.AngleAxis(s * 35f, Vector3.Cross(Vector3.up, (Vector3)bumpDir));
            if (k >= 1f)
            {
                transform.position = Cell;
                transform.rotation = Quaternion.identity;
                state = State.Idle;
            }
        }

        /// <summary>マスに到着した時の処理（着地・動く床・落下・イベント）</summary>
        void Arrive(Vector3Int cell)
        {
            Cell = cell;
            transform.position = cell;
            transform.rotation = Quaternion.identity;
            state = State.Idle;

            Pusher head = GridWorld.PusherHeadAt(cell);
            if (head != null) { StartPush(head); return; }

            Vector3Int below = cell + Vector3Int.down;
            MovingPlatform plat = GridWorld.PlatformAt(below);
            if (plat != null)
            {
                ride = plat;
                rideOffset = (Vector3)cell - plat.CurrentPosition;
                GridWorld.NotifyEnter(this, cell, false);
                return;
            }

            if (!Q.Floor(below) && !Q.Block(below))
            {
                GridWorld.NotifyEnter(this, cell, true);
                StartFall();
                return;
            }

            GridWorld.NotifyLand(this, below);
            GridWorld.NotifyEnter(this, cell, false);
        }

        void StartFall()
        {
            ride = null;
            state = State.Falling;
            fallVel = 0f;
            fallNotifiedY = Cell.y;
        }

        void UpdateFalling(float dt)
        {
            fallVel -= gravity * dt;
            Vector3 p = transform.position;
            p.y += fallVel * dt;
            transform.position = p;
            int ny = Mathf.RoundToInt(p.y);

            // 落ちながら通過したマスのジェムを取る
            while (fallNotifiedY > ny)
            {
                fallNotifiedY--;
                GridWorld.NotifyEnter(this, new Vector3Int(Cell.x, fallNotifiedY, Cell.z), true);
                if (state != State.Falling) return;
            }

            int? land = null;
            for (int y = Cell.y; y > Q.KillY; y--)
            {
                var b = new Vector3Int(Cell.x, y - 1, Cell.z);
                if (Q.Floor(b) || GridWorld.PlatformAt(b) != null) { land = y; break; }
            }

            if (land.HasValue && p.y <= land.Value)
            {
                SoundPlayer.Play(Sfx.Land);
                CameraFollow.Shake(0.06f);
                Arrive(new Vector3Int(Cell.x, land.Value, Cell.z));
                return;
            }

            Cell = new Vector3Int(Cell.x, land.HasValue ? Mathf.Max(ny, land.Value) : Mathf.Max(ny, Q.KillY), Cell.z);
            if (p.y < Q.KillY) Die();
        }

        void StartPush(Pusher p)
        {
            ride = null;
            pushFrom = p.ExtendCell;
            pushTo = pushFrom + p.Direction;
            Cell = pushFrom;
            transform.rotation = Quaternion.identity;
            SoundPlayer.Play(Sfx.Push);
            CameraFollow.Shake(0.12f);
            if (Q.Block(pushTo)) { Die(); return; } // 押しつぶされた
            state = State.Pushed;
            t = 0f;
        }

        void UpdatePushed(float dt)
        {
            t += dt;
            float k = Mathf.Clamp01(t / pushDuration);
            transform.position = Vector3.Lerp(pushFrom, pushTo, GridUtil.EaseInOut(k));
            if (k >= 1f) Arrive(pushTo);
        }

        void UpdateDead(float dt)
        {
            t += dt;
            float delay = GameManager.Instance != null ? GameManager.Instance.respawnDelay : 0.75f;
            if (t >= delay)
            {
                if (GameManager.Instance != null) GameManager.Instance.Respawn();
                else Spawn(Cell, false);
            }
        }

        void UpdateSpawning(float dt)
        {
            t += dt;
            float k = Mathf.Clamp01(t / duration);
            float e = 1f - Mathf.Pow(1f - k, 3f);
            transform.position = Vector3.Lerp(spawnFrom, (Vector3)Cell, e);
            transform.localScale = Vector3.one * cubeScale * (0.3f + 0.7f * e);
            if (k >= 1f)
            {
                transform.position = Cell;
                transform.localScale = Vector3.one * cubeScale;
                state = State.Idle;
                SoundPlayer.Play(Sfx.Land);
                BurstEffect.Spawn((Vector3)Cell + Vector3.down * 0.45f, new Color(0.8f, 0.85f, 1f), 8, 2f);
                MovingPlatform plat = GridWorld.PlatformAt(Cell + Vector3Int.down);
                if (plat != null) { ride = plat; rideOffset = (Vector3)Cell - plat.CurrentPosition; }
            }
        }

        void UpdateWin(float dt)
        {
            t += dt;
            float rise = GridUtil.EaseInOut(Mathf.Clamp01(t / (winDuration * 0.85f))) * winRise;
            transform.position = (Vector3)winBase + Vector3.up * rise;
            transform.rotation = Quaternion.AngleAxis(t * t * 460f, Vector3.up) * Quaternion.AngleAxis(t * 170f, new Vector3(1f, 0f, 1f).normalized);
            float shrink = Mathf.Clamp01(1f - Mathf.Max(0f, t - winDuration * 0.43f) / (winDuration * 0.57f));
            transform.localScale = Vector3.one * cubeScale * shrink;
            if (t >= winDuration && !clearShown)
            {
                clearShown = true;
                if (GameManager.Instance != null) GameManager.Instance.ShowClear();
            }
        }

        // ================= ヘルパー =================

        /// <summary>押し出し判定に使う「今いるとみなすマス」</summary>
        bool TryGetLogicalCell(out Vector3Int cell)
        {
            switch (state)
            {
                case State.Idle:
                case State.Bumping:
                case State.Falling:
                    cell = Cell; return true;
                case State.Rolling:
                    cell = t / duration > 0.5f ? rollTo : rollFrom; return true;
            }
            cell = Cell;
            return false;
        }

        void SetVisible(bool on)
        {
            if (renderers == null) return;
            foreach (var r in renderers) if (r != null) r.enabled = on;
        }
    }
}
