using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveable
{
    string CaptureState();        // 保存
    void RestoreState(string json); // 读取
    string GetUniqueID();         // 唯一ID
}
