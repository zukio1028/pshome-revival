using System.Threading.Tasks;
using Fusion;
using UnityEngine;

namespace PsHome.Network
{
    /// <summary>
    /// ラウンジ（Fusion Shared Mode のセッション）への接続を担当する。
    /// 満員なら lounge-2, lounge-3 ... と順に空きを探す。
    /// </summary>
    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager Instance { get; private set; }

        [SerializeField] private NetworkRunner runnerPrefab;
        [SerializeField] private string sessionPrefix = "lounge";
        [SerializeField] private int maxPlayersPerLounge = 24;
        [SerializeField] private int maxLoungeInstances = 10;

        /// <summary>接続時にローカルプレイヤーへ渡す値。PlayerSpawner が参照する。</summary>
        public string LocalNickname { get; private set; } = "Guest";
        public int LocalAvatarIndex { get; private set; }

        public NetworkRunner Runner { get; private set; }
        public string CurrentSessionName { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async Task<bool> ConnectAsync(string nickname, int avatarIndex)
        {
            LocalNickname = string.IsNullOrWhiteSpace(nickname) ? "Guest" : nickname.Trim();
            LocalAvatarIndex = avatarIndex;

            for (int i = 1; i <= maxLoungeInstances; i++)
            {
                string sessionName = $"{sessionPrefix}-{i}";
                StartGameResult result = await TryStartAsync(sessionName);
                if (result.Ok)
                {
                    CurrentSessionName = sessionName;
                    Debug.Log($"[Connection] Joined {sessionName}");
                    return true;
                }

                if (result.ShutdownReason != ShutdownReason.GameIsFull)
                {
                    Debug.LogError($"[Connection] Failed to join {sessionName}: {result.ShutdownReason}");
                    return false;
                }
            }

            Debug.LogError("[Connection] All lounges are full.");
            return false;
        }

        public async Task DisconnectAsync()
        {
            if (Runner != null)
            {
                await Runner.Shutdown();
                Runner = null;
            }
        }

        private async Task<StartGameResult> TryStartAsync(string sessionName)
        {
            // 失敗した Runner は再利用できないため毎回作り直す
            Runner = Instantiate(runnerPrefab);
            Runner.name = $"NetworkRunner ({sessionName})";
            DontDestroyOnLoad(Runner.gameObject);

            var sceneManager = Runner.GetComponent<NetworkSceneManagerDefault>();
            if (sceneManager == null)
                sceneManager = Runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

            var args = new StartGameArgs
            {
                GameMode = GameMode.Shared,
                SessionName = sessionName,
                PlayerCount = maxPlayersPerLounge,
                Scene = SceneRef.FromIndex(LoungeSceneBuildIndex),
                SceneManager = sceneManager,
            };

            StartGameResult result = await Runner.StartGame(args);
            if (!result.Ok && Runner != null)
            {
                Destroy(Runner.gameObject);
                Runner = null;
            }
            return result;
        }

        /// <summary>Build Settings 上のラウンジシーンの番号（0 = Boot, 1 = Lounge を想定）。</summary>
        private const int LoungeSceneBuildIndex = 1;
    }
}
