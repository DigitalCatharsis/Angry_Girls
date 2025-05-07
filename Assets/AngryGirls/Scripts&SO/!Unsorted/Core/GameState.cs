using System;
using UnityEngine;

namespace Angry_Girls
{
    public enum GameState
    {
        // �������� ���������
        StageSetup,     // ���������� ������ (�����, ������)
        LaunchPhase,    // ���� ������� ���������� (2 ������� � ������ ���, ��������� �� ������)
        AlternatePhase, // ���� ����������� ����
        StageComplete,  // ��� ����� �� ������ ���������
        Victory,        // ��� ������ ��������
        Defeat,         // ��������� ������ ������

        // �������������� ���������
        Paused,        // ���� �� �����
        GameStart,      // ������ ���� (�������������)
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