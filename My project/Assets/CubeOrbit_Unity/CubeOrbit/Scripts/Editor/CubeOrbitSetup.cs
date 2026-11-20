using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CubeOrbit.EditorTools
{
    /// <summary>
    /// 初期セットアップ：マテリアルとプレハブを自動で作る。
    /// メニュー「Tools > Cube Orbit > 1. 初期セットアップ」から実行。何度実行してもOK（上書き更新）。
    /// </summary>
    public static class CubeOrbitSetup
    {
        /// <summary>CubeOrbit フォルダの場所（シェーダーの場所から自動で探す）</summary>
        public static string Root
        {
            get
            {
                var guids = AssetDatabase.FindAssets("CubeOrbitBlock t:Shader");
                if (guids.Length == 0) return "Assets/CubeOrbit";
                string shaderPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                return Path.GetDirectoryName(Path.GetDirectoryName(shaderPath)).Replace('\\', '/');
            }
        }
        public static string PrefabDir { get { return Root + "/Prefabs"; } }
        public static string MaterialDir { get { return Root + "/Materials"; } }
        public static string StageDir { get { return Root + "/Stages"; } }

        public static bool IsSetUp { get { return LoadPrefab("Player") != null; } }

        public static GameObject LoadPrefab(string name)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/" + name + ".prefab");
        }

        public static Material LoadMaterial(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Material>(MaterialDir + "/" + name + ".mat");
        }

        [MenuItem("Tools/Cube Orbit/1. 初期セットアップ（マテリアルとプレハブを作る）", false, 1)]
        public static void Run()
        {
            Shader block = Shader.Find("CubeOrbit/Block");
            Shader ghost = Shader.Find("CubeOrbit/Ghost");
            if (block == null || ghost == null)
            {
                EditorUtility.DisplayDialog("Cube Orbit", "シェーダー CubeOrbit/Block と CubeOrbit/Ghost が見つかりません。\nShaders フォルダが入っているか確認してください。", "OK");
                return;
            }
            EnsureFolder(MaterialDir);
            EnsureFolder(PrefabDir);
            EnsureFolder(StageDir);

            // ---------- マテリアル ----------
            var mBlock = Mat("Block", block, new Color(0.54f, 0.56f, 0.64f), 0f, 0.07f, false);
            var mSolid = Mat("GhostSolid", block, new Color(0.58f, 0.6f, 0.68f), 0f, 0.07f, false);
            var mPlayer = Mat("Player", block, new Color(1f, 0.8f, 0.3f), 0.55f, 0.08f, true);
            var mPlat = Mat("Platform", block, new Color(0.52f, 0.62f, 0.86f), 0f, 0.1f, true);
            var mPlatLight = Mat("PlatformLight", block, new Color(0.35f, 0.85f, 1f), 1f, 0f, true);
            var mPusher = Mat("Pusher", block, new Color(0.95f, 0.5f, 0.32f), 0.25f, 0.18f, true);
            var mStripe = Mat("PusherStripe", block, new Color(0.15f, 0.12f, 0.12f), 1f, 0f, true);
            var mCrumble = Mat("Crumble", block, new Color(0.64f, 0.56f, 0.52f), 0f, 0.32f, true);
            var mCrack = Mat("Crack", block, new Color(0.25f, 0.2f, 0.2f), 1f, 0f, true);
            var mSwitch = Mat("Switch", block, new Color(0.95f, 0.96f, 1f), 0.9f, 0.05f, true);
            var mGem = Mat("Gem", block, new Color(1f, 0.85f, 0.3f), 0.8f, 0f, true, -50f);
            var mCp = Mat("Checkpoint", block, new Color(0.95f, 0.6f, 0.45f), 1f, 0f, true);
            var mGoal = Mat("Goal", block, new Color(0.5f, 1f, 0.6f), 1f, 0f, true);
            var mEffect = Mat("Effect", block, Color.white, 1f, 0f, true, -50f);
            var mStar = Mat("Star", block, new Color(0.85f, 0.9f, 1f), 1f, 0f, true, -500f);

            var mGhost = GhostMat("Ghost", ghost, Color.white, 0.08f, 0.85f, 0.25f, false);
            var mShadow = GhostMat("Shadow", ghost, new Color(0f, 0f, 0f, 1f), 0.42f, 0.42f, 0f, true);

            // ---------- プレハブ ----------
            {   // ブロック
                var root = NewRoot("Block", Vector3.zero, Vector3.one);
                var bv = root.AddComponent<BlockVolume>();
                bv.visual = Cube("Visual", root.transform, mBlock, Vector3.zero, Vector3.one).transform;
                Save(root, "Block");
            }
            {   // 透明ブロック
                var root = NewRoot("GhostBlock", Vector3.zero, Vector3.one);
                var gb = root.AddComponent<GhostBlockVolume>();
                gb.visual = Cube("Visual", root.transform, mGhost, Vector3.zero, Vector3.one).transform;
                gb.ghostMaterial = mGhost;
                gb.solidMaterial = mSolid;
                Save(root, "GhostBlock");
            }
            {   // スイッチ
                var root = NewRoot("Switch", new Vector3(0, -0.45f, 0), new Vector3(0.9f, 0.1f, 0.9f));
                var sw = root.AddComponent<FloorSwitch>();
                sw.plate = Cube("Plate", root.transform, mSwitch, new Vector3(0, -0.47f, 0), new Vector3(0.42f, 0.06f, 0.42f)).GetComponent<Renderer>();
                Save(root, "Switch");
            }
            {   // ジェム
                var root = NewRoot("Gem", new Vector3(0, -0.1f, 0), Vector3.one * 0.5f);
                var gem = root.AddComponent<Gem>();
                var v = Cube("Visual", root.transform, mGem, new Vector3(0, -0.12f, 0), Vector3.one * 0.24f);
                gem.visual = v.transform;
                gem.visualRenderer = v.GetComponent<Renderer>();
                Save(root, "Gem");
            }
            {   // チェックポイント
                var root = NewRoot("Checkpoint", new Vector3(0, -0.45f, 0), new Vector3(0.9f, 0.1f, 0.9f));
                var cp = root.AddComponent<Checkpoint>();
                Vector2[] offs = { new Vector2(-0.12f, 0.12f), new Vector2(0f, 0f), new Vector2(0.12f, -0.12f), new Vector2(0.12f, 0.12f) };
                cp.marks = new Renderer[offs.Length];
                for (int i = 0; i < offs.Length; i++)
                    cp.marks[i] = Cube("Mark" + i, root.transform, mCp, new Vector3(offs[i].x, -0.488f, offs[i].y), new Vector3(0.1f, 0.02f, 0.1f)).GetComponent<Renderer>();
                Save(root, "Checkpoint");
            }
            {   // ゴール
                var root = NewRoot("Goal", new Vector3(0, -0.45f, 0), new Vector3(1f, 0.1f, 1f));
                var g = root.AddComponent<Goal>();
                g.pad = Cube("Pad", root.transform, mGoal, new Vector3(0, -0.49f, 0), new Vector3(1f, 0.02f, 1f)).GetComponent<Renderer>();
                var inner = Cube("Inner", root.transform, mGoal, new Vector3(0, -0.475f, 0), new Vector3(0.42f, 0.02f, 0.42f));
                g.inner = inner.GetComponent<Renderer>();
                g.innerTransform = inner.transform;
                Save(root, "Goal");
            }
            {   // 動く床
                var root = NewRoot("MovingPlatform", Vector3.zero, Vector3.one);
                var mp = root.AddComponent<MovingPlatform>();
                Cube("Visual", root.transform, mPlat, Vector3.zero, Vector3.one * 0.98f);
                mp.topLight = Cube("TopLight", root.transform, mPlatLight, new Vector3(0, 0.495f, 0), new Vector3(0.56f, 0.01f, 0.56f)).GetComponent<Renderer>();
                Save(root, "MovingPlatform");
            }
            {   // エレベーター（中身は動く床。向きだけ縦）
                var root = NewRoot("Elevator", Vector3.zero, Vector3.one);
                var mp = root.AddComponent<MovingPlatform>();
                mp.moveOffset = new Vector3Int(0, 3, 0);
                Cube("Visual", root.transform, mPlat, Vector3.zero, Vector3.one * 0.98f);
                mp.topLight = Cube("TopLight", root.transform, mPlatLight, new Vector3(0, 0.495f, 0), new Vector3(0.56f, 0.01f, 0.56f)).GetComponent<Renderer>();
                Save(root, "Elevator");
            }
            {   // 押し出す壁
                var root = NewRoot("Pusher", Vector3.zero, Vector3.one);
                var p = root.AddComponent<Pusher>();
                var head = new GameObject("Head");
                head.transform.SetParent(root.transform, false);
                var body = Cube("Body", head.transform, mPusher, Vector3.zero, Vector3.one * 0.97f);
                Cube("Stripe1", head.transform, mStripe, new Vector3(0, 0.22f, 0.49f), new Vector3(0.8f, 0.1f, 0.03f));
                Cube("Stripe2", head.transform, mStripe, new Vector3(0, -0.22f, 0.49f), new Vector3(0.8f, 0.1f, 0.03f));
                p.head = head.transform;
                p.headRenderer = body.GetComponent<Renderer>();
                Save(root, "Pusher");
            }
            {   // 崩れる床
                var root = NewRoot("CrumbleTile", Vector3.zero, Vector3.one);
                var c = root.AddComponent<CrumbleTile>();
                var v = Cube("Visual", root.transform, mCrumble, Vector3.zero, Vector3.one * 0.97f);
                Cube("Crack1", v.transform, mCrack, new Vector3(-0.12f, 0.51f, 0.05f), new Vector3(0.37f, 0.01f, 0.04f));
                Cube("Crack2", v.transform, mCrack, new Vector3(0.12f, 0.51f, -0.14f), new Vector3(0.04f, 0.01f, 0.31f));
                Cube("Crack3", v.transform, mCrack, new Vector3(0.05f, 0.51f, 0.2f), new Vector3(0.26f, 0.01f, 0.04f));
                c.visual = v.transform;
                c.visualRenderer = v.GetComponent<Renderer>();
                Save(root, "CrumbleTile");
            }
            {   // プレイヤー
                var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
                root.name = "Player";
                root.GetComponent<Renderer>().sharedMaterial = mPlayer;
                NoShadows(root.GetComponent<Renderer>());
                root.AddComponent<PlayerCube>();
                var cc = root.AddComponent<CubeColorCycle>();
                cc.target = root.GetComponent<Renderer>();
                Save(root, "Player");
            }
            {   // 影
                var root = GameObject.CreatePrimitive(PrimitiveType.Quad);
                root.name = "PlayerShadow";
                Object.DestroyImmediate(root.GetComponent<Collider>());
                var r = root.GetComponent<Renderer>();
                r.sharedMaterial = mShadow;
                NoShadows(r);
                var bs = root.AddComponent<BlobShadow>();
                bs.shadowRenderer = r;
                Save(root, "PlayerShadow");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Cube Orbit] セットアップ完了: " + PrefabDir);
            EditorUtility.DisplayDialog("Cube Orbit", "セットアップ完了！\n\n次は「Tools > Cube Orbit > ステージエディタ」を開いて、\n新しいステージを作るか、サンプルステージを生成してください。", "OK");
        }

        // ================= ヘルパー =================

        public static void EnsureFolder(string path)
        {
            path = path.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        static Material Mat(string name, Shader shader, Color color, float emission, float edge, bool objectSpace, float fogY = -3.5f)
        {
            var m = GetOrCreateMaterial(name, shader);
            m.SetColor("_Color", color);
            m.SetFloat("_Emission", emission);
            m.SetFloat("_EdgeDarken", edge);
            m.SetFloat("_ObjectSpace", objectSpace ? 1f : 0f);
            m.SetFloat("_FogY", fogY);
            EditorUtility.SetDirty(m);
            return m;
        }

        static Material GhostMat(string name, Shader shader, Color color, float alpha, float edgeAlpha, float pulse, bool objectSpace)
        {
            var m = GetOrCreateMaterial(name, shader);
            m.SetColor("_Color", color);
            m.SetFloat("_Alpha", alpha);
            m.SetFloat("_EdgeAlpha", edgeAlpha);
            m.SetFloat("_Pulse", pulse);
            m.SetFloat("_ObjectSpace", objectSpace ? 1f : 0f);
            EditorUtility.SetDirty(m);
            return m;
        }

        static Material GetOrCreateMaterial(string name, Shader shader)
        {
            string path = MaterialDir + "/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(shader);
                AssetDatabase.CreateAsset(m, path);
            }
            else m.shader = shader;
            return m;
        }

        static GameObject NewRoot(string name, Vector3 colliderCenter, Vector3 colliderSize)
        {
            var go = new GameObject(name);
            var bc = go.AddComponent<BoxCollider>();
            bc.center = colliderCenter;
            bc.size = colliderSize;
            return go;
        }

        static GameObject Cube(string name, Transform parent, Material m, Vector3 localPos, Vector3 localScale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            var r = go.GetComponent<Renderer>();
            r.sharedMaterial = m;
            NoShadows(r);
            return go;
        }

        static void NoShadows(Renderer r)
        {
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
        }

        static void Save(GameObject root, string name)
        {
            PrefabUtility.SaveAsPrefabAsset(root, PrefabDir + "/" + name + ".prefab");
            Object.DestroyImmediate(root);
        }
    }
}
