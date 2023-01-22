using System.Collections;
using _Scripts.Audio;
using _Scripts.Player;
using UnityEngine;
using UnityEngine.AI;

namespace _Scripts.Enemy
{
    public class Enemy : MonoBehaviour
    {
        public NavMeshAgent agent;
        public Transform player;
        public LayerMask whatIsGround, whatIsPlayer;
        public float health;
        //Patrolling
        public Vector3 walkPoint;
        public float walkPointRange;
        //States
        public float sightRange, attackRange;
        public bool playerInSightRange, playerInAttackRange;
        
        //Raycast
        [SerializeField] private bool AddBulletSpread = true;
        [SerializeField] private Vector3 BulletSpreadVariance = new Vector3(0.1f, 0.1f, 0.1f);
        [SerializeField] private Transform BulletSpawnPoint;
        [SerializeField] private TrailRenderer BulletTrail;
        [SerializeField] private float ShootDelay = 0.5f;
        [SerializeField] private LayerMask Mask;
        [SerializeField] private float BulletSpeed = 100;
        [SerializeField] private int bulletDamage = 25;
        [SerializeField] private GameObject explosionParticle;

        private bool _walkPointSet, _isGrabbed;
        private float _lastShootTime;
        
        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            //Check for sight and attack range
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
            playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

            if (!playerInSightRange && !playerInAttackRange) Patrolling();
            if (playerInSightRange && !playerInAttackRange) ChasePlayer();
            if (playerInAttackRange && playerInSightRange) AttackPlayer();
        }

        private void Patrolling()
        {
            if (!_walkPointSet) SearchWalkPoint();
            if (_walkPointSet) agent.SetDestination(walkPoint);
            Vector3 distanceToWalkPoint = transform.position - walkPoint;
            //Waypoint reached
            if (distanceToWalkPoint.magnitude < 1f)
                _walkPointSet = false;
        }

        private void SearchWalkPoint()
        {
            //Calculate random point in range
            float randomZ = Random.Range(-walkPointRange, walkPointRange);
            float randomX = Random.Range(-walkPointRange, walkPointRange);

            walkPoint = new Vector3(transform.position.x + randomX, transform.position.y,
                transform.position.z + randomZ);

            if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
                _walkPointSet = true;
        }

        private void ChasePlayer() => agent.SetDestination(player.position);

        private void AttackPlayer()
        {
            //Make sure enemy doesn't move
            agent.SetDestination(transform.position);
            if(!_isGrabbed) transform.LookAt(player);

            if (!_isGrabbed && _lastShootTime + ShootDelay < Time.time)
            {
                Vector3 direction = GetDirection();
                if (Physics.Raycast(BulletSpawnPoint.position, direction, out RaycastHit hit, float.MaxValue, Mask))
                {
                    TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);
                    StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, true));
                    _lastShootTime = Time.time;
                    if (hit.transform.TryGetComponent(out PlayerMovement _))
                        GameManager.Instance.PlayerHealth -= bulletDamage;
                }
                else
                {
                    TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);
                    StartCoroutine(SpawnTrail(trail, BulletSpawnPoint.position + GetDirection() * 100, Vector3.zero,
                        false));
                    _lastShootTime = Time.time;
                }
                
                AudioManager.Instance.PlaySound(AudioSo.Sounds.RobotShoot, BulletSpawnPoint.position);
            }
        }

        public void TakeDamage(int damage)
        {
            health -= damage;
            if (health <= 0) DestroyEnemy();
        }

        private void DestroyEnemy()
        {
            if (GameManager.Instance) GameManager.Instance.EnemiesAlive -= 1;
            Instantiate(explosionParticle, transform.position, Quaternion.identity);
            AudioManager.Instance.PlaySound(AudioSo.Sounds.RobotExplosion, transform.position);
            Destroy(gameObject);
        }

        private Vector3 GetDirection()
        {
            Vector3 direction = (player.position - transform.position).normalized;

            if (AddBulletSpread)
            {
                direction += new Vector3(
                    Random.Range(-BulletSpreadVariance.x, BulletSpreadVariance.x),
                    Random.Range(-BulletSpreadVariance.y, BulletSpreadVariance.y),
                    Random.Range(-BulletSpreadVariance.z, BulletSpreadVariance.z)
                );

                direction.Normalize();
            }

            return direction + new Vector3(0, -0.1f, 0);
        }

        private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 hitPoint, Vector3 hitNormal, bool madeImpact)
        {
            Vector3 startPosition = trail.transform.position;
            float distance = Vector3.Distance(trail.transform.position, hitPoint);
            float remainingDistance = distance;

            while (remainingDistance > 0)
            {
                trail.transform.position = Vector3.Lerp(startPosition, hitPoint, 1 - (remainingDistance / distance));
                remainingDistance -= BulletSpeed * Time.deltaTime;
                yield return null;
            }

            trail.transform.position = hitPoint;
            Destroy(trail.gameObject, trail.time);
        }

        public void SetPlayer(Transform playerTransform) => player = playerTransform;

        public void SetGrabbed() => _isGrabbed = true;
    }
}