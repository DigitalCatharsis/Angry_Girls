
namespace Angry_Girls
{
    public abstract class AttackAbility
    {
        public AttackAbilityData LaunchPrepData { get; private set; }
        public AttackAbilityData LaunchFinishData { get; private set; }
        public AttackAbilityData AlternatePrepData { get; private set; }
        public AttackAbilityData AlternateFinishData { get; private set; }

        protected AttackAbility(
            AttackAbilityData launchPrep,
            AttackAbilityData launchFinish,
            AttackAbilityData alternatePrep,
            AttackAbilityData alternateFinish)
        {
            LaunchPrepData = launchPrep;
            LaunchFinishData = launchFinish;
            AlternatePrepData = alternatePrep;
            AlternateFinishData = alternateFinish;
        }

        // Launch Prep
        public virtual void OnLaunchPrepEnter(CControl control) { }
        public virtual void OnLaunchPrepUpdate(CControl control) { }
        public virtual void OnLaunchPrepExit(CControl control) { }

        // Launch Finish
        public virtual void OnLaunchFinishEnter(CControl control) { }
        public virtual void OnLaunchFinishUpdate(CControl control) { }
        public virtual void OnLaunchFinishExit(CControl control) { }

        // Alternate Prep
        public virtual void OnAlternatePrepEnter(CControl control) { }
        public virtual void OnAlternatePrepUpdate(CControl control) { }
        public virtual void OnAlternatePrepExit(CControl control) { }

        // Alternate Finish
        public virtual void OnAlternateFinishEnter(CControl control) { }
        public virtual void OnAlternateFinishUpdate(CControl control) { }
        public virtual void OnAlternateFinishExit(CControl control) { }
    }
}