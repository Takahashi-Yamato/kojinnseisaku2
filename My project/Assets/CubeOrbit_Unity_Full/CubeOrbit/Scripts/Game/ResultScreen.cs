using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CubeOrbit
{
    /// <summary>
    /// ゲームクリア画面（豪華版）。Result シーンの空オブジェクトに付けるだけで動く。
    /// カメラ・星・花火・紙吹雪・回るキューブ・ファンファーレは全部このスクリプトが自動で作る。
    /// Build Settings の一番最後に置くこと（自分より前のシーンをステージとして集計する）。
    /// </summary>
    public class ResultScreen : MonoBehaviour
    {
        [Header("文字")]
        public string title = "GAME CLEAR!";
        public string message = "すべてのステージをクリアしました！";
        public string thanks = "THANK YOU FOR PLAYING";
        [Range(0.015f, 0.06f)] public float fontScale = 0.028f;
        public float titleScale = 3.4f;
        [Tooltip("タイトル文字の虹色が流れる速さ")] public float hueSpeed = 0.1f;
        public Color textColor = new Color(0.93f, 0.95f, 0.98f);
        public Color dimColor = new Color(0.6f, 0.63f, 0.72f);
        public Color panelColor = new Color(0.02f, 0.025f, 0.06f, 0.82f);

        [Header("ランク")]
        [Tooltip("1ステージあたりの目標タイム（秒）。ベストタイム合計がこれ×ステージ数以内ならSを狙える")]
        public float targetSecondsPerStage = 40f;

        [Header("演出")]
        [Tooltip("カメラを自動で設定する（黒背景・正面から）")]
        public bool autoSetupCamera = true;
        [Tooltip("花火が上がる間隔（秒）の最小と最大")]
        public Vector2 fireworkInterval = new Vector2(0.35f, 0.9f);
        [Tooltip("花火1発の火花の数")] public int fireworkSparks = 60;
        [Tooltip("1秒あたりの紙吹雪の数")] public float confettiPerSecond = 25f;
        [Tooltip("中央のキューブのまわりを回る小さいキューブの数（1つの輪あたり）")] public int ringCubes = 18;
        [Tooltip("同時に出せる破片の最大数")] public int maxBits = 900;
        [Range(0f, 1f)] public float volume = 0.6f;

        [Header("移動先")]
        public string titleSceneName = "Title";
        [Tooltip("最初のステージの Build Settings 番号")]
        public int firstStageBuildIndex = 1;

        // ---------- 記録 ----------
        class StageRecord { public bool cleared; public float time; public int gems, score; }
        readonly List<StageRecord> records = new List<StageRecord>();
        int stages, cleared, gems, totalScore;
        float totalTime;
        string rank = "S";

        // ---------- タイムライン（秒） ----------
        const float LetterStart = 0.2f, LetterGap = 0.09f, LetterDur = 0.6f;
        const float MessageAt = 1.4f, PanelAt = 1.9f, StatAt = 2.2f, StatGap = 0.75f, StatDur = 0.6f;
        const float StampAt = 5.35f, ButtonsAt = 6.05f, ThanksAt = 6.55f;
        const int StatCount = 4;
        float t;
        bool stampPlayed, fanfarePlayed;
        int lastTickValue = -1;
        float lastTickTime;
        bool showList;
        Vector2 listScroll;

        // ---------- 3D ----------
        class Bit
        {
            public Transform tr; public Renderer r;
            public Vector3 vel, axis, shape;
            public float life, max, size, drag, gravity, spin, hue;
            public int kind; // 0=火花 1=ロケット 2=紙吹雪
        }
        readonly List<Bit> bits = new List<Bit>();
        readonly Stack<Bit> pool = new Stack<Bit>();
        Transform fxRoot, bigCube;
        readonly List<Transform> rings = new List<Transform>();
        readonly List<Renderer> ringRenderers = new List<Renderer>();
        Renderer bigCubeRenderer;
        Material glowMat, shadedMat;
        MaterialPropertyBlock mpb;
        float nextFirework, confettiAcc;

        // ---------- 音 ----------
        AudioSource audioSrc;
        AudioClip sFanfare, sTick, sStamp, sPop, sLaunch;

        // ---------- GUI ----------
        GUIStyle label, button, titleStyle, bigStyle, smallStyle;
        Texture2D panelTex;
        int cachedSize = -1;

        // ================= 初期化 =================

        void Start()
        {
            Time.timeScale = 1f;
            mpb = new MaterialPropertyBlock();
            CollectRecords();
            SetupMaterials();
            if (autoSetupCamera) SetupCamera();
            BuildDecor();
            BuildSounds();
            nextFirework = 0.8f;
        }

        void CollectRecords()
        {
            int end = SceneManager.GetActiveScene().buildIndex; // 自分より前がステージ
            for (int i = firstStageBuildIndex; i < end; i++)
            {
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
                var rec = new StageRecord();
                float bt; int bg;
                if (GameManager.TryGetBestFor(sceneName, out bt, out bg))
                {
                    rec.cleared = true; rec.time = bt; rec.gems = bg; rec.score = GameManager.GetBestScore(sceneName);
                    cleared++; totalTime += bt; gems += bg;
                }
                records.Add(rec);
            }
            stages = records.Count;
            totalScore = GameManager.TotalBestScore(firstStageBuildIndex, end);

            float ratio = stages > 0 ? (float)cleared / stages : 1f;
            float target = targetSecondsPerStage * Mathf.Max(1, cleared);
            float timeScore = totalTime > 0f ? Mathf.Clamp01(target / totalTime) : 0f;
            float score = ratio * 70f + timeScore * 30f;
            rank = score >= 95f ? "S" : score >= 80f ? "A" : score >= 60f ? "B" : "C";
        }

        void SetupMaterials()
        {
            Shader sh = Shader.Find("CubeOrbit/Block");
            if (sh == null) sh = Shader.Find("Unlit/Color");
            glowMat = new Material(sh);
            glowMat.SetFloat("_Emission", 1f);
            glowMat.SetFloat("_EdgeDarken", 0f);
            glowMat.SetFloat("_ObjectSpace", 1f);
            glowMat.SetFloat("_FogY", -1000f);
            shadedMat = new Material(sh);
            shadedMat.SetFloat("_Emission", 0.7f);
            shadedMat.SetFloat("_EdgeDarken", 0.12f);
            shadedMat.SetFloat("_ObjectSpace", 1f);
            shadedMat.SetFloat("_FogY", -1000f);
        }

        void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0f, 0f, 0.015f);
            cam.orthographic = true;
            cam.orthographicSize = 7f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 300f;
            cam.transform.position = new Vector3(0f, 1f, -30f);
            cam.transform.rotation = Quaternion.identity;

            if (cam.GetComponent<StarField>() == null)
            {
                var stars = cam.gameObject.AddComponent<StarField>();
                var starMat = new Material(glowMat);
                starMat.SetColor("_Color", new Color(0.85f, 0.9f, 1f));
                stars.starMaterial = starMat;
                stars.count = 260;
            }
        }

        void BuildDecor()
        {
            fxRoot = new GameObject("ResultFX").transform;

            var big = MakeCube("BigCube", fxRoot, shadedMat);
            bigCube = big.transform;
            bigCube.position = new Vector3(0f, 1.8f, 0f);
            bigCube.localScale = Vector3.one * 1.8f;
            bigCubeRenderer = big.GetComponent<Renderer>();

            Vector3[] tilts = { new Vector3(18f, 0f, 0f), new Vector3(-35f, 0f, 40f), new Vector3(62f, 0f, -28f) };
            float[] radii = { 2.6f, 3.2f, 3.8f };
            for (int k = 0; k < tilts.Length; k++)
            {
                var pivot = new GameObject("Ring" + k).transform;
                pivot.SetParent(fxRoot, false);
                pivot.position = bigCube.position;
                pivot.rotation = Quaternion.Euler(tilts[k]);
                rings.Add(pivot);
                for (int i = 0; i < ringCubes; i++)
                {
                    float a = i * Mathf.PI * 2f / ringCubes;
                    var c = MakeCube("RingCube", pivot, glowMat);
                    c.transform.localPosition = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * radii[k];
                    c.transform.localScale = Vector3.one * (0.22f + 0.08f * ((i + k) % 3));
                    ringRenderers.Add(c.GetComponent<Renderer>());
                }
            }
        }

        GameObject MakeCube(string name, Transform parent, Material m)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
            go.name = name;
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<Renderer>();
            r.sharedMaterial = m;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            return go;
        }

        // ================= 毎フレーム =================

        void Update()
        {
            float dt = Time.deltaTime;
            float prevT = t;
            t += dt;

            // 決定キー / クリックで演出を飛ばす → ボタン表示後はタイトルへ
            Vector2 p;
            bool pressed = InputCompat.Down(GameKey.Confirm) || (InputCompat.PointerDown(out p) && t < ButtonsAt);
            if (pressed)
            {
                if (t < ButtonsAt) t = ButtonsAt;
                else if (!showList && InputCompat.Down(GameKey.Confirm)) GoTitle();
            }
            if (showList && InputCompat.Down(GameKey.Pause)) showList = false;

            if (!fanfarePlayed && t >= 0.1f) { fanfarePlayed = true; Play(sFanfare, 1f, 1f); }
            if (!stampPlayed && t >= StampAt)
            {
                stampPlayed = true;
                Play(sStamp, 1f, 1f);
                for (int i = 0; i < 4; i++) Explode(new Vector3(Random.Range(-9f, 9f), Random.Range(2.5f, 7f), 6f), Random.value, (int)(fireworkSparks * 1.3f));
            }
            if (prevT < StampAt && t >= StampAt) nextFirework = 0.25f;

            UpdateTicks();
            UpdateDecor(dt);
            UpdateFireworks(dt);
            UpdateBits(dt);
        }

        void UpdateTicks()
        {
            int v = 0;
            for (int k = 0; k < StatCount; k++) v += Mathf.FloorToInt(StatProgress(k) * 40f);
            if (v != lastTickValue && Time.time - lastTickTime > 0.045f && t < StampAt)
            {
                if (lastTickValue >= 0 && v > lastTickValue) Play(sTick, 0.5f, 1f + v * 0.004f);
                lastTickValue = v;
                lastTickTime = Time.time;
            }
        }

        void UpdateDecor(float dt)
        {
            float time = Time.time;
            if (bigCube != null)
            {
                float intro = GridUtil.EaseInOut(Mathf.Clamp01(t / 1.2f));
                float pulse = 1f + (stampPlayed ? Mathf.Exp(-(t - StampAt) * 4f) * 0.35f : 0f);
                bigCube.localScale = Vector3.one * 1.8f * intro * pulse;
                bigCube.rotation = Quaternion.Euler(time * 23f, time * 41f, time * 13f);
                SetColor(bigCubeRenderer, Color.HSVToRGB(Mathf.Repeat(time * 0.08f, 1f), 0.65f, 1f));
            }
            for (int k = 0; k < rings.Count; k++)
            {
                rings[k].Rotate(Vector3.up, (25f + k * 12f) * (k % 2 == 0 ? 1f : -1f) * dt, Space.Self);
                float s = GridUtil.EaseInOut(Mathf.Clamp01((t - 0.3f - k * 0.2f) / 1f));
                rings[k].localScale = Vector3.one * s;
            }
            for (int i = 0; i < ringRenderers.Count; i++)
            {
                var r = ringRenderers[i];
                r.transform.Rotate(new Vector3(60f, 90f, 30f) * dt, Space.Self);
                SetColor(r, Color.HSVToRGB(Mathf.Repeat(time * 0.15f + i / (float)ringCubes, 1f), 0.7f, 1f));
            }
        }

        void UpdateFireworks(float dt)
        {
            if (t < 0.6f) return;
            nextFirework -= dt;
            if (nextFirework <= 0f)
            {
                float busy = stampPlayed ? 0.6f : 1f;
                nextFirework = Random.Range(fireworkInterval.x, fireworkInterval.y) * busy;
                LaunchRocket();
            }
            if (t > PanelAt)
            {
                confettiAcc += confettiPerSecond * dt;
                while (confettiAcc >= 1f) { confettiAcc -= 1f; SpawnConfetti(); }
            }
        }

        // ================= 花火・紙吹雪 =================

        Bit Take()
        {
            if (bits.Count >= maxBits) return null;
            Bit b;
            if (pool.Count > 0) b = pool.Pop();
            else
            {
                var go = MakeCube("Bit", fxRoot, glowMat);
                b = new Bit { tr = go.transform, r = go.GetComponent<Renderer>() };
            }
            b.tr.gameObject.SetActive(true);
            bits.Add(b);
            return b;
        }

        void LaunchRocket()
        {
            var b = Take();
            if (b == null) return;
            b.kind = 1;
            b.hue = Random.value;
            b.tr.position = new Vector3(Random.Range(-10f, 10f), -7.5f, 6f);
            b.vel = new Vector3(Random.Range(-1.2f, 1.2f), Random.Range(10f, 13f), 0f);
            b.gravity = 6f; b.drag = 0f;
            b.max = b.life = 5f;
            b.size = 0.16f; b.shape = Vector3.one;
            b.axis = Random.onUnitSphere; b.spin = 400f;
            SetColor(b.r, Color.HSVToRGB(b.hue, 0.3f, 1f));
            Play(sLaunch, 0.35f, Random.Range(0.9f, 1.15f));
        }

        void Explode(Vector3 pos, float hue, int count)
        {
            Play(sPop, 0.9f, Random.Range(0.85f, 1.15f));
            float hue2 = Mathf.Repeat(hue + Random.Range(0.15f, 0.4f), 1f);
            float speed = Random.Range(5f, 7.5f);
            for (int i = 0; i < count; i++)
            {
                var b = Take();
                if (b == null) return;
                b.kind = 0;
                b.tr.position = pos;
                b.vel = Random.onUnitSphere * speed * Random.Range(0.75f, 1f);
                b.gravity = 2.5f; b.drag = 1.6f;
                b.max = b.life = Random.Range(1.1f, 1.8f);
                b.size = Random.Range(0.1f, 0.2f); b.shape = Vector3.one;
                b.axis = Random.onUnitSphere; b.spin = Random.Range(180f, 540f);
                SetColor(b.r, Color.HSVToRGB(i % 2 == 0 ? hue : hue2, 0.75f, 1f));
            }
        }

        void SpawnConfetti()
        {
            var b = Take();
            if (b == null) return;
            b.kind = 2;
            b.tr.position = new Vector3(Random.Range(-13f, 13f), 8.5f, Random.Range(-2f, 4f));
            b.vel = new Vector3(Random.Range(-0.6f, 0.6f), Random.Range(-2.2f, -1.2f), 0f);
            b.gravity = 0f; b.drag = 0f;
            b.max = b.life = 9f;
            b.size = 1f; b.shape = new Vector3(0.22f, 0.02f, 0.13f);
            b.axis = Random.onUnitSphere; b.spin = Random.Range(120f, 360f);
            SetColor(b.r, Color.HSVToRGB(Random.value, 0.7f, 1f));
        }

        void UpdateBits(float dt)
        {
            for (int i = bits.Count - 1; i >= 0; i--)
            {
                var b = bits[i];
                b.life -= dt;
                b.vel.y -= b.gravity * dt;
                if (b.drag > 0f) b.vel *= Mathf.Exp(-b.drag * dt);
                b.tr.position += b.vel * dt;
                b.tr.rotation = Quaternion.AngleAxis(Time.time * b.spin, b.axis);

                if (b.kind == 1)
                {
                    // 尾を引きながら上がり、勢いがなくなったら破裂
                    var trail = Take();
                    if (trail != null)
                    {
                        trail.kind = 0;
                        trail.tr.position = b.tr.position;
                        trail.vel = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-1f, 0f), 0f);
                        trail.gravity = 0f; trail.drag = 3f;
                        trail.max = trail.life = 0.35f;
                        trail.size = 0.09f; trail.shape = Vector3.one;
                        trail.axis = Vector3.up; trail.spin = 0f;
                        SetColor(trail.r, Color.HSVToRGB(b.hue, 0.25f, 1f));
                    }
                    b.tr.localScale = Vector3.one * b.size;
                    if (b.vel.y < 1.5f || b.life <= 0f)
                    {
                        Explode(b.tr.position, b.hue, fireworkSparks);
                        Release(i);
                    }
                    continue;
                }

                if (b.life <= 0f || b.tr.position.y < -9f) { Release(i); continue; }
                float k = Mathf.Clamp01(b.life / b.max);
                float s = b.kind == 2 ? Mathf.Clamp01(b.life / 0.5f) : Mathf.Sqrt(k);
                b.tr.localScale = b.shape * b.size * s;
            }
        }

        void Release(int index)
        {
            var b = bits[index];
            b.tr.gameObject.SetActive(false);
            bits.RemoveAt(index);
            pool.Push(b);
        }

        void SetColor(Renderer r, Color c)
        {
            if (r == null) return;
            r.GetPropertyBlock(mpb);
            mpb.SetColor("_Color", c);
            r.SetPropertyBlock(mpb);
        }

        // ================= 画面表示 =================

        float StatProgress(int k) { return Mathf.Clamp01((t - (StatAt + k * StatGap)) / StatDur); }

        void EnsureStyles()
        {
            int fs = Mathf.RoundToInt(Screen.height * fontScale);
            if (fs == cachedSize && label != null) return;
            cachedSize = fs;
            label = new GUIStyle(GUI.skin.label) { fontSize = fs, alignment = TextAnchor.MiddleCenter };
            label.normal.textColor = Color.white;
            smallStyle = new GUIStyle(label) { fontSize = Mathf.RoundToInt(fs * 0.75f) };
            titleStyle = new GUIStyle(label) { fontSize = Mathf.RoundToInt(fs * titleScale), fontStyle = FontStyle.Bold };
            bigStyle = new GUIStyle(label) { fontSize = fs * 5, fontStyle = FontStyle.Bold };
            button = new GUIStyle(GUI.skin.button) { fontSize = fs };
            if (panelTex == null)
            {
                panelTex = new Texture2D(1, 1);
                panelTex.SetPixel(0, 0, Color.white);
                panelTex.Apply();
            }
        }

        void OnGUI()
        {
            EnsureStyles();
            int fs = label.fontSize;
            Color baseCol = GUI.color;

            // 判子のタイミングで画面を揺らす
            float shake = stampPlayed ? Mathf.Max(0f, 0.3f - (t - StampAt)) * fs * 1.2f : 0f;
            Vector2 sh = new Vector2(Random.Range(-shake, shake), Random.Range(-shake, shake));

            DrawTitle(fs, sh);

            // メッセージ
            float ma = Mathf.Clamp01((t - MessageAt) / 0.5f);
            DrawText(new Rect(sh.x, Screen.height * 0.255f + sh.y, Screen.width, fs * 1.6f), message, label, WithAlpha(dimColor, ma));

            // パネル
            float pa = Mathf.Clamp01((t - PanelAt) / 0.4f);
            if (pa > 0f) DrawPanel(fs, pa, sh);

            // ボタン
            float ba = Mathf.Clamp01((t - ButtonsAt) / 0.4f);
            if (ba > 0f && !showList)
            {
                GUI.color = new Color(1f, 1f, 1f, ba);
                float bw = fs * 10f, bh = fs * 2.1f, gap = fs * 0.6f;
                float totalW = bw * 3f + gap * 2f;
                float bx = (Screen.width - totalW) / 2f, by = Screen.height * 0.82f;
                if (GUI.Button(new Rect(bx, by, bw, bh), "タイトルへ", button) && ba >= 1f) GoTitle();
                if (GUI.Button(new Rect(bx + bw + gap, by, bw, bh), "最初から遊ぶ", button) && ba >= 1f) SceneManager.LoadScene(firstStageBuildIndex);
                if (GUI.Button(new Rect(bx + (bw + gap) * 2f, by, bw, bh), "ステージ別の記録", button) && ba >= 1f) showList = true;
                GUI.color = baseCol;
            }

            // THANK YOU
            float ta = Mathf.Clamp01((t - ThanksAt) / 0.8f) * (0.65f + 0.35f * Mathf.Sin(Time.time * 2f));
            DrawText(new Rect(0, Screen.height - fs * 2.2f, Screen.width, fs * 1.4f), thanks, smallStyle, WithAlpha(dimColor, ta));

            if (showList) DrawList(fs);
            GUI.color = baseCol;
        }

        void DrawTitle(int fs, Vector2 sh)
        {
            float cy = Screen.height * 0.15f;
            float total = 0f;
            var sizes = new Vector2[title.Length];
            for (int i = 0; i < title.Length; i++) { sizes[i] = titleStyle.CalcSize(new GUIContent(title[i].ToString())); total += sizes[i].x; }
            float x = (Screen.width - total) / 2f;
            for (int i = 0; i < title.Length; i++)
            {
                float lt = t - (LetterStart + i * LetterGap);
                if (lt > 0f)
                {
                    float k = Mathf.Clamp01(lt / LetterDur);
                    float yOff = (1f - EaseOutBack(k)) * -fs * 6f;
                    float wave = Mathf.Sin(Time.time * 3f + i * 0.5f) * fs * 0.15f * Mathf.Clamp01(lt - LetterDur);
                    Color c = Color.HSVToRGB(Mathf.Repeat(Time.time * hueSpeed + i * 0.07f, 1f), 0.55f, 1f);
                    var r = new Rect(x + sh.x, cy - sizes[i].y / 2f + yOff + wave + sh.y, sizes[i].x, sizes[i].y);
                    DrawText(r, title[i].ToString(), titleStyle, WithAlpha(c, Mathf.Clamp01(k * 2f)));
                }
                x += sizes[i].x;
            }
        }

        void DrawPanel(int fs, float pa, Vector2 sh)
        {
            float w = fs * 24f, h = fs * 13.1f;
            float slide = (1f - GridUtil.EaseInOut(pa)) * fs * 2f;
            var r = new Rect((Screen.width - w) / 2f + sh.x, Screen.height * 0.4f + slide + sh.y, w, h);
            Color pc = panelColor; pc.a *= pa;
            GUI.color = pc;
            GUI.DrawTexture(r, panelTex);
            GUI.color = Color.white;

            // 左：数字のカウントアップ
            float lx = r.x + fs * 1.2f, lw = w * 0.58f, ly = r.y + fs * 1.2f, lh = fs * 2.6f;
            var left = new GUIStyle(label) { alignment = TextAnchor.MiddleLeft };
            var right = new GUIStyle(label) { alignment = TextAnchor.MiddleRight, fontSize = Mathf.RoundToInt(fs * 1.25f), fontStyle = FontStyle.Bold };
            string[] names = { "クリアしたステージ", "集めたキューブ", "ベストタイム合計", "合計スコア" };
            string[] values =
            {
                Mathf.FloorToInt(cleared * StatProgress(0)) + " / " + stages,
                "◆ " + Mathf.FloorToInt(gems * StatProgress(1)),
                StageHUD.FormatTime(totalTime * StatProgress(2)),
                StageHUD.FormatScore(totalScore * StatProgress(3)),
            };
            for (int k = 0; k < StatCount; k++)
            {
                float a = Mathf.Clamp01((t - (StatAt + k * StatGap)) / 0.25f) * pa;
                var row = new Rect(lx, ly + lh * k, lw, lh);
                DrawText(row, names[k], left, WithAlpha(dimColor, a));
                DrawText(row, values[k], right, WithAlpha(k == 3 ? new Color(1f, 0.85f, 0.3f) : textColor, a));
            }

            // 右：総合ランクの判子
            var rr = new Rect(r.x + w * 0.66f, r.y, w * 0.32f, h);
            DrawText(new Rect(rr.x, rr.y + fs * 0.8f, rr.width, fs * 1.4f), "TOTAL RANK", smallStyle, WithAlpha(dimColor, pa));
            if (t >= StampAt)
            {
                float st = t - StampAt;
                float scale = st < 0.22f ? Mathf.Lerp(3.2f, 1f, GridUtil.EaseInOut(st / 0.22f)) : 1f + Mathf.Sin(st * 18f) * Mathf.Exp(-st * 6f) * 0.08f;
                float alpha = Mathf.Clamp01(st / 0.12f);
                Vector2 pivot = new Vector2(rr.center.x, rr.center.y + fs * 0.6f);
                Matrix4x4 m = GUI.matrix;
                GUIUtility.ScaleAroundPivot(Vector2.one * scale, pivot);
                GUIUtility.RotateAroundPivot(-12f, pivot);
                var stampRect = new Rect(pivot.x - fs * 4f, pivot.y - fs * 3.2f, fs * 8f, fs * 6.4f);
                DrawText(stampRect, rank, bigStyle, WithAlpha(RankColor(rank), alpha));
                GUI.matrix = m;
            }
        }

        void DrawList(int fs)
        {
            float w = fs * 26f, h = Screen.height * 0.75f;
            var r = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
            GUI.color = new Color(panelColor.r, panelColor.g, panelColor.b, 0.95f);
            GUI.DrawTexture(r, panelTex);
            GUI.color = Color.white;
            DrawText(new Rect(r.x, r.y + fs * 0.6f, w, fs * 2f), "ステージ別の記録", label, textColor);

            float rowH = fs * 1.8f;
            var view = new Rect(r.x + fs, r.y + fs * 3f, w - fs * 2f, h - fs * 6.5f);
            var content = new Rect(0, 0, view.width - 20f, rowH * records.Count);
            listScroll = GUI.BeginScrollView(view, listScroll, content);
            var left = new GUIStyle(label) { alignment = TextAnchor.MiddleLeft };
            var right = new GUIStyle(label) { alignment = TextAnchor.MiddleRight };
            for (int i = 0; i < records.Count; i++)
            {
                var row = new Rect(0, rowH * i, content.width, rowH);
                var rec = records[i];
                DrawText(row, "STAGE " + (i + 1), left, dimColor);
                if (rec.cleared) DrawText(new Rect(row.x + row.width * 0.28f, row.y, row.width * 0.3f, row.height), StageHUD.FormatScore(rec.score), left, new Color(1f, 0.85f, 0.3f));
                DrawText(row, rec.cleared ? "◆ " + rec.gems + "    " + StageHUD.FormatTime(rec.time) : "未クリア", right, rec.cleared ? textColor : dimColor);
            }
            GUI.EndScrollView();

            float bw = fs * 8f, bh = fs * 2f;
            if (GUI.Button(new Rect(r.center.x - bw / 2f, r.yMax - bh - fs * 0.8f, bw, bh), "とじる", button)) showList = false;
        }

        /// <summary>影付きで文字を描く</summary>
        void DrawText(Rect r, string text, GUIStyle style, Color color)
        {
            if (color.a <= 0.001f) return;
            Color prev = GUI.color;
            float off = Mathf.Max(1f, style.fontSize * 0.06f);
            GUI.color = new Color(0f, 0f, 0f, color.a * 0.6f);
            GUI.Label(new Rect(r.x + off, r.y + off, r.width, r.height), text, style);
            GUI.color = color;
            GUI.Label(r, text, style);
            GUI.color = prev;
        }

        static Color WithAlpha(Color c, float a) { c.a *= a; return c; }

        static float EaseOutBack(float k)
        {
            const float c1 = 1.70158f, c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(k - 1f, 3f) + c1 * Mathf.Pow(k - 1f, 2f);
        }

        static Color RankColor(string r)
        {
            switch (r)
            {
                case "S": return new Color(1f, 0.85f, 0.3f);
                case "A": return new Color(0.5f, 0.96f, 0.78f);
                case "B": return new Color(0.44f, 0.72f, 1f);
                default: return new Color(0.79f, 0.8f, 0.88f);
            }
        }

        void GoTitle()
        {
            if (Application.CanStreamedLevelBeLoaded(titleSceneName)) SceneManager.LoadScene(titleSceneName);
            else SceneManager.LoadScene(0);
        }

        // ================= 音（自動生成） =================

        void Play(AudioClip c, float vol, float pitch)
        {
            if (c == null || audioSrc == null || SoundPlayer.Muted) return;
            audioSrc.pitch = pitch;
            audioSrc.PlayOneShot(c, vol * volume);
        }

        void BuildSounds()
        {
            audioSrc = gameObject.AddComponent<AudioSource>();
            audioSrc.playOnAwake = false;
            const int R = 44100;

            // ファンファーレ：ド ミ ソ ド〜 ラ シ ド〜（和音）
            float[] notes = { 523.3f, 659.3f, 784f, 1046.5f, 0f, 880f, 987.8f, 1046.5f };
            float[] durs = { 0.12f, 0.12f, 0.12f, 0.36f, 0.06f, 0.12f, 0.12f, 0.9f };
            float len = 0f; foreach (var d in durs) len += d;
            var fan = new float[Mathf.CeilToInt((len + 0.6f) * R)];
            float at = 0f;
            for (int n = 0; n < notes.Length; n++)
            {
                if (notes[n] > 0f)
                {
                    bool last = n == notes.Length - 1;
                    AddTone(fan, R, at, durs[n] + (last ? 0.5f : 0.05f), notes[n], 0.22f);
                    if (last) { AddTone(fan, R, at, durs[n] + 0.5f, notes[n] * 1.26f, 0.14f); AddTone(fan, R, at, durs[n] + 0.5f, notes[n] * 1.5f, 0.14f); }
                }
                at += durs[n];
            }
            sFanfare = Clip("fanfare", fan, R);

            var tick = new float[(int)(0.035f * R)];
            AddTone(tick, R, 0f, 0.035f, 1400f, 0.25f);
            sTick = Clip("tick", tick, R);

            var stamp = new float[(int)(0.7f * R)];
            AddThump(stamp, R, 0f, 0.35f, 70f, 0.8f);
            AddNoise(stamp, R, 0f, 0.15f, 0.5f);
            AddTone(stamp, R, 0.05f, 0.6f, 784f, 0.12f); AddTone(stamp, R, 0.05f, 0.6f, 1046.5f, 0.12f); AddTone(stamp, R, 0.05f, 0.6f, 1318.5f, 0.12f);
            sStamp = Clip("stamp", stamp, R);

            var pop = new float[(int)(0.6f * R)];
            AddThump(pop, R, 0f, 0.25f, 90f, 0.6f);
            AddNoise(pop, R, 0f, 0.55f, 0.45f);
            sPop = Clip("pop", pop, R);

            var launch = new float[(int)(0.55f * R)];
            float ph = 0f;
            for (int i = 0; i < launch.Length; i++)
            {
                float tt = (float)i / R;
                ph += Mathf.Lerp(700f, 1500f, tt / 0.55f) / R;
                launch[i] = Mathf.Sin(ph * Mathf.PI * 2f) * 0.08f * Mathf.Sin(Mathf.PI * tt / 0.55f);
            }
            sLaunch = Clip("launch", launch, R);
        }

        static void AddTone(float[] buf, int R, float start, float dur, float freq, float vol)
        {
            int s = (int)(start * R), n = (int)(dur * R);
            for (int i = 0; i < n && s + i < buf.Length; i++)
            {
                float tt = (float)i / R;
                float env = Mathf.Clamp01(tt / 0.008f) * Mathf.Exp(-3f * tt / dur);
                float ph = freq * tt;
                float sq = (ph - Mathf.Floor(ph)) < 0.5f ? 1f : -1f;
                float tri = 1f - 4f * Mathf.Abs((ph - Mathf.Floor(ph)) - 0.5f);
                buf[s + i] += (sq * 0.35f + tri * 0.65f) * env * vol;
            }
        }

        static void AddThump(float[] buf, int R, float start, float dur, float freq, float vol)
        {
            int s = (int)(start * R), n = (int)(dur * R);
            float ph = 0f;
            for (int i = 0; i < n && s + i < buf.Length; i++)
            {
                float tt = (float)i / R;
                ph += freq * Mathf.Lerp(1.6f, 0.6f, tt / dur) / R;
                buf[s + i] += Mathf.Sin(ph * Mathf.PI * 2f) * Mathf.Exp(-6f * tt / dur) * vol;
            }
        }

        static void AddNoise(float[] buf, int R, float start, float dur, float vol)
        {
            int s = (int)(start * R), n = (int)(dur * R);
            var rng = new System.Random(n);
            float lp = 0f;
            for (int i = 0; i < n && s + i < buf.Length; i++)
            {
                float tt = (float)i / R;
                lp += ((float)(rng.NextDouble() * 2.0 - 1.0) - lp) * 0.3f;
                buf[s + i] += lp * Mathf.Exp(-5f * tt / dur) * vol;
            }
        }

        static AudioClip Clip(string name, float[] data, int R)
        {
            float peak = 0f;
            foreach (var v in data) peak = Mathf.Max(peak, Mathf.Abs(v));
            if (peak > 0.95f) for (int i = 0; i < data.Length; i++) data[i] *= 0.95f / peak;
            var c = AudioClip.Create("result_" + name, data.Length, 1, R, false);
            c.SetData(data, 0);
            return c;
        }
    }
}
