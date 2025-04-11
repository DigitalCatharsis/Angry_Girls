using System;
using UnityEngine;

namespace Angry_Girls
{
    public class PoolObjectLoader : MonoBehaviour
    {
        private CharacterFactory _characterFactory;
        private VFXFactory _vFXFactory;

        private void Awake()
        {
            _characterFactory = gameObject.AddComponent(typeof(CharacterFactory)) as CharacterFactory;
            _vFXFactory = gameObject.AddComponent(typeof(VFXFactory)) as VFXFactory;
        }

        public PoolObject InstantiatePrefab<T>(T objType, Vector3 position, Quaternion rotation)
        {
            return objType switch
            {
                CharacterType characterType => InstantiateCharacter(characterType, position, rotation),
                VFX_Type powerUpType => InstantiateVFX(powerUpType, position, rotation),
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
    }
}