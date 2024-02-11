using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SOWUtils;

public class FactoryPropUI : MonoBehaviour
{
    private SelectUnitPanel selectUnit;
    private SelectUnitPanel panelUnits;
    private SelectUnitPanel panelAirForces;
    private MainMenuPanel selectUnitPanel;
    private MainMenuPanel selectResourcePanel;

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
    private CallbackType callbackType;

    public void Init(string unitTitle, int unitSlots, string[] allowedUnits, string defaultUnitId, CallbackType callbackType)
    {
        this.allowedUnits = allowedUnits;
        this.callbackType = callbackType;

        gameObject.GetComponent<Text>().text = unitTitle;

        EditorManager em = Resources.FindObjectsOfTypeAll<EditorManager>()[0];
        panelUnits = em.unitsPanel;
        panelAirForces = em.airForceSelectPanel;

        selectUnitPanel = panelUnits.gameObject.GetComponent<MainMenuPanel>();
        selectResourcePanel = panelAirForces.gameObject.GetComponent<MainMenuPanel>();

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
            puIDs[i] = defaultUnitId;
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
        if (callbackType == CallbackType.FACTORY_UNIT)
        {
            panelUnits.setSelectCallback(new SC_SUP_FactoryUnit(this));
            panelUnits.disableFilter();
            selectUnitPanel.show();
            panelUnits.enableFilter(allowedUnits);
        }
        if (callbackType == CallbackType.RESOURCE_UNIT)
        {
            panelAirForces.setSelectCallback(new SC_SUP_FactoryUnit(this));
            panelAirForces.disableFilter();
            selectResourcePanel.show();
            panelAirForces.enableFilter(allowedUnits);
        }
    }

    public void ChangeCurImageIdUnit(string puId)
    {
        puIDs[pu_ui_image_Id] = puId;
        UpdateProducingUnitTextures();
    }

    public void OnUnitCountChange()
    {
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
        if (producingUnitTextures == null) return; //Sometimes happens that this method calls earlier than is created.
        for (int i = 0; i < puIDs.Length; i++)
        {
            //print("U: " + LevelEditorStaticList.producingUnits[puIDs[i]].puId);
            //print("U: " + puIDs[i]);
            producingUnitTextures[i].texture = GlobalList.producingUnits[puIDs[i]].puIcon;
        }
    }

    public class SC_SUP_FactoryUnit : SelectUnitPanel.SelectCallback
    {
        FactoryPropUI fp; public SC_SUP_FactoryUnit(FactoryPropUI fp) { this.fp = fp; }
        public void OnSelect(string id)
        {
            fp.ChangeCurImageIdUnit(id);
        }
    }

    public class SC_SUP_ResourceUnit : SelectUnitPanel.SelectCallback
    {
        FactoryPropUI rp; public SC_SUP_ResourceUnit(FactoryPropUI rp) { this.rp = rp; }
        public void OnSelect(string id)
        {
            rp.ChangeCurImageIdUnit(id);
        }
    }

    public enum CallbackType
    {
        FACTORY_UNIT,
        RESOURCE_UNIT
    }
}
