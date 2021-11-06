using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FrameWork.Main
{
    public class AsyncManager : MonoBehaviour
    {
        public void RunCoroutine(IEnumerator coroutine)
        {
            if(coroutine == null)
            {
                Debug.LogError("coroutine is null!");
                return;
            }
            StartCoroutine(coroutine);
        }

        public void RunCoroutine(IEnumerable coroutine)
        {
            if (coroutine == null)
            {
                Debug.LogError("coroutine is null!");
                return;
            }
            StartCoroutine(coroutine.GetEnumerator());
        }
    }
}

