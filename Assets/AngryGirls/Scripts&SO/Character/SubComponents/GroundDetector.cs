using TestNameSpace;
using UnityEngine;
using UnityEngine.UIElements;

namespace Angry_Girls
{
    public class GroundDetector : SubComponent<SubcomponentMediator_EventNames>
    {
        [Header("Setup")]
        [SerializeField] private float _collidingBlockDistance;

        [Header("Debug")]
        //[SerializeField] private GameObject _ground;
        [ShowOnly] public Vector3 landingPosition = Vector3.zero;
        public Vector3 bottomRaycastContactPoint;

        public override void OnComponentEnable()
        {
        }
        public override void OnUpdate()
        {
            control.isGrounded = IsGrounded();
        }



        private bool IsGrounded()
        {
            //Если что-то коллайдит главный BoxCollider
            if (control.boxColliderContacts != null)
            {
                foreach (var contact in control.boxColliderContacts)
                {
                    var contactCharacter = contact.otherCollider.GetComponent<CControl>();
                    if (contactCharacter != null && contactCharacter.playerOrAi != control.playerOrAi)
                    {
                        if (contactCharacter.isDead)
                        {
                            continue;
                        }
                        //prevent from stuck above enemy
                        if (control.rigidBody.velocity == Vector3.zero )
                        {
                            control.JostleFromEnemy(2);
                        }

                        return false;
                    }



                    var colliderBottom = (control.transform.position.y + control.boxCollider.center.y) - (control.boxCollider.size.y / 2f);
                    var yDiffirence = Mathf.Abs(contact.point.y - colliderBottom);

                    if (yDiffirence < 0.13f)
                    {
                        if (Mathf.Abs(control.rigidBody.velocity.y) < 0.001f)
                        {
                            //_ground = contact.otherCollider.transform.gameObject;
                            landingPosition = new Vector3(0f, contact.point.y, contact.point.z);
                            return true;
                        }
                    }
                }
            }

            //Если падаем
            if (control.rigidBody.velocity.y < 0.1f)
            {
                foreach (var bottomSphere in control.collisionSpheresData.bottomSpheres)
                {
                    var blockingObj = CollisionDetection.GetCollidingObject
                        (control, bottomSphere.transform.position, -Vector3.up, _collidingBlockDistance, ref bottomRaycastContactPoint);

                    if (blockingObj == null)
                    {
                        return false;
                    }


                    var contactCharacter = blockingObj.GetComponent<CControl>();

                    if (contactCharacter != null && contactCharacter.playerOrAi != control.playerOrAi)
                    {
                        if (contactCharacter.isDead)
                        {
                            continue;
                        }

                        control.JostleFromEnemy(2);
                        return false;
                    }

                    landingPosition = new Vector3(
                        0f,
                        bottomRaycastContactPoint.y,
                        bottomRaycastContactPoint.z);

                    return true;
                }
            }
            //_ground = null;
            return false;
        }
        public override void OnFixedUpdate()
        {
        }

        public override void OnStart()
        {
        }
        public override void OnAwake()
        {
        }

        public override void OnLateUpdate()
        {
        }
    }
}