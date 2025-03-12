using Angry_Girls;
using System.Drawing;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    private bool _gameOver = false;
    public bool GameOver => _gameOver;


    public void ExecuteGameOver()
    {
        _gameOver = true;
        GameLoader.Instance.gameLogic_UIManager.ShowGameoverUI();
        ColorDebugLog.Log("GAME OVER", KnownColor.Cyan);
    }
}
