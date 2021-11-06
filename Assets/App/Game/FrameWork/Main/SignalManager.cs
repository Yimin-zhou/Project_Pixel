using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FrameWork.Main
{
    public class SignalManager:IDisposable
    {
        private Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

        public void Subscribe<T>(T handler)
        {
            if (handler != null && handler is Delegate)
            {
                Delegate newHandler = handler as Delegate;
                Type type = handler.GetType();
                Delegate savedHandler;
                if (_handlers.TryGetValue(type, out savedHandler))
                {
                    foreach(var h in savedHandler.GetInvocationList())
                    {
                        if (h == newHandler)
                            return;
                    }
                    _handlers[type]= Delegate.Combine(savedHandler, newHandler);
                }
                else
                {
                    _handlers[type] = newHandler;
                }
            }
        }

        public void UnSubscibe<T>(T handler)
        {
            if (handler != null && handler is Delegate)
            {
                Delegate deleteHandler = handler as Delegate;
                Type type = handler.GetType();
                Delegate savedHandler;
                if (_handlers.TryGetValue(type, out savedHandler))
                {
                    bool canDelete = false;
                    foreach (var h in savedHandler.GetInvocationList())
                    {
                        if (h == deleteHandler)
                            canDelete = true;
                    }
                    if (canDelete)
                    {
                        _handlers[type] = Delegate.Remove(savedHandler, deleteHandler);
                        if (_handlers[type] == null)
                        {
                            _handlers.Remove(type);
                        }
                    }
                }
            }
        }

        public object[] Rasie<T>(params object[] args)
        {
            Type type = typeof(T);
            Delegate savedHandler;
            if (_handlers.TryGetValue(type, out savedHandler))
            {
                if (savedHandler != null)
                {
                    var invocationList = savedHandler.GetInvocationList();
                    object[] returns =  new object[invocationList.Length];
                    for(int i =0;i< invocationList.Length;i++)
                    {
                        returns[i] = invocationList[i].DynamicInvoke(args);
                    }
                    return returns;
                }
            }
            return null;
        }

        public void Dispose()
        {
            _handlers.Clear();
            _handlers = null;
        }
    }
}


