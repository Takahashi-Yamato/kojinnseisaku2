using UnityEngine;

namespace CubeOrbit
{
    /// <summary>
    /// グリッドに置くもの全部の基底クラス。
    /// ・エディタ上では自動で整数座標にスナップする
    /// ・ゲーム中は GridWorld に登録され、マスの問い合わせに答える
    /// 新しいギミックを作るときはこれを継承して Blocks などを上書きする。
    /// </summary>
    [ExecuteAlways]
    public abstract class GridElement : MonoBehaviour
    {
        /// <summary>今いるマス（transform の位置を丸めたもの）</summary>
        public Vector3Int Cell { get { return GridUtil.ToCell(transform.position); } }

        protected virtual void OnEnable()
        {
            if (Application.isPlaying) GridWorld.Register(this);
        }

        protected virtual void OnDisable()
        {
            GridWorld.Unregister(this);
        }

        protected virtual void Update()
        {
            if (!Application.isPlaying) EditorUpdate();
        }

        /// <summary>エディタ上（再生していない時）だけ毎回呼ばれる</summary>
        protected virtual void EditorUpdate()
        {
            SnapToGrid();
        }

        public void SnapToGrid()
        {
            Vector3 p = transform.position;
            Vector3 r = new Vector3(Mathf.Round(p.x), Mathf.Round(p.y), Mathf.Round(p.z));
            if ((p - r).sqrMagnitude > 1e-8f) transform.position = r;
            if (transform.rotation != Quaternion.identity) transform.rotation = Quaternion.identity;
            if (transform.localScale != Vector3.one) transform.localScale = Vector3.one;
        }

        // ---------- ゲーム中の問い合わせ（時間で変わる） ----------
        public virtual bool Blocks(Vector3Int c) { return false; }
        public virtual bool Climbable(Vector3Int c) { return Blocks(c); }
        public virtual bool IsFloor(Vector3Int c) { return Blocks(c); }
        public virtual bool BodyWait(Vector3Int c) { return false; }
        public virtual bool FloorWait(Vector3Int c) { return false; }

        // ---------- ステージ検証用（時間を無視） ----------
        /// <summary>検証ツールから見て、そのマスが足場/壁になりうるか。activeMask はONになっているスイッチ番号のビット</summary>
        public virtual bool SolverSolid(Vector3Int c, int activeMask) { return false; }

        /// <summary>このオブジェクトが占める範囲（自動の落下ライン計算などに使う）</summary>
        public virtual void GetBounds(out Vector3Int min, out Vector3Int max) { min = max = Cell; }
    }
}
