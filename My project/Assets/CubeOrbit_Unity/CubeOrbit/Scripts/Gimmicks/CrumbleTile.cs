using UnityEngine;

namespace CubeOrbit
{
    /// <summary>
    /// 崩れる床。乗ると少し震えてから落ちる。落下してやり直すと元に戻る。
    /// </summary>
    [ExecuteAlways]
    [SelectionBase]
    [DefaultExecutionOrder(-50)]
    public class CrumbleTile : GridElement, IPlayerLandHandler, IStageResettable
    {
        [Header("崩れ方")]
        [Tooltip("乗ってから崩れるまでの時間（秒）")]
        [Min(0.05f)] public float crumbleDelay = 0.5f;
        [Tooltip("震えの大きさ（最初）")]
        public float shakeStart = 0.035f;
        [Tooltip("震えの増え方（1秒あたり）")]
        public float shakeGrow = 0.07f;
        [Tooltip("崩れた後に落ちていく重力")]
        public float fallGravity = 18f;
        [Tooltip("この距離だけ落ちたら非表示にする")]
        public float hideDistance = 14f;

        [Header("見た目")]
        public Transform visual;
        public Renderer visualRenderer;
        public Color normalColor = new Color(0.64f, 0.56f, 0.52f);
        public Color shakingColor = new Color(0.72f, 0.56f, 0.46f);

        enum State { Intact, Shaking, Gone }
        State state = State.Intact;
        float timer, fallY, fallVel, spin;
        Vector3Int home;
        bool started;
        MaterialPropertyBlock mpb;

        Vector3Int HomeCell { get { return started ? home : Cell; } }

        void Awake()
        {
            if (!Application.isPlaying) return;
            home = Cell;
            started = true;
        }

        public override bool Blocks(Vector3Int c) { return state != State.Gone && c == HomeCell; }
        public override bool SolverSolid(Vector3Int c, int activeMask) { return c == Cell; }

        public void OnPlayerLand(PlayerCube player)
        {
            if (state != State.Intact) return;
            state = State.Shaking;
            timer = 0f;
            SetColor(shakingColor);
            SoundPlayer.Play(Sfx.Crack);
        }

        public void OnStageReset()
        {
            state = State.Intact;
            timer = fallY = fallVel = spin = 0f;
            if (visual != null)
            {
                visual.gameObject.SetActive(true);
                visual.localPosition = Vector3.zero;
                visual.localRotation = Quaternion.identity;
            }
            SetColor(normalColor);
        }

        protected override void Update()
        {
            base.Update();
            if (!Application.isPlaying || visual == null) return;
            float dt = Time.deltaTime;

            if (state == State.Shaking)
            {
                timer += dt;
                float j = shakeStart + timer * shakeGrow;
                visual.localPosition = new Vector3(Random.Range(-j, j), Random.Range(-j, j), Random.Range(-j, j)) * 0.5f;
                if (timer >= crumbleDelay)
                {
                    state = State.Gone;
                    fallVel = 0f;
                    SoundPlayer.Play(Sfx.Crumble);
                }
            }
            else if (state == State.Gone && visual.gameObject.activeSelf)
            {
                fallVel -= fallGravity * dt;
                fallY += fallVel * dt;
                spin += dt * 90f;
                visual.localPosition = new Vector3(0f, fallY, 0f);
                visual.localRotation = Quaternion.Euler(spin, 0f, spin * 0.6f);
                if (fallY < -hideDistance) visual.gameObject.SetActive(false);
            }
        }

        void SetColor(Color c)
        {
            if (visualRenderer == null) return;
            if (mpb == null) mpb = new MaterialPropertyBlock();
            visualRenderer.GetPropertyBlock(mpb);
            mpb.SetColor("_Color", c);
            visualRenderer.SetPropertyBlock(mpb);
        }
    }
}
