using System;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Describes the mission that has just been completed.
    /// </summary>
    public readonly struct MissionXpContext
    {
        public readonly int EnemyLevel;
        public readonly MissionDifficulty Difficulty;
        public readonly float MissionXpMultiplier;
        public readonly bool IsFirstClear;

        public MissionXpContext(int enemyLevel, MissionDifficulty difficulty, float missionXpMultiplier, bool isFirstClear)
        {
            EnemyLevel = enemyLevel;
            Difficulty = difficulty;
            MissionXpMultiplier = missionXpMultiplier;
            IsFirstClear = isFirstClear;
        }
    }

    /// <summary>
    /// Before and after snapshot of a character that received experience.
    /// </summary>
    public sealed class CharacterXpResult
    {
        public CharacterProfile Profile { get; set; }
        public int XpGained { get; set; }
        public bool Survived { get; set; }

        public int LevelBefore { get; set; }
        public int XpBefore { get; set; }
        public int XpToNextBefore { get; set; }

        public int LevelAfter { get; set; }
        public int XpAfter { get; set; }
        public int XpToNextAfter { get; set; }

        public float HealthGain { get; set; }
        public float DamageGain { get; set; }

        public bool LeveledUp => LevelAfter > LevelBefore;
    }

    /// <summary>
    /// Awards experience to the characters after a mission. Contains no UI and no scene dependencies.
    /// </summary>
    public sealed class ExperienceService
    {
        private readonly ProgressionConfig _config;

        public ExperienceService(ProgressionConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Gives every team member the experience of the mission. Survivors receive a bonus.
        /// </summary>
        public List<CharacterXpResult> AwardMissionExperience(
            IEnumerable<CharacterProfile> team,
            ICollection<CharacterProfile> survivors,
            in MissionXpContext context)
        {
            var results = new List<CharacterXpResult>();
            if (team == null) return results;

            var processed = new HashSet<CharacterProfile>();

            foreach (var profile in team)
            {
                if (profile == null || profile.CharacterSettings == null || !processed.Add(profile))
                    continue;

                var survived = survivors != null && survivors.Contains(profile);
                var xp = _config.CalculateMissionXp(
                    context.EnemyLevel,
                    profile.Level,
                    context.Difficulty,
                    context.MissionXpMultiplier,
                    context.IsFirstClear,
                    survived);

                results.Add(AwardExperience(profile, xp, survived));
            }

            return results;
        }

        /// <summary>
        /// Gives experience to one character and returns the before and after snapshot.
        /// </summary>
        public CharacterXpResult AwardExperience(CharacterProfile profile, int xp, bool survived = true)
        {
            var levelBefore = profile.Level;
            var xpBefore = profile.Experience;
            var xpToNextBefore = profile.XpToNextLevel;
            var statsBefore = profile.GetSettingsStats;

            profile.AddExperience(xp);

            var statsAfter = profile.GetSettingsStats;

            return new CharacterXpResult
            {
                Profile = profile,
                XpGained = xp,
                Survived = survived,
                LevelBefore = levelBefore,
                XpBefore = xpBefore,
                XpToNextBefore = xpToNextBefore,
                LevelAfter = profile.Level,
                XpAfter = profile.Experience,
                XpToNextAfter = profile.XpToNextLevel,
                HealthGain = statsAfter.health - statsBefore.health,
                DamageGain = statsAfter.damage - statsBefore.damage
            };
        }

        /// <summary>
        /// Level of a character that joins the roster (reward), based on the current roster.
        /// </summary>
        public int GetRecruitLevel(IEnumerable<CharacterProfile> roster)
        {
            var levels = new List<int>();

            if (roster != null)
            {
                foreach (var profile in roster)
                {
                    if (profile != null) levels.Add(profile.Level);
                }
            }

            return _config.GetRecruitLevel(levels);
        }
    }
}
