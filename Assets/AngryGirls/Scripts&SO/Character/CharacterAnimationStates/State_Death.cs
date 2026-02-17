using Angry_Girls;

public class State_Death : AnimationStateBase
{
    public State_Death(CControl control)
        : base(control) { }

    public override void OnEnter()
    {
        var randomDeathAnimation = _settings.GetRandomState(_settings.death_States).animation;
        AnimationTransitioner.ChangeAnimationState(
            _control.animator,
            GameplayCoreManager.Instance.StatesContainer.death_States_Dictionary[randomDeathAnimation],
            transitionDuration: 0.1f);
    }

    public override bool CanTransitionTo(IAnimationState nextState)
    {
        return false; // »з состо€ни€ смерти нельз€ перейти в другие состо€ни€
    }
}