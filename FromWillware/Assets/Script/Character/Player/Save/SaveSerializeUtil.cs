using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SaveSerializeUtil
{
    public static string ToJson(object obj)
    {
        return JsonUtility.ToJson(obj);
    }

    public static T FromJson<T>(string json)
    {
        return JsonUtility.FromJson<T>(json);
    }
}