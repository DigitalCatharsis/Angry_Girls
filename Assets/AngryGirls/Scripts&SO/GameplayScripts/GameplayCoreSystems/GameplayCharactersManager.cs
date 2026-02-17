using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Angry_Girls
{
    public class GameplayCharactersManager : GameplayManagerClass
    {
        private readonly List<CControl> _allCharacters = new();
        private readonly List<CControl> _playerCharacters = new();
        private readonly List<CControl> _enemyCharacters = new();
        private PoolManager _poolManager;
        private CharacterProfile[] _initialCharactersProfiles;

        public IReadOnlyList<CControl> AllCharacters => _allCharacters;
        public IReadOnlyList<CControl> PlayerCharacters => _playerCharacters;
        public IReadOnlyList<CControl> EnemyCharacters => _enemyCharacters;
        public CControl CurrentlyAttackingUnit { get; set; }
        public List<CControl> LaunchableCharacters => GetLaunchableCharacters();

        public event System.Action OnLaunchableCharactersChanged;


        public override void Initialize()
        {
            _poolManager = CoreManager.Instance.PoolManager;
            _initialCharactersProfiles = CoreManager.Instance.CharactersManager.CharactersData.SelectedCharactersPool.ToArray();

            //_initialPlayerSettings = new CharacterSettings[selectedProfiles.Count];

            //for (int i = 0; i < selectedProfiles.Count; i++)
            //{
            //    if (selectedProfiles[i] != null)
            //    {
            //        _initialPlayerSettings[i] = selectedProfiles[i].CharacterSettings;
            //    }
            //}

            isInitialized = true;
        }

        /// <summary>
        /// Notify listeners when launchable characters collection changes 
        /// (launch/death/reinit/swap)
        /// </summary>
        public void NotifyLaunchableCharactersChanged()
        {
            OnLaunchableCharactersChanged?.Invoke();
        }

        public void SpawnInitialPlayerCharacters(Transform[] spawnPositions)
        {
            for (int i = 0; i < _initialCharactersProfiles.Length && i < spawnPositions.Length; i++)
            {
                if (_initialCharactersProfiles[i] == null) continue;

                var poolObject = _poolManager.GetObject(
                    _initialCharactersProfiles[i].CharacterSettings.characterType,
                    spawnPositions[i].position,
                    Quaternion.identity
                );

                var character = poolObject.GetComponent<CControl>();
                character.playerOrAi = PlayerOrAi.Player;
                character.profile = _initialCharactersProfiles[i];

                RegisterCharacter(character, PlayerOrAi.Player);

                //    RegisterCharacter(character, PlayerOrAi.Player);
                //for (int i = 0; i < _initialPlayerSettings.Length && i < spawnPositions.Length; i++)
                //{
                //    if (_initialPlayerSettings[i] == null) continue;

                //    var poolObject = _poolManager.GetObject(
                //        _initialPlayerSettings[i].characterType,
                //        spawnPositions[i].position,
                //        Quaternion.identity
                //    );

                //    var character = poolObject.GetComponent<CControl>();
                //    character.playerOrAi = PlayerOrAi.Player;
                //    character.profile = _initialPlayerSettings[i];

                //    RegisterCharacter(character, PlayerOrAi.Player);
            }
        }

        public CControl SpawnEnemy(CharacterProfile profile, Vector3 position)
        {
            var poolObject = _poolManager.GetObject(
                profile.CharacterSettings.characterType,
                position,
                Quaternion.identity
            );

            var character = poolObject.GetComponent<CControl>();
            character.playerOrAi = PlayerOrAi.Bot;
            character.profile = profile;

            character.hasBeenLaunched = false;

            RegisterCharacter(character, PlayerOrAi.Bot);
            return character;
        }

        public void RegisterCharacter(CControl character, PlayerOrAi team)
        {
            _allCharacters.Add(character);
            if (team == PlayerOrAi.Player)
            {
                _playerCharacters.Add(character);
                NotifyLaunchableCharactersChanged(); 
            }
            else
                _enemyCharacters.Add(character);
        }

        public void UnregisterCharacter(CControl character)
        {
            _allCharacters.Remove(character);
            _playerCharacters.Remove(character);
            NotifyLaunchableCharactersChanged();
            _enemyCharacters.Remove(character);
        }

        public List<CControl> GetAliveCharacters(PlayerOrAi team)
        {
            return team == PlayerOrAi.Player
                ? _playerCharacters.FindAll(c => !c.isDead)
                : _enemyCharacters.FindAll(c => !c.isDead);
        }

        public List<CControl> GetLaunchableCharacters()
        {
            return _playerCharacters.FindAll(c => !c.isDead && !c.hasBeenLaunched);
        }

        public List<CControl> GetTurnOrder()
        {
            var order = new List<CControl>();
            order.AddRange(GetAliveCharacters(PlayerOrAi.Player));
            order.AddRange(GetAliveCharacters(PlayerOrAi.Bot));
            return order;
        }

        public void ReinitSurvivingCharacters()
        {
            var survivors = GetAliveCharacters(PlayerOrAi.Player);
            foreach (var character in survivors)
            {
                if (character.profile.CharacterSettings.unitType == UnitType.Air)
                    character.CharacterMovement.Rigidbody.isKinematic = false;

                character.CharacterMovement.Rigidbody.rotation = Quaternion.identity;
                character.gameObject.layer = LayerMask.NameToLayer("CharacterToLaunch");
                character.isUnitBehaviorIsAlternate = false;
                character.hasBeenLaunched = false;
                character.hasUsedAbility = false;
                character.hasFinishedLaunchingTurn = false;
                character.hasFinishedAlternateAttackTurn = true;
                character.canCheckGlobalBehavior = false;
            }

            NotifyLaunchableCharactersChanged();
        }

        public void SwapWithFirst(int index)
        {
            var launchable = LaunchableCharacters;
            if (launchable.Count <= 1 || index <= 0 || index >= launchable.Count)
                return;

            var firstChar = launchable[0];
            var otherChar = launchable[index];

            int firstIdx = _playerCharacters.IndexOf(firstChar);
            int otherIdx = _playerCharacters.IndexOf(otherChar);

            if (firstIdx == -1 || otherIdx == -1)
                return;

            (_playerCharacters[firstIdx], _playerCharacters[otherIdx]) = (_playerCharacters[otherIdx], _playerCharacters[firstIdx]);

            NotifyLaunchableCharactersChanged();
        }
    }
}