using UnityEngine;
using System;

namespace Angry_Girls
{
    [Serializable]
    public class CharacteProfileInfo : MonoBehaviour
    {
        [Header("CharacteProfileInfo Information")]
        [Space(10)]
        [Header("Inventory Stats")]
        [ShowOnly][SerializeField] private float _inDamage;
        [ShowOnly][SerializeField] private float _inHealth;

        [Space(5)]                              
        [Header("Base (CharacterSettings) Stats")]
        [ShowOnly][SerializeField] private float _csDamage;
        [ShowOnly][SerializeField] private float _csHealth;
                                                
        [Space(5)]                              
        [Header("Bonus Stats")]                 
        [ShowOnly][SerializeField] private float _bDamage;
        [ShowOnly][SerializeField] private float _bHealth;
                                                
        [Space(5)]                              
        [Header("Result Stats")]                
        [ShowOnly][SerializeField] private float _rDamage;
        [ShowOnly][SerializeField] private float _rHealth;

        [Space(5)]
        [Header("Inventory")]
        [ShowOnly][SerializeField] private string _weapon;
        [ShowOnly][SerializeField] private string _armor;
        [ShowOnly][SerializeField] private string _accessory1;
        [ShowOnly][SerializeField] private string _accessory2;

        private CharacterProfile _profile;
        private CharactersStatsBase _invStats;
        private CharactersStatsBase _CharacterSettingsStats;
        private CharactersStatsBase _bonusStats;
        private CharactersStatsBase _resultStats;


        public void InitAndRun(CharacterProfile profile)
        {
            _profile = profile;


            _invStats = _profile.GetItemsStats;
            _CharacterSettingsStats = _profile.GetSettingsStats;
            _bonusStats = _profile.BonusStats;
            _resultStats = _profile.GetCurrentStats;
        }

        private void Update()
        {
            if (_profile == null) { return; }

            UpdateCharacterProdileInfo();
        }

        private void UpdateCharacterProdileInfo()
        {
            _weapon = _profile.Weapon?.ToString();
            _armor = _profile.Armor?.ToString();
            _accessory1 = _profile.Accessory1?.ToString();
            _accessory2 = _profile.Accessory2?.ToString();

            _inDamage = _invStats.damage;
            _inHealth = _invStats.health;

            _csDamage = _CharacterSettingsStats.damage;
            _csHealth = _CharacterSettingsStats.health;

            _bDamage = _bonusStats.damage;
            _bHealth = _bonusStats.health;

            _rDamage = _resultStats.damage;
            _rHealth = _resultStats.health;
        }
    }
}