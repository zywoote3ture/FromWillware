using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SaveDataEntry
{
    public string key;
    public string json;
}

[Serializable]
public class SerializationWrapper
{
    public List<SaveDataEntry> data = new List<SaveDataEntry>();
}