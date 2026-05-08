using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement; // 必须引用场景管理

public class MainMenu : MonoBehaviour
{
    [Header("UI面板")]
    public GameObject Settings;[Header("音效设置")]
    public AudioSource sfxSource;
    public AudioClip clickSound;
    
    private void PlayClickSFX()
    {
        if (sfxSource != null && clickSound != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }

    // 1. 新的游戏
    public void NewGame()
{
    PlayClickSFX();
    
    // 存入要加载的场景路径
    PlayerPrefs.SetString("TargetScene", "Scenes/SampleScene/MainScene");
    PlayerPrefs.SetInt("UseIndex", 0); // 0 代表用字符串名，1 代表用索引

    SceneManager.LoadScene("Scenes/Loading");
}

// 在 MainMenu.cs 中
public void ContinueGame()
{
    PlayClickSFX();

    string savePath = SaveSystem.GetSavePath("save.json");
    bool hasJsonSave = File.Exists(savePath);
    int savedSceneIndex = PlayerPrefs.GetInt("SavedSceneIndex", 0);

    if (hasJsonSave && savedSceneIndex != 0)
    {
        Debug.Log("准备加载存档，索引：" + savedSceneIndex);
        
        // 【关键逻辑】：
        // 1. 存入要加载的索引
        PlayerPrefs.SetInt("TargetIndex", savedSceneIndex);
        // 2. 标记使用索引加载 (1 代表使用索引)
        PlayerPrefs.SetInt("UseIndex", 1); 
        // 3. 告诉你的存档系统在下一场景加载时去读取文件
        SaveSystem.shouldLoadSaveGame = true; 

        SceneManager.LoadScene("Scenes/Loading");
    }
    else
    {
        Debug.Log("没有发现存档或存档无效");
    }
}

    // 3. 设置按钮
    public void OpenSettings()
    {
        PlayClickSFX();
        Settings.SetActive(true);
    }

    public void CloseSettings()
    {
        PlayClickSFX();
        Settings.SetActive(false);
    }

    // 4. 退出游戏
    public void ExitGame()
    {
        PlayClickSFX();
        Debug.Log("退出游戏");
        Application.Quit();
    }
}