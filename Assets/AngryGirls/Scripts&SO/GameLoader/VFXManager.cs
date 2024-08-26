using UnityEngine;
using UnityEngine.VFX;

namespace Angry_Girls
{
    public class VFXManager : GameLoaderComponent
    {
        public VFXManager Instance;

        public override void OnComponentEnable()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        public GameObject SpawnVFX(
            Transform parentsTransform, 
            VFX_Type vfx_Type, 
            Color VFXColor, 
            Vector3 spawnPosition, 
            Quaternion spawnRotation, 
            float timeToLive,
            bool isTimeToLiveIsNormilizedTime,
            bool destroyOnCollision,
            float VFXDamage, 
            bool enableCollider = false, 
            bool enableTrigger = false,
            GameObject owner = null
            )
        {
            //Spawn (taking from pool)
            var poolManager = GameLoader.Instance.poolManager;
            var vfx = poolManager.GetObject(vfx_Type, poolManager.vfxPoolDictionary, spawnPosition, spawnRotation);

            //Set Color
            if (vfx.GetComponentInChildren<VisualEffect>() != null)
            {
                vfx.GetComponentInChildren<VisualEffect>().SetVector4("Color", VFXColor);
            }

            //Set parent
            vfx.transform.parent = parentsTransform;

            //Init and run VFX
            GetComponent<VFX>().InitAndRunVFX(timeToLive,isTimeToLiveIsNormilizedTime,destroyOnCollision,VFXDamage, enableCollider, enableTrigger, owner: owner);
            return vfx;
        }

        public GameObject SpawnVFX(CControl control, VFX_Type vfx_Type)
        {

            //spawn 
            var poolManager = GameLoader.Instance.poolManager;
            var vfx = poolManager.GetObject(vfx_Type, poolManager.vfxPoolDictionary, control.projectileSpawnTransform.position, Quaternion.identity);

            //set color
            if (vfx.GetComponentInChildren<VisualEffect>() != null)
            {
                vfx.GetComponentInChildren<VisualEffect>().SetVector4("Color", control.VFX_Color);
            }

            //set parent and position
            vfx.transform.parent = control.transform;

            var vfxComponent = vfx.GetComponent<VFX>();

            //Init and Run VFX
            if (TurnManager.Instance.CurrentPhase == CurrentPhase.StaticPhase)
            {
                var staticAbility = control.characterSettings.staticAttackAbility;
                vfxComponent.InitAndRunVFX(
                    staticAbility.timeToLive, 
                    staticAbility.isTimeToLiveIsNormilizedTime, 
                    staticAbility.destroyOnCollision, 
                    staticAbility.attackDamage,
                    staticAbility.enableCollider, 
                    staticAbility.enableTrigger, 
                    owner: control.gameObject
                    );
            }
            else
            {
                var launchingAbility = control.characterSettings.launchedAttackPrepAbility;
                vfxComponent.InitAndRunVFX(
                    launchingAbility.timeToLive, 
                    launchingAbility.isTimeToLiveIsNormilizedTime,
                    launchingAbility.destroyOnCollision,
                    launchingAbility.attackDamage,
                    launchingAbility.enableCollider, 
                    launchingAbility.enableTrigger,
                    owner: control.gameObject
                    );
            }           

            return vfx;
        }

        public GameObject SpawnVFX_AtPosition(VFX_Type vfx_Type, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            //Spawn (taking from pool)
            var poolManager = GameLoader.Instance.poolManager;
            var vfx = poolManager.GetObject(vfx_Type, poolManager.vfxPoolDictionary, spawnPosition, spawnRotation);
            return vfx;
        }
    }
}