using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SingleplayerTeamSettings : MonoBehaviour
{
    //Static values
    public const string DRIVER_CATEGORY_STATIC = "dc_static";
    public const string DRIVER_CATEGORY_PLAYER = "dc_player";
    public const string DRIVER_CATEGORY_AI = "dc_ai";

    private string selectedDriverCategoryID = DRIVER_CATEGORY_STATIC;

    private int teamId;

    [Header("Basic UI elements")]
    public TMP_Text teamName;
    public CanvasGroup godRaysEffect;
    public TMP_Dropdown controllingDriverSelect;

    [Header("Selecting team buttons")]
    public GameObject switchTeamBtn;

    [Header("Selecting driver buttons")]
    public RawImage[] selectDriverBtns;
    public Texture[] unselectedBtnTextures;
    public Texture[] selectedBtnTextures;

    [Header("Coloring")]
    public RawImage sideStriptImg;
    public Image godRaysEffectImg;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void init(int teamId, int defaultDriverCategoryID)
    {
        this.teamId = teamId;

        onSelectDriverConfiguration(defaultDriverCategoryID);
        Team team = GlobalList.teams[teamId];

        teamName.text = team.name;
        sideStriptImg.color = team.minimapColor;
        godRaysEffectImg.color = team.minimapColor;

        updateControllingDriverDropdownItems();
    }

    public void updateControllingDriverDropdownItems()
    {
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

        for (int i = 0; i < GlobalList.teamDrivers[selectedDriverCategoryID].Count; i++)
        {
            options.Add(new TMP_Dropdown.OptionData(GlobalList.teamDrivers[selectedDriverCategoryID].ElementAt(i).Value.driverName));
        }

        controllingDriverSelect.options = options;
    }

    public void onSelectDriverConfiguration(int id)
    {
        for (int i = 0; i < selectDriverBtns.Length; i++)
        {
            selectDriverBtns[i].texture = unselectedBtnTextures[i];
        }
        selectDriverBtns[id].texture = selectedBtnTextures[id];

        selectedDriverCategoryID = selectDriverBtns[id].gameObject.name;
        updateControllingDriverDropdownItems();
    }

    public void OnSwitchTeam()
    {
        //GameLevelUIController gameLevelController = FindObjectOfType<GameLevelUIController>();
        GameLevelLoader gameLevelLoader = FindObjectOfType<GameLevelLoader>();
        gameLevelLoader.changePlayerTeam(teamId);
        print(transform.parent.parent.parent.parent.parent.name);
        transform.parent.parent.parent.parent.GetComponent<MainMenuPanel>().OnClose();
    }

    public void switchToInGameMode()
    {
        setButtonInteractable(false);
        disableDriverDropdownSelect();

        if (selectedDriverCategoryID == DRIVER_CATEGORY_PLAYER)
        {
            enableSwitchTeamMode();
        }
    }

    public void setButtonInteractable(bool interactable)
    {
        for (int i = 0; i < selectDriverBtns.Length; i++)
        {
            selectDriverBtns[i].GetComponent<Button>().interactable = interactable;
        }
    }
    public void disableDriverDropdownSelect()
    {
        controllingDriverSelect.gameObject.SetActive(false);
    }

    public void enableSwitchTeamMode()
    {
        switchTeamBtn.SetActive(true);
    }

    public TeamDriver getSelectedDriverId()
    {
        return GlobalList.teamDrivers[selectedDriverCategoryID].ElementAt(controllingDriverSelect.value).Value;
    }

    public void forceSelectDriveCategoryID(string id)
    {
        selectedDriverCategoryID = id;
    }
}

