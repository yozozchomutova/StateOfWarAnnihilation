using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SelectedGameUnitUI : MonoBehaviour
{
    public RectTransform parentRect;
    public Image bcg;
    public Text countText;
    public RawImage icon;
    public Slider averageHealth;

    //For validating/not visible
    public List<float> allHealths = new List<float>();
    public int unitCount;

    public void initStats(float x, Team team, Texture iconTexture)
    {
        updateX(x);
        bcg.color = team.minimapColor;
        icon.texture = iconTexture;
    }

    public void addUnit(float health)
    {
        allHealths.Add(health);
        unitCount++;
    }

    public void updateX(float newX)
    {
        parentRect.anchoredPosition = new Vector2(newX, parentRect.anchoredPosition.y);
    }

    public void clear()
    {
        allHealths.Clear();
        unitCount = 0;
    }

    public bool isStillUsable()
    {
        return unitCount != 0;
    }

    public void pushStats()
    {
        countText.text = unitCount + "x";
        averageHealth.value = allHealths.Count > 0 ? (float) allHealths.Average() : 0.0f;
    }
}
