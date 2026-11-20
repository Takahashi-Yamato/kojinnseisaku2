using UnityEngine;

namespace CubeOrbit
{
    /// <summary>斜め上からの見下ろしカメラ（平行投影）。キューブをなめらかに追いかける。</summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        [Tooltip("追いかける対象（空ならプレイヤー）")]
        public Transform target;

        [Header("角度")]
        [Tooltip("注視点から見たカメラの方向。(10, 11.5, 10) で動画のような斜め見下ろし")]
        public Vector3 viewDirection = new Vector3(10f, 11.5f, 10f);
        public float distance = 40f;

        [Header("ズーム")]
        [Tooltip("横長画面での表示の広さ（大きいほど引き）")]
        public float orthographicSize = 6.6f;
        [Tooltip("縦長画面（スマホ）での横幅の半分")]
        public float portraitHalfWidth = 5.6f;

        [Header("追従")]
        [Tooltip("追いかける速さ（大きいほどキビキビ）")]
        public float followSpeed = 5f;
        [Tooltip("カメラがこれより下を見ないようにする")]
        public float minFocusY = -1f;

        [Header("ゆれ")]
        public float shakeStrength = 0.6f;
        public float shakeDecay = 1f;

        static float shake;
        Camera cam;
        Vector3 focus;
        PlayerCube player;

        /// <summary>画面をゆらす（amount は0.05〜0.3くらい）</summary>
        public static void Shake(float amount) { shake = Mathf.Max(shake, amount); }

        void Start()
        {
            cam = GetComponent<Camera>();
            cam.orthographic = true;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = distance * 4f;
            player = PlayerCube.Instance != null ? PlayerCube.Instance : GridUtil.FindOne<PlayerCube>();
            if (target == null && player != null) target = player.transform;
            if (target != null) focus = Clamp(target.position);
            Apply();
        }

        void LateUpdate()
        {
            bool dead = player != null && player.CurrentState == PlayerCube.State.Dead;
            if (target != null && !dead)
            {
                float k = 1f - Mathf.Exp(-followSpeed * Time.unscaledDeltaTime);
                focus = Vector3.Lerp(focus, Clamp(target.position), k);
            }
            shake = Mathf.Max(0f, shake - shakeDecay * Time.unscaledDeltaTime);
            Apply();
        }

        Vector3 Clamp(Vector3 p) { p.y = Mathf.Max(p.y, minFocusY); return p; }

        void Apply()
        {
            if (cam == null) cam = GetComponent<Camera>();
            float aspect = cam.aspect;
            cam.orthographicSize = aspect >= 1f ? orthographicSize : portraitHalfWidth / Mathf.Max(0.3f, aspect);
            Vector3 dir = viewDirection.sqrMagnitude > 0.001f ? viewDirection.normalized : Vector3.one.normalized;
            transform.rotation = Quaternion.LookRotation(-dir, Vector3.up);
            Vector3 jitter = shake > 0f ? (transform.right * Random.Range(-1f, 1f) + transform.up * Random.Range(-1f, 1f)) * shake * shakeStrength * 0.5f : Vector3.zero;
            transform.position = focus + dir * distance + jitter;
        }
    }
}
