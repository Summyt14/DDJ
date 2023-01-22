using System;
using _Scripts.Audio;
using UnityEngine;

namespace _Scripts.Player
{
    public class Grappling : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private LineRenderer lr;
        [SerializeField] private Transform grappleClaw;
        [SerializeField] private Transform[] grappleChildren;

        [SerializeField] private Transform gunTip;
        [SerializeField] private Transform cam;
        [SerializeField] private Transform player;
        [SerializeField] private LayerMask grappleMask;

        [Header("Grappling")] [SerializeField] private float maxGrappleDistance;
        [SerializeField] private float grappleDelayTime;
        [SerializeField] private float grappleTravelSpeed;
        [SerializeField] private float overshootYAxis;
        [SerializeField] private float speedToGrappleEnemy = 25f;

        private Vector3 _grapplePoint, _currentGrapplePosition;
        private bool _isGrappling, _hasFinishedGrapple = true;
        private PlayerMovement _pm;
        private Rigidbody _rb;
        
        private void Awake()
        {
            _pm = player.gameObject.GetComponent<PlayerMovement>();
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (InputHandler.instance.LeftClickBtn.WasPressedThisFrame()) StartGrapple();
        }

        private void StartGrapple()
        {
            if (!_hasFinishedGrapple) return;
            
            if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hitInfo, maxGrappleDistance, grappleMask))
            {
                if (_pm.Speed < speedToGrappleEnemy) return;
                if (!lr.enabled) lr.enabled = true;
                _isGrappling = true;
                _pm.IsFrozen = true;
                _hasFinishedGrapple = false;
                lr.positionCount = 2;

                _grapplePoint = hitInfo.point;
                _currentGrapplePosition = gunTip.position;
                
                grappleClaw.gameObject.layer = LayerMask.NameToLayer("Default");
                foreach (Transform child in grappleChildren)
                    child.gameObject.layer = LayerMask.NameToLayer("Default");
                
                if (hitInfo.transform.parent.parent.TryGetComponent(out Enemy.Enemy enemy))
                    enemy.SetGrabbed();
                
                AudioManager.Instance.PlaySound(AudioSo.Sounds.GrappleShoot, gunTip.position);
                Invoke(nameof(ExecuteGrapple), grappleDelayTime);
            }
        }

        private void ExecuteGrapple()
        {
            _pm.IsFrozen = false;
            Vector3 playerPosition = transform.position;
            Vector3 lowestPoint = new(playerPosition.x, playerPosition.y - 1f, playerPosition.z);

            float grapplePointRelativeYPos = _grapplePoint.y - lowestPoint.y;
            float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

            if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;
            _pm.JumpToPosition(_grapplePoint, highestPointOnArc);
            //Invoke(nameof(StopGrapple), 1f);
        }

        public void StopGrapple()
        {
            _isGrappling = false;
            _pm.IsFrozen = false;
            _hasFinishedGrapple = true;
            lr.positionCount = 0;
            
            grappleClaw.position = gunTip.position;
            grappleClaw.gameObject.layer = LayerMask.NameToLayer("Grapple");
            foreach (Transform child in grappleChildren)
                child.gameObject.layer = LayerMask.NameToLayer("Grapple");
        }

        private void LateUpdate()
        {
            if (!_isGrappling) return;
            _currentGrapplePosition =
                Vector3.Lerp(_currentGrapplePosition, _grapplePoint, Time.deltaTime * grappleTravelSpeed);
            grappleClaw.position = Vector3.Lerp(_currentGrapplePosition, _grapplePoint, Time.deltaTime * grappleTravelSpeed);

            lr.SetPosition(0, gunTip.position);
            lr.SetPosition(1, _currentGrapplePosition);
        }
    }
}