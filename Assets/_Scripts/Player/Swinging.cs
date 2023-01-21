using System;
using System.Collections;
using _Scripts.Audio;
using UnityEngine;

namespace _Scripts.Player
{
    public class Swinging : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private LineRenderer lr;
        [SerializeField] private Transform grappleClaw;
        [SerializeField] private Transform[] grappleChildren;

        [SerializeField] private Transform gunTip;
        [SerializeField] private Transform cam;
        [SerializeField] private Transform player;
        [SerializeField] private LayerMask grappleMask;

        [Header("Swinging")] [SerializeField] private float maxSwingDistance = 25f;
        [SerializeField] private float jointSpring = 4.5f;
        [SerializeField] private float jointDamper = 7f;
        [SerializeField] private float jointMassScale = 4.5f;
        [SerializeField] private float jointMinDistance = 0.25f;
        [SerializeField] private float jointMaxDistance = 0.8f;
        [SerializeField] private float grappleTravelSpeed = 10f;
        [SerializeField] private float thresholdAngle;
        [SerializeField] private float swingAcceleration;
        
        [Header("OdmGear")]
        [SerializeField] private float horizontalThrustForce;
        [SerializeField] private float forwardThrustForce;
        [SerializeField] private float extendCableSpeed;

        private Vector3 _swingPoint, _currentGrapplePosition, _velocity;
        private SpringJoint _joint;
        private PlayerMovement _pm;
        private Rigidbody _rb;

        private void Awake()
        {
            _pm = player.gameObject.GetComponent<PlayerMovement>();
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (_pm.State is PlayerMovement.MovementState.Freeze or PlayerMovement.MovementState.Grappling)
            {
                if (_joint) Destroy(_joint);
                _pm.IsSwinging = false;
                return;
            }
            
            if (InputHandler.instance.LeftClickBtn.WasPressedThisFrame()) StartSwing();
            if (InputHandler.instance.LeftClickBtn.WasReleasedThisFrame()) StopSwing();
            
            if (_joint != null) OdmGearMovement();
        }

        private void StartSwing()
        {
            if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hitInfo, maxSwingDistance, grappleMask))
            {
                if (!lr.enabled) lr.enabled = true;

                _pm.IsSwinging = true;
                _swingPoint = hitInfo.point;
                _joint = player.gameObject.AddComponent<SpringJoint>();
                _joint.autoConfigureConnectedAnchor = false;
                _joint.connectedAnchor = _swingPoint;

                float distanceFromPoint = Vector3.Distance(player.position, _swingPoint);

                // the distance grapple will try to keep from grapple point
                _joint.maxDistance = distanceFromPoint * jointMaxDistance;
                _joint.minDistance = distanceFromPoint * jointMinDistance;
                _joint.spring = jointSpring;
                _joint.damper = jointDamper;
                _joint.massScale = jointMassScale;

                lr.positionCount = 2;
                _currentGrapplePosition = gunTip.position;
                grappleClaw.gameObject.layer = LayerMask.NameToLayer("Default");
                foreach (Transform child in grappleChildren)
                    child.gameObject.layer = LayerMask.NameToLayer("Default");
                
                AudioManager.Instance.PlaySound(AudioSo.Sounds.GrappleShoot, gunTip.position);
            }
        }

        private void StopSwing()
        {
            lr.positionCount = 0;
            Destroy(_joint);
            _pm.IsSwinging = false;
            
            grappleClaw.position = gunTip.position;
            grappleClaw.gameObject.layer = LayerMask.NameToLayer("Grapple");
            foreach (Transform child in grappleChildren)
                child.gameObject.layer = LayerMask.NameToLayer("Grapple");
        }

        private void OdmGearMovement()
        {
            switch (InputHandler.instance.MoveVector.x)
            {
                case > 0: // right
                    _rb.AddForce(transform.right * (horizontalThrustForce * Time.deltaTime), ForceMode.Acceleration);
                    break;
                case < 0: // left
                    _rb.AddForce(-transform.right * (horizontalThrustForce * Time.deltaTime), ForceMode.Acceleration);
                    break;
            }

            switch (InputHandler.instance.MoveVector.y)
            {
                case > 0: // forward
                    _rb.AddForce(player.forward * (horizontalThrustForce * Time.deltaTime), ForceMode.Acceleration);
                    break;
                case < 0: // extend cable
                {
                    float extendedDistanceFromPoint = Vector3.Distance(transform.position, _swingPoint) + extendCableSpeed;

                    _joint.maxDistance = extendedDistanceFromPoint * jointMaxDistance;
                    _joint.minDistance = extendedDistanceFromPoint * jointMinDistance;
                    break;
                }
            }

            // shorten cable
            if (InputHandler.instance.JumpBtn.IsPressed())
            {
                Vector3 directionToPoint = _swingPoint - transform.position;
                _rb.AddForce(directionToPoint.normalized * (forwardThrustForce * Time.deltaTime), ForceMode.Acceleration);

                float distanceFromPoint = Vector3.Distance(transform.position, _swingPoint);

                _joint.maxDistance = distanceFromPoint * jointMaxDistance;
                _joint.minDistance = distanceFromPoint * jointMinDistance;
            }
        }

        private void LateUpdate()
        {
            if (!_joint) return;

            _currentGrapplePosition =
                Vector3.Lerp(_currentGrapplePosition, _swingPoint, Time.deltaTime * grappleTravelSpeed);
            grappleClaw.position = Vector3.Lerp(_currentGrapplePosition, _swingPoint, Time.deltaTime * grappleTravelSpeed);

            lr.SetPosition(0, gunTip.position);
            lr.SetPosition(1, _currentGrapplePosition);
        }
    }
}