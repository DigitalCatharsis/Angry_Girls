using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Launch_SendFireball_Front : AttackAbility
    {
        public AttackLogic_Launch_SendFireball_Front(AttackAbilityData attackAbilityData) : base(attackAbilityData) { }

        private int _loopsCount;
        private float _timeInCurrentLoop;
        private bool _fireballSentThisLoop = false;
        private int _timesToRepeat_Attack_State = 3;
        private float _spawnProjectile_TransitionOffset = 0.4f;

        private float _attackAngleChangeValue = 0f;
        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.CharacterMovement.Rigidbody.isKinematic = false;

            _loopsCount = 1;
            _timeInCurrentLoop = 0f;
            _fireballSentThisLoop = false;

            base.OnStateEnter(control, animator, stateInfo);
            control.CharacterMovement.Rigidbody.useGravity = false;
            _attackAngleChangeValue = 0f;
        }
        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            _timeInCurrentLoop += Time.deltaTime;

            // Determine the target rotation along the X and Y axes
            var targetEuler = control.CharacterMovement.Rigidbody.transform.forward.z > 0
            ? new Vector3(45, 0, 0) // Rotate downwards
            : new Vector3(45, 180, 0); // Rotate upward

            // Check for 40% in the CURRENT animation loop
            if (!_fireballSentThisLoop && _timeInCurrentLoop / stateInfo.length >= _spawnProjectile_TransitionOffset)
            {

                GameplayCoreManager.Instance.ProjectileManager.SendFireballFrontLaunch(control, control.projectileSpawnTransform.position, targetEuler, _attackAngleChangeValue);
                _fireballSentThisLoop = true;
                _attackAngleChangeValue += 4;
            }

            // Check for the end of ALL animation loops
            if (stateInfo.normalizedTime >= _timesToRepeat_Attack_State * stateInfo.length + 0.8f)
            {
                _loopsCount = 0;
                _timeInCurrentLoop = 0f;
                control.isAttacking = false;
                control.FinishTurn();
            }

            //Reset the flag at the end of the loop
            if (_timeInCurrentLoop >= stateInfo.length)
            {
                _timeInCurrentLoop -= stateInfo.length;
                _fireballSentThisLoop = false;
                _loopsCount++;
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            _attackAngleChangeValue = 0f;
            control.CharacterMovement.Rigidbody.velocity = Vector3.zero;
            control.CharacterMovement.Rigidbody.isKinematic = true;
        }
    }
}