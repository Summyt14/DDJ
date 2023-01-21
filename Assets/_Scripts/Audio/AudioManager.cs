using UnityEngine;

namespace _Scripts.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioListSo audioList;

        public static AudioManager Instance { get; private set; }

        private GameObject _oneShotGameObject;
        private AudioSource _oneShotAudioSource;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }

            PlaySound(AudioSo.Sounds.BackgroundMusic);
        }

        public void PlaySound(AudioSo.Sounds soundType)
        {
            AudioSo audioSo = GetAudioSo(soundType);
            if (audioSo == null) return;

            if (audioSo.loop)
            {
                GameObject soundGameObject = new($"Loop Sound: {soundType}");
                AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
                audioSource.clip = audioSo.audioClip;
                audioSource.loop = true;
                audioSource.volume = audioSo.volume;
                audioSource.Play();
                return;
            }

            if (_oneShotGameObject == null)
            {
                _oneShotGameObject = new GameObject($"One Shot Sound");
                _oneShotAudioSource = _oneShotGameObject.AddComponent<AudioSource>();
            }

            _oneShotAudioSource.PlayOneShot(audioSo.audioClip, audioSo.volume);
        }

        public void PlaySound(AudioSo.Sounds soundType, Vector3 position)
        {
            AudioSo audioSo = GetAudioSo(soundType);
            if (audioSo == null) return;
            GameObject soundGameObject = new($"Sound: {soundType}");
            soundGameObject.transform.position = position;
            AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
            audioSource.clip = audioSo.audioClip;
            audioSource.volume = audioSo.volume;
            audioSource.maxDistance = 100f;
            audioSource.spatialBlend = 1f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.dopplerLevel = 0f;
            audioSource.Play();
            Destroy(soundGameObject, audioSource.clip.length);
        }

        private AudioSo GetAudioSo(AudioSo.Sounds soundType)
        {
            foreach (AudioSo audioSO in audioList.audioList)
                if (audioSO.soundType == soundType)
                    return audioSO;

            Debug.LogError($"AudioSO for {soundType} not found!");
            return null;
        }
    }
}