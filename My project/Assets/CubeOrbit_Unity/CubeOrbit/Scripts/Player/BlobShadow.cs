using UnityEngine;

namespace CubeOrbit
{
    /// <summary>キューブの真下の足場に丸くない影（四角い影）を落とす。キューブとは別のオブジェクトに付ける。</summary>
    public class BlobShadow : MonoBehaviour
    {
        public PlayerCube player;
        public Renderer shadowRenderer;
        [Tooltip("接地している時の濃さ")] [Range(0f, 1f)] public float maxAlpha = 0.42f;
        [Tooltip("1マス離れるごとに薄くなる量")] public float fadePerUnit = 0.05f;
        [Tooltip("1マス離れるごとに小さくなる量")] public float shrinkPerUnit = 0.12f;
        [Tooltip("最小サイズ")] public float minSize = 0.3f;
        [Tooltip("下を探す最大距離")] public int searchDepth = 30;

        MaterialPropertyBlock mpb;

        void LateUpdate()
        {
            if (player == null) player = PlayerCube.Instance;
            if (player == null || shadowRenderer == null) return;

            bool visible = player.CurrentState != PlayerCube.State.Dead && player.CurrentState != PlayerCube.State.Win;
            Vector3 p = player.transform.position;
            int x = Mathf.RoundToInt(p.x), z = Mathf.RoundToInt(p.z);
            int? surface = null;
            if (visible)
            {
                int top = Mathf.CeilToInt(p.y - 0.01f);
                for (int y = top; y > top - searchDepth; y--)
                {
                    var b = new Vector3Int(x, y - 1, z);
                    if (GridWorld.Live.Block(b) || GridWorld.Live.Floor(b)) { surface = y; break; }
                }
            }
            shadowRenderer.enabled = surface.HasValue;
            if (!surface.HasValue) return;

            float h = Mathf.Max(0f, p.y - surface.Value);
            float s = Mathf.Max(minSize, 1.05f - h * shrinkPerUnit);
            transform.position = new Vector3(p.x, surface.Value - 0.5f + 0.006f, p.z);
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            transform.localScale = new Vector3(s, s, 1f);

            if (mpb == null) mpb = new MaterialPropertyBlock();
            shadowRenderer.GetPropertyBlock(mpb);
            mpb.SetColor("_Color", new Color(0f, 0f, 0f, Mathf.Max(0f, maxAlpha - h * fadePerUnit)));
            shadowRenderer.SetPropertyBlock(mpb);
        }
    }
}
