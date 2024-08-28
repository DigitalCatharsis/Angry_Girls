using System;
using UnityEngine;

namespace Angry_Girls
{
    public class PoolObjectLoader: MonoBehaviour
    {
        public GameObject InstantiatePrefab<T>(T objType, Vector3 position, Quaternion rotation)
        {
            var typelist = objType switch
            {
                CharacterType characterType => InstantiateCharacter(characterType, position, rotation),
                VFX_Type powerUpType => InstantiateVFX(powerUpType, position, rotation),
                DataType dataType => InstantiateProjectile(dataType, position, rotation),
                var unknownType => throw new Exception($"{unknownType?.GetType()}")
            };
            return typelist;
        }

        private GameObject InstantiateCharacter(CharacterType poolObjectType, Vector3 position, Quaternion rotation)
        {
            return GameLoader.Instance.spawnManager.CharacterFactory.SpawnGameobject(poolObjectType, position, rotation);
        }

        private GameObject InstantiateVFX(VFX_Type poolObjectType, Vector3 position, Quaternion rotation)
        {
            return GameLoader.Instance.spawnManager.vFXFactory.SpawnGameobject(poolObjectType, position, rotation);
        }
        private GameObject InstantiateProjectile(DataType poolObjectType, Vector3 position, Quaternion rotation)
        {
            return GameLoader.Instance.spawnManager.dataFactory.SpawnGameobject(poolObjectType, position, rotation);
        }
    }
} 