using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Alternate_SwordAttack : AttackAbilityLogic
    {
        public AttackLogic_Alternate_SwordAttack(AttackAbilityData attackAbilityData) : base(attackAbilityData) { }

        private float _timeInCurrentLoop;
        private int _timesToRepeat_Attack_State = 22;
        private GameObject _vfx;
        private bool _hasEnteredAttackState = false;

        // ��������� ������: ������� normalizedTime, ����� ��� ������ ������ (��������, 0.0 - ������, 0.5 - ��������)
        private readonly float[] _soundTriggers = { 0.0f, 0.5f }; // ����� �������� �� {0.0f, 0.3f, 0.7f} � �.�.
        private bool[] _hasPlayedSoundInThisCycle; // �����������, ����� ����� ��� �������

        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            base.OnStateEnter(control, animator, stateInfo);
            _vfx = GameLoader.Instance.VFXManager.SpawnVFX(control, VFX_Type.VFX_Eclipse, setAsOwner: true);

            if (!_hasEnteredAttackState)
            {
                GameLoader.Instance.audioManager.PlayCustomSound(AudioSourceType.SFX_Impact, 4);
                _hasEnteredAttackState = true;
            }

            // �������������� ������ ��� ������������ ������ � ���� �����
            _hasPlayedSoundInThisCycle = new bool[_soundTriggers.Length];
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            float normalizedTime = stateInfo.normalizedTime % 1; // ������� ������� � ����� [0, 1)

            // ��������� ��� �������� ������
            for (int i = 0; i < _soundTriggers.Length; i++)
            {
                // ���� ������ ������� � ��� �� ������ ����
                if (normalizedTime >= _soundTriggers[i] && !_hasPlayedSoundInThisCycle[i])
                {
                    GameLoader.Instance.audioManager.PlayCustomSound(AudioSourceType.SFX_Impact, 4);
                    _hasPlayedSoundInThisCycle[i] = true;
                }
            }

            // ���� ���� ���������� (normalizedTime ���������� � ~1.0 �� 0.0)
            if (normalizedTime < _timeInCurrentLoop)
            {
                // ���������� ����� ��� ������ �����
                _hasPlayedSoundInThisCycle = new bool[_soundTriggers.Length];
            }

            _timeInCurrentLoop = normalizedTime;

            // �������� �� ���������� ���� ��������
            if (stateInfo.normalizedTime >= _timesToRepeat_Attack_State)
            {
                _timeInCurrentLoop = 0f;
                control.isAttacking = false;
                _hasEnteredAttackState = false;
            }
        }

        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            control.isAttacking = false;
            _hasEnteredAttackState = false;

            if (_vfx != null)
                _vfx.GetComponent<VFX>().Dispose();
        }
    }
}