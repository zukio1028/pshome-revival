using Fusion;
using PsHome.Chat;
using Unity.Cinemachine;
using UnityEngine;

namespace PsHome.Player
{
    /// <summary>
    /// プレイヤーの同期状態（名前・アバター・移動速度）とチャット送信を持つ。
    /// 位置・回転は同じ GameObject の NetworkTransform が同期する。
    /// </summary>
    public class NetworkPlayer : NetworkBehaviour
    {
        public const int MaxChatLength = 60;

        [Networked] public NetworkString<_16> Nickname { get; set; }
        [Networked] public int AvatarIndex { get; set; }
        [Networked] public float Speed { get; set; }

        [SerializeField] private AvatarCatalog avatarCatalog;
        [SerializeField] private RuntimeAnimatorController locomotionController;
        [SerializeField] private Transform avatarRoot;
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private Nameplate nameplate;
        [SerializeField] private ChatBubble chatBubble;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        private ChangeDetector _changes;
        private Animator _animator;
        private GameObject _avatarInstance;

        public override void Spawned()
        {
            _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
            ApplyAvatar();
            ApplyNickname();

            if (HasStateAuthority)
            {
                var vcam = FindFirstObjectByType<CinemachineCamera>();
                if (vcam != null)
                    vcam.Target.TrackingTarget = cameraTarget != null ? cameraTarget : transform;
            }
        }

        public override void Render()
        {
            foreach (var change in _changes.DetectChanges(this))
            {
                switch (change)
                {
                    case nameof(AvatarIndex):
                        ApplyAvatar();
                        break;
                    case nameof(Nickname):
                        ApplyNickname();
                        break;
                }
            }

            if (_animator != null)
                _animator.SetFloat(SpeedHash, Speed);
        }

        /// <summary>ローカルプレイヤーの UI から呼ぶ。</summary>
        public void SendChat(string message)
        {
            if (!HasStateAuthority || string.IsNullOrWhiteSpace(message))
                return;

            message = message.Trim();
            if (message.Length > MaxChatLength)
                message = message.Substring(0, MaxChatLength);

            RPC_Chat(message);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_Chat(string message)
        {
            string sender = Nickname.ToString();
            if (chatBubble != null)
                chatBubble.Show(message);
            if (ChatManager.Instance != null)
                ChatManager.Instance.AddLine(sender, message, HasStateAuthority);
        }

        private void ApplyNickname()
        {
            if (nameplate != null)
                nameplate.SetText(Nickname.ToString());
        }

        private void ApplyAvatar()
        {
            var entry = avatarCatalog != null ? avatarCatalog.Get(AvatarIndex) : null;
            if (entry == null || entry.vrmPrefab == null)
                return;

            if (_avatarInstance != null)
                Destroy(_avatarInstance);

            _avatarInstance = Instantiate(entry.vrmPrefab, avatarRoot != null ? avatarRoot : transform);
            _avatarInstance.transform.localPosition = Vector3.zero;
            _avatarInstance.transform.localRotation = Quaternion.identity;

            // VRM プレハブには Humanoid の Animator が付いているので、共通の移動アニメを差し込む
            _animator = _avatarInstance.GetComponentInChildren<Animator>();
            if (_animator != null)
            {
                _animator.runtimeAnimatorController = locomotionController;
                _animator.applyRootMotion = false;
            }
        }
    }
}
