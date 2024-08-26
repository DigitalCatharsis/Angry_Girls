namespace Angry_Girls
{
    public enum GameLoaderMediator_EventNames
    {
        ProceedOnLaunchIsOver,
        AllowCharacterPress,
        UpdateDebugInfo,
        Increment_CurrentTurn,
    }

    public class GameLoaderMediator : IMediator<GameLoaderMediator_EventNames>
    {
        private CameraManager _ñameraManager;
        private LaunchManager _launchManager;
        private TurnManager _turnManager;

        public GameLoaderMediator(CameraManager ñameraManager, LaunchManager launchManager, TurnManager turnManager)
        {
            _ñameraManager = ñameraManager;
            _launchManager = launchManager;
            _turnManager = turnManager;
        }

        public void Notify(object sender, GameLoaderMediator_EventNames eventName)
        {
            if (eventName == GameLoaderMediator_EventNames.ProceedOnLaunchIsOver)
            {
                _launchManager.OnLaunchIsOver();
            }
            if (eventName == GameLoaderMediator_EventNames.AllowCharacterPress)
            {
                _launchManager.Allow_CharacterPress();
            }
            if (eventName == GameLoaderMediator_EventNames.UpdateDebugInfo)
            {
                _launchManager.Allow_CharacterPress();
            }
        }
    }
}