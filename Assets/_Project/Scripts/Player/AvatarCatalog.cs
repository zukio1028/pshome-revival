using UnityEngine;

namespace PsHome.Player
{
    /// <summary>
    /// アプリに同梱する VRM アバターの一覧。
    /// .vrm を Assets に置くと UniVRM がプレハブを生成するので、それをここに登録する。
    /// </summary>
    [CreateAssetMenu(menuName = "PsHome/Avatar Catalog", fileName = "AvatarCatalog")]
    public class AvatarCatalog : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public string displayName;
            public GameObject vrmPrefab;
            public Sprite thumbnail;
        }

        [SerializeField] private Entry[] avatars;

        public int Count => avatars?.Length ?? 0;

        public Entry Get(int index)
        {
            if (Count == 0)
                return null;
            return avatars[Mathf.Clamp(index, 0, Count - 1)];
        }
    }
}
