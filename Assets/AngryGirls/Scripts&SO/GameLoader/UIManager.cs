using AYellowpaper.SerializedCollections;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private SerializedDictionary<CControl, Slider> _healtBar_Dict;
        public void CreateHealthBar(CControl control)
        {
            var healthBar = Resources.Load("HealthBar") as GameObject;
            var HealthBarGameobject = Instantiate(healthBar);
            control.healthSlider = HealthBarGameobject.GetComponentInChildren<Slider>();
            _healtBar_Dict.Add(control, control.healthSlider);
            InitHealthBar(control);
        }

        public void RemoveHealthBar(CControl control)
        {
            _healtBar_Dict.Remove(control);
        }

        private void InitHealthBar(CControl control)
        {
            control.healthSlider.maxValue = control.currentHealth;
            UpdateHealthBarValueAndVision(control);
        }

        public void UpdateHealthBarValueAndVision(CControl control)
        {
            control.healthSlider.value = control.currentHealth;
            if (control.currentHealth <= 0)
            {
                _healtBar_Dict[control].gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            foreach (var elem in _healtBar_Dict)
            {
                elem.Value.transform.root.position = new Vector3(
                    0, 
                    elem.Key.boxCollider.bounds.max.y + 0.35f, 
                    elem.Key.boxCollider.bounds.center.z
                    ) ;

            }
        }
    }
}