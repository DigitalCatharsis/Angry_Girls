using UnityEngine;

namespace Angry_Girls
{
    public class CharacterControl : MonoBehaviour
    {
        public Rigidbody rigidBody;
        public BoxCollider boxcollider;
        public bool isLaunched;

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
            boxcollider = gameObject.GetComponent<BoxCollider>();
        }

        private void LateUpdate()
        {
            if (isLaunched)
            {
                CameraManager.Instance.CenterCameraAgainst(boxcollider);
            }
        }
    }
}