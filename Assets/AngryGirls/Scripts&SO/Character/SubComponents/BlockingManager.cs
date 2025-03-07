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

        private int _ignoreLayerMask;

        public override void OnStart()
        {
            _ignoreLayerMask = LayerMask.GetMask("Projectile", "Pickable");
        }

        public override void OnFixedUpdate()
        {
            DefineFrontSpheres();
            CheckFrontBlocking();
        }

        private bool IsForwardReversed()
        {
            return control.gameObject.transform.forward.z < 0;
        }

        private void DefineFrontSpheres()
        {
            _frontSpheresArray = IsForwardReversed() ? control.collisionSpheresData.backSpheres : control.collisionSpheresData.frontSpheres;
        }

        private void CheckFrontBlocking()
        {
            foreach (var sphere in _frontSpheresArray)
            {
                if (Physics.Raycast(sphere.transform.position, transform.forward, out var hit, _frontBlocking_Distance, ~_ignoreLayerMask))
                {
                    if (!IsOwnBodyPart(hit.collider))
                    {
                        AddBlockingObjToDictionary(_frontBlockingObjects_dictionary, sphere, hit.collider.gameObject);
                        continue;
                    }
                }

                RemoveBlockingObjFromDictionary(_frontBlockingObjects_dictionary, sphere);
            }
        }

        private void AddBlockingObjToDictionary(SerializedDictionary<GameObject, GameObject> dic, GameObject key, GameObject value)
        {
            dic[key] = value;
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
            return IsForwardReversed() ? IsSideBlocked(true) : IsSideBlocked(false);
        }

        private bool IsSideBlocked(bool isLeftSide)
        {
            foreach (var data in _frontBlockingObjects_dictionary)
            {
                var zDifference = (data.Value.transform.position - control.rigidBody.position).z;
                if (isLeftSide ? zDifference < 0f : zDifference > 0f)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsOwnBodyPart(Collider col)
        {
            return col.transform.root.gameObject == control.gameObject;
        }

        public override void OnAwake()
        {
        }

        public override void OnUpdate()
        {
        }

        public override void OnComponentEnable()
        {
        }

        public override void OnLateUpdate()
        {
        }
    }
}