using SOWUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectUnitPanel : MonoBehaviour, EditorManager.InactiveStartCaller
{
    [HideInInspector] public MainMenuPanel selectUnitPanel;

    public GameObject content;

    private SelectCallback selectCallback;

    private ArrayList selectableButtons = new ArrayList();

    public void InactiveStart()
    {
        selectUnitPanel = gameObject.GetComponent<MainMenuPanel>();

        //Find all buttons that can be toggled
        for (int i = 0; i < content.transform.childCount; i++)
        {
            Button foundBtn;
            if (content.transform.GetChild(i).TryGetComponent<Button>(out foundBtn))
            {
                selectableButtons.Add(foundBtn);
            }
        }
    }

    public void disableFilter()
    {
        //Disable all buttons
        foreach (Button btn in selectableButtons)
        {
            btn.interactable = false;
        }
    }

    public void enableFilter(string[] allowedUnits)
    {
        //Enable only allowed buttons
        for (int i = 0; i < allowedUnits.Length; i++)
        {
            Button b = GO.getGameObject(content, allowedUnits[i]).GetComponent<Button>();
            b.interactable = true;
        }
    }

    public void enableFilterAll()
    {
        //Disable all buttons
        foreach (Button btn in selectableButtons)
        {
            btn.interactable = true;
        }
    }

    public void setSelectCallback(SelectCallback selectCallback)
    {
        this.selectCallback = selectCallback;
    }

    public void ReturnID(string id)
    {
        selectUnitPanel.OnClose();
        selectCallback.OnSelect(id);
    }

    public interface SelectCallback
    {
        void OnSelect(string id);
    }
}
