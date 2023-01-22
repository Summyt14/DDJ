using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts.Player
{
    public class Void : MonoBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.transform.TryGetComponent(out PlayerMovement _))
                GameManager.Instance.PlayerHealth = 0;
        }
    }
}