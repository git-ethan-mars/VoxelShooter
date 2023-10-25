using System;
using System.Collections;
using UnityEngine;

public static class Utils
{
    public static IEnumerator DoActionAfterDelay(Action action, float delayInSeconds)
    {
        yield return new WaitForSeconds(delayInSeconds);
        action();
    }
}