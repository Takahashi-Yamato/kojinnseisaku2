using UnityEngine;

namespace CubeOrbit
{
    /// <summary>チェックポイント。通ると、落下した時にここから再開する。ブロックの「上のマス」に置く。</summary>
    [ExecuteAlways]
    [SelectionBase]
    public class Checkpoint : GridElement, IPlayerEnterHandler
    {
        [Header("見た目")]
        public Renderer[] marks;
        public Color inactiveColor = new Color(0.95f, 0.6f, 0.45f);
        public Color activeColor = new Color(0.5f, 0.85f, 1f);

        MaterialPropertyBlock mpb;

        protected override void OnEnable()
        {
            base.OnEnable();
            SetActiveVisual(false);
        }

        public void OnPlayerEnter(PlayerCube player, bool passingThrough)
        {
            if (passingThrough) return;
            if (GameManager.Instance != null) GameManager.Instance.SetCheckpoint(this);
        }

        public void SetActiveVisual(bool on)
        {
            if (marks == null) return;
            if (mpb == null) mpb = new MaterialPropertyBlock();
            foreach (var r in marks)
            {
                if (r == null) continue;
                r.GetPropertyBlock(mpb);
                mpb.SetColor("_Color", on ? activeColor : inactiveColor);
                r.SetPropertyBlock(mpb);
            }
        }
    }
}
