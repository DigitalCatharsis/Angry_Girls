using Angry_Girls;

public class State_Death : AnimationStateBase
{
    public State_Death(CControl control, AnimationController animationController)
        : base(control, animationController) { }

    public override void OnEnter()
    {
        var randomDeathAnimation = _settings.GetRandomState(_settings.death_States).animation;
        _animationController.ChangeAnimationState(
            GameLoader.Instance.statesContainer.death_States_Dictionary[randomDeathAnimation],
            transitionDuration: 0.1f);
    }

    public override bool CanTransitionTo(IAnimationState nextState)
    {
        return false; // �� ��������� ������ ������ ������� � ������ ���������
    }
}