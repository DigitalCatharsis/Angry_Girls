using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    public class BattleHUDUI : MonoBehaviour
    {
        [Header("Текущая фаза")]
        public TextMeshProUGUI phaseText;

        [Header("Текущий атакующий")]
        public TextMeshProUGUI attackerNameText;

        [Header("Оставшиеся юниты")]
        public TextMeshProUGUI enemyCountText;
        public TextMeshProUGUI playerCountText;

        [Header("Стадия")]
        public TextMeshProUGUI stageText;

        private GameFlowController _flow => GameLoader.Instance.gameFlowController;
        private TurnManager _turnManager => GameLoader.Instance.turnManager;
        private CharacterManager _charManager => GameLoader.Instance.characterManager;
        private StageManager _stageManager => GameLoader.Instance.stageManager;

        private void Start()
        {
            UpdateHUD();
        }

        private void Update()
        {
            UpdateHUD();
        }

        private void UpdateHUD()
        {
            if (_flow == null || _charManager == null || _stageManager == null)
                return;

            phaseText.text = $"Фаза: <b>{_flow.CurrentState}</b>";

            var attacker = _turnManager?.currentAttackingUnit;
            if (attacker != null)
                attackerNameText.text = $"Ходит: <b>{attacker.name}</b>";
            else
                attackerNameText.text = "Ходит: <i>ожидание...</i>";

            int aliveEnemies = _charManager.enemyCharacters.FindAll(c => !c.isDead).Count;
            int alivePlayers = _charManager.playableCharacters.FindAll(c => !c.isDead).Count;

            enemyCountText.text = $"Врагов: <b>{aliveEnemies}</b>";
            playerCountText.text = $"Героев: <b>{alivePlayers}</b>";

            stageText.text = $"Стадия: <b>{_stageManager.CurrentStageIndex + 1}</b>";
        }
    }
}
