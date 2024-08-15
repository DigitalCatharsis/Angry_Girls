using Angry_Girls;
using UnityEngine;
using UnityEngine.VFX;

public class AttackLogic_Static_Prep_Shoryuken : AttackAbilityLogic
{
    public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
    {
        CastFlameVFX(control);
    }

    public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
    {
        if (stateInfo.normalizedTime >= 1
            && control.isGrounded)
        {
            var ap = control.subComponentProcessor.animationProcessor;

            //TODO: fuck...
            ap.ChangeAnimationState(ap.staticAttack_States_Dictionary[StaticAttack_States.A_Shoryuken_Landing_Static], 0, transitionDuration: control.characterSettings.idle_State.transitionDuration);
        }
    }

    public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
    {
    }

    private GameObject CastFlameVFX(CControl control)
    {
        //var vfx = Singleton.Instance.spawnManager.SpawnThing<VFX_Type>(VFX_Type.VFX_Flame2, control.subComponentProcessor.attackSystem.projectileSpawnTransform.position, Quaternion.identity); !old
        var poolManager = Singleton.Instance.poolManager;
        var vfx = poolManager.GetObject(VFX_Type.VFX_Flame2,poolManager.vfxPoolDictionary, control.subComponentProcessor.attackSystem.projectileSpawnTransform.position, Quaternion.identity);
        vfx.GetComponentInChildren<VisualEffect>().SetVector4("Color", control.subComponentProcessor.attackSystem.VFX_Color);
        vfx.transform.parent = control.transform;
        vfx.transform.position = control.subComponentProcessor.attackSystem.projectileSpawnTransform.position;
        vfx.GetComponent<VFX>().ApplyFlame(control.characterSettings.launchedAttackPrepAbility.attackDamage);

        return vfx;
    }
}
