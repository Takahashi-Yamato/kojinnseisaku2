using UnityEngine;

namespace CubeOrbit
{
    /// <summary>
    /// 押し出す壁。一定間隔で1マス飛び出し、その前にいるキューブを押し出す。
    /// 壁（BlockVolume）の表面のマスに重ねて置けばOK（壁に穴を開ける必要はない）。
    /// </summary>
    [ExecuteAlways]
    [SelectionBase]
    [DefaultExecutionOrder(-50)]
    public class Pusher : GridElement
    {
        [Header("向き")]
        [Tooltip("飛び出す方向")]
        public GridDir pushDirection = GridDir.PlusX;

        [Header("タイミング（秒）")]
        [Tooltip("1サイクルの長さ")]
        [Min(0.5f)] public float period = 2.2f;
        [Tooltip("タイミングをずらす。並べた壁を交互に動かす時に使う")]
        public float timeOffset = 0f;
        [Tooltip("飛び出すのにかかる時間")]
        [Min(0.01f)] public float extendTime = 0.12f;
        [Tooltip("出たまま止まっている時間")]
        [Min(0f)] public float holdTime = 0.45f;
        [Tooltip("引っ込むのにかかる時間")]
        [Min(0.01f)] public float retractTime = 0.28f;

        [Header("予告")]
        [Tooltip("飛び出す何秒前から点滅するか")]
        public float warningTime = 0.45f;
        public Color baseColor = new Color(0.95f, 0.5f, 0.32f);
        public Color warningColor = new Color(1f, 0.85f, 0.6f);

        [Header("見た目")]
        [Tooltip("動く部分（子オブジェクト）")]
        public Transform head;
        public Renderer headRenderer;
        [Tooltip("引っ込んでいる時に壁から少しだけ出しておく量（見えるように）")]
        public float restPoke = 0.03f;

        Vector3Int home;
        bool started;
        MaterialPropertyBlock mpb;

        /// <summary>0 = 引っ込んでいる, 1 = 出きっている</summary>
        public float Extension { get; private set; }
        /// <summary>次に飛び出すまでの時間</summary>
        public float TimeToNextPush { get; private set; }

        public Vector3Int Direction { get { return pushDirection.ToVector(); } }
        public Vector3Int HomeCell { get { return started ? home : Cell; } }
        public Vector3Int ExtendCell { get { return HomeCell + Direction; } }

        const float SolidThreshold = 0.35f;

        void Awake()
        {
            if (!Application.isPlaying) return;
            home = Cell;
            started = true;
        }

        public void Evaluate(float time)
        {
            float active = extendTime + holdTime + retractTime;
            float rest = Mathf.Max(0f, period - active);
            float tt = Mathf.Repeat(time + timeOffset, period);
            if (tt < rest) Extension = 0f;
            else if (tt < rest + extendTime) Extension = (tt - rest) / extendTime;
            else if (tt < rest + extendTime + holdTime) Extension = 1f;
            else Extension = Mathf.Clamp01(1f - (tt - rest - extendTime - holdTime) / retractTime);
            TimeToNextPush = tt < rest ? rest - tt : period - tt + rest;
        }

        public bool IsExtendedInto(Vector3Int c) { return Extension > SolidThreshold && c == ExtendCell; }

        protected override void Update()
        {
            base.Update();
            UpdateHeadVisual();
            if (!Application.isPlaying) return;

            float prev = Extension;
            Evaluate(GameManager.StageTime);
            UpdateHeadVisual();

            // 飛び出した瞬間にキューブが前にいたら押し出す
            if (prev <= SolidThreshold && Extension > SolidThreshold && PlayerCube.Instance != null)
                PlayerCube.Instance.OnPusherFired(this);

            if (headRenderer != null)
            {
                if (mpb == null) mpb = new MaterialPropertyBlock();
                bool warn = Extension <= 0f && TimeToNextPush < warningTime;
                float blink = warn ? 0.5f + 0.5f * Mathf.Sin(Time.time * 40f) : 0f;
                headRenderer.GetPropertyBlock(mpb);
                mpb.SetColor("_Color", Color.Lerp(baseColor, warningColor, blink));
                headRenderer.SetPropertyBlock(mpb);
            }
        }

        void UpdateHeadVisual()
        {
            if (head == null) return;
            Vector3 d = Direction;
            Vector3 pos = d * (restPoke + Extension * (0.98f - restPoke));
            Quaternion rot = Quaternion.LookRotation(d, Vector3.up);
            if (head.localPosition != pos) head.localPosition = pos;
            if (head.localRotation != rot) head.localRotation = rot;
        }

        public override bool Blocks(Vector3Int c) { return c == HomeCell || IsExtendedInto(c); }
        public override bool Climbable(Vector3Int c) { return c == HomeCell; }
        public override bool IsFloor(Vector3Int c) { return c == HomeCell; }
        public override bool SolverSolid(Vector3Int c, int activeMask) { return c == Cell; }

        void OnDrawGizmos()
        {
            Vector3 a = HomeCell, b = (Vector3)ExtendCell;
            Gizmos.color = new Color(1f, 0.55f, 0.3f, 0.9f);
            Gizmos.DrawLine(a, b);
            Gizmos.DrawWireCube(b, Vector3.one * 0.9f);
            Vector3 side = Vector3.Cross(Vector3.up, b - a) * 0.25f;
            Gizmos.DrawLine(b, b - (b - a) * 0.3f + side);
            Gizmos.DrawLine(b, b - (b - a) * 0.3f - side);
        }
    }
}
