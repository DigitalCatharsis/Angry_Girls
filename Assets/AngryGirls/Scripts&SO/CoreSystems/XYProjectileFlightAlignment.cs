using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Keeps a projectile's forward axis aligned with its actual XY velocity.
    /// Useful for ballistic projectiles whose direction changes under gravity.
    /// </summary>
    public sealed class XYProjectileFlightAlignment : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        private Vector3 _fallbackDirection;

        public void Begin(Rigidbody rigidbody, Vector3 initialDirection)
        {
            _rigidbody = rigidbody;
            _fallbackDirection = initialDirection;
            Align(_fallbackDirection);
        }

        public void Stop()
        {
            _rigidbody = null;
            _fallbackDirection = Vector3.zero;
        }

        private void LateUpdate()
        {
            if (_rigidbody == null)
                return;

            Vector3 velocity = _rigidbody.linearVelocity;
            velocity.z = 0f;

            if (velocity.sqrMagnitude > 0.0001f)
            {
                _fallbackDirection = velocity.normalized;
                Align(_fallbackDirection);
            }
            else if (_fallbackDirection.sqrMagnitude > 0.0001f)
            {
                Align(_fallbackDirection);
            }
        }

        private void OnDisable()
        {
            // Pooled projectiles must not keep tracking a Rigidbody after deactivation.
            _rigidbody = null;
        }

        private void Align(Vector3 direction)
        {
            direction.z = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
                return;

            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.forward);
        }
    }
}
