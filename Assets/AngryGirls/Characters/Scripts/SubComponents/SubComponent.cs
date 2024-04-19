using UnityEngine;

namespace Angry_Girls
{

    public class SubComponent : MonoBehaviour
    {
        public CharacterControl Control;
        public void Awake()
        {
            Control = GetComponentInParent<CharacterControl>();
        }
    }
}
