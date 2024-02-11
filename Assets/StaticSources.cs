using UnityEngine;

public class StaticSources : MonoBehaviour
{
    public static Texture2D mouseSpriteNormal;
    public static Texture2D mouseSpriteAttack;
    public static Texture2D mouseSpriteUpgrade;
    public static Texture2D mouseSpriteRepair;
    public static Texture2D mouseSpriteSendAirforce;
    public static Texture2D mouseSpriteFreeCam;
    public static Texture2D mouseSpriteSelect;

    void Start()
    {
        mouseSpriteNormal = Resources.Load<Texture2D>("UI/Cursors/normal1");
        mouseSpriteAttack = Resources.Load<Texture2D>("UI/Cursors/attack");
        mouseSpriteUpgrade = Resources.Load<Texture2D>("UI/Cursors/upgradeBuilding");
        mouseSpriteRepair = Resources.Load<Texture2D>("UI/Cursors/repair");
        mouseSpriteSendAirforce = Resources.Load<Texture2D>("UI/Cursors/sendAirforce");
        mouseSpriteFreeCam = Resources.Load<Texture2D>("UI/Cursors/freeCamera");
        mouseSpriteSelect = Resources.Load<Texture2D>("UI/Cursors/select");
    }
}
