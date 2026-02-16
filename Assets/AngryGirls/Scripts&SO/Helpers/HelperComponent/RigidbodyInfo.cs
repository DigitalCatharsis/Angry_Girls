using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Displays Rigidbody information in the Inspector
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyInfo : MonoBehaviour
    {
        private Rigidbody _rb;

        [Header("Rigidbody Information")]
        [ShowOnly][SerializeField] private string _rigidbodyWorldPosition;
        [ShowOnly][SerializeField] private string _speed;
        [ShowOnly][SerializeField] private string _velocity;
        [ShowOnly][SerializeField] private string _angularVelocity;
        [ShowOnly][SerializeField] private string _initialTensorRotation;
        [ShowOnly][SerializeField] private string _localCenterOfMass;
        [ShowOnly][SerializeField] private string _worldCenterOfMass;
        [ShowOnly][SerializeField] private string _sleepState;

        [Header("Gizmos Settings")]
        [SerializeField] private bool _showCenterOfMass = false;

        private void OnEnable()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            UpdateRigidbodyInfo();
        }

        private void UpdateRigidbodyInfo()
        {
            _rigidbodyWorldPosition = _rb.position.ToString();
            _speed = _rb.velocity.magnitude.ToString("F2");
            _velocity = _rb.velocity.ToString();
            _angularVelocity = _rb.angularVelocity.ToString();
            _initialTensorRotation = _rb.inertiaTensorRotation.ToString();
            _localCenterOfMass = _rb.centerOfMass.ToString();
            _worldCenterOfMass = _rb.worldCenterOfMass.ToString();
            _sleepState = _rb.IsSleeping().ToString();
        }

        private void OnDrawGizmos()
        {
            if (_showCenterOfMass && _rb != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(_rb.worldCenterOfMass, 0.1f);
            }
        }
    }
}