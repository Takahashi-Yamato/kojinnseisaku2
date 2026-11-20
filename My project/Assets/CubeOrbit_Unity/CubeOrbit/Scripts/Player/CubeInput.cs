using UnityEngine;

namespace CubeOrbit
{
    /// <summary>
    /// キーボード・スワイプ・タップを「グリッドの方向」に変換する。
    /// 画面の見た目の向きに合わせて自動で方向を選ぶので、カメラの角度を変えても操作がずれない。
    /// </summary>
    public class CubeInput : MonoBehaviour
    {
        [Header("キーボード")]
        [Tooltip("↑キーを画面のどの向きにするか（度、時計回り）。45 = 右上、0 = 真上")]
        public float upKeyScreenAngle = 45f;

        [Header("タッチ / マウス")]
        public bool enablePointer = true;
        [Tooltip("これ以上動かしたらスワイプ、未満ならタップ（画面高さに対する割合）")]
        [Range(0.005f, 0.2f)] public float swipeThreshold = 0.03f;
        [Tooltip("タップした時は「画面の中心から見てタップした向き」に進む")]
        public bool tapToMove = true;

        [Header("先行入力")]
        [Tooltip("動けない間に押した入力を覚えておく時間（秒）")]
        public float bufferTime = 1.2f;

        [Tooltip("空ならメインカメラ")]
        public Camera viewCamera;

        Vector3Int? buffered;
        float bufferedAt;
        Vector2 pointerStart;
        bool pointerActive;

        static readonly GameKey[] MoveKeys = { GameKey.Up, GameKey.Right, GameKey.Down, GameKey.Left };

        Camera Cam { get { return viewCamera != null ? viewCamera : Camera.main; } }

        void Update()
        {
            for (int i = 0; i < 4; i++)
                if (InputCompat.Down(MoveKeys[i])) Buffer(KeyToGrid(i));

            if (!enablePointer) return;
            Vector2 pos;
            if (InputCompat.PointerDown(out pos) && !StageHUD.IsOverButton(pos)) { pointerStart = pos; pointerActive = true; }
            if (pointerActive && InputCompat.PointerUp(out pos))
            {
                pointerActive = false;
                if (GameManager.Instance != null && GameManager.Instance.IsUiBlockingInput) return;
                Vector2 delta = pos - pointerStart;
                if (delta.magnitude < swipeThreshold * Screen.height)
                {
                    if (!tapToMove) return;
                    delta = pos - new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                }
                if (delta.sqrMagnitude > 1f) Buffer(ScreenToGrid(delta));
            }
        }

        void Buffer(Vector3Int d) { buffered = d; bufferedAt = Time.time; }

        /// <summary>今使える入力があれば返す（消費はしない）</summary>
        public bool TryPeek(out Vector3Int dir)
        {
            if (buffered.HasValue && Time.time - bufferedAt <= bufferTime) { dir = buffered.Value; return true; }
            buffered = null;
            for (int i = 0; i < 4; i++)
                if (InputCompat.Held(MoveKeys[i])) { dir = KeyToGrid(i); return true; }
            dir = Vector3Int.zero;
            return false;
        }

        /// <summary>入力を使ったら呼ぶ</summary>
        public void Consume() { buffered = null; }

        public void Clear() { buffered = null; pointerActive = false; }

        Vector3Int KeyToGrid(int keyIndex)
        {
            float angle = (upKeyScreenAngle + keyIndex * 90f) * Mathf.Deg2Rad;
            return ScreenToGrid(new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)));
        }

        /// <summary>画面上の向き → 一番近いグリッド方向</summary>
        public Vector3Int ScreenToGrid(Vector2 screenDir)
        {
            var cam = Cam;
            if (cam == null) return GridUtil.Directions[0];
            Vector3 origin = transform.position;
            Vector2 o = cam.WorldToScreenPoint(origin);
            Vector3Int best = GridUtil.Directions[0];
            float bestDot = float.NegativeInfinity;
            Vector2 want = screenDir.normalized;
            foreach (var d in GridUtil.Directions)
            {
                Vector2 s = (Vector2)cam.WorldToScreenPoint(origin + (Vector3)d) - o;
                float dot = Vector2.Dot(s.normalized, want);
                if (dot > bestDot) { bestDot = dot; best = d; }
            }
            return best;
        }
    }
}
