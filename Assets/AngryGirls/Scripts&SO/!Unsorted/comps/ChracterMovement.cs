using UnityEngine;

namespace Angry_Girls
{
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterMovement : MonoBehaviour
    {
        //repel
        [SerializeField] private float _repelValue = 0.4f;

        private float _currentRepelForce;
        private CharacterMovement _repelTarget;
        private bool _isRepelling;
        //


        private Rigidbody _rigidbody;
        private CControl _control;

        public Rigidbody Rigidbody => _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void Initialize(CControl control)
        {
            _control = control;
        }

        public void ResetVelocity()
        {
            _rigidbody.velocity = Vector3.zero;
        }

        public void ApplyRigidForce(Vector3 force, ForceMode forceMode = ForceMode.VelocityChange)
        {
            _rigidbody.AddForce(force, forceMode);
        }

        public void ApplyKnockbackFromEnemy(GameObject opponent, float knockbackForce)
        {
            if (_rigidbody.velocity.z != 0) return;

            var direction = _rigidbody.position - opponent.transform.position;
            direction.z = direction.z < 0 ? -1 : 1;
            _rigidbody.AddForce(new Vector3(0, 0, direction.z * knockbackForce), ForceMode.VelocityChange);
        }

        private void OnCollisionEnter(Collision collision)
        {
            GameLoader.Instance.interactionManager.HandleCollision(gameObject, collision);
        }
        private void OnTriggerEnter(Collider other)
        {
            GameLoader.Instance.interactionManager.HandleTrigger(gameObject, other);
        }

        private void OnCollisionStay(Collision collision)
        {
            GameLoader.Instance.interactionManager.HandleCollision(gameObject, collision);
        }

        public void SetPosition(Vector3 position)
        {
            _rigidbody.position = position;
        }

        public void SetRotation(Quaternion rotation)
        {
            _rigidbody.MoveRotation(rotation);
        }

        public void SetVelocity(Vector3 velocity)
        {
            _rigidbody.velocity = velocity;
        }

        public void Teleport(Vector3 position)
        {
            _rigidbody.MovePosition(position);
        }

        public void HandleRepel(CharacterMovement target)
        {
            float zDiff = Rigidbody.position.z - target.Rigidbody.position.z;
            Vector3 repelDirection = zDiff switch
            {
                > 0 => Vector3.forward,
                < 0 => Vector3.back,
                _ => UnityEngine.Random.Range(0, 2) == 0 ? Vector3.back : Vector3.forward
            };

            Teleport(Rigidbody.position + repelDirection * (_repelValue * Time.fixedDeltaTime));
        }
    }
}