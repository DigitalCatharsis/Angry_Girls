using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using DG.Tweening;

namespace Angry_Girls
{
    public class Test_ShowDamageAmount : MonoBehaviour
    {
        [SerializeField] float _timeToLive = 1f;
        private void OnEnable()
        {
            transform.DOMove(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), 1);
            StartCoroutine(DisableItself());
        }

        private IEnumerator DisableItself()
        {
            yield return new WaitForSeconds(_timeToLive);
            transform.root.gameObject.SetActive(false);
        }

        public void ShowDamage(float damageAmount)
        {
            GetComponentInChildren<TextMeshPro>().text = damageAmount.ToString();
        }
    }
}