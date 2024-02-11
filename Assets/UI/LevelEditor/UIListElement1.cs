using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIListElement1 : MonoBehaviour
{
    public RectTransform rootTrans;
    public Button btn;
    public Image bcg;
    public RawImage icon;
    public TMP_Text title;

    public static readonly Color COL_HIGHLIGHT = new Color(1f, 0f, 0f, 0.17f);
    public static readonly Color COL_UNHIGHLIGHT = new Color(1f, 1f, 1f, 0.17f);

    public void updateParameters(Texture2D icon, string title)
    {
        this.icon.texture = icon;
        this.title.text = title;
    }

    public void setHighlight(bool highlight)
    {
        bcg.color = highlight ? COL_HIGHLIGHT : COL_UNHIGHLIGHT;
    }

    public void setPosition(float x, float y)
    {
        rootTrans.anchoredPosition = new Vector2(x, y);
    }
}
