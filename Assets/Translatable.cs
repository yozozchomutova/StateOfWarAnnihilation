using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.SimpleLocalization.Scripts;

public class Translatable : MonoBehaviour
{
    public string translateKey;

    private bool translated = false;

    /*private void Awake()
    {
        translate();
    }*/

    public void retranslate()
    {
        translated = false;
        translate();
    }

    public void translate()
    {
        if (translated)
            return;

        Text txt;
        TMP_Text tmpTxt;
        Dropdown dp;
        TMP_Dropdown tmpDp;
        //LevelEditor.TerrainEdit.

        if (gameObject.TryGetComponent<Text>(out txt))
        {
            txt.text = LocalizationManager.Localize(translateKey);
        }
        else if (gameObject.TryGetComponent<TMP_Text>(out tmpTxt))
        {
            tmpTxt.text = LocalizationManager.Localize(translateKey);
        }
        else if (gameObject.TryGetComponent<Dropdown>(out dp))
        {
            for (int i = 0; i < dp.options.Count; i++)
            {
                dp.options[i].text = LocalizationManager.Localize(dp.options[i].text);
            }
        }
        else if (gameObject.TryGetComponent<TMP_Dropdown>(out tmpDp))
        {
            for (int i = 0; i < tmpDp.options.Count; i++)
            {
                tmpDp.options[i].text = LocalizationManager.Localize(tmpDp.options[i].text);
            }
        }
        //print("T: " + LocalizationManager.Localize(translateKey));
        translated = true;
    }
}
