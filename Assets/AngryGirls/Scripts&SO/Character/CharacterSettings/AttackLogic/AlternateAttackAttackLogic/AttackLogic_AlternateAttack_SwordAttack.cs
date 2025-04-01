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

        // Настройки звуков: моменты normalizedTime, когда они должны играть (например, 0.0 - начало, 0.5 - середина)
        private readonly float[] _soundTriggers = { 0.0f, 0.5f }; // Можно изменить на {0.0f, 0.3f, 0.7f} и т.д.
        private bool[] _hasPlayedSoundInThisCycle; // Отслеживает, какие звуки уже сыграли

        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            base.OnStateEnter(control, animator, stateInfo);
            _vfx = GameLoader.Instance.VFXManager.SpawnVFX(control, VFX_Type.VFX_Eclipse, setAsOwner: true);

            if (!_hasEnteredAttackState)
            {
                GameLoader.Instance.audioManager.PlayCustomSound(AudioSourceType.SFX_Impact, 4);
                _hasEnteredAttackState = true;
            }

            // Инициализируем массив для отслеживания звуков в этом цикле
            _hasPlayedSoundInThisCycle = new bool[_soundTriggers.Length];
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            float normalizedTime = stateInfo.normalizedTime % 1; // Текущая позиция в цикле [0, 1)

            // Проверяем все триггеры звуков
            for (int i = 0; i < _soundTriggers.Length; i++)
            {
                // Если прошли триггер и ещё не играли звук
                if (normalizedTime >= _soundTriggers[i] && !_hasPlayedSoundInThisCycle[i])
                {
                    GameLoader.Instance.audioManager.PlayCustomSound(AudioSourceType.SFX_Impact, 4);
                    _hasPlayedSoundInThisCycle[i] = true;
                }
            }

            // Если цикл завершился (normalizedTime перескочил с ~1.0 на 0.0)
            if (normalizedTime < _timeInCurrentLoop)
            {
                // Сбрасываем флаги для нового цикла
                _hasPlayedSoundInThisCycle = new bool[_soundTriggers.Length];
            }

            _timeInCurrentLoop = normalizedTime;

            // Проверка на завершение всей анимации
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