using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Manages ragdoll physics for characters, handling transition from animated to physical simulation
    /// </summary>
    public class Ragdoll : MonoBehaviour
    {
        [SerializeField] private GameObject _ragdoll;
        [SerializeField] private GameObject _animatedModel;

        private Dictionary<string, Transform> _ragdollDict = new();
        private Dictionary<string, Transform> _animatedModelDict = new();

        private Dictionary<GameObject, GameObject> _extraObjectClones = new();

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

            CopyMaterialsFromModelToRagdoll();
            CopyExtraObjects();
        }

        /// <summary>
        /// Activates ragdoll physics with applied force at specific position
        /// </summary>
        /// <param name="rigidbody">Reference rigidbody (unused, kept for compatibility)</param>
        /// <param name="forceValue">Force vector to apply</param>
        /// <param name="forceApplyPosition">World position to apply force</param>
        /// <param name="forceMode">Physics force mode</param>
        public void ProcessRagdoll(Rigidbody rigidbody, Vector3 forceValue, Vector3 forceApplyPosition, ForceMode forceMode)
        {
            CopyTransformData(_animatedModel.transform, _ragdoll.transform);
            _ragdoll.gameObject.SetActive(true);
            _animatedModel.gameObject.SetActive(false);

            foreach (var extraObj in _extraObjectClones.Values)
            {
                if (extraObj.TryGetComponent(out IExtraCharacterObject handler))
                {
                    handler.OnRagdollEnabled();
                }
            }

            Rigidbody[] ragdollRigidbodies = _ragdoll.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody bone in ragdollRigidbodies)
            {
                float distanceFactor = 1f / (1f + Vector3.Distance(bone.position, forceApplyPosition));
                bone.AddForceAtPosition(forceValue * distanceFactor, forceApplyPosition, forceMode);
            }
        }

        private void CopyMaterialsFromModelToRagdoll()
        {
            Renderer[] modelRenderers = _animatedModel.GetComponentsInChildren<Renderer>();
            Renderer[] ragdollRenderers = _ragdoll.GetComponentsInChildren<Renderer>();

            Dictionary<string, Renderer> ragdollRendererDict = new();
            foreach (var renderer in ragdollRenderers)
            {
                ragdollRendererDict[renderer.gameObject.name] = renderer;
            }

            foreach (var modelRenderer in modelRenderers)
            {
                string name = modelRenderer.gameObject.name;
                if (ragdollRendererDict.TryGetValue(name, out Renderer ragdollRenderer))
                {
                    ragdollRenderer.material = modelRenderer.material;
                }
            }
        }

        private void CopyTransformData(Transform sourceTransform, Transform destinationTransform)
        {
            foreach (var t in _ragdollDict.Keys)
            {
                if (_ragdollDict[t] == null) continue;
                if (!_animatedModelDict.ContainsKey(t)) continue;

                _ragdollDict[t].position = _animatedModelDict[t].position;
                _ragdollDict[t].rotation = _animatedModelDict[t].rotation;
            }

            foreach (var pair in _extraObjectClones)
            {
                GameObject original = pair.Key;
                GameObject clone = pair.Value;

                if (clone != null)
                {
                    clone.transform.position = original.transform.position;
                    clone.transform.rotation = original.transform.rotation;
                }
            }
        }

        private void CopyExtraObjects()
        {
            foreach (var clone in _extraObjectClones.Values)
            {
                if (clone != null)
                {
                    Destroy(clone);
                }
            }
            _extraObjectClones.Clear();

            IExtraCharacterObject[] extraObjects = _animatedModel.GetComponentsInChildren<IExtraCharacterObject>();

            foreach (var extraObj in extraObjects)
            {
                GameObject original = ((Component)extraObj).gameObject;

                string originalPath = GetGameObjectPath(original);
                string ragdollPath = originalPath.Replace("/" + _animatedModel.name, "/" + _ragdoll.name);

                Transform parentInRagdoll = FindOrCreateTransformInRagdoll(ragdollPath);

                if (parentInRagdoll == null)
                {
                    Debug.LogError($"[Ragdoll] Failed to create or find parent for path: {ragdollPath}");
                    continue;
                }

                GameObject clone = Instantiate(original, parentInRagdoll);
                clone.name = original.name;

                _extraObjectClones[original] = clone;
            }
        }

        /// <summary>
        /// Finds or creates transform hierarchy in ragdoll based on path
        /// </summary>
        /// <param name="fullPath">Full GameObject path</param>
        /// <returns>Transform at specified path</returns>
        private Transform FindOrCreateTransformInRagdoll(string fullPath)
        {
            string[] parts = fullPath.Split('/');

            Transform current = null;

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                if (string.IsNullOrEmpty(part)) continue;

                if (current == null)
                {
                    if (part == _ragdoll.name)
                    {
                        current = _ragdoll.transform;
                    }
                    else
                    {
                        current = GameObject.Find(part)?.transform;
                        if (current == null)
                        {
                            Debug.LogError($"[Ragdoll] Root object '{part}' not found in scene.");
                            return null;
                        }
                    }
                }
                else
                {
                    Transform child = current.Find(part);
                    if (child == null)
                    {
                        GameObject newChild = new GameObject(part);
                        newChild.transform.SetParent(current);
                        child = newChild.transform;
                    }
                    current = child;
                }
            }

            return current;
        }

        /// <summary>
        /// Gets full hierarchical path of a GameObject
        /// </summary>
        /// <param name="obj">Target GameObject</param>
        /// <returns>Full path string</returns>
        private string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            var parentTransform = obj.transform.parent;

            while (parentTransform != null)
            {
                path = parentTransform.name + "/" + path;
                parentTransform = parentTransform.parent;
            }

            return path;
        }
    }
}