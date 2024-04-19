using UnityEngine;

namespace Angry_Girls
{
    public class GroundDetector : SubComponent
    {
        [Header("Setup")]
        [SerializeField] private float _collidingBlockDistance;

        [Header("Debug")]
        [SerializeField] private GameObject _collidedObject;
        [SerializeField] private Vector3 _contactPoint;
        public bool IsAirboned;
        private void FixedUpdate()
        {
            _collidedObject = CollisionDetection.GetCollidingObject(Control, Control.gameObject, -Vector3.up, _collidingBlockDistance, ref _contactPoint);

            if (Mathf.Abs(Control.RigidBody.velocity.y) < 0.001f && _collidedObject != null)
            {
                IsAirboned = false;
            }
            else
            {
                IsAirboned = true;
            }
        }
    }
}