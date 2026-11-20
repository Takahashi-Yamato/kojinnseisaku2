using UnityEngine;

namespace CubeOrbit
{
    /// <summary>
    /// 床スイッチ。踏むと同じ番号の透明ブロック（GhostBlockVolume）が実体化する。
    /// ブロックの「上のマス」に置く。
    /// </summary>
    [ExecuteAlways]
    [SelectionBase]
    public class FloorSwitch : GridElement, IPlayerEnterHandler
    {
        [Tooltip("スイッチ番号。同じ番号の透明ブロックが出現する")]
        public int group = 1;

        [Header("見た目")]
        public Renderer plate;
        public Color offColor = new Color(0.95f, 0.96f, 1f);
        [Tooltip("押した後はグループ色にこの明るさを掛けた色になる")]
        [Range(0f, 1f)] public float usedBrightness = 0.6f;

        bool used;
        MaterialPropertyBlock mpb;

        protected override void OnEnable()
        {
            base.OnEnable();
            SetColor(offColor);
        }

        public void OnPlayerEnter(PlayerCube player, bool passingThrough)
        {
            if (passingThrough || used) return;
            used = true;
            SwitchSystem.Activate(group, transform.position);
            SetColor(GridUtil.GroupColor(group) * usedBrightness);
            SoundPlayer.Play(Sfx.Switch);
            CameraFollow.Shake(0.15f);
            BurstEffect.Spawn(transform.position + Vector3.down * 0.4f, GridUtil.GroupColor(group), 14, 3f);
        }

        void SetColor(Color c)
        {
            if (plate == null) return;
            if (mpb == null) mpb = new MaterialPropertyBlock();
            plate.GetPropertyBlock(mpb);
            mpb.SetColor("_Color", c);
            plate.SetPropertyBlock(mpb);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = GridUtil.GroupColor(group);
            Gizmos.DrawWireCube(transform.position + Vector3.down * 0.47f, new Vector3(0.5f, 0.06f, 0.5f));
        }
    }
}
