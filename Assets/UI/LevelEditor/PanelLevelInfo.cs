using SimpleFileBrowser;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PanelLevelInfo : MonoBehaviour
{
    //
    public PanelNewLevel panelNewLevel;
    public PlayerSettingsPanel panelPlayerSettings;

    public InputField terrainResolution;
    public RawImage iconImg;
    public InputField description;

    public Texture2D defaultIcon;

    public byte[] imageBytes; //PNG FORMAT

    public TerrainEdging terrainEdging;
    public Terrain mainTerrain;
    public Transform water;

    public BarMapObjects barMapObjects;
    public EditorManager editorManager;

    public void selectLevelIcon()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".png", ".jpg"));

        StartCoroutine(ShowImageLoadDialog());
    }
    IEnumerator ShowImageLoadDialog()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, false, null, null, "Load Image for Level", "Load");

        if (FileBrowser.Success)
        {
            for (int i = 0; i < FileBrowser.Result.Length; i++)
                Debug.Log(FileBrowser.Result[i]);

            updateLevelIcon(FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]));
        }
    }

    public void updateLevelIcon(byte[] imageBytes)
    {
        this.imageBytes = imageBytes;

        //Update UI texture
        Texture2D loadedImg = new Texture2D(1, 1);
        if (ImageConversion.LoadImage(loadedImg, imageBytes))
            iconImg.texture = loadedImg;
        else
            throw new Exception("Unable to update level icon image.");
    }

    public void setDefaultIcon()
    {
        iconImg.texture = defaultIcon;
        imageBytes = defaultIcon.EncodeToPNG();
    }
    
    public void OnPlayerBtn()
    {
        editorManager.showPlayerSettingsTabs();
        panelPlayerSettings.gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        terrainResolution.text = "" + mainTerrain.terrainData.size.x;
    }

    public void Ok()
    {
        TerrainData tData = mainTerrain.terrainData;

        //Change terrain size
        LevelData.ResizeTerrain(int.Parse(terrainResolution.text), terrainEdging, int.Parse(terrainResolution.text), true);

        //Hide panel
        gameObject.SetActive(false);
    }
}
