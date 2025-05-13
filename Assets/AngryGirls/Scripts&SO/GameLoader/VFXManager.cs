using AYellowpaper.SerializedCollections;
using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.VFX;

namespace Angry_Girls
{
    public class VFXManager : MonoBehaviour
    {
        private PoolManager _poolManager;
        private TurnManager _turnManager;

        private SerializedDictionary<GameObject, VFXConfig> _runningVfxDict = new();

        private IEnumerator LifeCycleRoutine(GameObject vfx, float timeToLive)
        {
            yield return new WaitForSeconds(timeToLive);
            CallVFXDispose(vfx);
        }

        public void CallVFXDispose(GameObject vfx)
        {
            _runningVfxDict.Remove(vfx.gameObject);

            var proj = vfx.gameObject.GetComponent<Projectile>();

            if (proj != null)
            {
                proj.OnDispose();
            }

            vfx.GetComponent<VFX>().Dispose();
        }

        private void Awake()
        {
            _poolManager = GameLoader.Instance.poolManager;
            _turnManager = GameLoader.Instance.turnManager;
        }

        public GameObject SpawnVFX(VFXConfig config)
        {
            var vfxObject = _poolManager.GetObject(config.Vfxtype,config.spawnPosition,Quaternion.identity);

            var vfx = vfxObject.GetComponent<VFX>();
            vfx.InitVfx(config);

            if (config.connectToOriginator && config.originator != null)
            {
                vfxObject.transform.SetParent(config.originator.transform);
            }

            _runningVfxDict.Add(vfx.gameObject, config);

            StartCoroutine(LifeCycleRoutine(vfxObject.gameObject, config.timeToLive));
            return vfxObject.gameObject;
        }

        public GameObject SpawnByProjectileAbility(CControl control)
        {
            var ability = GetCurrentAbility(control);

            var projectileConfig = new ProjectileConfig
            {
                VFXConfig = new VFXConfig
                {
                    spawnPosition = control.projectileSpawnTransform.position,
                    Vfxtype = ability.vfxType,
                    color = ability.vfxColor,
                    originator = control.gameObject,
                    timeToLive = ability.timeToLive,
                    layermask = control.GetVfxLayermask(),
                    connectToOriginator = ability.connectToOwner,
                },
                projectileSpawnTransform = control.projectileSpawnTransform,
                enemyKnockBackValue = ability.enemyKnockbackValue,
                destroyOnCollision = ability.destroyOnCollision,
                destroyOnCharacterTrigger = ability.destroyOnCharacterCollision,
                damage = ability.attackDamage,
                enableCollider = ability.enableCollider,
                enableTrigger = ability.enableTrigger,
                teamfire = ability.teamfire,
                spawnSound = new Tuple<AudioSourceType, int>(ability.spawnSourceType, ability.spawnIndex),
                destroySound = new Tuple<AudioSourceType, int>(ability.destroySourceType, ability.destoyIndex),
                deadbodyForceMultiplier = ability.deadbodyForceMultiplier,
                deadbodyForceMode = ability.deadbodyForceMode
            };
            var vfxGameobject = SpawnVFX(projectileConfig.VFXConfig);
            vfxGameobject.GetComponent<Projectile>().InitProjectile(projectileConfig);
            return vfxGameobject;
        }

        public GameObject SpawnByProjectileAbility
            (
                Transform spawnTramsform,
                VFX_Type vfxType,
                Color vfxColor,
                GameObject originator,
                LayerMask layerMask,
                float timeToLive,
                bool connectToOriginator,
                float enemyKnockBackValue,
                bool destroyOnCollision,
                bool destroyOnCharacterTrigger,
                float attackDamageValue,
                bool enableCollider,
                bool enableTrigger,
                bool teamfire,
                Tuple<AudioSourceType, int> spawnSound,
                Tuple<AudioSourceType, int> destroySound,
                float deadbodyForceMultiplier,
                ForceMode deadbodyForceMode
            )
        {
            var projectileConfig = new ProjectileConfig
            {
                VFXConfig = new VFXConfig
                {
                    Vfxtype = vfxType,
                    color = vfxColor,
                    originator = originator,
                    timeToLive = timeToLive,
                    layermask = layerMask,
                    connectToOriginator = connectToOriginator,
                },
                projectileSpawnTransform = spawnTramsform,
                enemyKnockBackValue = enemyKnockBackValue,
                destroyOnCollision = destroyOnCollision,
                destroyOnCharacterTrigger = destroyOnCharacterTrigger,
                damage = attackDamageValue,
                enableCollider = enableCollider,
                enableTrigger = enableTrigger,
                teamfire = teamfire,
                spawnSound = spawnSound,
                destroySound = destroySound,
                deadbodyForceMultiplier = deadbodyForceMultiplier,
                deadbodyForceMode = deadbodyForceMode
            };
            var vfxGameobject = SpawnVFX(projectileConfig.VFXConfig);
            vfxGameobject.GetComponent<Projectile>().InitProjectile(projectileConfig);
            return vfxGameobject;
        }


        private AttackAbilityData GetCurrentAbility(CControl control)
        {
            return GameLoader.Instance.gameFlowController.CurrentState == GameState.AlternatePhase
                ? control.characterSettings.AttackAbility_Alternate
                : control.characterSettings.AttackAbility_Launch;
        }

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

            GameLoader.Instance.VFXManager.CallVFXDispose(vfx.gameObject);
        }

        public void ShowDamageNumbers(Collider targetCollider, GameObject projectileOriginator, float damageAmount)
        {
            var contactPoint = targetCollider.ClosestPoint(projectileOriginator.GetComponent<CharacterMovement>().Rigidbody.position);

            var config = new VFXConfig
            {
                spawnPosition = contactPoint,
                originator = projectileOriginator,
                Vfxtype = VFX_Type.VFX_DamageNumbers,
                timeToLive = 1f,
                color = Color.white,
                connectToOriginator = false,
            };

            var damageVfx = SpawnVFX(config);
            DisplayFloatingNumbers(damageVfx, damageAmount.ToString());
        }

        public void ProcessFireballs_HeadSpin(CControl control, Vector3[] angles, float moveDuration = 1.5f)
        {
            var _impulseY = 7f;
            var _impulseZ = 5f;
            var _finalProjectileRotation = new Vector3(75f, 0, 0);

            //spawn
            for (var i = 0; i < angles.Length; i++)
            {
                var projectile = GameLoader.Instance.VFXManager.SpawnByProjectileAbility(control);

                projectile.transform.position = control.projectileSpawnTransform.position;
                projectile.transform.rotation = Quaternion.Euler(angles[i]);

                //set final rotation value 
                var finalRotationDegree = _finalProjectileRotation;
                if (Math.Sign(projectile.transform.forward.z) < 0)
                {
                    finalRotationDegree.y += 180f;
                }

                var impulse = new Vector3(0, _impulseY * projectile.transform.forward.y, _impulseZ * projectile.transform.forward.z);

                //add impulse and rotate
                projectile.GetComponent<Rigidbody>().AddForce(impulse, ForceMode.VelocityChange);
                projectile.transform.DORotate(endValue: new Vector3(finalRotationDegree.x, finalRotationDegree.y, finalRotationDegree.y * projectile.transform.forward.z), duration: moveDuration, mode: RotateMode.Fast);
            }
        }
        public void ShowCoinsValue(Vector3 position, int coinValue)
        {
            var config = new VFXConfig
            {
                spawnPosition = position,
                originator = null,
                Vfxtype = VFX_Type.VFX_CoinValue,
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
                textMesh.text = value.ToString();
                vfxGameObject.transform.DOMoveY(vfxGameObject.transform.position.y + 1, 1);
            }
        }
    }
}