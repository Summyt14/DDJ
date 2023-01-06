using UnityEngine;

namespace _Scripts.Player
{
    public class MoveCamera : MonoBehaviour
    {
        [SerializeField] private Transform cameraPosition;

        private void Update()
        {
            transform.position = cameraPosition.position;
        }
    }
}
