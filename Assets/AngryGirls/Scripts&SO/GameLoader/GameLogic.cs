using Angry_Girls;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    private bool _victory = false;
    private bool _gameOver = false;
    public bool GameOver => _gameOver;
    public bool Victory => _victory;

    public void ExecuteGameOver()
    {
        _gameOver = true;
        GameLoader.Instance.gameLogic_UIManager.ShowGameoverUI();
        ColorDebugLog.Log("GAME OVER", System.Drawing.KnownColor.Cyan);
    }

    public void ExecuteVictory()
    {
        _victory = true;
        GameLoader.Instance.gameLogic_UIManager.ShowVictoryUI();
        ColorDebugLog.Log("VICTORY!", System.Drawing.KnownColor.Lime);
    }
}