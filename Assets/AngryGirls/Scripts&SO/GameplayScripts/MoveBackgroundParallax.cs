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
            _offset = GetComponent<SpriteRenderer>().bounds.size.z / 4;
        }

        void Update()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + speed * Time.deltaTime);

            if (transform.position.z < _startPosition.z - _offset)
            {
                transform.position = _startPosition;
            }
        }
    }
}