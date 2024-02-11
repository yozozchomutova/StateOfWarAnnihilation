using UnityEngine;

public class MapObject : MonoBehaviour
{
    public string objectId;
    public string objectName;
    public Texture2D icon;

    void Start()
    {
        gameObject.layer = 12; ///MapObject
    }
}
