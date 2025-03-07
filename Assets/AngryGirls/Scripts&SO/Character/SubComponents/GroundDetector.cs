using UnityEngine;

namespace Angry_Girls
{
    public class GroundDetector : SubComponent
    {
        [Header("Setup")]
        [SerializeField] private float _collidingBlockDistance = 0.13f;

        [Header("Debug")]
        [ShowOnly] public Vector3 landingPosition = Vector3.zero;

        private int _ignoreLayerMask;

        public override void OnStart()
        {
            // Игнорируем слои "Projectile", "Pickable", "Bot" и другие, если необходимо
            _ignoreLayerMask = LayerMask.GetMask("Projectile", "Pickable");
        }

        public override void OnUpdate()
        {
            control.isGrounded = IsGrounded();
        }

        private bool IsGrounded()
        {
            // Проверка столкновений с BoxCollider
            if (control.boxColliderContacts != null)
            {
                foreach (var contact in control.boxColliderContacts)
                {
                    if (IsValidGroundContact(contact))
                    {
                        landingPosition = new Vector3(0f, contact.point.y, contact.point.z);
                        return true;
                    }
                }
            }

            // Проверка столкновений с нижними сферами
            if (control.rigidBody.velocity.y < 0.1f)
            {
                foreach (var bottomSphere in control.collisionSpheresData.bottomSpheres)
                {
                    if (IsValidGroundRaycast(bottomSphere.transform.position))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsValidGroundContact(ContactPoint contact)
        {
            // Игнорируем объекты с указанными слоями
            if (((1 << contact.otherCollider.gameObject.layer) & _ignoreLayerMask) != 0)
            {
                return false;
            }

            //ignore dead
            //var contactCharacter = contact.otherCollider.GetComponent<CControl>();
            //if (contactCharacter != null && contactCharacter.isDead)
            //{
            //    return false;
            //}

            //var colliderBottom = (control.rigidBody.position.y + control.boxCollider.bounds.min.y);
            var colliderBottom = (control.rigidBody.position.y + control.boxCollider.center.y) - (control.boxCollider.size.y / 2f);
            var yDifference = Mathf.Abs(contact.point.y - colliderBottom);

            return yDifference < _collidingBlockDistance && Mathf.Abs(control.rigidBody.velocity.y) < 0.001f;
        }

        private bool IsValidGroundRaycast(Vector3 rayOrigin)
        {
            Debug.DrawRay(rayOrigin, -Vector3.up * _collidingBlockDistance, Color.yellow);

            if (Physics.Raycast(rayOrigin, -Vector3.up, out var hit, _collidingBlockDistance, ~_ignoreLayerMask))
            {
                if (!IsOwnBodyPart(hit.collider))
                {
                    landingPosition = new Vector3(0f, hit.point.y, hit.point.z);
                    return true;
                }
            }

            return false;
        }

        private bool IsOwnBodyPart(Collider col)
        {
            return col.transform.root.gameObject == control.gameObject;
        }

        public override void OnAwake()
        {
        }

        public override void OnComponentEnable()
        {
        }

        public override void OnFixedUpdate()
        {
        }

        public override void OnLateUpdate()
        {
        }
    }
}