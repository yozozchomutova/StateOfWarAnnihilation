using UnityEngine;
using UnityEngine.UI;

public class SelectUnitTriggerer : MonoBehaviour
{
    private SelectUnitPanel selectUnit;
    private MainMenuPanel selectUnitPanel;

    private Image curImg;

    private void Start()
    {
        selectUnit = Resources.FindObjectsOfTypeAll<SelectUnitPanel>()[0];
        selectUnitPanel = selectUnit.gameObject.GetComponent<MainMenuPanel>();
    }

    public void OnSelectUnitShow(Image img)
    {
        curImg = img;
    }

    public void OnSelectUnitShow2(Text text)
    {
        //selectUnit.setValues(curImg, text);
        selectUnitPanel.show();
    }
}
