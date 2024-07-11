using System.Collections;
using UnityEngine;

namespace Angry_Girls
{
    public class MyExtentions : MonoBehaviour
    {
        #region Lerp
        //https://habr.com/ru/articles/318046/

        //position
        public void Lerp_Position(GameObject objectToOperate, Vector3 startPosition, Vector3 endPosition, float duration)
        {
            StartCoroutine(OnLerpPosition_Routine(objectToOperate, startPosition, endPosition, duration));
        }

        private IEnumerator OnLerpPosition_Routine(GameObject objectToOperate, Vector3 startPosition, Vector3 endPosition, float duration)
        {
            float time = 0;

            while (time < duration)
            {
                objectToOperate.transform.position = Vector3.Lerp(startPosition, endPosition, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            objectToOperate.transform.position = endPosition;
        }

        //Scale
        public void Lerp_Scale(GameObject objectToOperate, Vector3 startScale, Vector3 endScale, float duration)
        {
            StartCoroutine(OnLerpScale_Routine(objectToOperate, startScale, endScale, duration));
        }

        private IEnumerator OnLerpScale_Routine(GameObject objectToOperate, Vector3 startScale, Vector3 endScale, float duration)
        {
            float time = 0;

            while (time < duration)
            {
                objectToOperate.transform.localScale = Vector3.Lerp(startScale, endScale, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            objectToOperate.transform.localScale = endScale;
        }

        //Orthographic camera Size
        public void Lerp_OrthographicCamera_Size(Camera Camera, float startValue, float endValue, float duration)
        {
            StartCoroutine(OnLerp_OrthographicCamera_Size_Routine(Camera, startValue, endValue, duration));
        }

        private IEnumerator OnLerp_OrthographicCamera_Size_Routine(Camera Camera, float startValue, float endValue, float duration)
        {
            float time = 0;

            while (time < duration)
            {
                Camera.orthographicSize = Mathf.Lerp(startValue, endValue, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            Camera.orthographicSize = endValue;
        }
        #endregion
    }
}