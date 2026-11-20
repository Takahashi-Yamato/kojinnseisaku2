using UnityEngine;

namespace CubeOrbit
{
    /// <summary>背景の星。カメラの子に小さいキューブをばらまいて、ゆっくり瞬かせる。</summary>
    [RequireComponent(typeof(Camera))]
    public class StarField : MonoBehaviour
    {
        public Material starMaterial;
        [Min(0)] public int count = 220;
        public Vector2 sizeRange = new Vector2(0.02f, 0.07f);
        [Tooltip("カメラからの距離（ファークリップより手前にする）")]
        public float depth = 120f;
        [Tooltip("カメラの移動に対してどれだけずれるか（遠近感）")]
        public float parallax = 0.03f;
        public float twinkleSpeed = 0.7f;

        Transform root;
        Transform[] stars;
        Vector2[] basePos;
        float[] baseSize, phase;
        Camera cam;

        void Start()
        {
            cam = GetComponent<Camera>();
            root = new GameObject("Stars").transform;
            root.SetParent(transform, false);
            stars = new Transform[count];
            basePos = new Vector2[count];
            baseSize = new float[count];
            phase = new float[count];
            for (int i = 0; i < count; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                var col = go.GetComponent<Collider>();
                if (col != null) Destroy(col);
                go.name = "Star";
                if (starMaterial != null) go.GetComponent<Renderer>().sharedMaterial = starMaterial;
                go.transform.SetParent(root, false);
                stars[i] = go.transform;
                basePos[i] = new Vector2(Random.value, Random.value);
                baseSize[i] = Random.value < 0.1f ? sizeRange.y * 1.6f : Random.Range(sizeRange.x, sizeRange.y);
                phase[i] = Random.value * 6.28f;
            }
        }

        void LateUpdate()
        {
            if (stars == null) return;
            float h = cam.orthographic ? cam.orthographicSize * 2f : 20f;
            float w = h * cam.aspect;
            Vector3 camPos = transform.position;
            Vector2 off = new Vector2(Vector3.Dot(camPos, transform.right), Vector3.Dot(camPos, transform.up)) * parallax;
            float scale = h / 13f;
            for (int i = 0; i < stars.Length; i++)
            {
                float x = Mathf.Repeat(basePos[i].x - off.x / w, 1f) - 0.5f;
                float y = Mathf.Repeat(basePos[i].y - off.y / h, 1f) - 0.5f;
                stars[i].localPosition = new Vector3(x * w, y * h, depth);
                float tw = 0.5f + 0.5f * Mathf.Abs(Mathf.Sin(Time.time * twinkleSpeed + phase[i]));
                stars[i].localScale = Vector3.one * baseSize[i] * scale * (0.6f + 0.4f * tw);
            }
        }
    }
}
