using TMPro;
using UnityEngine;

namespace PsHome.Player
{
    /// <summary>頭上の名札。常にカメラの方を向く。</summary>
    public class Nameplate : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;

        public void SetText(string text)
        {
            if (label != null)
                label.text = text;
        }

        private void LateUpdate()
        {
            var cam = Camera.main;
            if (cam != null)
                transform.rotation = cam.transform.rotation;
        }
    }
}
