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
            AttackAbilityData[] allData = Resources.LoadAll<AttackAbilityData>("AttackAbilitiesTemplates");
            var grouped = allData.GroupBy(d => d.attackType);

            //TODO: refresh this i nyour memory
            foreach (var group in grouped)
            {
                var attackType = group.Key;
                var launchPrep = group.FirstOrDefault(d => d.usage == AttackUsage.Launch);
                var alternatePrep = group.FirstOrDefault(d => d.usage == AttackUsage.Alternate);

                AttackAbility ability = CreateAbility(attackType, launchPrep,alternatePrep);
                if (ability != null)
                    _abilities[attackType] = ability;
            }
        }

        private AttackAbility CreateAbility( AttackType type,AttackAbilityData launchPrep, AttackAbilityData alternatePrep)
        {
            return type switch
            {
                AttackType.Fireball => new FireballAttack(launchPrep, alternatePrep),
                AttackType.SwordSpin => new SwordSpinAttack(launchPrep, alternatePrep),
                AttackType.HeadSpin => new HeadspinAttack(launchPrep, alternatePrep),
                AttackType.Uppercut => new UppercutAttack(launchPrep, alternatePrep),
                _ => null
            };
        }

        public AttackAbility GetAbility(AttackType type) =>
            _abilities.TryGetValue(type, out var ability) ? ability : null;

    }
}