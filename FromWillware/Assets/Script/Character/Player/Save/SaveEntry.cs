using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveEntry
{
    public string id;        // 唯一ID
    public string type;     // 类型（可选）
    public string json;     // 数据
}

[System.Serializable]
public class SaveFile
{
    public List<SaveEntry> entries = new List<SaveEntry>();
}