using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    public class DeathZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var chararacter = other.GetComponent<CControl>();
            if (chararacter != null)
            {
                chararacter.isDead = true;
            }
        }
    }
}