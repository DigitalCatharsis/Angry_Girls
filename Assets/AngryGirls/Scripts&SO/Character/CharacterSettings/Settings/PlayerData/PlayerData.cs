using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

namespace Angry_Girls
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Angry_Girls/CharacterSettings/PlayerData")]
    public class PlayerData : ScriptableObject
    {
        public CharacterSettings[] selectedCharacters = new CharacterSettings[6]; // �������� 6 ��������� ����������
        public List<CharacterSettings> avaibleCharacterPool = new List<CharacterSettings>(); 

        public bool GetFromAvaible(int index)
        {
            if (avaibleCharacterPool.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < selectedCharacters.Count(); i++)
            {
                if (selectedCharacters[i] == null)
                {
                    selectedCharacters[i] = avaibleCharacterPool[index];
                    ColorDebugLog.Log($"Selected added {avaibleCharacterPool[index]}", KnownColor.Yellow);
                    ColorDebugLog.Log($"Avaible removed {avaibleCharacterPool[index]}", KnownColor.Yellow);
                    avaibleCharacterPool[index] = null;

                    //SortSelectedCharacters(ref selectedCharacters);
                    SortAndRemoveNullsFromAvailableCharacters(ref avaibleCharacterPool);
                    return true;
                }
            }

            return false;
        }

        public void RemoveFromSelected(int index)
        {
            if (selectedCharacters[index] == null)
            {
                return;
            }

            avaibleCharacterPool.Add(selectedCharacters[index]);
            selectedCharacters[index] = null;
            SortSelectedCharacters(ref selectedCharacters);
            //SortAndRemoveNullsFromAvailableCharacters(ref avaibleCharacterPool);
        }

        // ��������� ������ selectedCharacters ���, ����� ������� ��� �������� ��������, ����� ������.
        public void SortSelectedCharacters(ref CharacterSettings[] collection)
        {
            // ������� ����� ��������������� ����� ������
            var sortedCollection = collection.OrderBy(item => item != null ? 0 : 1).ToArray();

            // ����������� ��������������� ������ ������� ������������ ����������
            collection = sortedCollection;
        }

        // ��������� ������ avaibleCharacterPool ���, ����� ������� ��� �������� ��������, ����� ������� ������.
        public void SortAndRemoveNullsFromAvailableCharacters(ref List<CharacterSettings> collection)
        {
            // ������� ���������, ����� ���������, ����� �������� ������ �������� ��������
            var sortedCollection = collection.Where(item => item != null).OrderBy(item => item != null ? 0 : 1).ToList();

            // ����������� ��������������� ������ ������� ������������ ����������
            collection = sortedCollection;
        }
    }
}