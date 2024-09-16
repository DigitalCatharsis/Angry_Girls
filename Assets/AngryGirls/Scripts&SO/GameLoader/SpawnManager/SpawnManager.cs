using UnityEngine;

namespace Angry_Girls
{
    public class SpawnManager : MonoBehaviour
    {
        public CharacterFactory CharacterFactory;
        public VFXFactory vFXFactory;

        private void Awake()
        {
            CharacterFactory = gameObject.AddComponent(typeof(CharacterFactory)) as CharacterFactory;
            vFXFactory = gameObject.AddComponent(typeof(VFXFactory)) as VFXFactory;
        }
    }
}


