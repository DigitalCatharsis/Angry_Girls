using Angry_Girls;
using System;

public class GameLogic : GameplayManagerClass 
{
    private bool _victory = false;
    private bool _gameOver = false;
    public bool GameOver => _gameOver;
    public bool Victory => _victory;

    public Action OnGameOver;
    public Action OnVictory;

    public override void Initialize()
    {
        isInitialized = true;
    }

    public void ExecuteGameOver()
    {
        _gameOver = true;
        OnGameOver?.Invoke();
        ColorDebugLog.Log("GAME OVER", System.Drawing.KnownColor.Cyan);
    }

    public void ExecuteVictory()
    {
        _victory = true;
        OnVictory?.Invoke();
        ColorDebugLog.Log("VICTORY!", System.Drawing.KnownColor.Lime);
    }

}