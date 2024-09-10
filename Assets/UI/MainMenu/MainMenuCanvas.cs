using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuCanvas : MonoBehaviour
{
    [Header("Top-right corner")]
    public Text versionText;

    [Header("Sbumenu properties")]
    public float submenuTransitionSpeed;

    [Header("Submenus")]
    public RectTransform singleplayerSubmenu;
    public RectTransform multiplayerSubmenu;
    public RectTransform editorSubmenu;
    public RectTransform settingsSubmenu;
    public RectTransform statSubmenu;

    private RectTransform currentSubmenu;

    [Header("Blackscreen")]
    public CanvasGroup blackScreen;

    void Start()
    {
        blackScreen.alpha = 1;

        versionText.text = "Major: " + GameManager.getBigPatchVer() + " |Side: " + GameManager.getSmallPatchVer() + " |Agr: " + GameManager.getAgreementVer() + " |Build: " + GameManager.getTagVersionFull();
    }
    
    void Update()
    {
        //Black screen alpha
        if (blackScreen.alpha > 0f)
            blackScreen.alpha -= Time.deltaTime * 2;

        //Move submenus
        updateSubmenu(singleplayerSubmenu);
        updateSubmenu(multiplayerSubmenu);
        updateSubmenu(editorSubmenu);
        updateSubmenu(settingsSubmenu);
        updateSubmenu(statSubmenu);
    }

    private void updateSubmenu(RectTransform submenu)
    {
        float targetY = currentSubmenu == submenu ? 210 : -350;
        float y = submenu.anchoredPosition.y;
        y += (targetY - y) * Time.deltaTime * submenuTransitionSpeed;
        submenu.anchoredPosition = new Vector2(submenu.anchoredPosition.x, y);
    }

    public void OnSubmenuChange(int id)
    {
        currentSubmenu = id == 0 ? singleplayerSubmenu :
            id == 1 ? multiplayerSubmenu :
            id == 2 ? editorSubmenu :
            id == 3 ? settingsSubmenu :
            id == 4 ? statSubmenu :
            null;

        if (id == 5)
            Application.Quit(0);
    }

    public static void ShowMessage(string title, string desc)
    {
        MessageBox m = GameObject.FindObjectOfType<MessageBox>(true);
        m.show(title, desc);
    }
}
