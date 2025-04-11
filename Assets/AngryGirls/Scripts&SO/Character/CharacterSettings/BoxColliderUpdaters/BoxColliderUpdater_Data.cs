using AYellowpaper.SerializedCollections;
using System;
using UnityEngine;

namespace Angry_Girls
{
    [CreateAssetMenu(fileName = "BoxColliderUpdater_Data", menuName = "Angry_Girls/CharacterSettings/BoxColliderUpdater_Data")]
    public class BoxColliderUpdater_Container : ScriptableObject
    {
        //Key = Center, Value = Size
        public SerializedDictionary<StateNames, BoxColliderUpdater_Container_Data> boxColliderAnimationData;
    }

    [Serializable]
    public class BoxColliderUpdater_Container_Data
    {
        public Vector3 boxColliderCenter;
        [Space(5)]        
        public Vector3 boxColliderSize;

        [Space(10)]
        public float changeSpeed = 1f;
        //public AnimationCurve changeSpeed;
    }
}