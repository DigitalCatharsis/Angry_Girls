namespace Angry_Girls

{
    /// <summary>
    /// Represents airborne state for character animation
    /// </summary>
    public class State_Airboned : AnimationStateBase
    {
        public State_Airboned(CControl control)
            : base(control) { }

        public override void OnEnter()
        {
            AnimationTransitioner.ChangeAnimationStateCrossFade(
                _control.animator,
                GameplayCoreManager.Instance.StatesContainer.airbonedFlying_Dictionary[_settings.airbonedFlying_States.animation],
                _settings.airbonedFlying_States.transitionDuration);
        }

        public override void OnUpdate() { }
    }
}