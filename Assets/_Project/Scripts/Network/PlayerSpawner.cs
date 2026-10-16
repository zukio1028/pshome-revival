using Fusion;
using PsHome.Player;
using UnityEngine;

namespace PsHome.Network
{
    /// <summary>
    /// NetworkRunner のプレハブに付ける。ラウンジシーンの読み込みが終わったら自分のアバターだけを Spawn する
    /// （Shared Mode では各クライアントが自分の分だけを Spawn し、その State Authority を持つ）。
    /// スポーン位置はシーン内の "Respawn" タグの付いた Transform から選ぶ。
    /// </summary>
    public class PlayerSpawner : SimulationBehaviour, ISceneLoadDone
    {
        [SerializeField] private NetworkObject playerPrefab;

        private bool _spawned;

        public void SceneLoadDone(in SceneLoadDoneArgs sceneInfo)
        {
            if (_spawned)
                return;
            _spawned = true;

            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;
            var points = GameObject.FindGameObjectsWithTag("Respawn");
            if (points.Length > 0)
            {
                var point = points[Random.Range(0, points.Length)].transform;
                position = point.position;
                rotation = point.rotation;
            }

            var connection = ConnectionManager.Instance;
            Runner.Spawn(playerPrefab, position, rotation, Runner.LocalPlayer,
                onBeforeSpawned: (runner, obj) =>
                {
                    var networkPlayer = obj.GetComponent<NetworkPlayer>();
                    networkPlayer.Nickname = connection != null ? connection.LocalNickname : "Guest";
                    networkPlayer.AvatarIndex = connection != null ? connection.LocalAvatarIndex : 0;
                });
        }
    }
}
