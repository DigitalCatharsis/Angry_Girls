using DG.Tweening;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Launch_SendFireball_Front : AttackAbilityLogic
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

            // ���������� ������� �������� �� ���� X � Y
            var targetEuler = control.CharacterMovement.Rigidbody.transform.forward.z > 0
                ? new Vector3(45, 0, 0) // ������� ����
                : new Vector3(45, 180, 0); // ������� �����

            // �������� �� 40% � ������� ����� ��������
            if (!_fireballSentThisLoop && _timeInCurrentLoop / stateInfo.length >= _spawnProjectile_TransitionOffset)
            {

                SendFireball(control, control.projectileSpawnTransform.position, targetEuler, _attackAngleChangeValue);
                _fireballSentThisLoop = true;
                _attackAngleChangeValue += 4;
            }

            // �������� �� ��������� ���� ������ ��������
            if (stateInfo.normalizedTime >= _timesToRepeat_Attack_State * stateInfo.length + 0.8f)
            {
                _loopsCount = 0;
                _timeInCurrentLoop = 0f;
                control.isAttacking = false;
                control.FinishTurn();
            }

            //����� ����� � ����� �����
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
        private void SendFireball(CControl control, Vector3 startPoint, Vector3 targetEuler, float attackAngleChangeValue, float rotationDuration = 1.5f)
        {
            var vfx = GameLoader.Instance.VFXManager.SpawnByProjectileAbility(control);

            // ������������� ��������� ���������
            vfx.transform.position = startPoint;

            // ���������� ��������� �������� � ����������� �� ����������� control
            float initialYRotation = control.CharacterMovement.Rigidbody.transform.forward.z > 0 ? 0f : 180f;
            var initialRotation = Quaternion.Euler(
                attackAngleChangeValue * 4, // ������ �� ��� X
                initialYRotation, // ����������� �� ��� Y (0 ��� 180)
                0 // ������ �� ��� Z (�� ������������)
            );

            // ������������� ��������� ����������� � ������ ���������
            vfx.transform.rotation = initialRotation;

            // ���������� ������� ���� �������� ������ �� ��� X
            var targetEulerAngles = new Vector3(
                targetEuler.x, // �������� ���� �� ��� X
                initialYRotation, // ��������� Y ����������
                0 // ��������� Z ����������
            );

            // ��������� ������� � ������� DOTween (������ �� ��� X)
            vfx.transform.DORotate(targetEulerAngles, rotationDuration, RotateMode.Fast);

            // ������������� �������
            var impulse = new Vector3(
                0,
                control.characterSettings.AttackAbility_Launch.projectileMovementSpeed.y * vfx.transform.forward.y - attackAngleChangeValue,
                control.characterSettings.AttackAbility_Launch.projectileMovementSpeed.z * vfx.transform.forward.z
            );

            vfx.GetComponent<Rigidbody>().AddForce(impulse, ForceMode.VelocityChange);

            // ��������� ���� � control.rigidBody
            control.CharacterMovement.ApplyRigidForce(control.characterSettings.AttackAbility_Launch.attackMovementForce * control.CharacterMovement.Rigidbody.transform.forward.z, ForceMode.VelocityChange);
        }
    }
}