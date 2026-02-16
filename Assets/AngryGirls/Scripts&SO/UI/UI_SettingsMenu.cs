using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// UI for adjusting audio settings
    /// </summary>
    public class UI_SettingsMenu : MonoBehaviour
    {
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider soundSlider;

        private void OnEnable()
        {
            musicSlider.value = CoreManager.Instance.SettingsManager.GetSettings().volumeMusic;
            soundSlider.value = CoreManager.Instance.SettingsManager.GetSettings().volumeSounds;

            musicSlider.onValueChanged.AddListener(CoreManager.Instance.SettingsManager.SetupMusicVolume);
            soundSlider.onValueChanged.AddListener(CoreManager.Instance.SettingsManager.SetupSoundsVolume);
        }

        private void OnDisable()
        {
            musicSlider.onValueChanged.RemoveListener(CoreManager.Instance.SettingsManager.SetupMusicVolume);
            soundSlider.onValueChanged.RemoveListener(CoreManager.Instance.SettingsManager.SetupSoundsVolume);
        }
    }
}