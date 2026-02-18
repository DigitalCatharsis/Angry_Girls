using UnityEngine;

namespace Angry_Girls
{
    public class AttackLogic_Launch_SwordAttack : AttackAbility
    {
        public AttackLogic_Launch_SwordAttack(AttackAbilityData attackAbilityData) : base(attackAbilityData) { }

        private float _timeInCurrentLoop;
        private int _timesToRepeat_Attack_State = 22;
        private GameObject _vfx;
        private bool _hasEnteredAttackState = false;

        // Sound settings: normalizedTime moments when they should play (e.g., 0.0 - start, 0.5 - middle)
        private readonly float[] _soundTriggers = { 0.0f, 0.5f }; // Can be changed to {0.0f, 0.3f, 0.7f}, etc.
        private bool[] _hasPlayedSoundInThisCycle; // Tracks which sounds have already played

        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            base.OnStateEnter(control, animator, stateInfo);
            _vfx = GameplayCoreManager.Instance.ProjectileManager.SpawnByProjectileAbility(control);

            if (!_hasEnteredAttackState)
            {
                CoreManager.Instance.AudioManager.PlayCustomSound(AudioSourceType.SFX_Impact, 4);
                _hasEnteredAttackState = true;
            }

            // Initialize the array for tracking sounds in this cycle
            _hasPlayedSoundInThisCycle = new bool[_soundTriggers.Length];
        }

        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
        {
            float normalizedTime = stateInfo.normalizedTime % 1; // Current position in the cycle [0, 1)

            // Check all sound triggers
            for (int i = 0; i < _soundTriggers.Length; i++)
            {
                // If the trigger has passed and the sound has not yet played
                if (normalizedTime >= _soundTriggers[i] && !_hasPlayedSoundInThisCycle[i])
                {
                    CoreManager.Instance.AudioManager.PlayCustomSound(AudioSourceType.SFX_Impact, 4);
                    _hasPlayedSoundInThisCycle[i] = true;
                }
            }

            // If the loop has completed (normalizedTime jumped from ~1.0 to 0.0)
            if (normalizedTime < _timeInCurrentLoop)
            {
                // Reset flags for a new loop
                _hasPlayedSoundInThisCycle = new bool[_soundTriggers.Length];
            }

            _timeInCurrentLoop = normalizedTime;

            // Check if the entire animation has completed
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
                GameplayCoreManager.Instance.ProjectileManager.DisposeProjectile(_vfx);
        }
    }
}