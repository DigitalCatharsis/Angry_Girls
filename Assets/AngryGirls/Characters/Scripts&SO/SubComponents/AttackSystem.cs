using UnityEngine;

namespace Angry_Girls
{
    public class AttackSystem : SubComponent
    {
        [Header("Setup")]
        [Header("VFX")]
        public Transform projectileSpawnTransform;
        public Color VFX_Color;

        //Logic
        public AttackAbilityLogic attackPrepLogic;
        public AttackAbilityLogic attackFinishLogic;

        public AttackAbilityLogic staticAttackLogic_Prep;
        public AttackAbilityLogic staticAttackLogic_Landing;
        public AttackAbilityLogic staticAttackLogic_OnGround;

        public bool hasFinishedStaticAttackTurn = true;

        public override void OnComponentEnable()
        {
            control.subComponentProcessor.attackSystem = this;

            InitAttackLogic();
        }

        private void InitAttackLogic()
        {
            //attackPrep
            switch (control.characterSettings.launchedAttackPrepAbility.attackPrep_State.animation)
            {
                case AttackPrep_States.A_ShootArrow:
                    attackPrepLogic = new AttackLogic_ShootArrow();
                    break;
                case AttackPrep_States.A_Shoryuken_DownSmash_Prep:
                    attackPrepLogic = new AttackLogic_ShoryukenDownSmash_Prep();
                    break;
                case AttackPrep_States.A_HeadSpin_Attack:
                    attackPrepLogic = new AttackLogic_HeadSpinAttack();
                    break;
                    //default:
                    //    throw new Exception("No logic for state like " + control.characterSettings.attackPrepAbility.attackPrep_State.animation.ToString());
            }

            //attackFinish
            switch (control.characterSettings.attackFininsh_State.animation)
            {
                case AttackFinish_States.A_Shoryuken_DownSmash_Finish:
                    attackFinishLogic = new AttackLogic_ShoryukenDownSmash_Finish();
                    break;
                    //default:
                    //    throw new Exception("No logic for state like " + control.characterSettings.attackPrepAbility.attackPrep_State.animation.ToString());
            }

            //StaticAttack
            switch (control.characterSettings.staticAttackAbility.staticAttack_State.animation)
            {
                case StaticAttack_States.A_Shoryuken_Prep_Static:
                    staticAttackLogic_Prep = new AttackLogic_Static_Prep_Shoryuken();
                    staticAttackLogic_Landing = new AttackLogic_Static_Landing_Shoryuken();
                    staticAttackLogic_OnGround = new AttackLogic_Static_OnGround_Shoryuken_Rise();
                    break;
                    //default:
                    //    throw new Exception("No logic for state like " + control.characterSettings.attackPrepAbility.attackPrep_State.animation.ToString());
            }
        }

        public override void OnUpdate()
        {
        }

        public override void OnAwake()
        {
        }

        public override void OnFixedUpdate()
        {
        }

        public override void OnLateUpdate()
        {
        }

        public override void OnStart()
        {
        }
    }
}