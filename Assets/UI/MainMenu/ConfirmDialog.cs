using System;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmDialog : MonoBehaviour
{
    public GameObject dialogRoot;
    private MainMenuPanel dialogRootMP;

    public Text dialogTitle;
    private Callback callback;

    void Start()
    {
        dialogRootMP = dialogRoot.GetComponent<MainMenuPanel>();
    }

    public void ShowDialog(string dialogTitle, Callback callback)
    {
        this.callback = callback;
        this.dialogTitle.text = dialogTitle;

        dialogRootMP.show();
    }

    public void Yes()
    {
        callback.OnYes();
        dialogRootMP.OnClose();
    }

    public void No()
    {
        callback.OnNo();
        dialogRootMP.OnClose();
    }

    public interface Callback
    {
        void OnYes();
        void OnNo();
    }
}
