using UnityEngine;
using UnityEngine.UI;

public class UnitInFront : MonoBehaviour
{
    public Button upgradeBtn;
    public RawImage insideIcon;
    public RawImage availabilityKnob;

    [HideInInspector] public bool reloading;
    [HideInInspector] public bool alphaOn;
    [HideInInspector] public bool canBeUpgraded;
    [HideInInspector] public int position;
    [HideInInspector] public int maxPosition;
    [HideInInspector] public int positionInArray;
    [HideInInspector] public RequestUnitUpgrade re_callback;

    //One of the following will be null and one referenced:
    [HideInInspector] public Unit unitInside;
    [HideInInspector] public ProducingUnit pUnitInside;

    [HideInInspector] private CanvasGroup alphaMask;

    private RectTransform rootTrans;
    private Vector2 destination;

    public static readonly Vector2[] UI_positions =
    {
        new Vector2(-173, 46), //START - Go away
        new Vector2(-277, 108),
        new Vector2(-309f, -3),
        new Vector2(-277, -107),
        new Vector2(-189, -186),
        new Vector2(-66, -212),
        new Vector2(-16, -258),
        new Vector2(-16, -315),
        new Vector2(-16, -375),
        new Vector2(-16, -435),
        new Vector2(-16, -495) //END - Come from
    };

    void Start()
    {
        rootTrans = gameObject.GetComponent<RectTransform>();
        alphaMask = gameObject.GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (pUnitInside == null)
        {
            if (canBeUpgraded && LevelData.ts.research >= 500 && !string.IsNullOrWhiteSpace(unitInside.upgradableToUnit))
            {
                upgradeBtn.interactable = true;
            }
            else
            {
                upgradeBtn.interactable = false;
            }
        }

        //Always move to position
        Vector2 a = rootTrans.anchoredPosition;
        Vector2 nextMove = (destination - a) / 15f;
        rootTrans.anchoredPosition += nextMove;

        //Handle reloading
        if (reloading)
        {
            if (position == 0)
            {
                alphaMask.alpha -= Time.deltaTime * 14f;
            
                if (alphaMask.alpha <= 0)
                {
                    position += maxPosition+1; //Return it to the back of the front
                    rootTrans.anchoredPosition = UI_positions[this.position];
                    updatePos();
                }
            } else if (Vector2.Distance(a, destination) < 4f) //Must be close to the back of the front
            {
                if (alphaMask.alpha <= 0)
                    moveNext();

                alphaMask.alpha += Time.deltaTime * 16f;

                if (alphaMask.alpha >= 1)
                    reloading = false; //It's ready to be produced again
            }
        }
    }
    public void link(Unit unitInside, bool canBeUpgraded, bool available, int maxPosition, int position, int positionInArray, RequestUnitUpgrade re_callback)
    {
        this.unitInside = unitInside;
        link(canBeUpgraded, available, maxPosition, position, positionInArray, re_callback);
    }

    public void link(ProducingUnit pUnitInside, bool canBeUpgraded, bool available, int maxPosition, int position, int positionInArray, RequestUnitUpgrade re_callback)
    {
        this.pUnitInside = pUnitInside;
        link(canBeUpgraded, available, maxPosition, position, positionInArray, re_callback);
    }

    public void link(bool canBeUpgraded, bool available, int maxPosition, int position, int positionInArray, RequestUnitUpgrade re_callback)
    {
        this.canBeUpgraded = canBeUpgraded;
        this.position = position+1; //position 0 = Fade away
        this.maxPosition = maxPosition;
        this.positionInArray = positionInArray;
        this.re_callback = re_callback;

        //Fix position, if less than 0
        if (this.position <= 0)
        {
            this.position += maxPosition;
        }

        updateIcon();
        updatePos();
        updateAvailabilityKnob(available);

        rootTrans = gameObject.GetComponent<RectTransform>();
        rootTrans.anchoredPosition = UI_positions[this.position];
    }

    public void updatePos()
    {
        destination = UI_positions[position];
    }

    public void moveNext()
    {
        position--;
        updatePos();

        if (position <= 0)
        {
            reloading = true;
        }
    }

    private void updateIcon()
    {
        if (pUnitInside != null)
            insideIcon.texture = pUnitInside.puIcon;
        else
            insideIcon.texture = unitInside.icon;
    }

    public void updateAvailabilityKnob(bool available)
    {
        //Availability
        if (available)
            availabilityKnob.color = new Color(0, 255, 0);
        else
            availabilityKnob.color = new Color(255, 0, 0);
    }

    public void upgradeUnit()
    {
        LevelData.ts.RemoveResearch(500);
        unitInside = GlobalList.units[unitInside.upgradableToUnit] as Unit;
        updateIcon();
        re_callback.onRequestUpgradeUnit(positionInArray);
        canBeUpgraded = re_callback.checkValidation(unitInside);
    }

    public interface RequestUnitUpgrade
    {
        public void onRequestUpgradeUnit(int positionInArray);

        public bool checkValidation(Unit unit);
    }
}
