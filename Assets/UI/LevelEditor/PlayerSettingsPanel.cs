using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSettingsPanel : MonoBehaviour
{
    public EditorManager editorManager;

    public void Ok()
    {
        //Hide panel
        gameObject.SetActive(false);
        editorManager.hidePlayerSettingsTabs();
    }
}
