using UnityEngine;

namespace CubeOrbit
{
    /// <summary>キューブの色をゆっくり虹色に変えていく。ジェムを取るとその色に寄る。</summary>
    public class CubeColorCycle : MonoBehaviour
    {
        public Renderer target;
        [Tooltip("色が一周する速さ（1秒あたり）")] public float hueSpeed = 0.06f;
        [Range(0f, 1f)] public float saturation = 0.75f;
        [Range(0f, 1f)] public float brightness = 0.95f;
        [Tooltip("開始時の色をランダムにする")] public bool randomStart = true;

        float hue;
        MaterialPropertyBlock mpb;

        void Awake()
        {
            if (target == null) target = GetComponentInChildren<Renderer>();
            hue = randomStart ? Random.value : 0f;
        }

        public void SetHue(float h) { hue = h; }

        void Update()
        {
            hue = Mathf.Repeat(hue + hueSpeed * Time.deltaTime, 1f);
            if (target == null) return;
            if (mpb == null) mpb = new MaterialPropertyBlock();
            target.GetPropertyBlock(mpb);
            mpb.SetColor("_Color", Color.HSVToRGB(hue, saturation, brightness));
            target.SetPropertyBlock(mpb);
        }
    }
}
