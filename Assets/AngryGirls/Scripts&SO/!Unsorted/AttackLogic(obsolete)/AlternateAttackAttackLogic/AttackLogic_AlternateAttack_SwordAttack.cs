//using UnityEngine;

//namespace Angry_Girls
//{
//    public class AttackLogic_Alternate_SwordAttack : AttackAbility
//    {
//        public AttackLogic_Alternate_SwordAttack(AttackAbilityData attackAbilityData) : base(attackAbilityData) { }

//        private float _timeInCurrentLoop;
//        private int _timesToRepeat_Attack_State = 22;
//        private GameObject _vfx;
//        private bool _hasEnteredAttackState = false;

//        // Sound settings: normalizedTime moments when they should play (e.g., 0.0 - start, 0.5 - middle)
//        private readonly float[] _soundTriggers = { 0.0f, 0.5f }; // Can be changed to {0.0f, 0.3f, 0.7f}, etc.
//        private bool[] _hasPlayedSoundInThisCycle; // Tracks which sounds have already played

//        public override void OnStateEnter(CControl control, Animator animator, AnimatorStateInfo stateInfo)
//        {
//            base.OnStateEnter(control, animator, stateInfo);
//            _vfx = GameplayCoreManager.Instance.ProjectileManager.SpawnByProjectileAbility(control);

//            if (!_hasEnteredAttackState)
//            {
//                CoreManager.Instance.AudioManager.PlayCustomSound(AudioSourceType.SFX_Impact, 4);
//                _hasEnteredAttackState = true;
//            }

//            // Initialize the array for tracking sounds in this cycle
//            _hasPlayedSoundInThisCycle = new bool[_soundTriggers.Length];
//        }

//        public override void OnStateUpdate(CControl control, Animator animator, AnimatorStateInfo stateInfo)
//        {
//        }

//        public override void OnStateExit(CControl control, Animator animator, AnimatorStateInfo stateInfo)
//        {
//            control.isAttacking = false;
//            _hasEnteredAttackState = false;

//            if (_vfx != null)
//                GameplayCoreManager.Instance.ProjectileManager.DisposeProjectile(_vfx);
//        }
//    }
//}