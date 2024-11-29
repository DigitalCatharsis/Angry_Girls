using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Launch_SwordAttack : AttackAbilityLogic
    {
        private TweenerCore<Quaternion, Vector3, QuaternionOptions> _rotationTween;
        private GameObject _vfx;

        //set proper rotation aftet changing state (landing for exmaple)
        private Quaternion _savedRotation;
        
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            _savedRotation = control.transform.rotation;
            control.rigidBody.velocity = Vector3.zero;
            //rotation
            control.rigidBody.velocity = (new Vector3(0, 10, 2 * control.transform.forward.z));
            _rotationTween = control.transform.DORotate(new Vector3(360, 0, 0), 0.3f, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.Linear).SetLoops(-1);
            _vfx = GameLoader.Instance.VFXManager.SpawnVFX(control, control.characterSettings.AttackAbility_Launch.AttackVFX.GetComponent<VFX>().GetVFXType(), setAsOwner: true);
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if(control.isGrounded)
            {
                control.isAttacking = false;
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.transform.rotation = _savedRotation;
            _rotationTween.Kill();
            if (control.subComponentMediator.Notify_GetBottomContactPoint(control) != Vector3.zero)
            {
                control.transform.position = control.subComponentMediator.Notify_GetBottomContactPoint(control);
            }
            control.isAttacking = false;
            _vfx.GetComponent<VFX>().Dispose();
        }
    }
}