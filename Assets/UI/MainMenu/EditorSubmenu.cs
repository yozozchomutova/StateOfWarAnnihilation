using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EditorSubmenu : MonoBehaviour
{
    public LoadingScreenPanel lsPanel;

    public void OnLevelEditor()
    {
        lsPanel.EngageLoading("Level editor", "State Of War Level editor", "EditorCanvas", 1, 3);
    }
}
