using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections;

public class SaveSystem : MonoBehaviour
{
    public string fileName = "save.json";

    private bool canSave = false;
    private PlayerInputHandler inputHandler;

    [Header("交互UI")]
    public GameObject interactPromptPrefab;
    public Transform uiCanvas;

    private GameObject currentPrompt;
    private Transform savePointTarget;

    public static string GetSavePath(string fileName)
    {
        return System.IO.Path.Combine(Application.persistentDataPath, fileName);
    }

    public static bool shouldLoadSaveGame = false;

    string Path => GetSavePath(fileName);

    public void Awake()
    {
        inputHandler = FindObjectOfType<PlayerInputHandler>();
    }

    IEnumerator Start()
    {
        if (shouldLoadSaveGame)
        {
            yield return null;
            Load();
            shouldLoadSaveGame = false;
        }
    }

    void Update()
    {
        // ===== UI 跟随 =====
        if (currentPrompt != null && savePointTarget != null)
        {
            Vector3 screenPos =
                Camera.main.WorldToScreenPoint(
                    savePointTarget.position + Vector3.up * 2f
                );

            currentPrompt.transform.position = screenPos;
        }

        // ===== 保存 =====
        if (canSave && inputHandler.interactPressed)
        {
            Save();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            Load();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SavePoint"))
        {
            canSave = true;
            savePointTarget = other.transform;

            ShowPrompt();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SavePoint"))
        {
            canSave = false;
            savePointTarget = null;

            HidePrompt();
        }
    }

    void ShowPrompt()
    {
        if (currentPrompt != null) return;

        currentPrompt = Instantiate(
            interactPromptPrefab,
            uiCanvas
        );

        if (savePointTarget != null)
        {
            Vector3 screenPos =
                Camera.main.WorldToScreenPoint(
                    savePointTarget.position + Vector3.up * 2f
                );

            currentPrompt.transform.position = screenPos;
        }
    }

    void HidePrompt()
    {
        if (currentPrompt != null)
        {
            Destroy(currentPrompt);
            currentPrompt = null;
        }
    }

    // ================= SAVE =================
    public void Save()
    {
        SaveFile file = new SaveFile();

        var saveables = FindObjectsOfType<MonoBehaviour>(true);

        foreach (var s in saveables)
        {
            if (s is ISaveable saveable)
            {
                SaveEntry entry = new SaveEntry
                {
                    id = saveable.GetUniqueID(),
                    json = saveable.CaptureState()
                };

                file.entries.Add(entry);
            }
        }

        string json = JsonUtility.ToJson(file, true);
        File.WriteAllText(Path, json);

        PlayerPrefs.SetInt(
            "SavedSceneIndex",
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );

        PlayerPrefs.Save();

        Debug.Log("Saved");
    }

    // ================= LOAD =================
    public void Load()
    {
        if (!File.Exists(Path))
        {
            Debug.LogWarning("未找到存档文件！");
            return;
        }

        string json = File.ReadAllText(Path);
        SaveFile file = JsonUtility.FromJson<SaveFile>(json);

        var saveables = FindObjectsOfType<MonoBehaviour>(true);

        foreach (var s in saveables)
        {
            if (s is ISaveable saveable)
            {
                foreach (var entry in file.entries)
                {
                    if (entry.id == saveable.GetUniqueID())
                    {
                        saveable.RestoreState(entry.json);
                        break;
                    }
                }
            }
        }

        Debug.Log("Loaded");
    }
}