using Angry_Girls;
using System.Drawing;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    public bool gameOver = false;

    public void ExecuteGameOver()
    {
        GameLoader.Instance.gameLogic_UIManager.ShowGameoverUI();
        ColorDebugLog.Log("GAME OVER", KnownColor.Cyan);
    }
}
