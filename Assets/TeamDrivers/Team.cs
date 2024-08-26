#region [Libraries] All libraries
using UnityEngine;
#endregion

public class Team
{
    #region [Variables] General team's IDs
    public static readonly int WHITE = 0; //Neutral AI
    public static readonly int BLUE = 1;
    public static readonly int GREEN = 2;
    public static readonly int RED = 3;
    public static readonly int YELLOW = 4;
    public static readonly int PURPLE = 5;
    public static readonly int PINK = 6;
    public static readonly int ORANGE = 7;
    public static readonly int BROWN = 8;
    public static readonly int BLACK = 9;
    #endregion
    #region [Variables] Team properties
    public int id;
    public int sowId;
    public Color minimapColor;
    public Color[] minimapColorSequence;
    public string name;
    #endregion
    #region [Variables] Team textures
    public static readonly string UNIT_TEXTURES_FOLDER_PATH = "Materials";
    public Texture textureUnit;
    public Texture textureStepper;
    #endregion

    #region [Constructors] Default Team constructor
    public Team(int id, int sowId, Color minimapColor, string name, string materialPath)
    {
        this.id = id;
        this.sowId = sowId;
        this.minimapColor = minimapColor;
        this.name = name;

        this.minimapColorSequence = new Color[16];
        for (int i = 0; i < minimapColorSequence.Length; i++)
            minimapColorSequence[i] = minimapColor;

        this.textureUnit =      Resources.Load<Texture>(UNIT_TEXTURES_FOLDER_PATH + "/" + materialPath + "Units");
        this.textureStepper =   Resources.Load<Texture>(UNIT_TEXTURES_FOLDER_PATH + "/" + materialPath + "Robot");
    }
    #endregion

    #region [Functions] Gathering textures
    public Texture getTextureByType(Unit.UnitType type)
    {
        if (type == Unit.UnitType.BUILDING)
            return textureUnit;
        else if (type == Unit.UnitType.TOWER)
            return textureUnit;
        else if (type == Unit.UnitType.UNIT)
            return textureUnit;
        else if (type == Unit.UnitType.STEPPER)
            return textureStepper;
        else if (type == Unit.UnitType.SMF)
            return textureUnit;
        else if (type == Unit.UnitType.AIRFORCE)
            return textureUnit;

        Debug.LogError("Unknown Unit type!");
        return null;
    }
    #endregion
}
