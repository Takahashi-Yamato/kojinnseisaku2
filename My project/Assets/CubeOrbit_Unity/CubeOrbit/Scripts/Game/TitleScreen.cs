using UnityEngine;
using UnityEngine.SceneManagement;

namespace CubeOrbit
{
    /// <summary>
    /// タイトル画面とステージ選択。Title シーンに1個置く。
    /// メニュー「Tools > Cube Orbit > タイトル画面を作る」で自動生成できる。
    /// </summary>
    public class TitleScreen : MonoBehaviour
    {
        [Header("文字")]
        public string title = "CUBE ORBIT";
        public string subtitle = "キューブを転がして、宇宙のゴールへ";
        [Tooltip("文字の大きさ（画面の高さに対する割合）")]
        [Range(0.015f, 0.06f)] public float fontScale = 0.028f;
        [Tooltip("タイトル文字の大きさ（普通の文字の何倍か）")]
        public float titleScale = 3.2f;
        [Tooltip("タイトルの色が流れる速さ")]
        public float hueSpeed = 0.08f;
        public Color textColor = new Color(0.91f, 0.93f, 0.96f);
        public Color dimColor = new Color(0.54f, 0.56f, 0.65f);

        [Header("ステージ")]
        [Tooltip("「スタート」で始める最初のステージの Build Settings 番号（タイトルが0番なら1）")]
        public int firstStageBuildIndex = 1;

        [Header("飾り")]
        [Tooltip("ゆっくり回すキューブ（なくてもOK）")]
        public Transform decorCube;
        public Vector3 decorSpin = new Vector3(20f, 40f, 10f);

        bool selecting;
        Vector2 scroll;
        GUIStyle label, button, titleStyle, cardStyle;
        int cachedSize = -1;

        void Start() { Time.timeScale = 1f; }

        void Update()
        {
            if (decorCube != null) decorCube.Rotate(decorSpin * Time.deltaTime, Space.World);
            if (!selecting && InputCompat.Down(GameKey.Confirm)) StartGame();
            if (selecting && InputCompat.Down(GameKey.Pause)) selecting = false;
        }

        public void StartGame()
        {
            if (firstStageBuildIndex < SceneManager.sceneCountInBuildSettings) SceneManager.LoadScene(firstStageBuildIndex);
            else Debug.LogWarning("Build Settings にステージがありません");
        }

        void EnsureStyles()
        {
            int fs = Mathf.RoundToInt(Screen.height * fontScale);
            if (fs == cachedSize && label != null) return;
            cachedSize = fs;
            label = new GUIStyle(GUI.skin.label) { fontSize = fs, alignment = TextAnchor.MiddleCenter };
            label.normal.textColor = textColor;
            titleStyle = new GUIStyle(label) { fontSize = Mathf.RoundToInt(fs * titleScale), fontStyle = FontStyle.Bold };
            button = new GUIStyle(GUI.skin.button) { fontSize = fs };
            cardStyle = new GUIStyle(GUI.skin.button) { fontSize = Mathf.RoundToInt(fs * 0.8f), alignment = TextAnchor.MiddleCenter, wordWrap = true };
        }

        void OnGUI()
        {
            EnsureStyles();
            if (selecting) DrawSelect(); else DrawTitle();
        }

        void DrawTitle()
        {
            int fs = label.fontSize;
            float cy = Screen.height * 0.36f;

            // 1文字ずつ色を変えて描く
            float total = 0f;
            var sizes = new Vector2[title.Length];
            for (int i = 0; i < title.Length; i++) { sizes[i] = titleStyle.CalcSize(new GUIContent(title[i].ToString())); total += sizes[i].x; }
            float x = (Screen.width - total) / 2f;
            Color prev = GUI.contentColor;
            for (int i = 0; i < title.Length; i++)
            {
                GUI.contentColor = Color.HSVToRGB(Mathf.Repeat(Time.time * hueSpeed + i * 0.07f, 1f), 0.55f, 1f);
                GUI.Label(new Rect(x, cy - sizes[i].y / 2f, sizes[i].x, sizes[i].y), title[i].ToString(), titleStyle);
                x += sizes[i].x;
            }
            GUI.contentColor = dimColor;
            GUI.Label(new Rect(0, cy + fs * 2.6f, Screen.width, fs * 1.6f), subtitle, label);
            GUI.contentColor = prev;

            float bw = fs * 11f, bh = fs * 2.2f, bx = (Screen.width - bw) / 2f, by = cy + fs * 5.5f;
            if (GUI.Button(new Rect(bx, by, bw, bh), "スタート", button)) StartGame();
            if (GUI.Button(new Rect(bx, by + bh * 1.3f, bw, bh), "ステージ選択", button)) selecting = true;
#if !UNITY_EDITOR && !UNITY_WEBGL
            if (GUI.Button(new Rect(bx, by + bh * 2.6f, bw, bh), "おわる", button)) Application.Quit();
#endif
            GUI.contentColor = dimColor;
            GUI.Label(new Rect(0, Screen.height - fs * 2.4f, Screen.width, fs * 1.6f), "移動：矢印キー / WASD ・ スマホはスワイプ　　R：やり直し　Esc：ポーズ", new GUIStyle(label) { fontSize = Mathf.RoundToInt(fs * 0.7f) });
            GUI.contentColor = prev;
        }

        void DrawSelect()
        {
            int fs = label.fontSize;
            GUI.Label(new Rect(0, fs, Screen.width, fs * 2.4f), "ステージ選択", new GUIStyle(label) { fontSize = Mathf.RoundToInt(fs * 1.4f) });

            int count = SceneManager.sceneCountInBuildSettings - firstStageBuildIndex;
            if (System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(SceneManager.sceneCountInBuildSettings - 1)) == "Result") count--;
            int cols = Mathf.Max(1, Mathf.FloorToInt(Screen.width * 0.9f / (fs * 8f)));
            float cw = fs * 7.4f, ch = fs * 4.2f, gap = fs * 0.6f;
            float gridW = cols * cw + (cols - 1) * gap;
            int rows = Mathf.CeilToInt(count / (float)cols);
            var view = new Rect((Screen.width - gridW) / 2f - 10f, fs * 4f, gridW + 30f, Screen.height - fs * 8.5f);
            scroll = GUI.BeginScrollView(view, scroll, new Rect(0, 0, gridW, rows * (ch + gap)));
            for (int i = 0; i < count; i++)
            {
                int buildIndex = firstStageBuildIndex + i;
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(buildIndex));
                float t; int gems;
                string best = GameManager.TryGetBestFor(sceneName, out t, out gems) ? "◆" + gems + "  " + StageHUD.FormatTime(t) : "未クリア";
                var r = new Rect((i % cols) * (cw + gap), (i / cols) * (ch + gap), cw, ch);
                if (GUI.Button(r, "STAGE " + (i + 1) + "\n" + best, cardStyle)) SceneManager.LoadScene(buildIndex);
            }
            GUI.EndScrollView();

            float bw = fs * 8f, bh = fs * 2f;
            if (GUI.Button(new Rect((Screen.width - bw) / 2f, Screen.height - bh - fs * 1.5f, bw, bh), "もどる", button)) selecting = false;
        }
    }
}
