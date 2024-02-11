using UnityEngine;
using UnityEngine.UI;

public class SC_TerrainEditor : MonoBehaviour
{
    public enum DeformMode { RaiseLower, FlattenByValue, FlattenByTerrain, Smooth }
    public DeformMode deformMode = DeformMode.RaiseLower;
    string[] deformModeNames = new string[] { "Raise Lower", "Flatten", "Smooth" };

    public Terrain terrain;
    public Texture2D deformTexture;
    public float strength = 1;
    public float area = 1;
    public float height = 1;
    private float lastHeight = 1;
    public bool showHelp;
    public int textureIndexSelected;

    public BarTerrainEdit barTerrainEdit;
    public Text pointingHeightText;

    Transform buildTarget;
    Vector3 buildTargPos;
    Light spotLight;

    [HideInInspector] public bool editingTerrain = false;
    [HideInInspector] public bool paintingTerrain = false;
    [HideInInspector] public bool editingDetails = false;

    //GUI
    Rect windowRect = new Rect(10, 10, 400, 185);
    bool onWindow = false;
    bool onTerrain;
    Texture2D newTex;
    float strengthSave;

    //Raycast
    private RaycastHit hit;

    //Deformation variables
    private int xRes;
    private int yRes;
    private float[,] savedHeights;
    private float[,,] savedAlphamaps;
    float flattenTarget = 0;
    Color[] craterData;

    TerrainData tData;

    float strengthNormalized
    {
        get
        {
            return (strength) / 9.0f;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //Find build target object
        buildTarget = GameObject.Find("BuildTarget").transform;

        //Add Spot Light to build target
        GameObject spotLightObj = new GameObject("SpotLight");
        spotLightObj.transform.SetParent(buildTarget);
        spotLightObj.transform.localPosition = new Vector3(0, 2, 0);
        spotLightObj.transform.localEulerAngles = new Vector3(90, 0, 0);
        spotLight = spotLightObj.AddComponent<Light>();
        spotLight.type = LightType.Spot;
        spotLight.range = 20;

        tData = terrain.terrainData;
        if (tData)
        {
            //Save original height data
            xRes = tData.heightmapResolution;
            yRes = tData.heightmapResolution;
            savedHeights = tData.GetHeights(0, 0, xRes, yRes);
            savedAlphamaps = tData.GetAlphamaps(0, 0, tData.alphamapResolution, tData.alphamapResolution);
        }

        //Change terrain layer to UI
        strength = 2;
        area = 2;
        brushScaling();
    }

    void FixedUpdate()
    {
        if (editingTerrain || paintingTerrain) {
            raycastHit();

            if (Input.mousePosition.x > 250f && onTerrain && !onWindow)
            {
                terrainDeform();
            }

            //Update Spot Light Angle according to the Area value
            spotLight.spotAngle = area * 25f;
        }
    }

    //Raycast
    //______________________________________________________________________________________________________________________________
    void raycastHit()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        hit = new RaycastHit();
        //Do Raycast hit only against UI layer
        if (Physics.Raycast(ray, out hit, 300, 1 << 13))
        {
            onTerrain = true;
            if (buildTarget)
            {
                buildTarget.position = Vector3.Lerp(buildTarget.position, hit.point + new Vector3(0, 1, 0), Time.time);

                //Get height data
                lastHeight = hit.point.y / tData.size.y;
                pointingHeightText.text = "Pointing height : " + lastHeight;
            }
        }
        else
        {
            if (buildTarget)
            {
                Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 200);
                Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint);
                buildTarget.position = curPosition;
                onTerrain = false;
            }
        }
    }

    //TerrainDeformation
    //___________________________________________________________________________________________________________________
    void terrainDeform()
    {
        if (Input.GetMouseButtonDown(0))
        {
            buildTargPos = buildTarget.position - terrain.GetPosition();
            float x = Mathf.Clamp01(buildTargPos.x / tData.size.x);
            float y = Mathf.Clamp01(buildTargPos.z / tData.size.z);
            flattenTarget = tData.GetInterpolatedHeight(x, y) / tData.heightmapScale.y;
        }

        //Terrain deform
        if (Input.GetMouseButton(0))
        {
            buildTargPos = buildTarget.position - terrain.GetPosition();

            if (Input.GetKey(KeyCode.LeftShift))
            {
                strengthSave = strength;
            }
            else
            {
                strengthSave = -strength;
            }

            if (newTex && tData && craterData != null)
            {
                int x = (int)Mathf.Lerp(0, xRes, Mathf.InverseLerp(0, tData.size.x, buildTargPos.x));
                int z = (int)Mathf.Lerp(0, yRes, Mathf.InverseLerp(0, tData.size.z, buildTargPos.z));
                x = Mathf.Clamp(x, newTex.width / 2, xRes - newTex.width / 2);
                z = Mathf.Clamp(z, newTex.height / 2, yRes - newTex.height / 2);
                int startX = x - newTex.width / 2;
                int startY = z - newTex.height / 2;

                //int fixedX = (int) (startX + newTex.width > newTex.width ? startX : tData.size.x - newTex.width);
                int fixedX = startX;
                //int fixedY = (int) (startY + newTex.height > newTex.height ? startY : tData.size.y - newTex.height);
                int fixedY = startY;

                float[,] areaT = tData.GetHeights(fixedX, fixedY, newTex.width, newTex.height);
                float[,,] alphaAreas = tData.GetAlphamaps(fixedX, fixedY, newTex.width, newTex.height);
                int[,] details = tData.GetDetailLayer(fixedX, fixedY, newTex.width, newTex.height, 0);

                for (int i = 0; i < newTex.height; i++)
                {
                    for (int j = 0; j < newTex.width; j++)
                    {
                        if (paintingTerrain)
                        {
                            if (editingDetails)
                            {
                                details[i, j] = Random.Range(0f, 1f) >= strength ? 1 : 0;
                            }
                            else
                            {
                                if (craterData[i * newTex.width + j].a > 0.5f)
                                {
                                    for (int at = 0; at < 8; at++)
                                    {
                                        if (at == textureIndexSelected)
                                        {
                                            alphaAreas[i, j, at] = 1;
                                        }
                                        else
                                        {
                                            alphaAreas[i, j, at] = 0;
                                        }
                                    }
                                }
                            }
                            
                        } else if (deformMode == DeformMode.RaiseLower)
                        {
                            areaT[i, j] = areaT[i, j] - craterData[i * newTex.width + j].a * strengthSave / 15000;
                        }
                        else if (deformMode == DeformMode.FlattenByValue)
                        {
                            //areaT[i, j] = Mathf.Lerp(areaT[i, j], flattenTarget, craterData[i * newTex.width + j].a * strengthNormalized);
                            if (craterData[i * newTex.width + j].a > 0.5f)
                                areaT[i, j] = height;
                        }
                        else if (deformMode == DeformMode.FlattenByTerrain)
                        {
                            if (craterData[i * newTex.width + j].a > 0.5f)
                                areaT[i, j] = lastHeight;
                        }
                        else if (deformMode == DeformMode.Smooth)
                        {
                            if (i == 0 || i == newTex.height - 1 || j == 0 || j == newTex.width - 1)
                                continue;

                            float heightSum = 0;
                            for (int ySub = -1; ySub <= 1; ySub++)
                            {
                                for (int xSub = -1; xSub <= 1; xSub++)
                                {
                                    heightSum += areaT[i + ySub, j + xSub];
                                }
                            }

                            areaT[i, j] = Mathf.Lerp(areaT[i, j], (heightSum / 9), craterData[i * newTex.width + j].a * strengthNormalized);
                        }
                    }
                }
                tData.SetHeights(startX, startY, areaT);
                tData.SetAlphamaps(startX, startY, alphaAreas);
                tData.SetDetailLayer(startX, startY, 0, details);
            }
        }
    }

    public void brushScaling()
    {
        //Apply current deform texture resolution 
        newTex = Instantiate(deformTexture) as Texture2D;
        TextureScale.Point(newTex, deformTexture.width * (int)area / 10, deformTexture.height * (int)area / 10);
        newTex.Apply();
        craterData = newTex.GetPixels();
    }

    void OnApplicationQuit()
    {
        //Reset terrain height/alphamaps when exiting play mode
        //tData.SetHeights(0, 0, savedHeights);
        //tData.SetAlphamaps(0, 0, savedAlphamaps);
    }
}