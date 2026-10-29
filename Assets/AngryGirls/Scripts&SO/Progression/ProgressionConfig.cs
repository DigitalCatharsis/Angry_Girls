using System;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Growth of a single stat with the character level.
    /// Multiplier = 1 + linear * (level - 1) + quadratic * (level - 1)^2.
    /// </summary>
    [Serializable]
    public struct StatGrowthCurve
    {
        [Tooltip("Linear growth per level above 1. 0.1 means +10% of the base stat per level.")]
        public float linear;

        [Tooltip("Quadratic growth, makes the higher levels give slightly more than the early ones.")]
        public float quadratic;

        public StatGrowthCurve(float linear, float quadratic)
        {
            this.linear = linear;
            this.quadratic = quadratic;
        }

        public float GetMultiplier(int level)
        {
            var steps = Mathf.Max(0, level - 1);
            return 1f + linear * steps + quadratic * steps * steps;
        }
    }

    /// <summary>
    /// How a mission difficulty changes the enemies and the experience reward.
    /// </summary>
    [Serializable]
    public class DifficultyTuning
    {
        public MissionDifficulty difficulty;

        [Tooltip("Added to the base enemy level of the mission.")]
        [Min(0)] public int enemyLevelOffset;

        [Tooltip("Extra multiplier applied to the level-scaled enemy health.")]
        [Min(0.1f)] public float enemyHealthMultiplier = 1f;

        [Tooltip("Extra multiplier applied to the level-scaled enemy damage.")]
        [Min(0.1f)] public float enemyDamageMultiplier = 1f;

        [Tooltip("Multiplier of the experience reward of the mission.")]
        [Min(0.1f)] public float xpMultiplier = 1f;
    }

    /// <summary>
    /// All tunable numbers of the progression system: levels, experience, stat growth,
    /// mission experience rewards and the difficulty scaling of the enemies.
    /// The class contains only pure calculations, runtime state lives in CharacterProfile.
    /// </summary>
    [CreateAssetMenu(fileName = "ProgressionConfig", menuName = "Angry_Girls/ProgressionConfig")]
    public class ProgressionConfig : ScriptableObject
    {
        [Header("Levels")]
        [SerializeField, Min(2)] private int _maxLevel = 30;

        [Header("XP to the next level = xpBase * level ^ xpExponent (rounded)")]
        [SerializeField, Min(1f)] private float _xpBase = 100f;
        [SerializeField, Min(0.1f)] private float _xpExponent = 1.5f;
        [SerializeField, Min(1)] private int _xpRounding = 5;

        [Header("Stat growth (multiplier of the base stats of the character)")]
        [SerializeField] private StatGrowthCurve _healthGrowth = new(0.10f, 0.0015f);
        [SerializeField] private StatGrowthCurve _damageGrowth = new(0.07f, 0.0010f);

        [Header("Mission XP = missionBaseXp * enemyLevel ^ missionXpExponent * difficulty * mission")]
        [SerializeField, Min(1f)] private float _missionBaseXp = 60f;
        [SerializeField, Min(0.1f)] private float _missionXpExponent = 1.4f;

        [Header("XP modifiers")]
        [Tooltip("XP change per level of difference between the enemies and the character (enemy level minus character level).")]
        [SerializeField, Min(0f)] private float _levelGapXpPerLevel = 0.12f;
        [SerializeField, Min(0f)] private float _minLevelGapXpFactor = 0.2f;
        [SerializeField, Min(1f)] private float _maxLevelGapXpFactor = 1.4f;
        [Tooltip("Bonus XP for completing a mission difficulty for the first time.")]
        [SerializeField, Min(0f)] private float _firstClearXpBonus = 0.5f;
        [Tooltip("Bonus XP for characters that are alive at the end of the mission.")]
        [SerializeField, Min(0f)] private float _survivorXpBonus = 0.25f;

        [Header("New characters")]
        [Tooltip("New characters join at the average roster level plus this offset (never below level 1).")]
        [SerializeField] private int _recruitLevelOffset = -2;

        [Header("Difficulty")]
        [SerializeField] private DifficultyTuning[] _difficulties =
        {
            new DifficultyTuning { difficulty = MissionDifficulty.Easy, enemyLevelOffset = 0, enemyHealthMultiplier = 1f, enemyDamageMultiplier = 1f, xpMultiplier = 1f },
            new DifficultyTuning { difficulty = MissionDifficulty.Normal, enemyLevelOffset = 4, enemyHealthMultiplier = 1.2f, enemyDamageMultiplier = 1.15f, xpMultiplier = 1.15f },
            new DifficultyTuning { difficulty = MissionDifficulty.Hard, enemyLevelOffset = 9, enemyHealthMultiplier = 1.45f, enemyDamageMultiplier = 1.3f, xpMultiplier = 1.3f },
        };

        public int MaxLevel => _maxLevel;
        public float FirstClearXpBonus => _firstClearXpBonus;
        public float SurvivorXpBonus => _survivorXpBonus;

        public int ClampLevel(int level) => Mathf.Clamp(level, 1, _maxLevel);

        /// <summary>
        /// Experience needed to advance from the given level to the next one. Zero on the maximum level.
        /// </summary>
        public int GetXpToNextLevel(int level)
        {
            if (level >= _maxLevel) return 0;

            var raw = _xpBase * Mathf.Pow(Mathf.Max(1, level), _xpExponent);
            return Mathf.Max(_xpRounding, Mathf.RoundToInt(raw / _xpRounding) * _xpRounding);
        }

        /// <summary>
        /// Total experience a character needs to collect from level 1 to reach the given level.
        /// </summary>
        public long GetCumulativeXpToReach(int level)
        {
            long total = 0;
            for (var current = 1; current < Mathf.Min(level, _maxLevel); current++)
            {
                total += GetXpToNextLevel(current);
            }

            return total;
        }

        public float GetHealthMultiplier(int level) => _healthGrowth.GetMultiplier(ClampLevel(level));

        public float GetDamageMultiplier(int level) => _damageGrowth.GetMultiplier(ClampLevel(level));

        /// <summary>
        /// Base stats of a character scaled to the given level and optional extra multipliers (enemy difficulty).
        /// </summary>
        public CharactersStatsBase CalculateLevelStats(
            CharactersStatsBase baseStats,
            int level,
            float healthMultiplier = 1f,
            float damageMultiplier = 1f)
        {
            if (baseStats == null) return new CharactersStatsBase();

            return new CharactersStatsBase
            {
                health = baseStats.health * GetHealthMultiplier(level) * healthMultiplier,
                damage = baseStats.damage * GetDamageMultiplier(level) * damageMultiplier
            };
        }

        public DifficultyTuning GetDifficulty(MissionDifficulty difficulty)
        {
            if (_difficulties != null)
            {
                foreach (var tuning in _difficulties)
                {
                    if (tuning != null && tuning.difficulty == difficulty) return tuning;
                }
            }

            return new DifficultyTuning { difficulty = difficulty };
        }

        /// <summary>
        /// Level of the enemies of a mission on the given difficulty.
        /// </summary>
        public int GetEnemyLevel(int baseEnemyLevel, MissionDifficulty difficulty)
        {
            return ClampLevel(Mathf.Max(1, baseEnemyLevel) + GetDifficulty(difficulty).enemyLevelOffset);
        }

        /// <summary>
        /// Experience of a mission before the character specific modifiers (used for UI estimates).
        /// </summary>
        public int CalculateBaseMissionXp(int enemyLevel, MissionDifficulty difficulty, float missionXpMultiplier)
        {
            var xp = _missionBaseXp * Mathf.Pow(Mathf.Max(1, enemyLevel), _missionXpExponent);
            xp *= GetDifficulty(difficulty).xpMultiplier * Mathf.Max(0.01f, missionXpMultiplier);
            return Mathf.Max(1, Mathf.RoundToInt(xp));
        }

        /// <summary>
        /// Experience a single character receives for a completed mission.
        /// </summary>
        public int CalculateMissionXp(
            int enemyLevel,
            int characterLevel,
            MissionDifficulty difficulty,
            float missionXpMultiplier,
            bool isFirstClear,
            bool survived)
        {
            var xp = (float)CalculateBaseMissionXp(enemyLevel, difficulty, missionXpMultiplier);

            var gapFactor = Mathf.Clamp(
                1f + _levelGapXpPerLevel * (enemyLevel - characterLevel),
                _minLevelGapXpFactor,
                _maxLevelGapXpFactor);
            xp *= gapFactor;

            if (isFirstClear) xp *= 1f + _firstClearXpBonus;
            if (survived) xp *= 1f + _survivorXpBonus;

            return Mathf.Max(1, Mathf.RoundToInt(xp));
        }

        /// <summary>
        /// Level of a newly obtained character, based on the levels of the current roster.
        /// </summary>
        public int GetRecruitLevel(IEnumerable<int> rosterLevels)
        {
            var sum = 0;
            var count = 0;

            if (rosterLevels != null)
            {
                foreach (var level in rosterLevels)
                {
                    sum += level;
                    count++;
                }
            }

            if (count == 0) return 1;

            var average = Mathf.RoundToInt((float)sum / count);
            return ClampLevel(average + _recruitLevelOffset);
        }
    }
}
