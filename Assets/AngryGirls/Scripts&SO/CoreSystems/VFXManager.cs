using AYellowpaper.SerializedCollections;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.VFX;

namespace Angry_Girls
{
    /// <summary>
    /// Manages pure visual effects lifecycle (non-interactive particles, numbers, coins).
    /// Does NOT handle projectile physics or collision logic (that's ProjectileManager's responsibility).
    /// </summary>
    public class VFXManager : MonoBehaviour
    {
        private PoolManager _poolManager;
        private SerializedDictionary<GameObject, VFXConfig> _runningVfxDict = new();

        public void Init()
        {
            _poolManager = CoreManager.Instance.PoolManager;
        }

        private IEnumerator LifeCycleRoutine(GameObject vfx, float timeToLive)
        {
            yield return new WaitForSeconds(timeToLive);
            if (vfx != null && vfx.gameObject != null)
            {
                CallVFXDispose(vfx);
            }
        }

        /// <summary>
        /// Stops ALL active VFX immediately before scene transition.
        /// MUST be called BEFORE loading new scene.
        /// </summary>
        public void StopAllVFX()
        {
            // Create a copy of the list for safe iteration
            var vfxList = new List<GameObject>(_runningVfxDict.Keys);
            foreach (var vfx in vfxList)
            {
                // Safely destroy even if the object is already partially destroyed
                if (vfx != null && vfx.gameObject != null)
                {
                    CallVFXDispose(vfx);
                }
            }
            _runningVfxDict.Clear();
            Debug.Log($"VFXManager cleaned up {vfxList.Count} objects before scene transition");
        }

        /// <summary>
        /// Spawns PURE visual effect (no physics, no collision logic)
        /// </summary>
        public GameObject SpawnVFX(VFXConfig config)
        {
            var vfxObject = _poolManager.GetObject(config.Vfxtype, config.spawnPosition, Quaternion.identity);
            if (vfxObject == null) return null;

            var vfx = vfxObject.GetComponent<VFX>();
            vfx.InitVfx(config);

            if (config.connectToOriginator && config.originator != null)
            {
                vfxObject.transform.SetParent(config.originator.transform);
            }

            _runningVfxDict.Add(vfxObject.gameObject, config);
            StartCoroutine(LifeCycleRoutine(vfxObject.gameObject, config.timeToLive));
            return vfxObject.gameObject;
        }

        /// <summary>
        /// Disposes VFX object and returns it to pool
        /// SAFE FOR BOTH: pure VFX and projectiles (delegates projectile cleanup to ProjectileManager)
        /// </summary>
        public void CallVFXDispose(GameObject vfx)
        {
            if (vfx == null || vfx.gameObject == null)
            {
                return; 
            }

            _runningVfxDict.Remove(vfx.gameObject);

            var proj = vfx.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.OnDispose(); // Only unsubscribe from InteractionManager
                                  // DO NOT call VFXManager.CallVFXDispose() here - this will cause recursion!
            }

            // Return to the pool
            var vfxComponent = vfx.GetComponent<VFX>();
            if (vfxComponent != null)
            {
                vfxComponent.Dispose();
            }
            else
            {
                Destroy(vfx); // Fallback for invalid objects
            }
        }

        /// <summary>
        /// Fades out VFX over time before disposal
        /// </summary>
        public void FadeOutAndDisposeVFX(GameObject vfx, float disposeDuration, float fadeDuration)
        {
            StartCoroutine(FadeOutAndDisposeCoroutine(vfx, disposeDuration, fadeDuration));
        }

        private IEnumerator FadeOutAndDisposeCoroutine(GameObject vfx, float disposeDuration, float fadeDuration)
        {
            var visualEffect = vfx.GetComponentInChildren<VisualEffect>();
            if (visualEffect == null) yield break;

            float elapsedTime = 0f;
            float[] initialValues = {
                visualEffect.GetFloat("FlamesLifetime"),
                visualEffect.GetFloat("SmokeLifetime"),
                visualEffect.GetFloat("FlameRate"),
                visualEffect.GetFloat("SmokeRate")
            };

            while (elapsedTime < disposeDuration)
            {
                float progress = elapsedTime / disposeDuration;
                visualEffect.SetFloat("FlamesLifetime", Mathf.Lerp(initialValues[0], 0, progress));
                visualEffect.SetFloat("SmokeLifetime", Mathf.Lerp(initialValues[1], 0, progress));
                visualEffect.SetFloat("FlameRate", Mathf.Lerp(initialValues[2], 0, progress));
                visualEffect.SetFloat("SmokeRate", Mathf.Lerp(initialValues[3], 0, progress));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            CallVFXDispose(vfx.gameObject);
        }

        /// <summary>
        /// Shows floating damage numbers at hit location (PURE VISUAL)
        /// </summary>
        public void ShowDamageNumbers(Collider targetCollider, GameObject projectileOriginator, float damageAmount)
        {
            var contactPoint = targetCollider.ClosestPoint(projectileOriginator.GetComponent<CharacterMovement>().Rigidbody.position);
            var config = new VFXConfig
            {
                spawnPosition = contactPoint,
                originator = projectileOriginator,
                Vfxtype = VFX_Type.PRE_VFX_DamageNumbers,
                timeToLive = 1f,
                color = Color.white,
                connectToOriginator = false,
            };

            var damageVfx = SpawnVFX(config);
            DisplayFloatingNumbers(damageVfx, damageAmount.ToString());
        }

        /// <summary>
        /// Shows coin value with floating numbers (PURE VISUAL)
        /// </summary>
        public void ShowCoinsValue(Vector3 position, int coinValue)
        {
            var config = new VFXConfig
            {
                spawnPosition = position,
                originator = null,
                Vfxtype = VFX_Type.PRE_VFX_CoinValue,
                color = Color.white,
                timeToLive = 1,
                connectToOriginator = false,
            };

            var previewVfx = SpawnVFX(config);
            previewVfx.transform.position = position;
            DisplayFloatingNumbers(previewVfx, coinValue.ToString());
        }

        private void DisplayFloatingNumbers(GameObject vfxGameObject, string value = "")
        {
            var textMesh = vfxGameObject.GetComponentInChildren<TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = value;
                vfxGameObject.transform.DOMoveY(vfxGameObject.transform.position.y + 1, 1);
            }
        }
    }
}