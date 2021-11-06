using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FrameWork.Helper
{
    /// <summary>
    /// 纯C#单例，要求T只要有一个空参数构造函数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> where T : class,IDisposable,new()
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
                _inst = new T();
                return _inst;
            }
        }

        public void Destroy()
        {
            _inst.Dispose();
            _inst = null;
        }
    }
}

