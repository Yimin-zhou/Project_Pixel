using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;

public class Debug
{
    private const string UNITY_EDITOR = "UNITY_EDITOR";

    [Conditional(UNITY_EDITOR)]
    public static void Log(object message)
    {
        UnityDebug.Log(message);
    }

    [Conditional(UNITY_EDITOR)]
    public static void LogError(object message)
    {
        UnityDebug.LogError(message);
    }

    [Conditional(UNITY_EDITOR)]
    public static void LogWarning(object message)
    {
        UnityDebug.LogWarning(message);
    }
}
