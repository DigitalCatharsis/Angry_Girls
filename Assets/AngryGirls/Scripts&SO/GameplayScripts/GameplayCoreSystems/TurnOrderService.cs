using System.Collections.Generic;

namespace Angry_Girls
{
    /// <summary>
    /// Service for building and applying turn order rules during alternate phase.
    /// Centralizes logic for player/enemy grouping and turn sequence construction.
    /// Delegates actual turn execution to TurnManager.
    /// </summary>
    public class TurnOrderService : GameplayManagerClass
    {
        private GameplayCharactersManager _charactersManager;

        public override void Initialize()
        {
            isInitialized = true;
            GameplayCoreManager.Instance.OnInitialized += LateInitialize;
        }

        private void LateInitialize()
        {
            _charactersManager = GameplayCoreManager.Instance.GameplayCharactersManager;
            GameplayCoreManager.Instance.OnInitialized -= LateInitialize;
        }

        public List<CControl> BuildTurnOrder()
        {
            return _charactersManager.GetTurnOrder();
        }

        public List<CControl> ApplyGroupingRules(List<CControl> order)
        {
            var result = new List<CControl>();
            for (int i = 0; i < order.Count; i++)
            {
                result.Add(order[i]);

                if ((i + 1) < order.Count &&
                    order[i].playerOrAi == PlayerOrAi.Player &&
                    order[i + 1].playerOrAi == PlayerOrAi.Bot)
                {
                    i++;
                }
            }
            return result;
        }
    }
}