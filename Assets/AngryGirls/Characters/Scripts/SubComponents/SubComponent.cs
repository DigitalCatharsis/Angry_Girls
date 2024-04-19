using UnityEngine;

namespace Angry_Girls
{

    public class SubComponent : MonoBehaviour
    {
        public CharacterControl control;
        public void Awake()
        {
            control = GetComponentInParent<CharacterControl>();
        }
    }
}
