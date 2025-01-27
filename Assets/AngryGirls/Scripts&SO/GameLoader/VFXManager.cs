using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.VFX;

namespace Angry_Girls
{
    public class VFXManager : MonoBehaviour
    {
        public void FadeOutFlame_And_Dispose(VFX vFX, float disposeDuration, float fadeDuration)
        {
            StartCoroutine(On_DisposeVfxAfterTime(vFX, disposeDuration, fadeDuration));
        }

        private IEnumerator On_DisposeVfxAfterTime(VFX vFX, float disposeDuration, float fadeDuration)
        {
            //fading
            var visualEffect = vFX.GetComponentInChildren<VisualEffect>();
            float elapsedTime = 0f;

            while (elapsedTime < disposeDuration)
            {
                float t = elapsedTime / disposeDuration; // Нормализуем время
                float newValue = Mathf.Lerp(fadeDuration, 0, t); // Плавно уменьшаем значение
                visualEffect.SetFloat("FlamesLifetime", newValue); // Устанавливаем новое значение параметра
                visualEffect.SetFloat("SmokeLifetime", newValue); // Устанавливаем новое значение параметра
                visualEffect.SetFloat("FlameRate", newValue); // Устанавливаем новое значение параметра
                visualEffect.SetFloat("SmokeRate", newValue); // Устанавливаем новое значение параметра

                elapsedTime += Time.deltaTime; // Увеличиваем прошедшее время
                yield return null; // Ждем следующего кадра
            }

            yield return new WaitForSeconds(disposeDuration);
            vFX.Dispose();
        }

        public GameObject SpawnVFX(CControl control, VFX_Type vfx_Type, bool setAsOwner = false)
        {
            //spawn 
            var poolManager = GameLoader.Instance.poolManager;
            var vfx = poolManager.GetObject(vfx_Type, poolManager.vfxPoolDictionary, control.projectileSpawnTransform.position, Quaternion.identity);

            //set color
            if (vfx.GetComponentInChildren<VisualEffect>() != null)
            {
                vfx.GetComponentInChildren<VisualEffect>().SetVector4("Color", control.VFX_Color);
            }

            if (setAsOwner)
            {
                //set parent and position
                vfx.transform.parent = control.transform;
            }

            var vfxComponent = vfx.GetComponent<VFX>();

            //Init and Run VFX
            if (GameLoader.Instance.turnManager.CurrentPhase == CurrentPhase.AlternatePhase)
            {
                var AlternateAbility = control.characterSettings.AttackAbility_Alternate;
                vfxComponent.InitAndRunVFX_ByAbility(AlternateAbility, control);
            }
            else
            {
                var launchingAbility = control.characterSettings.AttackAbility_Launch;
                vfxComponent.InitAndRunVFX_ByAbility(launchingAbility, control);
            }

            return vfx.gameObject;
        }

        public GameObject SpawnVFX_AtPosition(VFX_Type vfx_Type, Vector3 spawnPosition, Quaternion spawnRotation, GameObject owner)
        {
            //Spawn (taking from pool)
            var poolManager = GameLoader.Instance.poolManager;
            var vfx = poolManager.GetObject(vfx_Type, poolManager.vfxPoolDictionary, spawnPosition, spawnRotation);
            return vfx.gameObject;
        }

        public void ShowDamageNumbers(Collider triggerCollider, CControl control, float damage)
        {
            var contactpoint = triggerCollider.ClosestPoint(control.transform.position);

            var previewVfx = GameLoader.Instance.VFXManager.SpawnVFX_AtPosition(VFX_Type.VFX_TestOnHitEffect, contactpoint, Quaternion.identity, owner: control.gameObject).GetComponent<VFX>();
            previewVfx.InitAndRunVFX_ByCustom(
                timeToLive: 1,
                isTimeToLiveIsNormilizedTime: false,
                destroyOnCollision: false,
                destroyOnCharacterCollision: false,
                damage: damage,
                knockbackValue: 0,
                enableCollider: false,
                enableTrigger: false,
                owner: control.gameObject);

            previewVfx.GetComponentInChildren<TextMeshPro>().text = damage.ToString();
            previewVfx.transform.DOMove(new Vector3(previewVfx.transform.position.x, previewVfx.transform.position.y + 1, previewVfx.transform.position.z), 1);
        }
    }
}