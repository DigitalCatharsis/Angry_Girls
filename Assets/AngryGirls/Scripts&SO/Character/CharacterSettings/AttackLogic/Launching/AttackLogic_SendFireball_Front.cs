using DG.Tweening;
using UnityEditor.Search;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_SendFireball_Front : AttackAbilityLogic
    {
        private float _currentAttackTimer;
        private int _attacksCount;

        private Vector3 _finalProjectileRotation = new Vector3(45f, 0, 0);

        private float _impulseY = 7f;
        private float _impulseZ = 10f;

        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            _currentAttackTimer = 0;
            _attacksCount = 0;

            control.isAttacking = true;
            control.rigidBody.useGravity = false;
            control.rigidBody.velocity = control.characterSettings.launchedAttackPrepAbility.attackPrepMovementSpeed;
            control.rigidBody.AddForce(control.characterSettings.launchedAttackPrepAbility.attackPrepMovementForce);

            SendFireball(control, control.projectileSpawnTransform.position, _finalProjectileRotation, control.characterSettings.launchedAttackPrepAbility.attackDamage);

        }
        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (_attacksCount < ((int)stateInfo.normalizedTime))
            {
                _attacksCount = (int)stateInfo.normalizedTime;                

                SendFireball(control, control.projectileSpawnTransform.position, _finalProjectileRotation, control.characterSettings.launchedAttackPrepAbility.attackDamage);
            }

            if (control.characterSettings.launchedAttackPrepAbility.useAnimationNormalizedTimeDuration)
            {
                if (stateInfo.normalizedTime >= control.characterSettings.launchedAttackPrepAbility.timesToRepeat_AttackPrep_State)
                {
                    control.isAttacking = false;
                    control.hasFinishedLaunchingTurn = true;
                }
            }
            else
            {
                _currentAttackTimer += Time.deltaTime;
                if (_currentAttackTimer >= control.characterSettings.launchedAttackPrepAbility.attackTimeDuration)
                {
                    control.isAttacking = false;
                    control.hasFinishedLaunchingTurn = true;
                }
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.rigidBody.velocity = Vector3.zero;
        }

        private void SendFireball(CControl control, Vector3 startPoint, Vector3 finalRotationDegree, float moveDuration = 1.5f)
        {
            //var frontDistance = new Vector3(0, 0, 2.2f);
            //var gravityDistance = new Vector3(0, 2.8f, 8f);

            var vfx = GameLoader.Instance.VFXManager.SpawnVFX(control, VFX_Type.VFX_FireBall);
            var impulse = new Vector3(0, _impulseY * vfx.transform.forward.y, _impulseZ * vfx.transform.forward.z);
            vfx.GetComponent<VFX>().InitAndRunVFX(control.characterSettings.launchedAttackPrepAbility, control.gameObject);
            vfx.GetComponent<Rigidbody>().AddForce(impulse, ForceMode.Impulse);
            vfx.transform.DORotate(endValue: new Vector3(finalRotationDegree.x, finalRotationDegree.y, finalRotationDegree.y * vfx.transform.forward.z), duration: moveDuration, mode: RotateMode.Fast);


            //var waypoints = new[]
            //{
            //    startPoint,
            //    new Vector3(vfxGameobject.transform.position.x, vfxGameobject.transform.position.y, vfxGameobject.transform.position.z + vfxGameobject.transform.forward.z * frontDistance.z),
            //    new Vector3(vfxGameobject.transform.position.x, vfxGameobject.transform.position.y + Vector3.down.y * gravityDistance.y, vfxGameobject.transform.position.z + vfxGameobject.transform.forward.z * gravityDistance.z),
            //};


            //vfxGameobject.transform.DOPath(waypoints, moveDuration, pathType: PathType.Linear, pathMode: PathMode.Full3D, resolution: 10);
            //vfxGameobject.transform.DORotate(endValue: new Vector3(finalRotationDegree.x, finalRotationDegree.y, finalRotationDegree.y * vfxGameobject.transform.forward.z), duration: moveDuration, mode: RotateMode.Fast);

            ////add impulse and rotate
            //projectile.GetComponent<Rigidbody>().AddForce(impulse, ForceMode.Impulse);
        }
    }
}