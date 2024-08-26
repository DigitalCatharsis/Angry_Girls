using UnityEngine;

namespace Angry_Girls
{
    public class SpawnManager : GameLoaderComponent
    {
        public CharacterFactory CharacterFactory;
        public VFXFactory vFXFactory;
        public DataFactory dataFactory;

        public override void OnComponentEnable()
        {
            CharacterFactory = gameObject.AddComponent(typeof(CharacterFactory)) as CharacterFactory;
            vFXFactory = gameObject.AddComponent(typeof(VFXFactory)) as VFXFactory;
            dataFactory = gameObject.AddComponent(typeof(DataFactory)) as DataFactory;
        }
        //private void Awake() 
        //{
        //    CharacterFactory = gameObject.AddComponent(typeof(CharacterFactory)) as CharacterFactory;
        //    vFXFactory = gameObject.AddComponent(typeof(VFXFactory)) as VFXFactory;
        //    dataFactory = gameObject.AddComponent(typeof(DataFactory)) as DataFactory;
        //}
    }
}


