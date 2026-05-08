using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    public Slider loadingSlider;
    public TextMeshProUGUI progressText;
    
    // 强制设置一个最小显示时间（秒），比如 2 秒
    public float minLoadingTime = 2.0f; 

    void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        // 1. 获取加载目标
        int useIndex = PlayerPrefs.GetInt("UseIndex", 0);
        AsyncOperation operation;

        if (useIndex == 1)
        {
            int index = PlayerPrefs.GetInt("TargetIndex", -1);
            if (index == -1) { Debug.LogError("索引错误！"); yield break; }
            operation = SceneManager.LoadSceneAsync(index);
        }
        else
        {
            string sceneName = PlayerPrefs.GetString("TargetScene", "");
            if (string.IsNullOrEmpty(sceneName)) { Debug.LogError("场景名为空！"); yield break; }
            operation = SceneManager.LoadSceneAsync(sceneName);
        }

        // 阻止自动跳转
        operation.allowSceneActivation = false;

        // 2. 同时计时器开始，保证 Loading 界面至少显示 minLoadingTime 秒
        float timer = 0f;

        // 3. 循环加载
        while (operation.progress < 0.9f || timer < minLoadingTime)
        {
            // 更新计时器
            timer += Time.deltaTime;

            // 获取加载进度 (0 ~ 0.9)
            float progress = operation.progress / 0.9f;

            // 进度条显示进度与计时器的较小值，保证平滑过渡
            float displayProgress = Mathf.Min(progress, timer / minLoadingTime);

            loadingSlider.value = displayProgress;
            progressText.text = (displayProgress * 100).ToString("F0") + "%";

            yield return null;
        }

        // 4. 加载完成且计时结束，准备激活
        loadingSlider.value = 1f;
        progressText.text = "100%";

        // 可选：在这里执行内存清理
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
        yield return new WaitForSeconds(0.2f); // 给清理留一丁点缓冲

        // 5. 激活主场景
        operation.allowSceneActivation = true;
    }
}