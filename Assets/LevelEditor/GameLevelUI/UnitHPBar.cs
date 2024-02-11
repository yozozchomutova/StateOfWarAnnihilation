using UnityEngine;
using UnityEngine.UI;

public class UnitHPBar : MonoBehaviour
{
    //Colors
    public Color c__bcg_50_100;
    public Color c_fill_50_100;
    public Color c__bcg_25_50;
    public Color c_fill_25_50;
    public Color c__bcg_0_25;
    public Color c_fill_0_25;

    //UI
    public Slider hpBar;
    public Image hpBar_background;
    public Image hpBar_fill;
    private RectTransform rootTrans;

    [HideInInspector] public Unit unit;

    private void Start()
    {
        rootTrans = gameObject.GetComponent<RectTransform>();
    }

    void Update()
    {
        if (unit != null)
        {
            hpBar.value = unit.getHpNormalized();
            rootTrans.anchoredPosition = Camera.main.WorldToScreenPoint(unit.transform.position) + new Vector3(0, 75, 0);
        } else
        {
            Destroy(gameObject);
        }

        //Change color by hp
        if (hpBar.value > 0.5f)
            changeHpBarColor(0.5f, c__bcg_25_50, c__bcg_50_100, c_fill_25_50, c_fill_50_100);
        else
            changeHpBarColor(0f, c__bcg_0_25, c__bcg_25_50, c_fill_0_25, c_fill_25_50);
    }

    private void changeHpBarColor(float multiplierOffset, Color bcg_min, Color bcg_max, Color fill_min, Color fill_max)
    {
        float multiplier = (hpBar.value - multiplierOffset) * 2f;
        Color diffCol = bcg_max - bcg_min;
        Color finalCol_b = bcg_min + diffCol * multiplier;
        finalCol_b.a = 1f;

        diffCol = fill_max - fill_min;
        Color finalCol_f = fill_min + diffCol * multiplier;
        finalCol_f.a = 1f;

        changeHpBarColor(finalCol_b, finalCol_f);
    }

    private void changeHpBarColor(Color bcg, Color fill)
    {
        hpBar_background.color = bcg;
        hpBar_fill.color = fill;
    }

    public void linkUnit(Unit unit)
    {
        this.unit = unit;
    }
}
