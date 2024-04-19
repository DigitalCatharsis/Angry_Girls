using UnityEngine;

namespace Angry_Girls
{
    public class CharacterControl : MonoBehaviour
    {
        [Header("Setup")]
        public Animator Animator;

        [Space (15)]
        public Rigidbody RigidBody;
        public BoxCollider Boxcollider;
        public bool IsLaunched;
        public SubComponentProcessor SubComponentProcessor;

        private void Awake()
        {
            RigidBody = GetComponent<Rigidbody>();
            Boxcollider = gameObject.GetComponent<BoxCollider>();
            SubComponentProcessor = GetComponentInChildren<SubComponentProcessor>();
        }

        private void LateUpdate()
        {
            if (IsLaunched)
            {
                CameraManager.Instance.CenterCameraAgainst(Boxcollider);
            }
        }
    }
}