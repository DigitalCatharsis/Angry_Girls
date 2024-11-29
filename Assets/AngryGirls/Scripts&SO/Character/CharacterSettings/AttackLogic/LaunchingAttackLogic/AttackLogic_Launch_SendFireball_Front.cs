using DG.Tweening;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Launch_SendFireball_Front : AttackAbilityLogic
    {
        private int _loopsCount;
        private float _timeInCurrentLoop;
        private bool _fireballSentThisLoop = false;
        private int _timesToRepeat_Attack_State = 3;
        private float _spawnProjectile_TransitionOffset = 0.4f;

        private Vector3 _finalProjectileRotation = new Vector3(45f, 0, 0);

        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
        ColorDebugLog.Log("Loop" + _loopsCount, System.Drawing.KnownColor.Red);
            _loopsCount = 1;
            _timeInCurrentLoop = 0f;
            _fireballSentThisLoop = false;

            control.isAttacking = true;
            control.rigidBody.useGravity = false;
            control.rigidBody.velocity = Vector3.zero;
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            _timeInCurrentLoop += Time.deltaTime;

            // Проверка на 40% в ТЕКУЩЕМ цикле анимации
            if (!_fireballSentThisLoop && _timeInCurrentLoop / stateInfo.length >= _spawnProjectile_TransitionOffset)
            {
                SendFireball(control, control.projectileSpawnTransform.position, _finalProjectileRotation, control.characterSettings.AttackAbility_Alternate.attackDamage);
                _fireballSentThisLoop = true;
            }

            // Проверка на окончание ВСЕХ циклов анимации
            if (stateInfo.normalizedTime >= _timesToRepeat_Attack_State * stateInfo.length + 0.8f)
            {
                _loopsCount = 0;
                _timeInCurrentLoop = 0f;
                control.FinishTurn();
            }

            //Сброс флага в конце цикла
            if (_timeInCurrentLoop >= stateInfo.length)
            {
                _timeInCurrentLoop -= stateInfo.length;
                _fireballSentThisLoop = false;
                _loopsCount++;
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            ColorDebugLog.Log("END" + _loopsCount, System.Drawing.KnownColor.Red);
            control.rigidBody.velocity = Vector3.zero;
        }

        private void SendFireball(CControl control, Vector3 startPoint, Vector3 finalRotationDegree, float moveDuration = 1.5f)
        {
            //spawn fireball
            var vfx = GameLoader.Instance.VFXManager.SpawnVFX(control, VFX_Type.VFX_FireBall);

            //rotate fireball to proper way
            vfx.transform.forward = control.transform.forward;

            //set impulse
            var impulse = new Vector3(
                0,
                control.characterSettings.AttackAbility_Launch.projectileMovementSpeed.y * vfx.transform.forward.y,
                control.characterSettings.AttackAbility_Launch.projectileMovementSpeed.z * control.transform.forward.z
                );

            //MoveCharacter when cast fireball
            //control.rigidBody.AddForce(control.characterSettings.AttackAbility_Launch.attackMovementForce * control.transform.forward.z); //turn it back
            control.rigidBody.velocity = control.characterSettings.AttackAbility_Alternate.attackMovementSpeed * control.transform.forward.z; //turn it back

            //Move fireball
            vfx.GetComponent<Rigidbody>().AddForce(impulse, ForceMode.VelocityChange);
            vfx.transform.DORotate(endValue: new Vector3(finalRotationDegree.x, finalRotationDegree.y, finalRotationDegree.y * control.transform.forward.z), duration: moveDuration, mode: RotateMode.Fast).SetLink(vfx, LinkBehaviour.PauseOnDisableRestartOnEnable);
        }
    }
}