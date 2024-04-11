using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{

    public class LayerChanger : MonoBehaviour
    {
        public MMP_Layers layerType;
        public bool changeAllChildren;

        public void ChangeLayer(Dictionary<string,int> layerDic)
        {
            if (!changeAllChildren)
            {
                Debug.Log($"{gameObject.name} changing layer {layerType}");
                this.gameObject.layer = layerDic[layerType.ToString()];
            }
            else
            {
                Transform[] arr =this.gameObject.GetComponentsInChildren<Transform>();

                foreach (Transform t in arr)
                {
                    Debug.Log($"{t.gameObject.name} changing layer {layerType}");
                    t.gameObject.layer = layerDic[layerType.ToString()];
                }
            }
        }
    }
}