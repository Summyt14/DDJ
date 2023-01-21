using UnityEngine;

namespace _Scripts.Audio
{
    [CreateAssetMenu(fileName = "AudioClip", menuName = "Audio Clip")]
    public class AudioSo : ScriptableObject
    {
        public enum Sounds
        {
            BackgroundMusic,
            GrappleShoot,
            RobotExplosion,
            RobotWalk,
            RobotShoot
        }

        public Sounds soundType;
        public AudioClip audioClip;
        public float volume = 1;
        public bool loop;
    }
}