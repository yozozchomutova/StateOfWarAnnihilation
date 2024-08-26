using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System.Drawing;

public class NNManagerPanel : MonoBehaviour
{
    public ComputeShader cs;

    public TMP_InputField if_HLayerCount;
    public TMP_InputField if_HLayerNodes;

    public static bool LAUNCHED = false;
    public static bool TRAINING = false;

    private NNFirstDriver[] fetchDrivers;

    //Maps
    public static byte[,] mapGroundUnits;
    public static Texture2D textGUI;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private GUIStyle style = new GUIStyle();
    private void OnGUI()
    {
        if (LAUNCHED && textGUI != null) {
            GUI.DrawTexture(new Rect(0, 0, 512, 512), textGUI); // Draw a rectangle with a label
            
        }
    }

    void Remap()
    {
        for (int x = 0; x < mapGroundUnits.GetLength(0); x++)
        {
            for (int y = 0; y < mapGroundUnits.GetLength(1); y++)
            {
                mapGroundUnits[x, y] = 255;
            }
        }

        foreach (Unit gameObj in LevelData.units)
        {
            int boolArrayX = (int)Mathf.Floor(gameObj.transform.position.x);
            int boolArrayZ = (int)Mathf.Floor(gameObj.transform.position.z);

            mapGroundUnits[boolArrayX, boolArrayZ] = (byte)gameObj.team.id;
        }

        for (int x = 0; x < mapGroundUnits.GetLength(0); x++)
        {
            for (int y = 0; y < mapGroundUnits.GetLength(1); y++)
            {
                UnityEngine.Color[] tgc = new UnityEngine.Color[16];
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        int index = i * 4 + j;
                        byte t = mapGroundUnits[x, y];
                        tgc[index] = t != 255 ? GlobalList.teams[t].minimapColor : UnityEngine.Color.black;
                        //tgc[index] = UnityEngine.Color.white;
                    }
                }


                textGUI.SetPixels(x * 4, y * 4, 4, 4, tgc);
                
            }
        }

        textGUI.Apply();
    }

    private void OnEnable()
    {
        
    }

    public void createAndLaunch()
    {
        LAUNCHED = true;
        TRAINING = false;

        fetchDrivers = FindObjectsOfType<NNFirstDriver>();
        for (int i = 0; i < fetchDrivers.Length; i++)
        {
            fetchDrivers[i].nnUnitManager = new NNFirstDriver.MLP(NNFirstDriver.INPUT_SIZE, NNFirstDriver.HIDDEN_SIZE, NNFirstDriver.OUTPUT_SIZE, cs);
        }

        mapGroundUnits = new byte[128, 128];
        textGUI = new Texture2D(512, 512);
        InvokeRepeating("Remap", 1f, 1f);
    }

    public void createAndTrain()
    {
        LAUNCHED = false;
        TRAINING = true;
    }

    public void closePanel()
    {
        gameObject.SetActive(false);
    }

    public void saveToFile()
    {

    }

    public void loadFromFile()
    {

    }

    
}
