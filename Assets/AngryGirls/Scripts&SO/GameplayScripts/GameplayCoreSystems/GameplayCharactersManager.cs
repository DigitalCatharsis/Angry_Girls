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
            if (spawnPositions == null)
            {
                Debug.LogError("GameplayCharactersManager: Player spawn positions are not configured.");
                return;
            }

            for (int i = 0; i < _initialCharactersProfiles.Length && i < spawnPositions.Length; i++)
            {
                if (_initialCharactersProfiles[i] == null || spawnPositions[i] == null)
                    continue;

                var spawnTransform = spawnPositions[i];
                var spawnRotation = GetXYSpawnRotation(spawnTransform, PlayerOrAi.Player);

                var poolObject = _poolManager.GetObject(
                    _initialCharactersProfiles[i].CharacterSettings.characterType,
                    spawnTransform.position,
                    spawnRotation);

                if (poolObject == null)
                    continue;

                var character = poolObject.GetComponent<CControl>();
                character.playerOrAi = PlayerOrAi.Player;
                character.profile = _initialCharactersProfiles[i];

                RegisterCharacter(character, PlayerOrAi.Player);
                character.InitializeCharacter();
            }
        }

        public CControl SpawnEnemy(CharacterProfile profile, Vector3 position)
        {
            return SpawnEnemyInternal(profile, position, GetXYSpawnRotation(null, PlayerOrAi.Bot));
        }

        public CControl SpawnEnemy(CharacterProfile profile, Transform spawnTransform)
        {
            if (profile == null || profile.CharacterSettings == null)
            {
                Debug.LogError("GameplayCharactersManager: Cannot spawn an enemy with an invalid profile.");
                return null;
            }

            Vector3 position = spawnTransform != null ? spawnTransform.position : Vector3.zero;
            Quaternion rotation = GetXYSpawnRotation(spawnTransform, PlayerOrAi.Bot);
            return SpawnEnemyInternal(profile, position, rotation);
        }

        private CControl SpawnEnemyInternal(CharacterProfile profile, Vector3 position, Quaternion rotation)
        {
            if (profile == null || profile.CharacterSettings == null)
            {
                Debug.LogError("GameplayCharactersManager: Cannot spawn an enemy with an invalid profile.");
                return null;
            }

            var poolObject = _poolManager.GetObject(
                profile.CharacterSettings.characterType,
                position,
                rotation);

            if (poolObject == null)
                return null;

            var character = poolObject.GetComponent<CControl>();
            character.playerOrAi = PlayerOrAi.Bot;
            character.profile = profile;

            RegisterCharacter(character, PlayerOrAi.Bot);
            character.InitializeCharacter();
            return character;
        }

        private static Quaternion GetXYSpawnRotation(Transform spawnTransform, PlayerOrAi team)
        {
            // A spawn point can explicitly face left or right by its forward direction on world X.
            // Legacy spawn points facing world Z fall back to the team's default XY orientation.
            float forwardX = spawnTransform != null ? spawnTransform.forward.x : 0f;

            if (Mathf.Abs(forwardX) < 0.5f)
                forwardX = team == PlayerOrAi.Player ? 1f : -1f;

            return Quaternion.Euler(0f, forwardX > 0f ? 90f : -90f, 0f);
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

                character.CharacterMovement.Rigidbody.rotation = Quaternion.Euler(0f, 90f, 0f);
                character.gameObject.layer = LayerMask.NameToLayer("CharacterToLaunch");
                character.isBehaviorAlternate = false;
                character.hasBeenLaunched = false;
                //character.hasUsedAbility = false;
                character.hasFinishedLaunchingTurn = false;
                character.hasFinishedAlternateAttackTurn = true;
                character.canCheckGlobalBehavior = false;
                character.SetAttackBoolfalse();
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
