using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Adds a subtle organic flicker to a light (torches, candles, braziers).
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class LightFlicker : MonoBehaviour
    {
        [Tooltip("Relative intensity swing, 0.2 means plus or minus 20 percent.")]
        [SerializeField, Range(0f, 1f)] private float _amplitude = 0.2f;

        [SerializeField, Min(0.01f)] private float _speed = 6f;

        private Light _light;
        private float _baseIntensity;
        private float _seed;

        private void Awake()
        {
            _light = GetComponent<Light>();
            _baseIntensity = _light.intensity;
            _seed = Random.value * 100f;
        }

        private void Update()
        {
            var noise = Mathf.PerlinNoise(_seed, Time.time * _speed) - 0.5f;
            _light.intensity = _baseIntensity * (1f + noise * 2f * _amplitude);
        }
    }
}
