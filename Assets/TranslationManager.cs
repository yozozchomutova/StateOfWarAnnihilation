using Assets.SimpleLocalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.SimpleLocalization.Scripts;

public class TranslationManager : MonoBehaviour
{
    private static bool translated = false;

    private void Awake()
    {
        if (translated == false)
        {
            LocalizationManager.Read();

            switch ((SystemLanguage) PlayerPrefs.GetInt("stg_language", (int) SystemLanguage.English))
            {
                case SystemLanguage.Czech:
                    LocalizationManager.Language = "Czech";
                    //LocalizationManager.Language = "Czech";
                    break;
                default:
                    LocalizationManager.Language = "English";
                    break;
            }
        }

        //Translate all components
        //Translatable[] t = GameObject.FindObjectsOfType<Translatable>();
        Translatable[] t = Resources.FindObjectsOfTypeAll<Translatable>();
        foreach (Translatable t2 in t)
        {
            t2.translate();
        }
        //print("Translated everything!");
        translated = true; //Prevent repeating twice or even more
    }
}