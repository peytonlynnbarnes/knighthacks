using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // (Optional) If you prefer to use scene index instead:
    public void LoadSceneByIndex(int index)
    {
        SceneManager.LoadScene(index);
    }
}
