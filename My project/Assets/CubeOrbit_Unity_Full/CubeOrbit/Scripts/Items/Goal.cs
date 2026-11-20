using UnityEngine;

namespace CubeOrbit
{
    /// <summary>ゴール。ブロックの「上のマス」に置く。乗った瞬間にクリア。</summary>
    [ExecuteAlways]
    [SelectionBase]
    public class Goal : GridElement, IPlayerEnterHandler
    {
        [Header("見た目")]
        public Renderer pad;
        public Renderer inner;
        public Transform innerTransform;
        [Tooltip("色が変わっていく速さ")] public float hueSpeed = 0.05f;
        [Tooltip("内側の四角の大きさ")] public float innerSize = 0.42f;
        [Tooltip("内側の四角の脈打つ幅")] public float pulse = 0.04f;

        MaterialPropertyBlock mpb;

        public void OnPlayerEnter(PlayerCube player, bool passingThrough)
        {
            if (GameManager.Instance != null) GameManager.Instance.ReachGoal();
        }

        protected override void Update()
        {
            base.Update();
            float t = Application.isPlaying ? Time.time : 0f;
            if (mpb == null) mpb = new MaterialPropertyBlock();
            if (pad != null)
            {
                pad.GetPropertyBlock(mpb);
                mpb.SetColor("_Color", Color.HSVToRGB(Mathf.Repeat(t * hueSpeed + 0.35f, 1f), 0.6f, 0.95f));
                pad.SetPropertyBlock(mpb);
            }
            if (inner != null)
            {
                inner.GetPropertyBlock(mpb);
                mpb.SetColor("_Color", Color.HSVToRGB(Mathf.Repeat(t * hueSpeed + 0.85f, 1f), 0.55f, 1f));
                inner.SetPropertyBlock(mpb);
            }
            if (innerTransform != null && Application.isPlaying)
            {
                float s = innerSize + Mathf.Sin(t * 3f) * pulse;
                innerTransform.localScale = new Vector3(s, innerTransform.localScale.y, s);
            }
        }
    }
}
