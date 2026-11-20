using UnityEngine;

namespace CubeOrbit
{
    /// <summary>
    /// 画面表示（ステージ名・ジェム数・タイム・ヒント・ポーズ・クリア画面）。
    /// セットアップ不要で動くよう OnGUI で描いている。見た目を凝りたくなったら uGUI / UI Toolkit に置き換えてOK。
    /// </summary>
    public class StageHUD : MonoBehaviour
    {
        [Tooltip("文字の大きさ（画面の高さに対する割合）")]
        [Range(0.015f, 0.06f)] public float fontScale = 0.028f;
        public Color textColor = new Color(0.91f, 0.93f, 0.96f);
        public Color dimColor = new Color(0.54f, 0.56f, 0.65f);
        public Color accentColor = new Color(0.5f, 0.96f, 0.78f);
        [Tooltip("何回動いたらヒントを消すか")]
        public int hideHintAfterMoves = 6;
        [Tooltip("右下にリトライ・ポーズのボタンを出す（スマホ向け）")]
        public bool showTouchButtons = true;

        static Rect[] buttonRects = new Rect[0];
        GUIStyle label, big, button, panel;
        Texture2D panelTex;
        float hintAlpha = 1f;

        /// <summary>画面座標（左下原点）がHUDのボタンの上か。タップ移動と区別するため</summary>
        public static bool IsOverButton(Vector2 screenPos)
        {
            Vector2 gui = new Vector2(screenPos.x, Screen.height - screenPos.y);
            foreach (var r in buttonRects) if (r.Contains(gui)) return true;
            return false;
        }

        void EnsureStyles()
        {
            int fs = Mathf.RoundToInt(Screen.height * fontScale);
            if (label != null && label.fontSize == fs) return;
            label = new GUIStyle(GUI.skin.label) { fontSize = fs };
            label.normal.textColor = textColor;
            big = new GUIStyle(label) { fontSize = fs * 2, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            button = new GUIStyle(GUI.skin.button) { fontSize = fs };
            if (panelTex == null)
            {
                panelTex = new Texture2D(1, 1);
                panelTex.SetPixel(0, 0, new Color(0.02f, 0.025f, 0.05f, 0.88f));
                panelTex.Apply();
            }
            panel = new GUIStyle(GUI.skin.box);
            panel.normal.background = panelTex;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            var p = PlayerCube.Instance;
            if (gm != null && p != null && p.MoveCount >= hideHintAfterMoves)
                hintAlpha = Mathf.MoveTowards(hintAlpha, 0f, Time.unscaledDeltaTime * 1.5f);
        }

        void OnGUI()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            EnsureStyles();
            int fs = label.fontSize;
            float pad = fs * 0.8f;
            var rects = new System.Collections.Generic.List<Rect>();

            // 左上：ステージ
            int num = gm.DisplayStageNumber;
            Color prev = GUI.color;
            GUI.color = dimColor;
            GUI.Label(new Rect(pad, pad, 600, fs * 1.5f), num > 0 ? "STAGE " + num : "STAGE", label);
            GUI.color = prev;
            GUI.Label(new Rect(pad, pad + fs * 1.3f, 800, fs * 1.6f), gm.stageName, label);

            // 右上：ジェム・タイム
            var right = new GUIStyle(label) { alignment = TextAnchor.UpperRight };
            GUI.color = accentColor;
            GUI.Label(new Rect(Screen.width - 400 - pad, pad, 400, fs * 1.5f), "◆ " + gm.GemsCollected + " / " + gm.GemsTotal, right);
            GUI.color = prev;
            GUI.Label(new Rect(Screen.width - 400 - pad, pad + fs * 1.4f, 400, fs * 1.5f), FormatTime(gm.ClearTime), right);

            // 下：ヒント
            if (!string.IsNullOrEmpty(gm.hint) && hintAlpha > 0f)
            {
                var hs = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
                GUI.color = new Color(dimColor.r, dimColor.g, dimColor.b, hintAlpha);
                GUI.Label(new Rect(Screen.width * 0.1f, Screen.height - fs * 4f, Screen.width * 0.8f, fs * 3f), gm.hint, hs);
                GUI.color = prev;
            }

            // 右下：ボタン
            if (showTouchButtons && !gm.IsCleared && !gm.IsPaused)
            {
                float bs = fs * 2.4f;
                var rRestart = new Rect(Screen.width - pad - bs * 2 - pad * 0.5f, Screen.height - pad - bs, bs, bs);
                var rPause = new Rect(Screen.width - pad - bs, Screen.height - pad - bs, bs, bs);
                if (GUI.Button(rRestart, "↺", button)) gm.Restart();
                if (GUI.Button(rPause, "Ⅱ", button)) gm.SetPaused(true);
                rects.Add(rRestart); rects.Add(rPause);
            }

            if (gm.IsPaused) DrawPause(gm, rects);
            if (gm.IsCleared) DrawClear(gm, rects);
            buttonRects = rects.ToArray();
        }

        void DrawPause(GameManager gm, System.Collections.Generic.List<Rect> rects)
        {
            int fs = label.fontSize;
            float w = fs * 14f, h = gm.HasTitle ? fs * 14.8f : fs * 12f;
            var r = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
            GUI.Box(r, GUIContent.none, panel);
            GUI.Label(new Rect(r.x, r.y + fs, w, fs * 2.5f), "ポーズ", big);
            float bw = w * 0.7f, bh = fs * 2.2f, bx = r.x + (w - bw) / 2f, by = r.y + fs * 4.2f;
            if (GUI.Button(new Rect(bx, by, bw, bh), "つづける", button)) gm.SetPaused(false);
            if (GUI.Button(new Rect(bx, by + bh * 1.25f, bw, bh), "やり直し", button)) gm.Restart();
            if (GUI.Button(new Rect(bx, by + bh * 2.5f, bw, bh), SoundPlayer.Muted ? "サウンド：OFF" : "サウンド：ON", button)) SoundPlayer.ToggleMute();
            if (gm.HasTitle && GUI.Button(new Rect(bx, by + bh * 3.75f, bw, bh), "タイトルへ", button)) gm.LoadTitle();
            rects.Add(r);
        }

        void DrawClear(GameManager gm, System.Collections.Generic.List<Rect> rects)
        {
            int fs = label.fontSize;
            float w = fs * 16f, h = gm.HasTitle ? fs * 22.6f : fs * 19f;
            var r = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
            GUI.Box(r, GUIContent.none, panel);
            GUI.Label(new Rect(r.x, r.y + fs, w, fs * 2.5f), gm.HasNextStage ? "STAGE CLEAR" : "ALL CLEAR!", big);

            var rankStyle = new GUIStyle(big) { fontSize = fs * 4 };
            rankStyle.normal.textColor = RankColor(gm.Rank);
            GUI.Label(new Rect(r.x, r.y + fs * 3.5f, w, fs * 4.5f), gm.Rank, rankStyle);

            var center = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter };
            float y = r.y + fs * 8.5f;
            GUI.Label(new Rect(r.x, y, w, fs * 1.6f), "タイム  " + FormatTime(gm.ClearTime), center);
            GUI.Label(new Rect(r.x, y + fs * 1.6f, w, fs * 1.6f), "キューブ  ◆ " + gm.GemsCollected + " / " + gm.GemsTotal, center);
            GUI.Label(new Rect(r.x, y + fs * 3.2f, w, fs * 1.6f), "落下  " + gm.Deaths, center);

            float bw = w * 0.7f, bh = fs * 2.2f, bx = r.x + (w - bw) / 2f, by = r.y + fs * 14f;
            if (gm.HasNextStage)
            {
                if (GUI.Button(new Rect(bx, by, bw, bh), "次のステージ", button)) gm.LoadNextStage();
                by += bh * 1.25f;
            }
            if (GUI.Button(new Rect(bx, by, bw, bh), "もう一度", button)) gm.Restart();
            if (gm.HasTitle && GUI.Button(new Rect(bx, by + bh * 1.25f, bw, bh), "タイトルへ", button)) gm.LoadTitle();
            rects.Add(r);
        }

        static Color RankColor(string rank)
        {
            switch (rank)
            {
                case "S": return new Color(1f, 0.85f, 0.3f);
                case "A": return new Color(0.5f, 0.96f, 0.78f);
                case "B": return new Color(0.44f, 0.72f, 1f);
                default: return new Color(0.79f, 0.8f, 0.88f);
            }
        }

        public static string FormatTime(float t)
        {
            int m = Mathf.FloorToInt(t / 60f);
            float s = t - m * 60f;
            return m.ToString("00") + ":" + s.ToString("00.0");
        }
    }
}
