using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Security;

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
            //GetComponentInChildren<TextMeshPro>().text = string.Empty;
            //transform.root.parent = null;
            //transform.root.position = Vector3.zero;
            //transform.root.rotation = Quaternion.identity;

            //var particle = GetComponent<ParticleSystem>();

            //particle.Clear(true);

            transform.root.gameObject.SetActive(false);
        }

        public void ShowDamage(float damageAmount)
        {
            GetComponentInChildren<TextMeshPro>().text = damageAmount.ToString();
        }
    }
}