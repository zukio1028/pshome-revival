using PsHome.Network;
using PsHome.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PsHome.UI
{
    /// <summary>Boot シーンの入室画面：ニックネームとアバターを選んでラウンジへ。</summary>
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nicknameInput;
        [SerializeField] private TMP_Dropdown avatarDropdown;
        [SerializeField] private Button enterButton;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private AvatarCatalog avatarCatalog;

        private const string NicknameKey = "nickname";
        private const string AvatarKey = "avatarIndex";

        private void Start()
        {
            nicknameInput.characterLimit = 16;
            nicknameInput.text = PlayerPrefs.GetString(NicknameKey, string.Empty);

            avatarDropdown.ClearOptions();
            for (int i = 0; i < avatarCatalog.Count; i++)
                avatarDropdown.options.Add(new TMP_Dropdown.OptionData(avatarCatalog.Get(i).displayName, avatarCatalog.Get(i).thumbnail, Color.white));
            avatarDropdown.value = PlayerPrefs.GetInt(AvatarKey, 0);
            avatarDropdown.RefreshShownValue();

            enterButton.onClick.AddListener(OnEnter);
            statusText.text = string.Empty;
        }

        private async void OnEnter()
        {
            enterButton.interactable = false;
            statusText.text = "接続中...";

            PlayerPrefs.SetString(NicknameKey, nicknameInput.text);
            PlayerPrefs.SetInt(AvatarKey, avatarDropdown.value);

            bool ok = await ConnectionManager.Instance.ConnectAsync(nicknameInput.text, avatarDropdown.value);
            if (!ok)
            {
                statusText.text = "接続できませんでした。時間をおいて再度お試しください。";
                enterButton.interactable = true;
            }
        }
    }
}
