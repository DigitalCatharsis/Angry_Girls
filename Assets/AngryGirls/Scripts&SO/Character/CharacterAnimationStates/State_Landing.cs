namespace Angry_Girls
{
    public class State_Landing : AnimationStateBase
    {
        public State_Landing(CControl control)
            : base(control) { }

        public override void OnEnter()
        {
             AnimationTransitioner.ChangeAnimationStateCrossFade(
                 _control.animator,
                GameplayCoreManager.Instance.StatesContainer.landingNames_Dictionary[_settings.landing_State.animation],
                _settings.landing_State.transitionDuration);
        }
    }

}