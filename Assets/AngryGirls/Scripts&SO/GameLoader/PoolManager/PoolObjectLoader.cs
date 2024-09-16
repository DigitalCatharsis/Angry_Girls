using System;
using UnityEngine;

namespace Angry_Girls
{
    public class PoolObjectLoader: MonoBehaviour
    {
        public PoolObject InstantiatePrefab<T>(T objType, Vector3 position, Quaternion rotation)
        {
            var typelist = objType switch
            {
                CharacterType characterType => InstantiateCharacter(characterType, position, rotation),
                VFX_Type powerUpType => InstantiateVFX(powerUpType, position, rotation),
                var unknownType => throw new Exception($"{unknownType?.GetType()}")
            };
            return typelist;
        }

        private PoolObject InstantiateCharacter(CharacterType poolObjectType, Vector3 position, Quaternion rotation)
        {
            return GameLoader.Instance.spawnManager.CharacterFactory.SpawnGameobject(poolObjectType, position, rotation).GetComponent<PoolObject>();
        }

        private PoolObject InstantiateVFX(VFX_Type poolObjectType, Vector3 position, Quaternion rotation)
        {
            return GameLoader.Instance.spawnManager.vFXFactory.SpawnGameobject(poolObjectType, position, rotation).GetComponent<PoolObject>();
        }
    }
} 