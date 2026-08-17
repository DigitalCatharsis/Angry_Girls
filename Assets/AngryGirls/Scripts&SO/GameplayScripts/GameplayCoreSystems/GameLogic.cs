using Angry_Girls;
using Cysharp.Threading.Tasks;
using System;

public class GameLogic : GameplayManagerClass 
{
    private bool _victory = false;
    private bool _gameOver = false;
    public bool GameOver => _gameOver;
    public bool Victory => _victory;

    public Action OnGameOver;
    public Action OnVictory;
    public Action OnRewardStart;

    public override void Initialize()
    {
        isInitialized = true;
    }

    public void ExecuteGameOver()
    {
        _gameOver = true;
        OnGameOver?.Invoke();
        ColorDebugLog.Log($"[{this.name}]: GAME OVER", System.Drawing.KnownColor.Cyan);
    }

    public void ExecuteVictory()
    {
        _victory = true;
        OnVictory?.Invoke();
        ColorDebugLog.Log($"[{this.name}]: : VICTORY!", System.Drawing.KnownColor.Lime);
    }

    public void ExecuteReward()
    {
        OnRewardStart?.Invoke();
        ColorDebugLog.Log($"[{this.name}]: Show and Grant rewards!", System.Drawing.KnownColor.Lime);
    }

    public async UniTask ExecuteRewardRecieved()
    {
        ColorDebugLog.Log($"[{this.name}]: ExecuteRewardRecieved", System.Drawing.KnownColor.Lime);
        await GameStateManager.Instance.ReturnToMissionPreparation();
    }
}