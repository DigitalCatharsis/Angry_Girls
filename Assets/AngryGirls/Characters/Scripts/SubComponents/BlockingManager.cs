using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    public class BlockingManager : SubComponent
    {
        [Header("Setup")]
        [SerializeField] private float _frontBlocking_Distance = 0.125f;
        [Header("Debug")]
        [SerializeField] private SerializedDictionary<GameObject, GameObject> _frontBlockingObjects_dictionary = new(); //key refers to the sphere where the raycast is coming from, and value is the actual gameobject being hit
        [SerializeField] private Vector3 raycastContactPoint;
        [SerializeField] private GameObject[] _frontSpheresArray;


        public override void OnComponentEnable()
        {
            control.subComponentProcessor.blockingManager = this;
        }

        private bool IsForwardReversed()
        {
            if (control.gameObject.transform.forward.z < 0)
            {
                return true;
            }
            if (control.gameObject.transform.forward.z > 0)
            {
                return false;
            }

            return false;
        }

        public override void OnFixedUpdate()
        {
            DefineFrontSpheres();
            CheckFrontBlocking();
        }

        private void DefineFrontSpheres()
        {
            if (IsForwardReversed() == true)
            {
                _frontSpheresArray = control.subComponentProcessor.collisionSpheres.backSpheres;
            }
            else
            {
                _frontSpheresArray = control.subComponentProcessor.collisionSpheres.frontSpheres;
            }
        }

        private void CheckFrontBlocking()
        {
            for (var i = 0; i < _frontSpheresArray.Length; i++)
            {
                var blockingObj = CollisionDetection.GetCollidingObject(
                    control: control,
                    start: _frontSpheresArray[i].transform.position,
                    dir: transform.forward * _frontBlocking_Distance,
                    blockDistance: _frontBlocking_Distance,
                    collisionPoint: ref raycastContactPoint);  //25 is just for visual ray

                if (blockingObj != null)
                {
                    AddBlockingObjToDictionary(_frontBlockingObjects_dictionary, _frontSpheresArray[i], blockingObj);
                }
                else
                {
                    RemoveBlockingObjFromDictionary(_frontBlockingObjects_dictionary, _frontSpheresArray[i]);
                }
            }
        }
        private void AddBlockingObjToDictionary(SerializedDictionary<GameObject, GameObject> dic, GameObject key, GameObject value)
        {
            if (dic.ContainsKey(key))
            {
                dic[key] = value;
            }
            else
            {
                dic.Add(key, value);
            }
        }

        private void RemoveBlockingObjFromDictionary(SerializedDictionary<GameObject, GameObject> dic, GameObject key)
        {
            if (dic.ContainsKey(key))
            {
                dic.Remove(key);
            }
        }

        public bool IsFrontBlocked()
        {
            if (IsForwardReversed() == false)
            {
                return IsRightSideBlocked();
            }
            else
            {
                return IsLeftSideBlocked();
            }
        }

        private bool IsRightSideBlocked()
        {
            foreach (KeyValuePair<GameObject, GameObject> data in _frontBlockingObjects_dictionary)
            {
                if ((data.Value.transform.position - control.transform.position).z > 0f)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsLeftSideBlocked()
        {
            foreach (KeyValuePair<GameObject, GameObject> data in _frontBlockingObjects_dictionary)
            {
                if ((data.Value.transform.position - control.transform.position).z < 0f)
                {
                    return true;
                }
            }
            return false;
        }

        public override void OnAwake()
        {

        }

        public override void OnLateUpdate()
        {
        }

        public override void OnStart()
        {
        }

        public override void OnUpdate()
        {
        }
    }
}