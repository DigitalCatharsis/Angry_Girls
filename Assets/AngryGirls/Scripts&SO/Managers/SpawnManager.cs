using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    public enum VFX_Type
    {
        VFX_Damage_White,
        VFX_Shouryken,
        VFX_FireBall,
        VFX_Flame,
    }
    public class SpawnManager : MonoBehaviour
    {
        public GameObject SpawnThing<T>(T thingType, Vector3 position, Quaternion rotation, GameObject parent = null) where T : Enum
        {
            var result = (Instantiate(Resources.Load(thingType.ToString()), position: position, rotation: rotation) as GameObject);
            if (parent != null)
            {
                result.transform.SetParent(parent.transform); ; 
            }
            return result;            
        }
    }
}

