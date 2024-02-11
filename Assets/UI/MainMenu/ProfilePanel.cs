using System.Diagnostics;
using UnityEngine;

public class ProfilePanel : MonoBehaviour
{
    [Header("Panels")]
    public MainMenuPanel statsPanel, achievementsPanel;

    public void OnShowPanel(int id)
    {
        if (id == 0)
            statsPanel.show();
        else if (id == 1)
            achievementsPanel.show();
        else if (id == 2)
            Process.Start("https://github.com/yozozchomutova/StateOfWarAnnihilation/blob/main/AgreementsRaw/" + GameManager.agreementFileName);
    }
}