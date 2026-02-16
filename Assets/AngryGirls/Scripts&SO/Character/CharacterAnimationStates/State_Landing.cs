using Angry_Girls;

public class State_Landing : AnimationStateBase
{
    public State_Landing(CControl control, AnimationController animationController)
        : base(control, animationController) { }

    public override void OnEnter()
    {
        _animationController.ChangeAnimationStateCrossFade(
            GameplayCoreManager.Instance.StatesContainer.landingNames_Dictionary[_settings.landing_State.animation],
            _settings.landing_State.transitionDuration);
    }
}