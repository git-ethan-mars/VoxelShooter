using System;
using System.Collections;
using UnityEngine;

public static class Utils
{
    public static IEnumerator DoActionAfterDelay(float delayInSeconds, Action action)
    {
        yield return new WaitForSeconds(delayInSeconds);
        action();
    }
}