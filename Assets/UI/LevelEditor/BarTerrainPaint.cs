using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class BarTerrainPaint : MonoBehaviour
{
    private bool painting = false;

    [Header("Editor manager")]
    public YZCH_TerrainEditor terrainEditor;

    [Header("Switching panels")]
    public GameObject editTerrain;
    public GameObject drawTexturesPanel;
    public GameObject drawDetailsPanel;

    [Header("Head UI")]
    public Slider waterLevelSlider;
    public Dropdown modeSelector;

    [Header("Editing terrain mode")]
    public Dropdown deformTerarainMode;
    public Slider heightSlider;

    [Header("Drawing textures mode")]
    public Slider DT_brushSize;
    public Slider alphaSlider;
    public Dropdown textureSelector;
    public RectTransform brushesContentList;
    public RectTransform terrainsContentList;

    [Header("Drawing details mode")]
    public Slider DD_brushSize;
    public Slider detailIntensity;

    [Header("UI Building references")]
    public Texture2D[] brushes;
    public UIListElement1 uiElement1;
    private UIListElement1[] brushUIElements;
    private UIListElement1[] textureUIElements;

    void Start()
    {
        Terrain t = GameObject.FindObjectOfType<Terrain>();

        //Fill brush-selection list
        int x = 5, y = -5;
        brushUIElements = new UIListElement1[brushes.Length];
        for (int i = 0; i < brushUIElements.Length; i++)
        {
            int finalI = i;
            UIListElement1 newElement = Instantiate(uiElement1, brushesContentList);
            Button.ButtonClickedEvent clickEvent = new Button.ButtonClickedEvent();
            clickEvent.AddListener(delegate { onBrushTextureChange(finalI); });
            newElement.btn.onClick = clickEvent;
            newElement.setPosition(x, y);
            newElement.updateParameters(brushes[i], brushes[i].name);
            brushUIElements[i] = newElement;

            x += 53;
            if ((i + 1) % 4 == 0)
            {
                x = 5;
                y -= 69;
            }
        }

        //Fill tearrain-selection list
        x = 5;
        y = -5;
        textureUIElements = new UIListElement1[t.terrainData.alphamapLayers];
        for (int i = 0; i < textureUIElements.Length; i++)
        {
            int finalI = i;
            UIListElement1 newElement = Instantiate(uiElement1, terrainsContentList);
            Button.ButtonClickedEvent clickEvent = new Button.ButtonClickedEvent();
            clickEvent.AddListener(delegate { onTerrainTextureChange(finalI); });
            newElement.btn.onClick = clickEvent;
            newElement.setPosition(x, y);
            newElement.updateParameters(t.terrainData.terrainLayers[i].diffuseTexture, t.terrainData.terrainLayers[i].name);
            newElement.setHighlight(i == 0);
            textureUIElements[i] = newElement;

            x += 53;
            if ((i + 1) % 4 == 0)
            {
                x = 5;
                y -= 69;
            }
        }

        //
        onModeChange();
        onBrushTextureChange(0);
        onTerrainTextureChange(0);
    }

    public void onBrushTextureChange(int id)
    {
        for (int i = 0; i < brushUIElements.Length; i++)
        {
            brushUIElements[i].setHighlight(i == id);
        }
        terrainEditor.changeBrush(brushes[id]);
    }

    public void onTerrainTextureChange(int id)
    {
        for (int i = 0; i < textureUIElements.Length; i++)
        {
            textureUIElements[i].setHighlight(i == id);
        }
        terrainEditor.setSelectedTextureId(id);
    }

    public void OnDeformModeChange()
    {
        if (deformTerarainMode.value == 0)
        {
            terrainEditor.editTerrainMode = YZCH_TerrainEditor.TerrainEditMode.RAISE_LOWER;
        }
        else if (deformTerarainMode.value == 1)
        {
            terrainEditor.editTerrainMode = YZCH_TerrainEditor.TerrainEditMode.FLATTEN_BY_VALUE;
        }
        else if (deformTerarainMode.value == 2)
        {
            terrainEditor.editTerrainMode = YZCH_TerrainEditor.TerrainEditMode.FLATTEN_BY_HEIGHT;
        }
        else if (deformTerarainMode.value == 3)
        {
            terrainEditor.editTerrainMode = YZCH_TerrainEditor.TerrainEditMode.SMOOTH;
        }
    }

    public void onModeChange()
    {
        editTerrain.SetActive(false);
        drawTexturesPanel.SetActive(false);
        drawDetailsPanel.SetActive(false);

        if (modeSelector.value == 0) //Edit terrain
        {
            editTerrain.SetActive(true);
            terrainEditor.changeMode(YZCH_TerrainEditor.Mode.EDIT_TERRAIN);
            OnBrushSettingsChange();
        } else if (modeSelector.value == 1) //Paint texture
        {
            drawTexturesPanel.SetActive(true);
            terrainEditor.changeMode(YZCH_TerrainEditor.Mode.PAINT_TERRAIN);
            OnBrushSettingsChange();
        } else if (modeSelector.value == 2) //Draw details
        {
            drawDetailsPanel.SetActive(true);
            terrainEditor.changeMode(YZCH_TerrainEditor.Mode.PAINT_DETAILS);
            //OnDrawingDetailsSettingsChange();
        }
    }

    public void OnBrushSettingsChange()
    {
        //Brush size
        //float offsetArea = DT_brushSize.value % 2;
        terrainEditor.setBrushSize((int)DT_brushSize.value/* - offsetArea*/);
        //terrainEditor.brushScaling();

        terrainEditor.strength = alphaSlider.value; //Alpha
        terrainEditor.height = heightSlider.value; //Height
    }

    public void OnTextureSelectorChange(int id)
    {
        terrainEditor.setSelectedTextureId(id);
    }

    private void OnEnable()
    {
        terrainEditor.setBrushImageVisibility(true);
    }

    private void OnDisable()
    {
        terrainEditor.setBrushImageVisibility(false);
        terrainEditor.mode = YZCH_TerrainEditor.Mode.NONE;
    }
}
