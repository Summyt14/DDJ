using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Transform[] enemySpawnPoints;
        [SerializeField] private Transform player;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private string winSceneName;
        [SerializeField] private string loseSceneName;
        [SerializeField] private int playerMaxHealth = 100;
        
        public static GameManager Instance { get; private set; }
        private int _playerHealth;

        public int PlayerHealth
        {
            get => _playerHealth;
            set => _playerHealth = Mathf.Clamp(value, 0, 100);
        }
        
        public int EnemiesAlive { get; set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }

            EnemiesAlive = enemySpawnPoints.Length;
            PlayerHealth = playerMaxHealth;
        }

        private void Start()
        {
            foreach (Transform spawnPoint in enemySpawnPoints)
            {
                Enemy.Enemy enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity).GetComponent<Enemy.Enemy>();
                enemy.SetPlayer(player);
            }
        }

        private void Update()
        {
            if (EnemiesAlive <= 0)
            {
                SceneManager.LoadScene(winSceneName);
            }

            if (PlayerHealth <= 0)
            {
                SceneManager.LoadScene(loseSceneName);
            }
        }
    }
}