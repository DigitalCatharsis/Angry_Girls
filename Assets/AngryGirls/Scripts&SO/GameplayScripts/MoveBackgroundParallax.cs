using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Creates a parallax scrolling effect for background elements
    /// </summary>
    public class MoveBackgroundParallax : MonoBehaviour
    {
        public float speed;
        private float _offset;
        private Vector3 _startPosition;

        void Start()
        {
            _startPosition = transform.position;
            _offset = GetComponent<SpriteRenderer>().bounds.size.x / 4;
        }

        void Update()
        {
            transform.position = new Vector3(transform.position.x + speed * Time.deltaTime, transform.position.y, transform.position.z);

            if (transform.position.x < _startPosition.x - _offset)
            {
                transform.position = _startPosition;
            }
        }
    }
}