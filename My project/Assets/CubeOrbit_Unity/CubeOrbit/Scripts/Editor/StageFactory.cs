using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CubeOrbit.EditorTools
{
    /// <summary>ステージ（シーン）を作る・要素を置く、などの共通処理。</summary>
    public static class StageFactory
    {
        public const string StageRootName = "Stage";

        /// <summary>新しいステージのシーンを作って保存する</summary>
        public static Scene CreateStageScene(string path, string stageName, bool addStarter)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // カメラ
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0f, 0f, 0.012f);
            cam.orthographic = true;
            camGo.AddComponent<CameraFollow>();
            var stars = camGo.AddComponent<StarField>();
            stars.starMaterial = CubeOrbitSetup.LoadMaterial("Star");
            camGo.transform.position = new Vector3(20, 25, 20);
            camGo.transform.rotation = Quaternion.LookRotation(-new Vector3(10f, 11.5f, 10f));

            // ゲーム管理
            var gmGo = new GameObject("GameManager");
            var gm = gmGo.AddComponent<GameManager>();
            gm.stageName = stageName;
            gmGo.AddComponent<StageHUD>();
            gmGo.AddComponent<SoundPlayer>();
            var burst = gmGo.AddComponent<BurstEffect>();
            burst.particleMaterial = CubeOrbitSetup.LoadMaterial("Effect");

            // プレイヤーと影
            var player = Instantiate("Player", new Vector3Int(1, 1, 1), null);
            gm.player = player != null ? player.GetComponent<PlayerCube>() : null;
            var shadow = Instantiate("PlayerShadow", Vector3Int.zero, null);
            if (shadow != null && gm.player != null) shadow.GetComponent<BlobShadow>().player = gm.player;

            var root = new GameObject(StageRootName).transform;

            if (addStarter)
            {
                // スタート床とゴール床のサンプル
                PlaceBlock(new Vector3Int(0, -3, 0), new Vector3Int(3, 4, 3), root);
                PlaceBlock(new Vector3Int(1, -3, -4), new Vector3Int(1, 4, 4), root);
                PlaceBlock(new Vector3Int(0, -3, -7), new Vector3Int(3, 4, 3), root);
                Instantiate("Gem", new Vector3Int(1, 1, -3), root);
                Instantiate("Goal", new Vector3Int(1, 1, -6), root);
            }

            EditorSceneManager.SaveScene(scene, path);
            AddToBuildSettings(path);
            return scene;
        }

        /// <summary>タイトル画面のシーンを作り、Build Settings の先頭（0番）に入れる</summary>
        public static void CreateTitleScene(string path)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0f, 0f, 0.012f);
            cam.orthographic = true;
            cam.orthographicSize = 6.6f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 200f;
            Vector3 dir = new Vector3(10f, 11.5f, 10f).normalized;
            Vector3 focus = new Vector3(1f, 0.5f, 1f);
            camGo.transform.position = focus + dir * 40f;
            camGo.transform.rotation = Quaternion.LookRotation(-dir, Vector3.up);
            var stars = camGo.AddComponent<StarField>();
            stars.starMaterial = CubeOrbitSetup.LoadMaterial("Star");

            // 飾り：浮かぶ足場と回るキューブ
            var decor = new GameObject("Decor").transform;
            PlaceBlock(new Vector3Int(-1, -3, -1), new Vector3Int(5, 3, 5), decor);
            PlaceBlock(new Vector3Int(5, -3, 2), new Vector3Int(2, 3, 2), decor);
            PlaceBlock(new Vector3Int(-3, -3, 4), new Vector3Int(1, 4, 1), decor);
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "SpinningCube";
            Object.DestroyImmediate(cube.GetComponent<Collider>());
            cube.GetComponent<Renderer>().sharedMaterial = CubeOrbitSetup.LoadMaterial("Player");
            cube.transform.position = new Vector3(1f, 1.4f, 1f);
            cube.transform.localScale = Vector3.one * 1.3f;
            var cc = cube.AddComponent<CubeColorCycle>();
            cc.target = cube.GetComponent<Renderer>();

            var titleGo = new GameObject("TitleScreen");
            var ts = titleGo.AddComponent<TitleScreen>();
            ts.decorCube = cube.transform;
            ts.firstStageBuildIndex = 1;

            EditorSceneManager.SaveScene(scene, path);

            // Build Settings の先頭に置く
            var list = EditorBuildSettings.scenes.ToList();
            list.RemoveAll(s => s.path == path);
            list.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = list.ToArray();
        }

        public static Transform GetStageRoot()
        {
            var go = GameObject.Find(StageRootName);
            if (go == null) go = new GameObject(StageRootName);
            return go.transform;
        }

        public static GameObject Instantiate(string prefabName, Vector3Int cell, Transform parent)
        {
            var prefab = CubeOrbitSetup.LoadPrefab(prefabName);
            if (prefab == null) { Debug.LogError("プレハブがありません: " + prefabName + "（先に初期セットアップを実行してください）"); return null; }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (parent != null) go.transform.SetParent(parent, true);
            go.transform.position = cell;
            return go;
        }

        public static BlockVolume PlaceBlock(Vector3Int origin, Vector3Int size, Transform parent)
        {
            var go = Instantiate("Block", origin, parent);
            if (go == null) return null;
            var bv = go.GetComponent<BlockVolume>();
            bv.size = size;
            bv.ApplySize();
            return bv;
        }

        public static void AddToBuildSettings(string path)
        {
            var list = EditorBuildSettings.scenes.ToList();
            if (list.Any(s => s.path == path)) return;
            list.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = list.ToArray();
        }

        /// <summary>Web版のステージデータからシーンを作る</summary>
        public static void BuildSample(StageData d, string path, int number)
        {
            CreateStageScene(path, d.name, false);
            var gm = GridUtil.FindOne<GameManager>();
            gm.hint = d.hint;
            gm.parTime = d.par;
            gm.stageNumber = number;
            var root = GetStageRoot();

            var player = GridUtil.FindOne<PlayerCube>();
            if (player != null) player.transform.position = V(d.start);

            foreach (var b in d.blocks) PlaceBlock(new Vector3Int(b[0], b[1], b[2]), new Vector3Int(b[3], b[4], b[5]), root);
            foreach (var g in d.ghosts)
            {
                var go = Instantiate("GhostBlock", new Vector3Int(g[0], g[1], g[2]), root);
                var gb = go.GetComponent<GhostBlockVolume>();
                gb.size = new Vector3Int(g[3], g[4], g[5]);
                gb.group = g[6];
                gb.ApplySize();
            }
            foreach (var g in d.gems) Instantiate("Gem", V(g), root);
            foreach (var s in d.switches) Instantiate("Switch", V(s), root).GetComponent<FloorSwitch>().group = s[3];
            foreach (var c in d.checkpoints) Instantiate("Checkpoint", V(c), root);
            Instantiate("Goal", V(d.goal), root);
            foreach (var p in d.platforms)
            {
                var mp = Instantiate("MovingPlatform", new Vector3Int((int)p[0], (int)p[1], (int)p[2]), root).GetComponent<MovingPlatform>();
                mp.moveOffset = new Vector3Int((int)p[3], (int)p[4], (int)p[5]);
                mp.speed = p[6]; mp.pauseTime = p[7]; mp.timeOffset = p[8];
            }
            foreach (var p in d.pushers)
            {
                var ps = Instantiate("Pusher", new Vector3Int((int)p[0], (int)p[1], (int)p[2]), root).GetComponent<Pusher>();
                ps.pushDirection = GridUtil.FromVector(new Vector3Int((int)p[3], 0, (int)p[4]));
                ps.period = p[5]; ps.timeOffset = p[6];
            }
            foreach (var c in d.crumbles) Instantiate("CrumbleTile", V(c), root);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        static Vector3Int V(int[] a) { return new Vector3Int(a[0], a[1], a[2]); }

        /// <summary>今開いているシーンの要素を集めて検証する</summary>
        public static SolverResult ValidateOpenScene(out string message, out List<Object> problems)
        {
            problems = new List<Object>();
            var elements = new List<GridElement>(GridUtil.FindAll<GridElement>());
            var player = GridUtil.FindOne<PlayerCube>();
            if (player == null) { message = "PlayerCube がありません。"; return null; }
            var gm = GridUtil.FindOne<GameManager>();
            int killY = (gm == null || gm.autoKillY) ? GridWorld.LowestY(elements) - (gm != null ? gm.autoKillMargin : 9) : gm.killY;

            var res = GridSolver.Validate(elements, GridUtil.ToCell(player.transform.position), killY);
            var sb = new System.Text.StringBuilder();
            if (!res.goalFound) sb.AppendLine("× ゴールが置かれていません");
            else if (res.goalReachable) sb.AppendLine("○ ゴールにたどり着けます");
            else sb.AppendLine("× ゴールにたどり着けません");
            if (res.unreachableGems.Count == 0) sb.AppendLine("○ ジェム " + res.totalGems + " 個すべて取れます");
            else
            {
                sb.AppendLine("× 取れないジェムが " + res.unreachableGems.Count + " 個あります（下のボタンで選択）");
                foreach (var g in res.unreachableGems) problems.Add(g.gameObject);
            }
            sb.Append("（調べた状態数: " + res.statesSearched + "）\n※タイミング（押し出す壁や動く床の間隔）は実際に遊んで確認してください");
            message = sb.ToString();
            return res;
        }
    }
}
