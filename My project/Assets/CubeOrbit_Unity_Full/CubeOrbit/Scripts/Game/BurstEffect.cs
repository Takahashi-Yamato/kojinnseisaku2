using System.Collections.Generic;
using UnityEngine;

namespace CubeOrbit
{
    /// <summary>小さいキューブがはじけ飛ぶエフェクト（ジェム取得・スイッチなど）。使い回しで軽い。</summary>
    public class BurstEffect : MonoBehaviour
    {
        public static BurstEffect Instance { get; private set; }

        [Tooltip("破片に使うマテリアル（CubeOrbit/Block シェーダー推奨）")]
        public Material particleMaterial;
        [Tooltip("破片の重力")] public float gravity = 9f;
        [Tooltip("破片の寿命（秒）")] public Vector2 lifeRange = new Vector2(0.6f, 1.1f);
        [Tooltip("破片の大きさ")] public Vector2 sizeRange = new Vector2(0.06f, 0.16f);
        [Tooltip("同時に出せる最大数")] public int maxParticles = 200;

        class Bit { public Transform tr; public Renderer r; public Vector3 vel; public Vector3 axis; public float life, maxLife, size; }

        readonly List<Bit> active = new List<Bit>();
        readonly Stack<Bit> pool = new Stack<Bit>();
        MaterialPropertyBlock mpb;

        void Awake() { Instance = this; mpb = new MaterialPropertyBlock(); }
        void OnDestroy() { if (Instance == this) Instance = null; }

        public static void Spawn(Vector3 pos, Color color, int count, float speed)
        {
            if (Instance != null) Instance.SpawnInternal(pos, color, count, speed);
        }

        void SpawnInternal(Vector3 pos, Color color, int count, float speed)
        {
            for (int i = 0; i < count && active.Count < maxParticles; i++)
            {
                Bit b = pool.Count > 0 ? pool.Pop() : CreateBit();
                b.tr.gameObject.SetActive(true);
                b.tr.position = pos;
                b.vel = new Vector3(Random.Range(-1f, 1f) * speed, Random.Range(0f, 1.2f) * speed + 1f, Random.Range(-1f, 1f) * speed);
                b.axis = Random.onUnitSphere;
                b.maxLife = b.life = Random.Range(lifeRange.x, lifeRange.y);
                b.size = Random.Range(sizeRange.x, sizeRange.y);
                Color c = color + new Color(Random.value, Random.value, Random.value) * 0.15f;
                b.r.GetPropertyBlock(mpb);
                mpb.SetColor("_Color", c);
                b.r.SetPropertyBlock(mpb);
                active.Add(b);
            }
        }

        Bit CreateBit()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
            go.name = "BurstBit";
            go.transform.SetParent(transform, false);
            var r = go.GetComponent<Renderer>();
            if (particleMaterial != null) r.sharedMaterial = particleMaterial;
            return new Bit { tr = go.transform, r = r };
        }

        void Update()
        {
            float dt = Time.deltaTime;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                var b = active[i];
                b.life -= dt;
                if (b.life <= 0f)
                {
                    b.tr.gameObject.SetActive(false);
                    pool.Push(b);
                    active.RemoveAt(i);
                    continue;
                }
                b.vel.y -= gravity * dt;
                b.tr.position += b.vel * dt;
                b.tr.rotation = Quaternion.AngleAxis(Time.time * 360f, b.axis);
                b.tr.localScale = Vector3.one * b.size * (0.4f + b.life / b.maxLife);
            }
        }
    }
}
