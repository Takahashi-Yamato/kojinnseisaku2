using System.Collections.Generic;
using UnityEngine;

namespace CubeOrbit
{
    /// <summary>
    /// 動く床（エレベーター兼用）。置いた位置と「置いた位置 + moveOffset」の間を往復する。
    /// 止まっている間だけ乗り降りできる。moveOffset の Y を使えばエレベーターになる。
    /// 動きは GameManager.StageTime で決まるので、同じ設定の床は必ず同じタイミングで動く。
    /// </summary>
    [ExecuteAlways]
    [SelectionBase]
    [DefaultExecutionOrder(-50)]
    public class MovingPlatform : GridElement
    {
        [Header("動き")]
        [Tooltip("スタート位置からどれだけ動くか（マス）。X・Y・Zのどれか1軸だけにする")]
        public Vector3Int moveOffset = new Vector3Int(0, 0, -3);
        [Tooltip("移動スピード（マス/秒）")]
        [Min(0.1f)] public float speed = 2.2f;
        [Tooltip("両端で止まっている時間（秒）")]
        [Min(0f)] public float pauseTime = 1.4f;
        [Tooltip("動き出すタイミングをずらす（秒）。複数の床をずらして動かす時に使う")]
        public float timeOffset = 0f;

        [Header("乗り降りの猶予（秒）")]
        [Tooltip("止まっている残り時間がこれより長い時だけ乗り込める")]
        public float enterMargin = 0.45f;
        [Tooltip("止まっている残り時間がこれより長い時だけ足場として扱う")]
        public float floorMargin = 0.3f;

        [Header("見た目")]
        [Tooltip("上面の光るパネル（止まっている間点滅する）")]
        public Renderer topLight;
        public Color lightColor = new Color(0.35f, 0.85f, 1f);

        Vector3Int startCell;
        bool started;
        readonly HashSet<Vector3Int> rail = new HashSet<Vector3Int>();
        MaterialPropertyBlock mpb;

        public Vector3 CurrentPosition { get; private set; }
        public bool Paused { get; private set; }
        public float RemainingPause { get; private set; }
        /// <summary>このフレームでちょうど止まったら true（乗っているプレイヤーが使う）</summary>
        public bool JustStopped { get; private set; }

        public Vector3Int StartCell { get { return started ? startCell : Cell; } }
        public Vector3Int EndCell { get { return StartCell + moveOffset; } }
        Vector3Int CurrentCell { get { return Vector3Int.RoundToInt(CurrentPosition); } }

        float MoveTime { get { return ((Vector3)moveOffset).magnitude / Mathf.Max(0.1f, speed); } }
        float Cycle { get { return 2f * pauseTime + 2f * MoveTime; } }

        void Awake()
        {
            if (!Application.isPlaying) return;
            startCell = Cell;
            started = true;
            BuildRail();
            Evaluate(0f);
        }

        void BuildRail()
        {
            rail.Clear();
            int n = Mathf.Max(Mathf.Abs(moveOffset.x), Mathf.Max(Mathf.Abs(moveOffset.y), Mathf.Abs(moveOffset.z)));
            if (n == 0) { rail.Add(startCell); return; }
            Vector3Int step = new Vector3Int(moveOffset.x / n, moveOffset.y / n, moveOffset.z / n);
            for (int i = 0; i <= n; i++) rail.Add(startCell + step * i);
        }

        /// <summary>時刻 time の時の位置と状態を計算する</summary>
        public void Evaluate(float time)
        {
            Vector3 a = startCell, b = startCell + moveOffset;
            if (moveOffset == Vector3Int.zero || Cycle <= 0f)
            {
                CurrentPosition = a; Paused = true; RemainingPause = 999f; return;
            }
            float tt = Mathf.Repeat(time + timeOffset, Cycle);
            float mv = MoveTime;
            if (tt < pauseTime) { CurrentPosition = a; Paused = true; RemainingPause = pauseTime - tt; return; }
            tt -= pauseTime;
            if (tt < mv) { CurrentPosition = Vector3.Lerp(a, b, GridUtil.Smooth(tt / mv)); Paused = false; RemainingPause = 0f; return; }
            tt -= mv;
            if (tt < pauseTime) { CurrentPosition = b; Paused = true; RemainingPause = pauseTime - tt; return; }
            tt -= pauseTime;
            CurrentPosition = Vector3.Lerp(b, a, GridUtil.Smooth(Mathf.Clamp01(tt / mv))); Paused = false; RemainingPause = 0f;
        }

        protected override void Update()
        {
            base.Update();
            if (!Application.isPlaying) return;
            bool wasPaused = Paused;
            Evaluate(GameManager.StageTime);
            JustStopped = Paused && !wasPaused;
            transform.position = CurrentPosition;

            if (topLight != null)
            {
                if (mpb == null) mpb = new MaterialPropertyBlock();
                float glow = Paused ? 0.75f + 0.25f * Mathf.Sin(Time.time * 6f) : 1f;
                topLight.GetPropertyBlock(mpb);
                mpb.SetColor("_Color", lightColor * glow);
                topLight.SetPropertyBlock(mpb);
            }
        }

        public override bool Blocks(Vector3Int c) { return Paused && c == CurrentCell; }
        public override bool Climbable(Vector3Int c) { return Blocks(c) && RemainingPause > enterMargin; }
        public override bool IsFloor(Vector3Int c) { return Blocks(c) && RemainingPause > floorMargin; }
        public override bool BodyWait(Vector3Int c) { return rail.Contains(c) && !(Paused && RemainingPause > enterMargin); }
        public override bool FloorWait(Vector3Int c) { return rail.Contains(c) && !IsFloor(c); }

        public override bool SolverSolid(Vector3Int c, int activeMask) { return c == Cell || c == Cell + moveOffset; }

        public override void GetBounds(out Vector3Int min, out Vector3Int max)
        {
            min = Vector3Int.Min(StartCell, EndCell); max = Vector3Int.Max(StartCell, EndCell);
        }

        void OnValidate()
        {
            int axes = (moveOffset.x != 0 ? 1 : 0) + (moveOffset.y != 0 ? 1 : 0) + (moveOffset.z != 0 ? 1 : 0);
            if (axes > 1) Debug.LogWarning(name + ": 動く床の moveOffset は X・Y・Z のどれか1軸だけにしてください", this);
        }

        void OnDrawGizmos()
        {
            Vector3 a = StartCell, b = EndCell;
            Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.9f);
            Gizmos.DrawLine(a, b);
            Gizmos.DrawWireCube(b, Vector3.one * 0.98f);
            Gizmos.DrawWireCube(a, Vector3.one * 0.98f);
        }
    }
}
