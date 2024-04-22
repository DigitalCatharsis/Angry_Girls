using UnityEngine;

namespace Angry_Girls
{

    public class SubComponent : MonoBehaviour
    {
        protected CharacterControl Control;
        public void Awake()
        {
            Control = GetComponentInParent<CharacterControl>();
        }
    }
}
