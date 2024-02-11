using UnityEngine;
using UnityEngine.UI;

public class AlphaButton : MonoBehaviour
{
    void Start()
    {
        gameObject.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
    }
}
