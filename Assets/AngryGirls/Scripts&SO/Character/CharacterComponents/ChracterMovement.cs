using UnityEngine;

namespace Angry_Girls
{
    [RequireComponent(typeof(Rigidbody), typeof(CControl))]
    public class CharacterMovement : MonoBehaviour
    {
        [SerializeField] private float _repelValue = 0.4f;

        public bool IsGrounded => CheckIfGrounded();
        private CControl _control;
        private Rigidbody _rigidbody;
        private BoxCollider _boxCollider;
        public Rigidbody Rigidbody => _rigidbody;

        private LayerMask _ignoreLayerMask; // Layers ignored for ground detection
        private LayerMask _enemyLayerMask; // Layers for enemy detection

        [Header("Multi-Collision Grounded Settings")]
        [SerializeField] private float _multiCollisionTimeThreshold = 4f; // Time threshold for multi-collision grounded
        private float _multiCollisionTimer; // Timer for multi-collision detection

        [Header("Coyote Time")]
        [Tooltip("The time the character is considered grounded after leaving the ground")]
        [SerializeField] private float _coyoteTime = 0.1f;
        private float _coyoteTimer;
        private bool _coyoteGroundedState;

        private void FixedUpdate()
        {
            UpdateGroundedStateWithCoyoteTime();
        }

        private void UpdateGroundedStateWithCoyoteTime()
        {
            bool physicalGrounded = CheckIfGrounded();
            bool wasCoyoteGrounded = _coyoteGroundedState;

            if (physicalGrounded)
            {
                _coyoteTimer = _coyoteTime;
                _coyoteGroundedState = true;
            }
            else
            {
                _coyoteTimer -= Time.fixedDeltaTime;
                if (_coyoteTimer <= 0)
                {
                    _coyoteGroundedState = false;
                }
            }

            if (wasCoyoteGrounded != _coyoteGroundedState)
            {
                NotifyGroundedStateChanged();
            }
        }

        private void NotifyGroundedStateChanged()
        {
            if (_coyoteGroundedState)
                _control.UnitHasPerformedLanding?.Invoke();
            else
                _control.UnitIsAirboned?.Invoke();
        }

        private void Awake()
        {
            _control = GetComponent<CControl>();
            _boxCollider = GetComponent<BoxCollider>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            // Initialize layer masks
            _ignoreLayerMask = LayerMask.GetMask("NoninitializedProjectile", "Pickable", "CharacterToLaunch", "Character", "Bot", "DeadBody", "Projectile_Bot", "Projectile_Character");
            _enemyLayerMask = LayerMask.GetMask("Bot");
        }

        public void ResetVelocity()
        {
            _rigidbody.velocity = Vector3.zero;
        }

        public void ApplyRigidForce(Vector3 force, ForceMode forceMode = ForceMode.VelocityChange)
        {
            _rigidbody.AddForce(force, forceMode);
        }

        /// <summary>
        /// Applies knockback force from enemy collision
        /// </summary>
        public void ApplyKnockbackFromEnemy(GameObject opponent, float knockbackForce)
        {
            if (_rigidbody.velocity.z != 0) return;

            var direction = _rigidbody.position - opponent.transform.position;
            direction.z = direction.z < 0 ? -1 : 1;
            _rigidbody.AddForce(new Vector3(0, 0, direction.z * knockbackForce), ForceMode.VelocityChange);
        }

        private void OnCollisionEnter(Collision collision)
        {
            GameplayCoreManager.Instance.InteractionManager.HandleCollision(gameObject, collision);
        }

        private void OnTriggerEnter(Collider other)
        {
            GameplayCoreManager.Instance.InteractionManager.HandleTrigger(gameObject, other);
        }

        private void OnCollisionStay(Collision collision)
        {
            GameplayCoreManager.Instance.InteractionManager.HandleCollision(gameObject, collision);
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

        /// <summary>
        /// Handles repulsion between characters when they overlap
        /// </summary>
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

        /// <summary>
        /// Checks if character is grounded using BoxCast with special multi-collision logic for enemies
        /// </summary>
        /// <returns>True if grounded</returns>
        private bool CheckIfGrounded()
        {
            if (_boxCollider == null || !_boxCollider.enabled) return false;

            // Calculate bottom point of collider
            var bottomPoint = new Vector3(_boxCollider.bounds.center.x, _boxCollider.bounds.min.y, _boxCollider.bounds.center.z);

            // BoxCast center (0.13 units above bottom)
            var boxCenter = bottomPoint + Vector3.up * 0.13f;

            // BoxCast size (slightly larger than collider)
            var boxSize = new Vector3(_boxCollider.size.x, 0.1f, _boxCollider.size.z);
            float rayLength = 0.13f;


            // Special logic: If player is standing on multiple enemies, consider grounded
            if (_control.playerOrAi == PlayerOrAi.Player)
            {
                var enemyHits = Physics.BoxCastAll(boxCenter, boxSize / 2, Vector3.down, Quaternion.identity, rayLength, _enemyLayerMask);

                if (enemyHits.Length >= 1 && _control.playerOrAi != PlayerOrAi.Bot)
                {
                    _multiCollisionTimer += Time.fixedDeltaTime;

                    // If timer exceeds threshold, consider grounded on enemies
                    if (_multiCollisionTimer >= _multiCollisionTimeThreshold)
                    {
                        _control.detectedGroundObject.Clear();
                        foreach (var enemy in enemyHits)
                        {
                            _control.detectedGroundObject.Add(enemy.collider.gameObject);
                        }
                        return true;
                    }
                }
                else
                {
                    // Reset timer if not colliding with enemies
                    _multiCollisionTimer = 0f;
                }
            }
            

            // Normal ground detection
            RaycastHit hit;
            var isGrounded = Physics.BoxCast(boxCenter, boxSize / 2, Vector3.down, out hit, Quaternion.identity, rayLength, ~_ignoreLayerMask);

            if (hit.collider != null && hit.collider.transform.root != gameObject.transform.root)
            {
                _control.detectedGroundObject.Clear();
                _control.detectedGroundObject.Add(hit.collider.gameObject);
            }

            return isGrounded;
        }

        // Debug visualization for ground detection
        private void OnDrawGizmos()
        {
            if (_boxCollider == null || !_boxCollider.enabled) return;

            var bottomPoint = new Vector3(_boxCollider.bounds.center.x, _boxCollider.bounds.min.y, _boxCollider.bounds.center.z);
            var boxCenter = bottomPoint + Vector3.up * 0.13f;
            var boxSize = new Vector3(_boxCollider.size.x, 0.1f, _boxCollider.size.z);

            Gizmos.color = CheckIfGrounded() ? Color.green : Color.red;
            Gizmos.DrawWireCube(boxCenter, boxSize);

            RaycastHit hit;
            if (Physics.BoxCast(boxCenter, boxSize / 2, Vector3.down, out hit, Quaternion.identity, 0.13f, ~_ignoreLayerMask))
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(hit.point, hit.point + hit.normal);
                Gizmos.DrawSphere(hit.point, 0.1f);
            }
        }
    }
}