using UnityEngine;
using UnityEngine.SceneManagement;

namespace CubeOrbit
{
    /// <summary>
    /// ステージ全体の進行役。1つのシーン（ステージ）に1個置く。
    /// タイマー、ジェム数、チェックポイント、落下→やり直し、クリア、次のステージへ、を担当。
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        /// <summary>ステージ開始からの時間。動く床や押し出す壁はこれを見て動く</summary>
        public static float StageTime { get; private set; }

        [Header("ステージ情報")]
        public string stageName = "新しいステージ";
        [Tooltip("画面に出すステージ番号。0なら Build Settings の順番から自動")]
        public int stageNumber = 0;
        [TextArea(1, 3)] [Tooltip("ステージ開始時に画面下に出る説明（空なら出さない）")]
        public string hint = "";
        [Tooltip("ランク計算の目安タイム（秒）")]
        public float parTime = 45f;

        [Header("落下")]
        [Tooltip("ONなら一番低いブロックの少し下を自動で落下ラインにする")]
        public bool autoKillY = true;
        [Tooltip("これより下に落ちたらやり直し（autoKillY が OFF の時に使う）")]
        public int killY = -12;
        [Tooltip("autoKillY の時、一番低いブロックからどれだけ下か")]
        public int autoKillMargin = 9;
        [Tooltip("落ちてから復活するまでの時間（秒）")]
        public float respawnDelay = 0.75f;

        [Header("次のステージ")]
        [Tooltip("空なら Build Settings の次のシーンへ進む")]
        public string nextSceneName = "";

        [Header("タイトル")]
        [Tooltip("タイトル画面のシーン名（「タイトルへ」ボタンで戻る先）")]
        public string titleSceneName = "Title";

        [Header("参照（空なら自動で探す）")]
        public PlayerCube player;

        public int KillY { get; private set; }
        public int GemsCollected { get; private set; }
        public int GemsTotal { get; private set; }
        public int Deaths { get; private set; }
        public float ClearTime { get; private set; }
        public bool IsCleared { get; private set; }
        public bool IsPaused { get; private set; }
        public string Rank { get; private set; }
        /// <summary>パネルが開いていてキューブ操作を受け付けない時</summary>
        public bool IsUiBlockingInput { get { return IsPaused || IsCleared; } }
        public bool HasNextStage
        {
            get
            {
                if (!string.IsNullOrEmpty(nextSceneName)) return true;
                return SceneManager.GetActiveScene().buildIndex + 1 < SceneManager.sceneCountInBuildSettings;
            }
        }

        /// <summary>タイトルのシーンが Build Settings にあるか</summary>
        public bool HasTitle { get { return Application.CanStreamedLevelBeLoaded(titleSceneName); } }

        /// <summary>画面に出すステージ番号（タイトルが先頭にあればその分ずらす）</summary>
        public int DisplayStageNumber
        {
            get
            {
                if (stageNumber > 0) return stageNumber;
                int idx = SceneManager.GetActiveScene().buildIndex;
                if (idx < 0) return 0;
                string first = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(0));
                return first == titleSceneName ? idx : idx + 1;
            }
        }

        Vector3Int checkpointCell;
        Checkpoint activeCheckpoint;

        void Awake()
        {
            Instance = this;
            StageTime = 0f;
            Time.timeScale = 1f;
            SwitchSystem.ResetAll();
        }

        void OnDestroy() { if (Instance == this) { Instance = null; Time.timeScale = 1f; } }

        void Start()
        {
            if (player == null) player = GridUtil.FindOne<PlayerCube>();
            GemsTotal = GridUtil.FindAll<Gem>().Length;
            KillY = autoKillY ? GridWorld.LowestY(GridWorld.Elements) - autoKillMargin : killY;
            if (player != null)
            {
                checkpointCell = GridUtil.ToCell(player.transform.position);
                player.Spawn(checkpointCell, true);
            }
            else Debug.LogError("PlayerCube がシーンにありません");
        }

        void Update()
        {
            StageTime += Time.deltaTime;
            if (!IsCleared && player != null && player.CurrentState != PlayerCube.State.Win)
                ClearTime += Time.deltaTime;

            if (InputCompat.Down(GameKey.Restart) && !IsCleared) Restart();
            if (InputCompat.Down(GameKey.Pause) && !IsCleared) SetPaused(!IsPaused);
            if (InputCompat.Down(GameKey.Mute)) SoundPlayer.ToggleMute();
            if (IsCleared && InputCompat.Down(GameKey.Confirm)) { if (HasNextStage) LoadNextStage(); else Restart(); }
        }

        // ---------- ギミック・アイテムから呼ばれる ----------

        public void CollectGem(Gem gem) { GemsCollected++; }

        public void SetCheckpoint(Checkpoint cp)
        {
            if (activeCheckpoint == cp) return;
            if (activeCheckpoint != null) activeCheckpoint.SetActiveVisual(false);
            activeCheckpoint = cp;
            cp.SetActiveVisual(true);
            checkpointCell = cp.Cell;
            SoundPlayer.Play(Sfx.Checkpoint);
            BurstEffect.Spawn(cp.transform.position + Vector3.down * 0.4f, new Color(0.6f, 0.9f, 1f), 10, 2.5f);
        }

        public void OnPlayerDied()
        {
            Deaths++;
            CameraFollow.Shake(0.2f);
        }

        public void Respawn()
        {
            GridWorld.ResetStage();
            if (player != null) player.Spawn(checkpointCell, false);
        }

        public void ReachGoal()
        {
            if (player != null) player.StartWin();
        }

        /// <summary>ゴール演出が終わったら PlayerCube から呼ばれる</summary>
        public void ShowClear()
        {
            if (IsCleared) return;
            IsCleared = true;
            Rank = CalcRank();
            SaveBest();
        }

        // ---------- 画面遷移 ----------

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            var s = SceneManager.GetActiveScene();
            if (s.buildIndex >= 0) SceneManager.LoadScene(s.buildIndex);
            else SceneManager.LoadScene(s.name);
        }

        public void LoadNextStage()
        {
            Time.timeScale = 1f;
            if (!string.IsNullOrEmpty(nextSceneName)) { SceneManager.LoadScene(nextSceneName); return; }
            int next = SceneManager.GetActiveScene().buildIndex + 1;
            if (next < SceneManager.sceneCountInBuildSettings) SceneManager.LoadScene(next);
            else Restart();
        }

        public void LoadTitle()
        {
            Time.timeScale = 1f;
            if (HasTitle) SceneManager.LoadScene(titleSceneName);
            else Restart();
        }

        public void LoadStage(int buildIndex)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(buildIndex);
        }

        // ---------- ランク・記録 ----------

        string CalcRank()
        {
            float gemRate = GemsTotal > 0 ? (float)GemsCollected / GemsTotal : 1f;
            float score = gemRate * 60f + Mathf.Max(0f, 1f - ClearTime / (parTime * 2f)) * 30f + Mathf.Max(0f, 10f - Deaths * 3f);
            return score >= 88f ? "S" : score >= 72f ? "A" : score >= 50f ? "B" : "C";
        }

        string BestKey { get { return "CubeOrbit_Best_" + SceneManager.GetActiveScene().name; } }

        /// <summary>シーン名を指定して自己ベストを読む（ステージ選択画面用）</summary>
        public static bool TryGetBestFor(string sceneName, out float time, out int gems)
        {
            string key = "CubeOrbit_Best_" + sceneName;
            time = PlayerPrefs.GetFloat(key + "_time", float.MaxValue);
            gems = PlayerPrefs.GetInt(key + "_gems", 0);
            return time < float.MaxValue;
        }

        void SaveBest()
        {
            float bestTime = PlayerPrefs.GetFloat(BestKey + "_time", float.MaxValue);
            int bestGems = PlayerPrefs.GetInt(BestKey + "_gems", 0);
            PlayerPrefs.SetFloat(BestKey + "_time", Mathf.Min(bestTime, ClearTime));
            PlayerPrefs.SetInt(BestKey + "_gems", Mathf.Max(bestGems, GemsCollected));
            PlayerPrefs.Save();
        }

        public bool TryGetBest(out float time, out int gems)
        {
            time = PlayerPrefs.GetFloat(BestKey + "_time", float.MaxValue);
            gems = PlayerPrefs.GetInt(BestKey + "_gems", 0);
            return time < float.MaxValue;
        }
    }
}
