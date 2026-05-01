using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashToLoading : MonoBehaviour
{
    public float delay = 2f; // ilang seconds bago mag next scene
    public string nextSceneName = "LoadingScreen";

    void Start()
    {
        Invoke("LoadNextScene", delay);
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}