using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_AlternateAttack_SwordAttack : AttackAbilityLogic
    {
        private int _loopsCount;
        private float _timeInCurrentLoop;
        private int _timesToRepeat_Attack_State = 1;

        private TweenerCore<Quaternion, Vector3, QuaternionOptions> _rotationTween;
        private GameObject _vfx;

        //set proper rotation aftet changing state (landing for exmaple)
        private Quaternion _savedRotation;

        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            _savedRotation = control.rigidBody.rotation;
            control.rigidBody.velocity = Vector3.zero;
            //rotation
            control.rigidBody.velocity = (new Vector3(0, 10, 2 * control.transform.forward.z));
            _rotationTween = control.rigidBody.DORotate(new Vector3(360, 0, 0), 0.3f, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.Linear).SetLoops(-1);
            _vfx = GameLoader.Instance.VFXManager.SpawnVFX(control, control.characterSettings.AttackAbility_Launch.AttackVFX.GetComponent<VFX>().GetVFXType(), setAsOwner: true);
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            // Проверка на окончание ВСЕХ циклов анимации
            if (stateInfo.normalizedTime >= _timesToRepeat_Attack_State * stateInfo.length + 0.8f)
            {
                _loopsCount = 0;
                _timeInCurrentLoop = 0f;
                _rotationTween.Kill();
                control.isAttacking = false;
            }

            //Сброс флага в конце цикла
            if (_timeInCurrentLoop >= stateInfo.length)
            {
                _timeInCurrentLoop -= stateInfo.length;
                _loopsCount++;
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.rigidBody.rotation = _savedRotation;
            _rotationTween.Kill();
            if (control.bottomRaycastContactPoint != Vector3.zero)
            {
                control.rigidBody.position = control.bottomRaycastContactPoint;
            }
            control.isAttacking = false;
            _vfx.GetComponent<VFX>().Dispose();
        }
    }
}