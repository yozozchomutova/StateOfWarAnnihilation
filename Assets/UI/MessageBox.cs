using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageBox : MainMenuPanel
{
    #region [Variables] UI
    public TMP_Text txtTitle;
    public TMP_Text txtDesc;
    #endregion

    public void show(string title, string desc)
    {
        txtTitle.text = title;
        txtDesc.text = desc;
        base.show();
    }

    public void copy()
    {
        GUIUtility.systemCopyBuffer = txtTitle.text + "\n" + txtDesc.text;
    }
}
