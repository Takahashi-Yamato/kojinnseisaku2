using UnityEngine;

namespace CubeOrbit
{
    /// <summary>
    /// ふつうのブロック。サイズを変えれば床・柱・壁がまとめて1個で作れる。
    /// 置いた位置が「一番小さい角」のマスになり、+X/+Y/+Z 方向へ size だけ広がる。
    /// </summary>
    [ExecuteAlways]
    [SelectionBase]
    [DefaultExecutionOrder(-50)]
    public class BlockVolume : GridElement
    {
        [Tooltip("ブロックの大きさ（マス数）。置いた位置から +X / +Y / +Z へ広がる")]
        public Vector3Int size = Vector3Int.one;

        [Tooltip("見た目のキューブ（子オブジェクト）。サイズに合わせて自動で伸縮する")]
        public Transform visual;

        public Vector3Int Min { get { return Cell; } }
        public Vector3Int Max { get { return Cell + size - Vector3Int.one; } }

        public bool Contains(Vector3Int c)
        {
            Vector3Int a = Min, b = Max;
            return c.x >= a.x && c.x <= b.x && c.y >= a.y && c.y <= b.y && c.z >= a.z && c.z <= b.z;
        }

        public override bool Blocks(Vector3Int c) { return Contains(c); }
        public override bool SolverSolid(Vector3Int c, int activeMask) { return Contains(c); }
        public override void GetBounds(out Vector3Int min, out Vector3Int max) { min = Min; max = Max; }

        protected virtual void Awake() { ApplySize(); }

        protected virtual void OnValidate()
        {
            size = Vector3Int.Max(size, Vector3Int.one);
        }

        protected override void EditorUpdate()
        {
            base.EditorUpdate();
            ApplySize();
        }

        /// <summary>見た目とコライダー（エディタでのクリック配置用）をサイズに合わせる</summary>
        public void ApplySize()
        {
            Vector3 center = (Vector3)(size - Vector3Int.one) * 0.5f;
            if (visual != null)
            {
                if (visual.localPosition != center) visual.localPosition = center;
                if (visual.localScale != (Vector3)size) visual.localScale = size;
            }
            var bc = GetComponent<BoxCollider>();
            if (bc != null)
            {
                bc.center = center;
                bc.size = size;
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 1f, 1f, 0.6f);
            Gizmos.DrawWireCube(transform.position + (Vector3)(size - Vector3Int.one) * 0.5f, (Vector3)size);
        }
    }
}
