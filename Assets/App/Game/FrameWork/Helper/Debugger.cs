using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;

public class Debug
{
    private const string UNITY_EDITOR = "UNITY_EDITOR";

#if DEVELOPMENT_BUILD
#else
    [Conditional(UNITY_EDITOR)]
#endif
    public static void Log(object message, Object context = null)
    {
        UnityDebug.Log(message, context);
    }

#if DEVELOPMENT_BUILD
#else
    [Conditional(UNITY_EDITOR)]
#endif
    public static void LogError(object message, Object context = null)
    {
        UnityDebug.LogError(message, context);
    }

#if DEVELOPMENT_BUILD
#else
    [Conditional(UNITY_EDITOR)]
#endif
    public static void LogWarning(object message, Object context = null)
    {
        UnityDebug.LogWarning(message, context);
    }

#if DEVELOPMENT_BUILD
#else
    [Conditional(UNITY_EDITOR)]
#endif
    public static void DrawRay(Vector3 start, Vector3 dir, Color color)
    {
        UnityDebug.DrawRay(start, dir, color);
    }


#if DEVELOPMENT_BUILD
#else
    [Conditional(UNITY_EDITOR)]
#endif
    public static void DrawLine(Vector3 start, Vector3 dir, Color color)
    {
        UnityDebug.DrawLine(start, dir, color);
    }
}

