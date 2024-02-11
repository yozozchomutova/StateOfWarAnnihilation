using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonLiftEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform targetImage;
    public Image targetImage_image;
    public RectTransform targetText;

    public bool changeTargetImageTransform = true;

    private bool liftUp;
    private float originalY;
    private Vector2 originalPosition;
    private float liftRate;

    void Start()
    {
        originalPosition = targetImage.anchoredPosition;
        originalY = targetImage.anchoredPosition.y;

        targetImage_image = targetImage.gameObject.GetComponent<Image>();
    }

    void Update()
    {
        //print(gameObject.name);
        float targetDistance = originalY + (liftUp ? 7.5f : 0) - targetImage.anchoredPosition.y;
        liftRate += targetDistance / 11f;

        originalPosition = new Vector2(originalPosition.x, originalY + liftRate);

        if (targetImage != null && changeTargetImageTransform) targetImage.anchoredPosition = originalPosition;
        if (targetImage_image != null) targetImage_image.raycastPadding = new Vector4(0, -(liftRate), 0, 0);
        if (targetText != null) targetText.anchoredPosition = originalPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        liftUp = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        liftUp = false;
    }
}
