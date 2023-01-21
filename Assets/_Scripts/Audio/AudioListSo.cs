using UnityEngine;

namespace _Scripts.Audio
{
    [CreateAssetMenu(fileName = "AudioList", menuName = "Audio List")]
    public class AudioListSo : ScriptableObject
    {
        public AudioSo[] audioList;
    }
}