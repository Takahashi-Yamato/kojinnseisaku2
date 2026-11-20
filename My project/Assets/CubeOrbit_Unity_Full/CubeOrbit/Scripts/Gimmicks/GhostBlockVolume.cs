using UnityEngine;

namespace CubeOrbit
{
    /// <summary>
    /// 透明ブロック。同じ番号の FloorSwitch を踏むと実体化して乗れるようになる。
    /// </summary>
    [ExecuteAlways]
    public class GhostBlockVolume : BlockVolume
    {
        [Header("スイッチ")]
        [Tooltip("この番号のスイッチを踏むと実体化する")]
        public int group = 1;

        [Header("見た目")]
        public Material ghostMaterial;
        public Material solidMaterial;
        [Tooltip("実体化アニメの長さ（秒）")]
        public float appearDuration = 0.35f;
        [Tooltip("スイッチから1マス離れるごとに遅れる時間（秒）。波のように出現する")]
        public float delayPerDistance = 0.06f;

        Renderer rend;
        float appearTimer;
        bool appearing;
        MaterialPropertyBlock mpb;

        bool Active { get { return SwitchSystem.IsActive(group); } }

        public override bool Blocks(Vector3Int c) { return Active && Contains(c); }
        public override bool SolverSolid(Vector3Int c, int activeMask)
        {
            return group >= 0 && group < 31 && (activeMask & (1 << group)) != 0 && Contains(c);
        }

        protected override void Awake()
        {
            base.Awake();
            rend = visual != null ? visual.GetComponent<Renderer>() : null;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Application.isPlaying) SwitchSystem.Activated += OnActivated;
            if (rend == null && visual != null) rend = visual.GetComponent<Renderer>();
            ShowGhost();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            SwitchSystem.Activated -= OnActivated;
        }

        void ShowGhost()
        {
            if (rend == null) return;
            if (ghostMaterial != null) rend.sharedMaterial = ghostMaterial;
            if (mpb == null) mpb = new MaterialPropertyBlock();
            rend.GetPropertyBlock(mpb);
            mpb.SetColor("_Color", GridUtil.GroupColor(group));
            rend.SetPropertyBlock(mpb);
        }

        void OnActivated(int g, Vector3 from)
        {
            if (g != group) return;
            Vector3 center = transform.position + (Vector3)(size - Vector3Int.one) * 0.5f;
            appearTimer = -Vector3.Distance(center, from) * delayPerDistance;
            appearing = true;
        }

        protected override void EditorUpdate()
        {
            base.EditorUpdate();
            ShowGhost();
        }

        protected override void Update()
        {
            base.Update();
            if (!Application.isPlaying || !appearing) return;
            appearTimer += Time.deltaTime;
            if (appearTimer < 0f) return; // 遅延中
            float k = Mathf.Clamp01(appearTimer / appearDuration);
            float e = 1f - Mathf.Pow(1f - k, 3f);
            if (rend != null && solidMaterial != null && rend.sharedMaterial != solidMaterial)
            {
                rend.sharedMaterial = solidMaterial;
                rend.SetPropertyBlock(null);
            }
            if (visual != null) visual.localScale = (Vector3)size * (0.3f + 0.7f * e);
            if (k >= 1f) { appearing = false; ApplySize(); }
        }

        protected override void OnDrawGizmosSelected()
        {
            Gizmos.color = GridUtil.GroupColor(group);
            Gizmos.DrawWireCube(transform.position + (Vector3)(size - Vector3Int.one) * 0.5f, (Vector3)size);
        }
    }
}
