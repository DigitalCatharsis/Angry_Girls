using Angry_Girls;

public class State_Airborned : AnimationStateBase
{
    public State_Airborned(CControl control, AnimationController animationController)
        : base(control, animationController) { }

    public override void OnEnter()
    {
        _animationController.ChangeAnimationStateCrossFade(
            GameLoader.Instance.statesContainer.airbonedFlying_Dictionary[_settings.airbonedFlying_States.animation],
            _settings.airbonedFlying_States.transitionDuration);
    }

    public override void OnUpdate()
    {

    }
}
