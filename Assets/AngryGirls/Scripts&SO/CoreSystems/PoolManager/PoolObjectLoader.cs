using System;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Loads and instantiates pool objects from factories
    /// </summary>
    public class PoolObjectLoader : MonoBehaviour
    {
        private CharacterFactory _characterFactory;
        private VFXFactory _vFXFactory;
        private UIObjectsFactory _uIObjectsFactory;

        private void Awake()
        {
            _characterFactory = gameObject.AddComponent(typeof(CharacterFactory)) as CharacterFactory;
            _vFXFactory = gameObject.AddComponent(typeof(VFXFactory)) as VFXFactory;
            _uIObjectsFactory = gameObject.AddComponent(typeof(UIObjectsFactory)) as UIObjectsFactory;
        }

        /// <summary>
        /// Instantiates prefab based on object type
        /// </summary>
        public PoolObject InstantiatePrefab<T>(T objType, Vector3 position, Quaternion rotation)
        {
            return objType switch
            {
                CharacterType characterType => InstantiateCharacter(characterType, position, rotation),
                VFX_Type powerUpType => InstantiateVFX(powerUpType, position, rotation),
                UIObjectType uIObjectType => InstantiateUIObject(uIObjectType, position, rotation),
                _ => throw new Exception($"{objType?.GetType()}")
            };
        }

        private PoolObject InstantiateCharacter(CharacterType poolObjectType, Vector3 position, Quaternion rotation)
        {
            return _characterFactory.SpawnGameobject(poolObjectType, position, rotation).GetComponent<PoolObject>();
        }

        private PoolObject InstantiateVFX(VFX_Type poolObjectType, Vector3 position, Quaternion rotation)
        {
            return _vFXFactory.SpawnGameobject(poolObjectType, position, rotation).GetComponent<PoolObject>();
        }

        private PoolObject InstantiateUIObject(UIObjectType uIObjectType, Vector3 position, Quaternion rotation)
        {
            return _uIObjectsFactory.SpawnGameobject(uIObjectType, position, rotation).GetComponent<PoolObject>();
        }
    }
}