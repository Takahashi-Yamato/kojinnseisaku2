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
        float shownScore, clearShownSince = -1f;
        public Color scoreColor = new Color(1f, 0.85f, 0.3f);

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
            if (gm != null)
            {
                shownScore = Mathf.Lerp(shownScore, gm.CurrentScore, 1f - Mathf.Exp(-10f * Time.unscaledDeltaTime));
                if (Mathf.Abs(shownScore - gm.CurrentScore) < 5f) shownScore = gm.CurrentScore;
                if (gm.IsCleared && clearShownSince < 0f) clearShownSince = Time.unscaledTime;
            }
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

            // 右上：スコア・ジェム・タイム
            var right = new GUIStyle(label) { alignment = TextAnchor.UpperRight };
            GUI.color = scoreColor;
            GUI.Label(new Rect(Screen.width - 400 - pad, pad, 400, fs * 1.5f), "SCORE " + FormatScore(shownScore), right);
            GUI.color = accentColor;
            GUI.Label(new Rect(Screen.width - 400 - pad, pad + fs * 1.4f, 400, fs * 1.5f), "◆ " + gm.GemsCollected + " / " + gm.GemsTotal, right);
            GUI.color = prev;
            GUI.Label(new Rect(Screen.width - 400 - pad, pad + fs * 2.8f, 400, fs * 1.5f), FormatTime(gm.ClearTime), right);

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

            if (gm.IsPaused && !gm.ConfirmingReset) DrawPause(gm, rects);
            if (gm.IsCleared && !gm.ConfirmingReset) DrawClear(gm, rects);
            if (gm.ConfirmingReset) DrawConfirm(gm, rects);
            buttonRects = rects.ToArray();
        }

        void DrawPause(GameManager gm, System.Collections.Generic.List<Rect> rects)
        {
            int fs = label.fontSize;
            float w = fs * 14f, h = gm.HasTitle ? fs * 17.6f : fs * 14.8f;
            var r = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
            GUI.Box(r, GUIContent.none, panel);
            GUI.Label(new Rect(r.x, r.y + fs, w, fs * 2.5f), "ポーズ", big);
            float bw = w * 0.7f, bh = fs * 2.2f, bx = r.x + (w - bw) / 2f, by = r.y + fs * 4.2f;
            if (GUI.Button(new Rect(bx, by, bw, bh), "つづける", button)) gm.SetPaused(false);
            if (GUI.Button(new Rect(bx, by + bh * 1.25f, bw, bh), "やり直し", button)) gm.Restart();
            if (GUI.Button(new Rect(bx, by + bh * 2.5f, bw, bh), SoundPlayer.Muted ? "サウンド：OFF" : "サウンド：ON", button)) SoundPlayer.ToggleMute();
            float ny = by + bh * 3.75f;
            if (gm.HasTitle) { if (GUI.Button(new Rect(bx, ny, bw, bh), "タイトルへ", button)) gm.LoadTitle(); ny += bh * 1.25f; }
            if (GUI.Button(new Rect(bx, ny, bw, bh), "記録を消して最初から", button)) gm.AskReset();
            rects.Add(r);
        }

        void DrawClear(GameManager gm, System.Collections.Generic.List<Rect> rects)
        {
            int fs = label.fontSize;
            float w = fs * 18f, h = gm.HasTitle ? fs * 30.5f : fs * 27.5f;
            var r = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
            GUI.Box(r, GUIContent.none, panel);
            GUI.Label(new Rect(r.x, r.y + fs, w, fs * 2.5f), gm.HasNextStage ? "STAGE CLEAR" : "ALL CLEAR!", big);

            var rankStyle = new GUIStyle(big) { fontSize = fs * 4 };
            rankStyle.normal.textColor = RankColor(gm.Rank);
            GUI.Label(new Rect(r.x, r.y + fs * 3.5f, w, fs * 4.5f), gm.Rank, rankStyle);

            // スコアの内訳
            var left = new GUIStyle(label) { alignment = TextAnchor.MiddleLeft };
            var rightS = new GUIStyle(label) { alignment = TextAnchor.MiddleRight };
            float lx = r.x + fs * 2f, lw = w - fs * 4f, y = r.y + fs * 8.3f, lh = fs * 1.6f;
            string[] names = { "キューブ ◆ " + gm.GemsCollected + " / " + gm.GemsTotal, "タイム " + FormatTime(gm.ClearTime), "落下 " + gm.Deaths + "回", "クリアボーナス" };
            int[] pts = { gm.ScoreGems, gm.ScoreTime, gm.ScoreNoMiss, gm.ScoreClear };
            Color prevC = GUI.color;
            for (int i = 0; i < names.Length; i++)
            {
                GUI.color = dimColor; GUI.Label(new Rect(lx, y + lh * i, lw, lh), names[i], left);
                GUI.color = textColor; GUI.Label(new Rect(lx, y + lh * i, lw, lh), "+" + FormatScore(pts[i]), rightS);
            }
            float k = clearShownSince < 0f ? 1f : Mathf.Clamp01((Time.unscaledTime - clearShownSince) / 0.9f);
            float shown = gm.FinalScore * (1f - Mathf.Pow(1f - k, 3f));
            var sumStyle = new GUIStyle(rightS) { fontSize = Mathf.RoundToInt(fs * 1.5f), fontStyle = FontStyle.Bold };
            float sy = y + lh * 4.4f;
            GUI.color = textColor; GUI.Label(new Rect(lx, sy, lw, fs * 2f), "SCORE", left);
            GUI.color = scoreColor; GUI.Label(new Rect(lx, sy, lw, fs * 2f), FormatScore(shown), sumStyle);
            if (gm.IsNewRecord && Mathf.Repeat(Time.unscaledTime, 1f) < 0.65f)
            {
                GUI.color = scoreColor;
                GUI.Label(new Rect(r.x, sy + fs * 2.1f, w, fs * 1.4f), "NEW RECORD!", new GUIStyle(label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold });
            }
            GUI.color = prevC;

            float bw = w * 0.7f, bh = fs * 2.2f, bx = r.x + (w - bw) / 2f, by = r.y + fs * 19.5f;
            if (gm.HasNextStage)
            {
                if (GUI.Button(new Rect(bx, by, bw, bh), "次のステージ", button)) gm.LoadNextStage();
                by += bh * 1.25f;
            }
            if (GUI.Button(new Rect(bx, by, bw, bh), "もう一度", button)) gm.Restart();
            by += bh * 1.25f;
            if (gm.HasTitle) { if (GUI.Button(new Rect(bx, by, bw, bh), "タイトルへ", button)) gm.LoadTitle(); by += bh * 1.25f; }
            if (GUI.Button(new Rect(bx, by, bw, bh), "記録を消して最初から", button)) gm.AskReset();
            rects.Add(r);
        }

        void DrawConfirm(GameManager gm, System.Collections.Generic.List<Rect> rects)
        {
            int fs = label.fontSize;
            float w = fs * 20f, h = fs * 15f;
            var r = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
            GUI.Box(r, GUIContent.none, panel);
            var c = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            GUI.Label(new Rect(r.x, r.y + fs, w, fs * 2f), "記録を消して最初から", new GUIStyle(c) { fontStyle = FontStyle.Bold, fontSize = Mathf.RoundToInt(fs * 1.2f) });
            Color prevC = GUI.color; GUI.color = dimColor;
            GUI.Label(new Rect(r.x + fs, r.y + fs * 3.4f, w - fs * 2f, fs * 4.5f), "スコア・ベストタイム・集めたキューブの記録がすべて消えて、ステージ1から始まります。元には戻せません。", c);
            GUI.color = prevC;
            float bw = w * 0.7f, bh = fs * 2.2f, bx = r.x + (w - bw) / 2f, by = r.y + fs * 8.6f;
            if (GUI.Button(new Rect(bx, by, bw, bh), "消して最初から", button)) gm.ResetAndStartOver();
            if (GUI.Button(new Rect(bx, by + bh * 1.25f, bw, bh), "やめる", button)) gm.CancelReset();
            rects.Add(r);
        }

        public static string FormatScore(float s) { return Mathf.RoundToInt(s).ToString("N0", System.Globalization.CultureInfo.InvariantCulture); }

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
