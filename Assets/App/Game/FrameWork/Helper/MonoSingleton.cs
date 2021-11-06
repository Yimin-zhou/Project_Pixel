using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FrameWork.Helper
{
    /// <summary>
    /// MonoBehaviourÀàµÄµ¥Àý
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoSingleton<T> where T : MonoBehaviour
    {
        private static T _inst;

        public static T Inst
        {
            get
            {
                if (_inst != null)
                {
                    return _inst;
                }
                T t = Object.FindObjectOfType<T>();
                if (t != null)
                {
                    _inst = t;
                }
                else
                {
                    GameObject go = new GameObject();
                    go.name = typeof(T).Name + "_handler";
                    Debug.LogError("there is a MonoSingeton that cant find parents,please fix it!");
                    _inst = go.AddComponent<T>();
                }
                return _inst;
            }
        }

        public void Destroy()
        {
            UnityEngine.Object.Destroy(_inst);
            _inst = null;
        }
    }
}
