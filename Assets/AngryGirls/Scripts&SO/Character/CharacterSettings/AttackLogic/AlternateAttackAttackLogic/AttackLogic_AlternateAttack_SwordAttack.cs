using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_AlternateAttack_SwordAttack : AttackAbilityLogic
    {
        public AttackLogic_AlternateAttack_SwordAttack(AttackAbilityData attackAbilityData) : base(attackAbilityData) { }

        private int _loopsCount;
        private float _timeInCurrentLoop;
        private int _timesToRepeat_Attack_State = 1;

        private TweenerCore<Quaternion, Vector3, QuaternionOptions> _rotationTween;
        private GameObject _vfx;

        //set proper rotation aftet changing state (landing for exmaple)
        private Quaternion _savedRotation;

        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            _savedRotation = control.CharacterMovement.Rigidbody.rotation;
            base.OnStateEnter(control, animator, stateInfo );

            //rotation
            _rotationTween = control.CharacterMovement.Rigidbody.DORotate(new Vector3(360, 0, 0), 0.3f, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.Linear).SetLoops(-1);
            _vfx = GameLoader.Instance.VFXManager.SpawnVFX(control, VFX_Type.VFX_Eclipse, setAsOwner: true);
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
            control.CharacterMovement.SetRotation(_savedRotation);
            _rotationTween.Kill();

            if (control.CharacterMovement.bottomRaycastContactPoint != Vector3.zero)
            {
                control.CharacterMovement.SetPosition(control.CharacterMovement.bottomRaycastContactPoint);
            }
            control.isAttacking = false;
            _vfx.GetComponent<VFX>().Dispose();
        }
    }
}