using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Component attached to the confirmation panel prefab.
    /// All references must be assigned manually in the prefab inspector.
    /// </summary>
    public class ConfirmationPanel : MonoBehaviour
    {
        [SerializeField] private Button _yesButton;
        [SerializeField] private Button _noButton;
        [SerializeField] private TextMeshProUGUI _messageText;

        public Button YesButton => _yesButton;
        public Button NoButton => _noButton;
        public TextMeshProUGUI MessageText => _messageText;
    }
}