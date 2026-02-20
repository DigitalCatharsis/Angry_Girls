using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Angry_Girls
{
    public class AttackAbilityManager : GameplayManagerClass
    {
        private Dictionary<AttackType, AttackAbility> _abilities = new();

        public override void Initialize()
        {
            LoadAbilities();
        }

        private void LoadAbilities()
        {
            AttackAbilityData[] allData = Resources.LoadAll<AttackAbilityData>("AttacksAbilities");
            var grouped = allData.GroupBy(d => d.attackType);

            //TODO: refresh this i nyour memory
            foreach (var group in grouped)
            {
                var attackType = group.Key;
                var launchPrep = group.FirstOrDefault(d => d.usage == AttackUsage.Launch && d.phase == AttackPhase.Prep);
                var launchFinish = group.FirstOrDefault(d => d.usage == AttackUsage.Launch && d.phase == AttackPhase.Finish);
                var alternatePrep = group.FirstOrDefault(d => d.usage == AttackUsage.Alternate && d.phase == AttackPhase.Prep);
                var alternateFinish = group.FirstOrDefault(d => d.usage == AttackUsage.Alternate && d.phase == AttackPhase.Finish);

                if (launchPrep == null || launchFinish == null || alternatePrep == null || alternateFinish == null)
                {
                    Debug.LogError($"Missing data for attack type {attackType}. All four combinations required.");
                    continue;
                }

                AttackAbility ability = CreateAbility(attackType, launchPrep, launchFinish, alternatePrep, alternateFinish);
                if (ability != null)
                    _abilities[attackType] = ability;
            }
        }

        private AttackAbility CreateAbility(
            AttackType type,
            AttackAbilityData launchPrep,
            AttackAbilityData launchFinish,
            AttackAbilityData alternatePrep,
            AttackAbilityData alternateFinish)
        {
            return type switch
            {
                AttackType.Uppercut => new UppercutAttack(launchPrep, launchFinish, alternatePrep, alternateFinish),
                AttackType.Fireball => new FireballAttack(launchPrep, launchFinish, alternatePrep, alternateFinish),
                AttackType.HeadSpin => new HeadspinAttack(launchPrep, launchFinish, alternatePrep, alternateFinish),
                AttackType.SwordSpin => new SwordSpinAttack(launchPrep, launchFinish, alternatePrep, alternateFinish),
                _ => null
            };
        }

        public AttackAbility GetAbility(AttackType type) =>
            _abilities.TryGetValue(type, out var ability) ? ability : null;

    }
}