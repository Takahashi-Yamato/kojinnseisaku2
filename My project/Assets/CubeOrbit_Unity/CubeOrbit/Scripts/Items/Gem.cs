using UnityEngine;

namespace CubeOrbit
{
    /// <summary>集める小さいキューブ。ブロックの「上のマス」に置く。</summary>
    [ExecuteAlways]
    [SelectionBase]
    public class Gem : GridElement, IPlayerEnterHandler
    {
        [Header("色")]
        [Tooltip("ONにすると置いた場所から自動で色を決める")]
        public bool autoColor = true;
        public Color color = new Color(1f, 0.85f, 0.3f);

        [Header("動き")]
        public Transform visual;
        public Renderer visualRenderer;
        [Tooltip("回転スピード（度/秒）")] public float spinSpeed = 90f;
        [Tooltip("上下にゆれる幅")] public float bobHeight = 0.08f;
        [Tooltip("上下にゆれる速さ")] public float bobSpeed = 2.4f;
        [Tooltip("見た目の基準の高さ（マスの中心からのずれ）")] public float baseHeight = -0.12f;

        MaterialPropertyBlock mpb;
        float phase;

        /// <summary>色相（0〜1）。プレイヤーの色をこれに合わせる</summary>
        public float Hue
        {
            get { float h, s, v; Color.RGBToHSV(CurrentColor, out h, out s, out v); return h; }
        }

        Color CurrentColor
        {
            get
            {
                if (!autoColor) return color;
                Vector3Int c = Cell;
                float h = Mathf.Repeat((c.x * 0.13f + c.y * 0.31f + c.z * 0.07f), 1f);
                return Color.HSVToRGB(h, 0.85f, 1f);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            phase = Random.value * 6f;
            ApplyColor();
        }

        protected override void EditorUpdate()
        {
            base.EditorUpdate();
            ApplyColor();
        }

        void ApplyColor()
        {
            if (visualRenderer == null) return;
            if (mpb == null) mpb = new MaterialPropertyBlock();
            visualRenderer.GetPropertyBlock(mpb);
            mpb.SetColor("_Color", CurrentColor);
            visualRenderer.SetPropertyBlock(mpb);
        }

        protected override void Update()
        {
            base.Update();
            if (!Application.isPlaying || visual == null) return;
            float t = Time.time;
            visual.localPosition = new Vector3(0f, baseHeight + Mathf.Sin(t * bobSpeed + phase) * bobHeight, 0f);
            visual.localRotation = Quaternion.AngleAxis(t * spinSpeed + phase * 50f, new Vector3(0.3f, 1f, 0.2f).normalized);
        }

        public void OnPlayerEnter(PlayerCube player, bool passingThrough)
        {
            if (GameManager.Instance != null) GameManager.Instance.CollectGem(this);
            SoundPlayer.Play(Sfx.Gem);
            BurstEffect.Spawn(transform.position, CurrentColor, 18, 4f);
            var cc = player.GetComponent<CubeColorCycle>();
            if (cc != null) cc.SetHue(Hue);
            gameObject.SetActive(false);
        }
    }
}
