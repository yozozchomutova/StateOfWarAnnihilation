#region [Libraries] All libraries
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Random;
using static MapLevelManager;
using static MapLevelManager.UnitSerializable;
using SOWUtils;
#endregion

public abstract class Unit : UnitReference
{
    #region [Variables] Data info
    [Header("Data info")]
    /// <summary> Most important information for identifying unit. Once set, It's highly not recommended to change, due to complications that it can cause. FORMAT: number_string (Ex. 0_commandCenter1) </summary>
    [Tooltip("Most important information for identifying unit. Once set, It's highly not recommended to change, due to complications that it can cause. FORMAT: number_string (Ex. 0_commandCenter1)")]
    public string id = "categoryId_unitId";

    /// <summary> This id is for State of War: Classic/Warmonger level exporter. (-1 = this unit won't be saved.) </summary>
    [Tooltip("This id is for State of War: Classic/Warmonger level exporter. (-1 = this unit won't be saved.)")]
    public int unitSOWCWId = -1;

    /// <summary>ID of required body</summary>
    [Tooltip("ID of required body")]
    public string bodyId = "";

    /// <summary> Displayed name of the unit. </summary>
    [Tooltip("Displayed name of the unit.")]
    public new string name;
    /// <summary> Displayed name of the unit. </summary>
    [Tooltip("Displayed icon of the unit.")]
    public Texture icon;
    /// <summary> How much space does it take on grid </summary>
    [Tooltip("How much space does it take on grid")]
    public Vector2Int gridSize;
    #endregion
    #region [Variables] Gameplay unit properties
    //Classification
    /// <summary> In which virtual space unit is. Check enum description for more info.</summary>
    [HideInInspector] public VirtualSpace virtualSpace;
    /// <summary> Unit type </summary>
    [HideInInspector] public UnitType unitType;
    /// <summary> Tells how much expensive is unit. For towers it means, how much do they cost. </summary>
    [HideInInspector] public float value;

    //Health
    /// <summary> Is unit valid & playable </summary>
    [HideInInspector] public bool valid;
    /// <summary> Maximal unit health </summary>
    [HideInInspector] public float hpMax;
    /// <summary> Current unit health </summary>
    [HideInInspector] public float hp;
    /// <summary> If unit dies, should it be respawned again? </summary>
    [HideInInspector] public bool respawnOnDeath;
    /// <summary> If unit is protected against air forces </summary>
    [HideInInspector] public bool airShield;

    //Upgradable formation
    /// <summary> Id of unit, which can be transfered to </summary>
    [HideInInspector] public string upgradableToUnit;
    /// <summary> Progress ranging from 0 to 100, whereas at 100 means next upgrade.</summary>
    [HideInInspector] [Range(0f, 100f)] public float upgradeProgress;

    //Motion (Moving, rotating)
    /// <summary> Maximal body rotating/angular velocity </summary>
    [HideInInspector] public float headMaxYawRotSpeed;
    /// <summary> Velocity of units head </summary>
    [HideInInspector] public Vector3 headVelocity;
    /// <summary> Angular velocity of units head </summary>
    [HideInInspector] public Vector3 headRotation;

    //Placing on map
    /// <summary> Minimum distance required for placing unit somewhere </summary>
    [HideInInspector] public float placementDistance;
    #endregion
    #region [Variables] Categorizing
    //Team categorizing
    [HideInInspector] public Team team;

    #endregion
    #region [Variables] UI
    [Header("UI")]
    /// <summary> UI  </summary>
    public GameObject propertiesUI_editor;
    /// <summary> Maximal body rotating/angular velocity </summary>
    public GameObject propertiesUI_game;
    #endregion
    #region [Variables] VFX
    /// <summary> Type of effect, when unit dies </summary>
    public DeathEffect unitDeathVFXType = DeathEffect.NONE;
    /// <summary> Do specific effect, when unit dies </summary>
    public GameObject unitDeathVFX;
    #endregion

    #region [Functions] Custom init()
    public override void init()
    {
        base.init();
        valid = true;
    }

    #endregion
    #region [Functions] Gameplay properties managing
    public static void assignGameplayProperties(object targetClass, Type classType, string csvFileName)
    {
        string csvText;
        try
        {
            csvText = Resources.Load<TextAsset>(csvFileName).text;
        } catch (NullReferenceException e)
        {
            Debug.LogError("Failed to load CSV file. Are you sure you are using valid path? Path: " + csvFileName);
            Debug.LogException(e);
            return;
        }

        string[] lines = csvText.Split(new char[] { (char)0x0A } );
        string[] fieldNames = new string[0];
        string[] fieldTypes = new string[0];

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Length <= 1) //Probably reached end?
                break;

            lines[i] = lines[i].Substring(0, lines[i].Length - 1); //Remove last byte, because it makes mess later in code

            string[] values = lines[i].Split(new char[] {','});
            if (i == 0) //First line MUST always contain keys
            {
                fieldNames = new string[values.Length];
                Array.Copy(values, fieldNames, values.Length);
                continue;
            } else if (i == 1) //Second line MUST always contain field types
            {
                fieldTypes = new string[values.Length];
                Array.Copy(values, fieldTypes, values.Length);
                continue;
            }

            if (!values[0].Equals((targetClass as Unit).id)) //First element in column MUST always contain key of unit.
                continue;

            if (fieldNames == null)
            {
                Debug.LogError("Header not initialized. Is file empty? File: '" + csvFileName + "'");
                return;
            }

            for (int k = 1; k < values.Length; k++)
            {
                FieldInfo field = classType.GetField(fieldNames[k], BindingFlags.Public | BindingFlags.Instance);
                if (field == null)
                {
                    Debug.LogError("Public field of '" + classType.Name + "', not found: '" + fieldNames[k] + "'");
                    return;
                }

                try
                {
                    if (fieldTypes[k] == "F")
                    {
                        field.SetValue(targetClass, float.Parse(values[k]));
                    }
                    else if (fieldTypes[k] == "I")
                    {
                        field.SetValue(targetClass, int.Parse(values[k]));
                    }
                    else if (fieldTypes[k] == "S") //String
                    {
                        field.SetValue(targetClass, values[k]);
                    }
                    else if (fieldTypes[k] == "A") //String array separated with '|'
                    {
                        string[] separatedValues = values[k].Split(new char[] { '|' });
                        field.SetValue(targetClass, separatedValues);
                    } else if (fieldTypes[k] == "B")
                    {
                        field.SetValue(targetClass, int.Parse(values[k]) == 1);
                    }
                } catch (ArgumentException e)
                {
                    Debug.LogError("Public field of '" + classType.Name + "', incorrect type converting in field: '" + fieldNames[k] + "' Field type: " + fieldTypes[k]);
                    Debug.LogError(e.Message);
                }
            }
        }
    }

    /// <returns>Calculates hp / hpMax and returns value between 0...1</returns>
    public float getHpNormalized()
    {
        return hp / hpMax;
    }

    /// <returns>If unit has or should have body ID</returns>
    public bool hasBodyID()
    {
        return bodyId != "";
    }

    /// <summary> Deals damage to unit </summary>
    public void damage(int sourceTeamId, float damage)
    {
        hp -= damage;
        hp = Mathf.Max(0, hp); // Border/Limit
        if (hp == 0)
            destroyUnit(sourceTeamId);
    }
    #endregion
    #region [Functions] UI Get/Set Editor/Game values
    public void setValues(float hpPercentage, Team newTeam, GameObject additionalProp)
    {
        hp = hpMax * hpPercentage;
        team = newTeam;

        onUnitDataUpdate(additionalProp);
    }

    public void getValues(Slider hpPercentage, out int teamID, GameObject additionalProp)
    {
        hpPercentage.value = getHpNormalized();
        teamID = team.id;

        onUnitDataGet(additionalProp);
    }

    public void getGameValUpdate(Slider ui_unitHealth, Slider ui_unitUpgrade, Text ui_unitHealthText, GameObject additionalProp)
    {
        ui_unitHealth.value = getHpNormalized();
        ui_unitHealthText.text = ((int)(getHpNormalized() * 100)) + "%";
        ui_unitUpgrade.value = upgradeProgress;

        onUnitGameDataGetUpdate(additionalProp);
    }

    public void getGameValStart(Image ui_unitTeamFrame, Image ui_unitTeamLine, Text ui_unitTeamText, GameObject additionalProp)
    {
        ui_unitTeamFrame.color = team.minimapColor;
        ui_unitTeamLine.color = team.minimapColor;
        ui_unitTeamText.text = team.name;
        ui_unitTeamText.color = team.id == Team.WHITE ? Color.black : Color.white;

        onUnitGameDataGetStart(additionalProp);
    }
    #endregion
    #region [Functions] Placing/Removing to/from game
    public void destroyUnit(int sourceTeamId)
    {
        if (!valid)
            return;

        valid = false;

        onDeath(); //Call all extenders of this abstract class

        //Used later
        TeamStats02_12 lostT = LevelData.teamStats[team.id];
        TeamStats02_12 gainT = LevelData.teamStats[sourceTeamId];

        if (respawnOnDeath)
        {
            UnitSerializable serialized = serializeUnit();
            serialized.setF(KEY_UNIT_HP, 1f);
            serialized.setI(KEY_UNIT_TEAMID, sourceTeamId);
            MapLevel.spawnUnit(serialized);

            if (LevelData.tsCmp(team.id))
                GameLevelUIController.logMessage("Building captured", true, gameObject.transform.position);

            //Register to won team
            if (unitType == UnitType.BUILDING)
                gainT.buildingsCaptured++;
        }

        //Register to lost team stats
        switch (unitType)
        {
            case UnitType.BUILDING:
                lostT.buildingsLost++;
                break;
            case UnitType.STEPPER:
            case UnitType.UNIT:
            case UnitType.SMF:
                lostT.unitsLost++;
                break;
            case UnitType.AIRFORCE:
                lostT.airForcesLost++;
                break;
            case UnitType.TOWER:
                lostT.towersLost++;
                break;
        }

        //Completely remove from game
        LevelManager.levelManager.selectedUnits.Remove(this);

        //Head blowing visual effect
        if (unitDeathVFXType == DeathEffect.HEAD_BLOW)
        {
            headVelocity = new Vector3(Range(-3, 3), 10f, Range(-3, 3));
            headRotation = new Vector3(Range(-2f, 2f), Range(-2f, 2f), Range(-2f, 2f));

            unit.gameObject.transform.parent = null;
            Rigidbody r = unit.gameObject.AddComponent<Rigidbody>();
            r.useGravity = true;
            r.velocity = headVelocity;
            r.angularVelocity = headRotation;

            //Show effect
            Destroy(Instantiate(unitDeathVFX, transform.position, Quaternion.identity), 6);
        }

        if (unitType == UnitType.BUILDING)
        {
            if (LevelData.tsCmp(team.id))
                GameLevelUIController.logMessage("Building lost", true, gameObject.transform.position);
        }

        destroyBoth();
    }
    public virtual bool canBePlaced(LevelData.Scene scene)
    {
        var (unitGridX, unitGridY) = LevelData.gridManager.SamplePosition(transform.position.x, transform.position.z);
        bool gridColliding = LevelData.gridManager.CheckForCollision(unitGridX, unitGridY, gridSize.x, gridSize.y, transform.eulerAngles.y);

        return !gridColliding;
    }
    #endregion
    #region [Functions] Utils
    public class UnitTypeComparer : IComparer<Unit>
    {
        public int Compare(Unit x,Unit y)
        {
            // Handle the case where x or y is null
            if (x == null)
            {
                return (y == null) ? 0 : 1;
            }
            else if (y == null)
            {
                return -1;
            }

            return -1 * x.unitType.CompareTo(y.unitType);
        }
    }
    /// <summary> Returns Unit from collider </summary>
    public static Unit getUnit(Collider col)
    {
        return col.GetComponent<UnitReference>().unit;
    }
    #endregion
    #region [Functions] Serialize/Deserialize unit class
    public UnitSerializable serializeUnit()
    {
        Dictionary<string, string> data = new Dictionary<string, string>
        {
            { KEY_UNIT_ROTATION, new Vector3Ser(transform.localEulerAngles).parseToString() },
            { KEY_BODY_POSITION, new Vector3Ser(getPosition()).parseToString() },
            { KEY_BODY_ROTATION, new Vector3Ser(getRotation()).parseToString() },
            { KEY_UNIT_ID, id },
            { KEY_UNIT_HP, "" + getHpNormalized() },
            { KEY_UNIT_TEAMID, "" + (team == null ? 0 : team.id) }
        };

        onUnitSerializing(data);
        return new UnitSerializable(data);
    }

    public void deserializeUnit(UnitSerializable unitData)
    {
        Dictionary<string, string> data = unitData.keys.Zip(unitData.values, (k, v) => new { Key = k, Value = v })
                                           .ToDictionary(item => item.Key, item => item.Value);
        transform.localEulerAngles = new Vector3Ser(data[KEY_UNIT_ROTATION]).toVector();
        setPosition(new Vector3Ser(data[KEY_BODY_POSITION]).toVector());
        setRotation(new Vector3Ser(data[KEY_BODY_ROTATION]).toVector());
        id = data[KEY_UNIT_ID];
        hp = float.Parse(data[KEY_UNIT_HP]) * hpMax;
        team = GlobalList.teams[int.Parse(data[KEY_UNIT_TEAMID])];

        onUnitDeserializing(data);
    }
    #endregion

    #region [Overrideable functions] Lifecycle
    /// <summary> Called, when unit is being loaded trough GlobalList</summary>
    public virtual void onLoad() {
        assignGameplayProperties(this, typeof(Unit), "UnitData/UnitData");
    }
    /// <summary> Called, when unit is spawned in MapLevel</summary>
    public virtual void onInit(Vector3 position, Vector3 rotation)
    {

    }
    #endregion
    #region [Overrideable functions] Basic commands
    /// <summary> Command from stopping unit doing anything </summary>
    public virtual void commandStop()
    {

    }
    /// <summary> Command for upgrading unit to next level </summary>
    public virtual void commandUpgradeLevel()
    {

    }
    #endregion
    #region [Overrideable functions] Can be Upgraded / Repaired
    /// <summary> Overridable function, called when some unit wants to upgrade this unit. In default returns always false</summary>
    /// <param name="source">Which unit requests to upgrade this unit</param>
    /// <returns>Allow if source unit can upgrade or not.</returns>
    public virtual bool canBeUpgraded(Unit source)
    {
        return false;
    }

    /// <summary> Overridable function, called when some unit wants to repair this unit. In default returns always false</summary>
    /// <param name="source">Which unit requests to repair this unit</param>
    /// <returns>Allow if source unit can repair or not</returns>
    public virtual bool canBeRepaired(Unit source)
    {
        return false;
    }
    #endregion
    #region [Overrideable functions] UI Get/Set Editor/Game unit data
    /// <summary> Called in EDITOR, when user DESELECTS unit</summary>
    /// <param name="additionalProp">UI which contains important values and settings for Unit</param>
    public virtual void onUnitDataUpdate(GameObject additionalProp) {}
    /// <summary> Called in EDITOR, when user SELECTS unit</summary>
    /// <param name="additionalProp">UI which contains important values and settings for Unit</param>
    public virtual void onUnitDataGet(GameObject additionalProp) { }

    /// <summary> Called IN LOOP in GAME, when user selects unit</summary>
    /// <param name="additionalProp">UI which contains important values and settings for Unit</param>
    public virtual void onUnitGameDataGetUpdate(GameObject additionalProp) { }
    /// <summary> Called ONCE in GAME, when user selects unit</summary>
    /// <param name="additionalProp">UI which contains important values and settings for Unit</param>
    public virtual void onUnitGameDataGetStart(GameObject additionalProp) { }
    #endregion
    #region [Overrideable functions] Serialize/Deserialize custom data of Unit
    /// <summary> Called when Unit is being saved somewhere in memory. Use it for storing data.</summary>
    public virtual void onUnitSerializing(Dictionary<string, string> data)
    {

    }
    /// <summary> Called when Unit is being loaded somewhere. Use it for loading data into variables.</summary>
    public virtual void onUnitDeserializing(Dictionary<string, string> data)
    {

    }
    #endregion
    #region [Overrideable functions] Death
    /// <summary> Called when Unit reaches 0 HP, or dies</summary>
    public virtual void onDeath()
    {

    }
    #endregion

    #region [Abstract functions] Frame Update
    /// <summary> Update() but called only in Game. Not in editor. </summary>
    public abstract void frameUpdate();
    #endregion

    #region [Enums] Unit Type
    public enum UnitType
    {
        BUILDING = 0,
        TOWER = 1,
        UNIT = 2,
        STEPPER = 3,
        SMF = 4,
        AIRFORCE = 5
    }
    #endregion
    #region [Enums] Unit Type
    public enum DeathEffect
    {
        NONE = 0,
        HEAD_BLOW = 1,
        BUILDING_EXPLOSION = 2
    }
    #endregion
    #region [Enums] Virtual space
    /// <summary> Virtual space tells how unit is currently being used. Different Virtual Spaces allows/forbids different functions for unit. </summary>
    public enum VirtualSpace
    {
        /// <summary> Unit behaves as normal unit, ready to go and fight </summary>
        NORMAL,
        /// <summary> Unit behaves as ghost unit, turning hologram green/red depending on collisions. Mostly used for placing on the map.</summary>
        GHOST,
        /// <summary> Unit behaves as construction unit, which is not done, not fighting, and is still constructing (by factory for example).</summary>
        CONSTRUCTION
    }
    #endregion

    #region [Extra] -> Converting to SOW:WARMONGER
    #region [Functions] Utils
    public virtual UnitSOWCW generateSOWCW(UnitSOWCW unitSOWCW, int mapSize)
    {
        unitSOWCW.unitId = unitSOWCWId;
        unitSOWCW.teamId = team.sowId;
        unitSOWCW.HPpercentage = getHpNormalized();
        // Debug.Log("H: " + unitSOWCW.HPpercentage);

        Vector3 tSize = LevelData.mainTerrain.terrainData.size;
        float halfMSize = tSize.x / 2f;
        float quaterMSize = tSize.x / 4f;
        int fullMapSize = mapSize * 32;

        unitSOWCW.x = fullMapSize - Mathf.RoundToInt((transform.position.z - quaterMSize) / halfMSize * fullMapSize);
        unitSOWCW.y = fullMapSize - Mathf.RoundToInt((transform.position.x - quaterMSize) / halfMSize * fullMapSize) - (1 * 32);

        unitSOWCW.x = Mathf.Clamp(unitSOWCW.x, 32, fullMapSize - (5 * 32));
        unitSOWCW.y = Mathf.Clamp(unitSOWCW.y, 32, fullMapSize - (5 * 32));

        unitSOWCW.xTile = (int)(unitSOWCW.x / 32f);
        unitSOWCW.yTile = (int)(unitSOWCW.y / 32f);
        return unitSOWCW;
    }

    public int countHowManyCanBeUpgraded()
    {
        //Count how many unit can be upgraded
        int upgradableTimes = 0;
        string nextStageUnitId = upgradableToUnit;
        while (!string.IsNullOrEmpty(nextStageUnitId))
        {
            upgradableTimes++;
            nextStageUnitId = GlobalList.units[nextStageUnitId].upgradableToUnit;
        }

        return 3 - upgradableTimes;
    }
    #endregion
    #endregion
}