using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelExitConfirm : MonoBehaviour
{
    public void Yes()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void No()
    {
        gameObject.SetActive(false);
    }
}
