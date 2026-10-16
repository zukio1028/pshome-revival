using System.Collections.Generic;
using System.Text;
using PsHome.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PsHome.Chat
{
    /// <summary>
    /// 画面下のチャット欄（ログ表示と入力）。ラウンジシーンの Canvas に置く。
    /// 送信は自分の NetworkPlayer の RPC を経由する。
    /// </summary>
    public class ChatManager : MonoBehaviour
    {
        public static ChatManager Instance { get; private set; }

        [SerializeField] private TMP_InputField input;
        [SerializeField] private Button sendButton;
        [SerializeField] private TMP_Text log;
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private int maxLines = 50;
        [SerializeField] private float sendCooldownSeconds = 1f;

        private readonly Queue<string> _lines = new Queue<string>();
        private readonly StringBuilder _builder = new StringBuilder();
        private float _nextSendTime;

        private void Awake()
        {
            Instance = this;
            input.characterLimit = NetworkPlayer.MaxChatLength;
            input.onSubmit.AddListener(_ => Send());
            input.onSelect.AddListener(_ => PlayerMovement.InputEnabled = false);
            input.onDeselect.AddListener(_ => PlayerMovement.InputEnabled = true);
            sendButton.onClick.AddListener(Send);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void AddLine(string sender, string message, bool isMine)
        {
            string color = isMine ? "#8fd3ff" : "#ffffff";
            _lines.Enqueue($"<color={color}><b>{Escape(sender)}</b></color>: {Escape(message)}");
            while (_lines.Count > maxLines)
                _lines.Dequeue();

            _builder.Clear();
            foreach (var line in _lines)
                _builder.AppendLine(line);
            log.text = _builder.ToString();

            Canvas.ForceUpdateCanvases();
            if (scroll != null)
                scroll.verticalNormalizedPosition = 0f;
        }

        private void Send()
        {
            if (Time.time < _nextSendTime)
                return;

            var local = FindLocalPlayer();
            if (local == null)
                return;

            local.SendChat(input.text);
            input.text = string.Empty;
            _nextSendTime = Time.time + sendCooldownSeconds;
        }

        private static NetworkPlayer FindLocalPlayer()
        {
            foreach (var player in FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None))
            {
                if (player.HasStateAuthority)
                    return player;
            }
            return null;
        }

        // 他人の発言にリッチテキストタグを埋め込まれないようにする
        private static string Escape(string text) => text.Replace("<", "<​");
    }
}
