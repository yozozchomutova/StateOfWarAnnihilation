using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    //Cursor
    [HideInInspector] public static Texture2D[] cursorType;
    private static Texture2D[] currentCursorType;

    private static Vector2Int cursorCenter = new Vector2Int(24, 24);

    //Texture sequences
    public float updateTime = 0.4f;
    private static float frame = 0;
    private static bool goUp = true;

    //Cursors
    public static Texture2D[] spriteNormal;
    public static Texture2D[] spriteAttack;
    public static Texture2D[] spriteUpgrade;
    public static Texture2D[] spriteRepair;
    public static Texture2D[] spriteAirforces;
    public static Texture2D[] spriteFreeCam;
    public static Texture2D[] spriteSelect;
    public static Texture2D[] spriteBuild;
    public static Texture2D[] spriteDestroy;

    // Start is called before the first frame update
    void Start()
    {
        //Load cursors dynamically
        spriteNormal = LoadTextures("normal/");
        spriteAttack = LoadTextures("attack/");
        spriteUpgrade = LoadTextures("upgrade/");
        spriteRepair = LoadTextures("repair/");
        spriteAirforces = LoadTextures("airforce/");
        spriteFreeCam = LoadTextures("freeCamera/");
        spriteSelect = LoadTextures("select/");
        spriteBuild = LoadTextures("build/");
        spriteDestroy = LoadTextures("destroy/");

        cursorType = spriteNormal;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCursorTexture();
    }

    private void UpdateCursorTexture()
    {
        frame += Time.deltaTime * updateTime * (goUp ? 1 : -1);

        if (frame > cursorType.Length - 1)
        {
            goUp = false;
        } else if (frame <= 0)
        {
            goUp = true;
        }

        frame = Mathf.Clamp(frame, 0, cursorType.Length - 1);

        if (cursorType == spriteNormal)
        {
            Cursor.SetCursor(cursorType[0], Vector2.zero, CursorMode.Auto);
        } else
        {
            Cursor.SetCursor(cursorType[(int)frame], cursorCenter, CursorMode.Auto);
        }

        currentCursorType = cursorType;
        cursorType = spriteNormal; //Reset
    }

    public static void SetCursor(Texture2D[] ct)
    {
        cursorType = ct;

        if (currentCursorType != ct)
        {
            goUp = true;
            frame = 0;
        }
    }

    private Texture2D[] LoadTextures(string subFolder)
    {
        Texture2D[] texts = Resources.LoadAll<Texture2D>("UI/Cursors/" + subFolder);
        Array.Sort(texts, CompareByName);
        return texts;
    }

    // Comparison method to sort textures by numeric filename
    private int CompareByName(Texture2D x, Texture2D y)
    {
        int xIndex = ExtractNumberFromName(x.name);
        int yIndex = ExtractNumberFromName(y.name);
        return xIndex.CompareTo(yIndex);
    }

    // Helper method to extract the numeric part from the texture's name
    private int ExtractNumberFromName(string name)
    {
        // Assuming names are simple numbers like "1", "2", ..., "16"
        // Convert the string to an integer
        int number;
        int.TryParse(name, out number);
        return number;
    }
}
