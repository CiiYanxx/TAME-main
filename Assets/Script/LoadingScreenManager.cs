using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingScreenManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject loadingScreenUI; 
    public Slider progressBar;         
    public TMP_Text progressText;

    [Header("Settings")]
    public int sceneToLoadIndex = 2;
    [Range(0.1f, 1f)]
    public float loadSpeed = 0.3f;    // Gaano kabilis mapuno ang bar
    public float waitBeforeExit = .5f; // Ilang segundo maghihintay pagkatapos ng 100%

    private AsyncOperation operation;

    void Start()
    {
        // 🔇 Stop menu/customization music pagpasok sa loading scene
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopMusic();

        if (loadingScreenUI != null)
            loadingScreenUI.SetActive(true);

        // Simulan agad ang loading
        StartCoroutine(LoadAsynchronously());
    }

    IEnumerator LoadAsynchronously()
    {
        // 1. Simulan ang loading sa background
        operation = SceneManager.LoadSceneAsync(sceneToLoadIndex);
        
        // Huwag munang hayaang lumipat ang scene
        operation.allowSceneActivation = false;

        float currentProgress = 0f;

        // 2. Habang hindi pa 100% ang bar
        while (currentProgress < 1f)
        {
            // Calculate target base sa Unity 0.9 cap
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // Smooth movement ng bar
            currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, loadSpeed * Time.deltaTime);

            if (progressBar != null)
                progressBar.value = currentProgress;

            if (progressText != null)
                progressText.text = "Loading... " + (currentProgress * 100f).ToString("F0") + "%";

            yield return null;
        }

        yield return new WaitForSeconds(waitBeforeExit);

        // 4. Lipat na sa susunod na scene
        operation.allowSceneActivation = true;
    }
}