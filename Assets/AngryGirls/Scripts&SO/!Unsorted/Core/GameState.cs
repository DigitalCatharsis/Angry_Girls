using System;
using UnityEngine;

namespace Angry_Girls
{
    public enum GameState
    {
        // Основные состояния
        StageSetup,     // Подготовка стадии (спавн, камеры)
        LaunchPhase,    // Фаза запуска персонажей (2 запуска В первый раз, остальные по одному)
        AlternatePhase, // Фаза поочередных атак
        StageComplete,  // Все враги на стадии побеждены
        Victory,        // Все стадии пройдены
        Defeat,         // Персонажи игрока мертвы

        // Дополнительные состояния
        Paused,        // Игра на паузе
        GameStart,      // Начало игры (инициализация)
    }

    public static class GameStateEvent
    {
        public static event Action<GameState> OnStateChanged;

        public static void Trigger(GameState newState)
        {
            Debug.Log($"GameState changed to: {newState}");
            OnStateChanged?.Invoke(newState);
        }
    }
}