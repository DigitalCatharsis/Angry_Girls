using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace Angry_Girls
{
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
            ColorDebugLog.Log("GAME OVER", KnownColor.Cyan);
        }

        //TODO:
        public void EvecuteVictory()
        {
            throw new Exception("not implemented");
        }
    }
}
