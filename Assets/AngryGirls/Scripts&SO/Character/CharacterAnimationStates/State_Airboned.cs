namespace Angry_Girls

{
    /// <summary>
    /// Represents airborne state for character animation
    /// </summary>
    public class State_Airboned : AnimationStateBase
    {
        public State_Airboned(CControl control, AnimationController animationController)
            : base(control, animationController) { }

        public override void OnEnter()
        {
            _animationController.ChangeAnimationStateCrossFade(
                GameplayCoreManager.Instance.StatesContainer.airbonedFlying_Dictionary[_settings.airbonedFlying_States.animation],
                _settings.airbonedFlying_States.transitionDuration);
        }

        public override void OnUpdate() { }
    }
}