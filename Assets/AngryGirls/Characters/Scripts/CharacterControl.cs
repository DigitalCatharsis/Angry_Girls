using UnityEngine;

namespace Angry_Girls
{
    public class CharacterControl : MonoBehaviour
    {
        [Header("Setup")]
        public Animator Animator;

        [Space (15)]
        [Header("Debug")]
        public Rigidbody RigidBody;
        public BoxCollider Boxcollider;        
        public SubComponentProcessor SubComponentProcessor;

        private void Awake()
        {
            RigidBody = GetComponent<Rigidbody>();
            Boxcollider = gameObject.GetComponent<BoxCollider>();
            SubComponentProcessor = GetComponentInChildren<SubComponentProcessor>();
        }
    }
}