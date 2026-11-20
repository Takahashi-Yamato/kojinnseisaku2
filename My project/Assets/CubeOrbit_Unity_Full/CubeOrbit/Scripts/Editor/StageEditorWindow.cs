using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CubeOrbit.EditorTools
{
    /// <summary>
    /// ステージエディタ。「Tools > Cube Orbit > ステージエディタ」で開く。
    /// ・パレットで置く物を選び、シーンビューをクリックすると配置（Shift+クリックで削除）
    /// ・ブロックの面をクリックすると、その面の隣のマスに置かれる（マイクラ風）
    /// ・「ステージを検証」でゴールとジェムに行けるかを自動チェック
    /// </summary>
    public class StageEditorWindow : EditorWindow
    {
        enum Item { Block, GhostBlock, Switch, Gem, Checkpoint, Goal, MovingPlatform, Elevator, Pusher, CrumbleTile, PlayerStart }

        static readonly string[] ItemLabels =
        {
            "ブロック", "透明ブロック", "スイッチ", "ジェム", "チェック\nポイント", "ゴール",
            "動く床", "エレベーター", "押し出す壁", "崩れる床", "開始位置",
        };

        Item item = Item.Block;
        bool placeMode = true;
        Vector3Int blockSize = Vector3Int.one;
        int extendDown = 0;
        int group = 1;
        Vector3Int platformOffset = new Vector3Int(0, 0, -3);
        int elevatorHeight = 3;
        float platformTimeOffset = 0f;
        GridDir pusherDir = GridDir.PlusX;
        float pusherTimeOffset = 0f;
        int emptyPlaneY = 0;

        string validateMessage = "";
        List<Object> problems = new List<Object>();
        Vector2 scroll;

        bool hasHover;
        Vector3Int hoverCell;
        GridElement hoverElement;

        [MenuItem("Tools/Cube Orbit/ステージエディタ", false, 10)]
        public static void Open()
        {
            var w = GetWindow<StageEditorWindow>("ステージエディタ");
            w.minSize = new Vector2(300, 400);
        }

        [MenuItem("Tools/Cube Orbit/2. 新しいステージを作る", false, 2)]
        static void MenuNewStage() { NewStage(); }

        [MenuItem("Tools/Cube Orbit/3. タイトル画面を作る", false, 3)]
        static void MenuTitle() { MakeTitle(); }

        [MenuItem("Tools/Cube Orbit/ステージを検証", false, 11)]
        static void MenuValidate()
        {
            string msg; List<Object> probs;
            StageFactory.ValidateOpenScene(out msg, out probs);
            EditorUtility.DisplayDialog("ステージ検証", msg, "OK");
            if (probs.Count > 0) Selection.objects = probs.ToArray();
        }

        void OnEnable() { SceneView.duringSceneGui += OnSceneGUI; }
        void OnDisable() { SceneView.duringSceneGui -= OnSceneGUI; }

        // ================= ウィンドウ =================

        void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);

            if (!CubeOrbitSetup.IsSetUp)
            {
                EditorGUILayout.HelpBox("まだ初期セットアップしていません。最初に1回だけ押してください。", MessageType.Warning);
                if (GUILayout.Button("初期セットアップ（マテリアルとプレハブを作る）", GUILayout.Height(32))) CubeOrbitSetup.Run();
                EditorGUILayout.EndScrollView();
                return;
            }

            // ステージ管理
            EditorGUILayout.LabelField("ステージ", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("新しいステージ", GUILayout.Height(26))) NewStage();
                if (GUILayout.Button("サンプル15ステージを生成", GUILayout.Height(26))) GenerateSamples();
            }
            if (GUILayout.Button("タイトル画面を作る（Build Settings の先頭に入る）", GUILayout.Height(24))) MakeTitle();
            bool hasManager = GridUtil.FindOne<GameManager>() != null;
            if (!hasManager)
                EditorGUILayout.HelpBox("このシーンには GameManager がありません。「新しいステージ」で作ったシーンを開いてください。", MessageType.Info);

            EditorGUILayout.Space(8);

            // パレット
            EditorGUILayout.LabelField("置く物", EditorStyles.boldLabel);
            item = (Item)GUILayout.SelectionGrid((int)item, ItemLabels, 3, GUILayout.Height(150));

            EditorGUILayout.Space(4);
            DrawItemOptions();

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("配置", EditorStyles.boldLabel);
            placeMode = EditorGUILayout.ToggleLeft("シーンビューのクリックで配置する", placeMode);
            emptyPlaneY = EditorGUILayout.IntField(new GUIContent("何もない所の高さ", "何もない場所をクリックした時に置く高さ（y）"), emptyPlaneY);
            EditorGUILayout.HelpBox(
                "クリック：配置（ブロックの面をクリックするとその隣に置く）\n" +
                "Shift+クリック：削除\n" +
                "Alt+ドラッグ：視点回転（通常のUnity操作）\n" +
                "置いた物はインスペクターで細かく調整できます", MessageType.None);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("チェック", EditorStyles.boldLabel);
            if (GUILayout.Button("ステージを検証（ゴール・ジェムに行けるか）", GUILayout.Height(28)))
                StageFactory.ValidateOpenScene(out validateMessage, out problems);
            if (!string.IsNullOrEmpty(validateMessage))
            {
                EditorGUILayout.HelpBox(validateMessage, problems.Count > 0 ? MessageType.Warning : MessageType.Info);
                if (problems.Count > 0 && GUILayout.Button("取れないジェムを選択")) Selection.objects = problems.ToArray();
            }

            EditorGUILayout.EndScrollView();
        }

        void DrawItemOptions()
        {
            switch (item)
            {
                case Item.Block:
                case Item.GhostBlock:
                    blockSize = Vector3Int.Max(Vector3Int.one, EditorGUILayout.Vector3IntField("サイズ", blockSize));
                    extendDown = Mathf.Max(0, EditorGUILayout.IntField(new GUIContent("下に伸ばす", "置いたマスから下に何マス伸ばすか（宙に浮いた柱っぽく見せる）"), extendDown));
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("1×1")) blockSize = Vector3Int.one;
                        if (GUILayout.Button("床3×3")) blockSize = new Vector3Int(3, 1, 3);
                        if (GUILayout.Button("通路1×5")) blockSize = new Vector3Int(1, 1, 5);
                        if (GUILayout.Button("壁")) blockSize = new Vector3Int(1, 4, 5);
                    }
                    if (item == Item.GhostBlock) group = EditorGUILayout.IntField("スイッチ番号", group);
                    break;
                case Item.Switch:
                    group = EditorGUILayout.IntField("スイッチ番号", group);
                    break;
                case Item.MovingPlatform:
                    platformOffset = EditorGUILayout.Vector3IntField(new GUIContent("移動量", "X・Y・Zのどれか1軸だけ"), platformOffset);
                    platformTimeOffset = EditorGUILayout.FloatField("タイミングずらし（秒）", platformTimeOffset);
                    break;
                case Item.Elevator:
                    elevatorHeight = EditorGUILayout.IntField("上がる高さ", elevatorHeight);
                    platformTimeOffset = EditorGUILayout.FloatField("タイミングずらし（秒）", platformTimeOffset);
                    break;
                case Item.Pusher:
                    pusherDir = (GridDir)EditorGUILayout.EnumPopup("飛び出す向き", pusherDir);
                    pusherTimeOffset = EditorGUILayout.FloatField("タイミングずらし（秒）", pusherTimeOffset);
                    EditorGUILayout.HelpBox("壁の表面のマスに重ねて置きます（壁の中に置いてOK）。壁の面をクリックすると、その面の外側に置かれるので、置いた後で1マス壁側へずらすか、壁の中を直接クリックしてください。", MessageType.None);
                    break;
                case Item.PlayerStart:
                    EditorGUILayout.HelpBox("クリックした場所にプレイヤーを移動します。", MessageType.None);
                    break;
            }
        }

        // ================= シーンビュー =================

        void OnSceneGUI(SceneView view)
        {
            if (!placeMode || !CubeOrbitSetup.IsSetUp) return;
            Event e = Event.current;
            if (e.alt) { hasHover = false; return; }

            int id = GUIUtility.GetControlID(FocusType.Passive);
            if (e.type == EventType.Layout) HandleUtility.AddDefaultControl(id);

            if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag || e.type == EventType.Repaint || e.type == EventType.Layout)
                UpdateHover(e.mousePosition);

            if (hasHover && e.type == EventType.Repaint) DrawPreview(e.shift);

            if (e.type == EventType.MouseDown && e.button == 0 && hasHover)
            {
                if (e.shift) DeleteHovered();
                else Place(hoverCell);
                e.Use();
            }
            if (e.type == EventType.MouseMove) view.Repaint();
        }

        void UpdateHover(Vector2 mouse)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mouse);
            Physics.SyncTransforms();
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f))
            {
                hoverElement = hit.collider.GetComponentInParent<GridElement>();
                hoverCell = GridUtil.ToCell(hit.point + hit.normal * 0.5f);
                hasHover = true;
                return;
            }
            hoverElement = null;
            var plane = new Plane(Vector3.up, new Vector3(0, emptyPlaneY, 0));
            float dist;
            if (plane.Raycast(ray, out dist))
            {
                Vector3 p = ray.GetPoint(dist);
                hoverCell = new Vector3Int(Mathf.RoundToInt(p.x), emptyPlaneY, Mathf.RoundToInt(p.z));
                hasHover = true;
            }
            else hasHover = false;
        }

        void DrawPreview(bool deleting)
        {
            if (deleting)
            {
                if (hoverElement == null) return;
                Handles.color = new Color(1f, 0.3f, 0.3f, 1f);
                Vector3Int min, max; hoverElement.GetBounds(out min, out max);
                Vector3 center = ((Vector3)min + (Vector3)max) * 0.5f;
                Handles.DrawWireCube(center, (Vector3)(max - min) + Vector3.one * 1.02f);
                return;
            }
            Vector3Int origin; Vector3Int size;
            GetPlacementBox(hoverCell, out origin, out size);
            Handles.color = item == Item.GhostBlock ? GridUtil.GroupColor(group) : new Color(0.5f, 1f, 0.8f, 1f);
            Handles.DrawWireCube((Vector3)origin + (Vector3)(size - Vector3Int.one) * 0.5f, (Vector3)size);
            if (item == Item.MovingPlatform || item == Item.Elevator)
            {
                Vector3Int off = item == Item.Elevator ? new Vector3Int(0, elevatorHeight, 0) : platformOffset;
                Handles.DrawDottedLine(hoverCell, hoverCell + off, 3f);
                Handles.DrawWireCube(hoverCell + off, Vector3.one);
            }
            if (item == Item.Pusher)
            {
                Vector3 to = hoverCell + pusherDir.ToVector();
                Handles.DrawDottedLine(hoverCell, to, 3f);
                Handles.DrawWireCube(to, Vector3.one * 0.8f);
            }
        }

        void GetPlacementBox(Vector3Int cell, out Vector3Int origin, out Vector3Int size)
        {
            origin = cell; size = Vector3Int.one;
            if (item == Item.Block || item == Item.GhostBlock)
            {
                size = blockSize;
                origin = cell - new Vector3Int(0, extendDown, 0);
                size.y += extendDown;
            }
        }

        void Place(Vector3Int cell)
        {
            Transform root = StageFactory.GetStageRoot();
            GameObject go = null;
            Vector3Int origin, size;
            GetPlacementBox(cell, out origin, out size);

            switch (item)
            {
                case Item.Block:
                    go = StageFactory.Instantiate("Block", origin, root);
                    if (go != null) { var bv = go.GetComponent<BlockVolume>(); bv.size = size; bv.ApplySize(); }
                    break;
                case Item.GhostBlock:
                    go = StageFactory.Instantiate("GhostBlock", origin, root);
                    if (go != null) { var gb = go.GetComponent<GhostBlockVolume>(); gb.size = size; gb.group = group; gb.ApplySize(); }
                    break;
                case Item.Switch:
                    go = StageFactory.Instantiate("Switch", cell, root);
                    if (go != null) go.GetComponent<FloorSwitch>().group = group;
                    break;
                case Item.Gem: go = StageFactory.Instantiate("Gem", cell, root); break;
                case Item.Checkpoint: go = StageFactory.Instantiate("Checkpoint", cell, root); break;
                case Item.Goal: go = StageFactory.Instantiate("Goal", cell, root); break;
                case Item.MovingPlatform:
                    go = StageFactory.Instantiate("MovingPlatform", cell, root);
                    if (go != null) { var mp = go.GetComponent<MovingPlatform>(); mp.moveOffset = platformOffset; mp.timeOffset = platformTimeOffset; }
                    break;
                case Item.Elevator:
                    go = StageFactory.Instantiate("Elevator", cell, root);
                    if (go != null) { var mp = go.GetComponent<MovingPlatform>(); mp.moveOffset = new Vector3Int(0, elevatorHeight, 0); mp.timeOffset = platformTimeOffset; }
                    break;
                case Item.Pusher:
                    go = StageFactory.Instantiate("Pusher", cell, root);
                    if (go != null) { var p = go.GetComponent<Pusher>(); p.pushDirection = pusherDir; p.timeOffset = pusherTimeOffset; }
                    break;
                case Item.CrumbleTile: go = StageFactory.Instantiate("CrumbleTile", cell, root); break;
                case Item.PlayerStart:
                    var player = GridUtil.FindOne<PlayerCube>();
                    if (player == null) { go = StageFactory.Instantiate("Player", cell, null); break; }
                    Undo.RecordObject(player.transform, "プレイヤー移動");
                    player.transform.position = cell;
                    Selection.activeGameObject = player.gameObject;
                    EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
                    return;
            }
            if (go != null)
            {
                Undo.RegisterCreatedObjectUndo(go, "配置: " + ItemLabels[(int)item]);
                Selection.activeGameObject = go;
                EditorSceneManager.MarkSceneDirty(go.scene);
            }
        }

        void DeleteHovered()
        {
            if (hoverElement == null) return;
            var go = hoverElement.gameObject;
            var root = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
            if (root != null) go = root;
            EditorSceneManager.MarkSceneDirty(go.scene);
            Undo.DestroyObjectImmediate(go);
            hoverElement = null;
        }

        // ================= ステージ作成 =================

        static void NewStage()
        {
            if (!CubeOrbitSetup.IsSetUp) CubeOrbitSetup.Run();
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            CubeOrbitSetup.EnsureFolder(CubeOrbitSetup.StageDir);
            int n = EditorBuildSettings.scenes.Length + 1;
            string path = EditorUtility.SaveFilePanelInProject("新しいステージ", "Stage" + n.ToString("00"), "unity", "保存する場所を選んでください", CubeOrbitSetup.StageDir);
            if (string.IsNullOrEmpty(path)) return;
            StageFactory.CreateStageScene(path, System.IO.Path.GetFileNameWithoutExtension(path), true);
            Open();
        }

        static void MakeTitle()
        {
            if (!CubeOrbitSetup.IsSetUp) CubeOrbitSetup.Run();
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            CubeOrbitSetup.EnsureFolder(CubeOrbitSetup.StageDir);
            string path = CubeOrbitSetup.StageDir + "/Title.unity";
            if (System.IO.File.Exists(path) && !EditorUtility.DisplayDialog("タイトル画面", "Title.unity は既にあります。作り直しますか？", "作り直す", "キャンセル")) return;
            StageFactory.CreateTitleScene(path);
            EditorUtility.DisplayDialog("タイトル画面", "タイトル画面を作って Build Settings の先頭に入れました。\n▶でタイトルから遊べます。", "OK");
        }

        static void GenerateSamples()
        {
            if (!EditorUtility.DisplayDialog("サンプルステージ", "Web版の15ステージをシーンとして生成します。\n（" + CubeOrbitSetup.StageDir + "/Samples に保存、Build Settings にも追加）\n同名のシーンは上書きされます。", "生成する", "キャンセル")) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string dir = CubeOrbitSetup.StageDir + "/Samples";
            CubeOrbitSetup.EnsureFolder(dir);
            string first = null;
            for (int i = 0; i < SampleStages.All.Length; i++)
            {
                EditorUtility.DisplayProgressBar("サンプルステージ", SampleStages.All[i].name, (float)i / SampleStages.All.Length);
                string path = dir + "/Sample" + (i + 1).ToString("00") + ".unity";
                StageFactory.BuildSample(SampleStages.All[i], path, i + 1);
                if (first == null) first = path;
            }
            EditorUtility.ClearProgressBar();
            if (first != null) EditorSceneManager.OpenScene(first);
            EditorUtility.DisplayDialog("サンプルステージ", "15ステージを生成しました。▶で遊べます。クリアすると次のステージへ進みます。", "OK");
        }
    }
}
