using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    public class Ragdoll : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private GameObject _ragdoll;
        [SerializeField] private GameObject _animatedModel;

        private Dictionary<string, Transform> _ragdollDict = new();
        private Dictionary<string, Transform> _animatedModelDict = new();

        private void OnEnable()
        {
            foreach (var t in _animatedModel.GetComponentsInChildren<Transform>())
            {
                _animatedModelDict.Add(t.name, t);
            }

            foreach (var t in _ragdoll.GetComponentsInChildren<Transform>())
            {
                _ragdollDict.Add(t.name, t);
            }
            _ragdollDict.Remove(_ragdoll.gameObject.name);
        }

        public void ProcessRagdoll(Rigidbody rigidbody, Vector3 forceValue, Vector3 forceApplyPosition, ForceMode forceMode)
        {
            CopyTransformData(_animatedModel.transform, _ragdoll.transform);
            _ragdoll.gameObject.SetActive(true);
            _animatedModel.gameObject.SetActive(false);

            // Применяем силу ко всем подходящим Rigidbody для более реалистичного эффекта
            Rigidbody[] ragdollRigidbodies = _ragdoll.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody bone in ragdollRigidbodies)
            {
                // Сила уменьшается с расстоянием от точки удара
                float distanceFactor = 1f / (1f + Vector3.Distance(bone.position, forceApplyPosition));
                bone.AddForceAtPosition(forceValue * distanceFactor, forceApplyPosition, forceMode);
            }
        }

        //private Rigidbody FindClosestBone(Vector3 position)
        //{
        //    Rigidbody[] ragdollRigidbodies = _ragdoll.GetComponentsInChildren<Rigidbody>();
        //    Rigidbody closestBone = null;
        //    float minDistance = Mathf.Infinity;

        //    foreach (Rigidbody bone in ragdollRigidbodies)
        //    {
        //        float distance = Vector3.Distance(bone.position, position);
        //        if (distance < minDistance)
        //        {
        //            minDistance = distance;
        //            closestBone = bone;
        //        }
        //    }

        //    return closestBone;
        //}

        private void CopyTransformData(Transform sourceTransform, Transform destinationTransform)
        {
            foreach (var t in _ragdollDict.Keys)
            {
                _ragdollDict[t].position = _animatedModelDict[t].position;
                _ragdollDict[t].rotation = _animatedModelDict[t].rotation;
            }
        }
    }
}