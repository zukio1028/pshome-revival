using TMPro;
using UnityEngine;

namespace PsHome.Chat
{
    /// <summary>頭上の吹き出し。一定時間で消える。</summary>
    public class ChatBubble : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text label;
        [SerializeField] private float baseSeconds = 3f;
        [SerializeField] private float secondsPerChar = 0.08f;

        private float _hideAt;

        private void Awake()
        {
            if (root != null)
                root.SetActive(false);
        }

        public void Show(string message)
        {
            if (label != null)
                label.text = message;
            if (root != null)
                root.SetActive(true);
            _hideAt = Time.time + baseSeconds + message.Length * secondsPerChar;
        }

        private void LateUpdate()
        {
            if (root == null || !root.activeSelf)
                return;

            var cam = Camera.main;
            if (cam != null)
                transform.rotation = cam.transform.rotation;

            if (Time.time >= _hideAt)
                root.SetActive(false);
        }
    }
}
