using SOWUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceProductionPropUI : MonoBehaviour
{
    private int pu_ui_image_Id;

    //UI
    public Slider unitCount;
    public Slider unlockedUnitsCount;

    //Production units
    public GameObject[] producingUnits;
    [HideInInspector] public string[] puIDs; //Auto-generated
    private RawImage[] producingUnitTextures; //Auto-generated
    private Image[] producingUnitBorders; //Auto-generated

    //Passed-trough
    private string[] allowedUnits;
    private SelectUnitPanel selectUnit;
    private MainMenuPanel selectUnitPanel;

    public void Init(string unitTitle, int unitSlots, string[] allowedUnits, SelectUnitPanel selectUnit, MainMenuPanel selectUnitPanel)
    {
        this.allowedUnits = allowedUnits;

        gameObject.GetComponent<Text>().text = unitTitle;

        this.selectUnit = selectUnit;
        this.selectUnitPanel = selectUnitPanel;

        //Config
        unitCount.maxValue = unitSlots;
        unitCount.minValue = 1;
        unitCount.value = 3;

        unlockedUnitsCount.maxValue = unitCount.value;
        unlockedUnitsCount.minValue = 0;
        unlockedUnitsCount.value = 1;

        //Autogenerate & fill with empty ids
        puIDs = new string[producingUnits.Length];
        for (int i = 0; i < puIDs.Length; i++) {
            puIDs[i] = "0_antiair1";
        }

        //Autogenerate producing unit textures & borders
        producingUnitTextures = new RawImage[producingUnits.Length];
        producingUnitBorders = new Image[producingUnits.Length];
        for (int i = 0; i < producingUnits.Length; i++)
        {
            producingUnitTextures[i] = GO.getRawImage(producingUnits[i].gameObject, "Image");
            producingUnitBorders[i] = GO.getImage(producingUnits[i].gameObject, "Border");
        }
    }

    public void UpdateUI()
    {
        OnUnitCountChange();
        OnUnlockedUnitsCountChange();
        UpdateProducingUnitTextures();
    }

    public void OnSelectPUnitShow(int pu_ui_image_Id)
    {
        this.pu_ui_image_Id = pu_ui_image_Id;

        selectUnit.setSelectCallback(new SC_SUP_ResourceUnit(this));
        selectUnit.disableFilter();
        selectUnitPanel.show();
        selectUnit.enableFilter(allowedUnits);
    }

    public void ChangeCurImageIdUnit(string puId)
    {
        puIDs[pu_ui_image_Id] = puId;
        UpdateProducingUnitTextures();
    }

    public void OnUnitCountChange()
    {
        if (producingUnitBorders == null) return; //Sometimes happens that this method calls earlier than is created.

        int value = (int) unitCount.value;
        for (int i = 0; i < producingUnits.Length; i++)
        {
            producingUnits[i].gameObject.SetActive(i < value);
        }

        unlockedUnitsCount.maxValue = value;
    }

    public void OnUnlockedUnitsCountChange()
    {
        if (producingUnitBorders == null) return; //Sometimes happens that this method calls earlier than is created.

        int value = (int) unlockedUnitsCount.value;
        for (int i = 0; i < producingUnits.Length; i++)
        {
            producingUnitBorders[i].color = i<value ? new Color(0, 1, 0, 0.7f) : new Color(1, 0, 0, 0.7f);
        }
    }

    public void UpdateProducingUnitTextures()
    {
        for (int i = 0; i < puIDs.Length; i++)
        {
            //print("U: " + LevelEditorStaticList.producingUnits[puIDs[i]].puId);
            producingUnitTextures[i].texture = GlobalList.producingUnits[puIDs[i]].puIcon;
        }
    }

    public class SC_SUP_ResourceUnit : SelectUnitPanel.SelectCallback
    {
        ResourceProductionPropUI rp; public SC_SUP_ResourceUnit(ResourceProductionPropUI rp) { this.rp = rp; }
        public void OnSelect(string id)
        {
            rp.ChangeCurImageIdUnit(id);
        }
    }
}
