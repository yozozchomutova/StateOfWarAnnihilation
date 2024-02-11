using System.Collections.Generic;
using UnityEngine;

public class GlobalList : MonoBehaviour
{
    //Materials
    public static Material matHologramGreen;
    public static Material matHologramRed;
    public static Material matUnitConstructing;
    public static Material matLCDScreen;

    public static Material matUnitRangeDebug; //Temporary

    //Game assets/data
    public static Dictionary<string, Unit> units = new Dictionary<string, Unit>();
    public static Dictionary<string, UnitBody> bodies = new Dictionary<string, UnitBody>();
    public static Dictionary<string, ProducingUnit> producingUnits = new Dictionary<string, ProducingUnit>();
    public static Dictionary<string, MapObject> mapObjects = new Dictionary<string, MapObject>();
    public static Dictionary<string, TTNode> eventNodes = new Dictionary<string, TTNode>();
    public static Dictionary<string, WorldEnvironment.Weather> weathers = new Dictionary<string, WorldEnvironment.Weather>();

    //UI
    public static List<ProducingUnit> airForecsPU = new List<ProducingUnit>();

    [HideInInspector] public static Team[] teams;

    //Format: 1. Dictionary = Category ; 2. Dictionary = Driver type from that category
    public static Dictionary<string, Dictionary<string, TeamDriver>> teamDrivers = new Dictionary<string, Dictionary<string, TeamDriver>>();

    public bool loadOnStart;

    public static bool loadFinished = false;

    public enum AirForce
    {
        JET,
        DESTROYER,
        CYCLONE,
        CARRYBUS,
        DEBRIS
    }

    private void Awake()
    {
        loadFinished = false;
        if (loadOnStart)
            initLoading();
    }

    public static void initLoading() 
    {
        if (loadFinished)
            return;
        //Save instance
        //DontDestroyOnLoad(gameObject);
        units.Clear();
        bodies.Clear();
        producingUnits.Clear();
        mapObjects.Clear();
        eventNodes.Clear();
        weathers.Clear();
        teamDrivers.Clear();
        airForecsPU.Clear();

        //Materials
        matHologramGreen = Resources.Load<Material>("Materials/unitHologramGreen");
        matHologramRed = Resources.Load<Material>("Materials/unitHologramRed");
        matUnitConstructing = Resources.Load<Material>("Materials/unitMatConstruction");
        matLCDScreen = Resources.Load<Material>("Materials/screenLCDMat");
        matUnitRangeDebug = Resources.Load<Material>("Materials/unitRangeDebugMat");

        List<Team> teamList = new List<Team>();
        teamList.Add(new Team(Team.WHITE, 2, new Color(1f, 1f, 1f), "Neutral", "white"));
        teamList.Add(new Team(Team.BLUE, 0, new Color(0, 0, 1f), "Blue", "blue"));
        teamList.Add(new Team(Team.GREEN, 1, new Color(0, 1f, 0), "Green", "green"));
        teamList.Add(new Team(Team.RED, -1, new Color(1f, 0, 0), "Red", "red"));
        teamList.Add(new Team(Team.YELLOW, -1, new Color(0.93f, 0.988f, 0.012f), "Yellow", "yellow"));
        teamList.Add(new Team(Team.PURPLE, -1, new Color(0.51f, 0, 0.93f), "Purple", "purple"));
        teamList.Add(new Team(Team.PINK, -1, new Color(0.953f, 0.06f, 1f), "Pink", "pink"));
        teamList.Add(new Team(Team.ORANGE, -1, new Color(1f, 0.616f, 0), "Orange", "orange"));
        teamList.Add(new Team(Team.BROWN, -1, new Color(0.549f, 0.294f, 0), "Brown", "brown"));
        teamList.Add(new Team(Team.BLACK, -1, new Color(0, 0, 0), "Black", "black"));

        //Teams List to Array
        teams = new Team[teamList.Count];
        for (int i = 0; i < teamList.Count; i++)
        {
            teams[i] = teamList[i];
        }

        //Units
        loadUnit("commandCenter1"); //-Buildings
        loadUnit("1stFactory");
        loadUnit("2ndFactory");
        loadUnit("3rdFactory");
        loadUnit("navigator");
        loadUnit("goldMine1");
        loadUnit("researchStation1");
        loadUnit("windTurbine1");
        loadUnit("stepperFactory1");

        loadUnit("ant"); //-Towers
        loadUnit("antiAircraft");
        loadUnit("plasmatic");
        loadUnit("machineGun");
        loadUnit("granader");

        loadUnit("antiAir1"); //-Tanks
        loadUnit("antiAir2");
        loadUnit("antiAir3");
        loadUnit("longarm1");
        loadUnit("longarm2");
        loadUnit("longarm3");
        loadUnit("tonk1");
        loadUnit("tonk2");
        loadUnit("tonk3");
        loadUnit("flamingo1");
        loadUnit("flamingo2");
        loadUnit("flamingo3");
        loadUnit("heavy1");
        loadUnit("heavy2");
        loadUnit("heavy3");

        loadUnit("kodimizerS1"); //-Steppers & atomizer
        loadUnit("pinkyS1");
        loadUnit("culometS1");
        loadUnit("antiAircraftS1");
        loadUnit("shockerS1");
        loadUnit("atomizer1");

        loadUnit("SMF1"); //-SMF

        loadUnit("jet"); //-Air forces
        loadUnit("destroyer");
        loadUnit("cyclone");
        loadUnit("carrybus");
        loadUnit("debris");

        //Bodies
        loadBody("tankSmall"); //-Tanks
        loadBody("tankMedium");
        loadBody("tankLarge");

        loadBody("towerSmall"); //-Towers
        loadBody("towerMedium");
        loadBody("towerLarge");

        loadBody("stepper"); //-Steppers

        //Producing units
        loadProducingUnit("antiAircraft1"); //-Tanks
        loadProducingUnit("antiAircraft2");
        loadProducingUnit("antiAircraft3");
        loadProducingUnit("longarm1");
        loadProducingUnit("longarm2");
        loadProducingUnit("longarm3");
        loadProducingUnit("tonk1");
        loadProducingUnit("tonk2");
        loadProducingUnit("tonk3");
        loadProducingUnit("flamingo1");
        loadProducingUnit("flamingo2");
        loadProducingUnit("flamingo3");
        loadProducingUnit("heavy1");
        loadProducingUnit("heavy2");
        loadProducingUnit("heavy3");

        loadProducingUnit("kodimizerS1"); //-Steppers
        loadProducingUnit("pinkyS1");
        loadProducingUnit("culometS1");
        loadProducingUnit("antiAircraftS1");
        loadProducingUnit("shockerS1");
        loadProducingUnit("atomizer1");

        loadProducingUnit("goldbrick1"); //-Building resources
        loadProducingUnit("research1");
        loadProducingUnit("energy1");

        loadProducingUnit("jet1"); //-Air forces
        loadProducingUnit("destroyer1");
        loadProducingUnit("cyclone1");
        loadProducingUnit("carrybus1");
        loadProducingUnit("debris1");

        //Map objects
        loadMapObject("tree1");
        loadMapObject("cactus1");

        //Event nodes
        loadEventNode("startBase");
        loadEventNode("timeCountdown");
        loadEventNode("giveResources");
        loadEventNode("giveAirforces");
        loadEventNode("launchAirforce");
        loadEventNode("lastCCStanding");
        loadEventNode("lastUnitStanding");
        loadEventNode("winGame");
        loadEventNode("loseGame");
        loadEventNode("autopathUnit");
        loadEventNode("unitCheckDistance");
        loadEventNode("isUnitDead");
        loadEventNode("extender");

        //Weathers
        createWeather(WorldEnvironment.WEATHER_SUNNY, "Sunny", "sun");
        createWeather(WorldEnvironment.WEATHER_PARTIALLY_SUNNY, "Partially Sunny", "partially_sun");
        createWeather(WorldEnvironment.WEATHER_CLOUDY, "Cloudy", "cloudy");
        createWeather(WorldEnvironment.WEATHER_WINDY, "Windy", "windy");
        createWeather(WorldEnvironment.WEATHER_FOGGY, "Foggy", "foggy");
        createWeather(WorldEnvironment.WEATHER_RAINY, "Rainy", "rainy");
        createWeather(WorldEnvironment.WEATHER_SNOWY, "Snowy", "snowy");
        createWeather(WorldEnvironment.WEATHER_THUNDER, "Thunder", "thunder");
        createWeather(WorldEnvironment.WEATHER_STORMY, "Stormy", "stormy");

        //AIs
        createTeamDriverCategory(SingleplayerTeamSettings.DRIVER_CATEGORY_STATIC);
        loadTeamDriverTYpe(SingleplayerTeamSettings.DRIVER_CATEGORY_STATIC, "staticIdle");
        loadTeamDriverTYpe(SingleplayerTeamSettings.DRIVER_CATEGORY_STATIC, "staticPusher");

        createTeamDriverCategory(SingleplayerTeamSettings.DRIVER_CATEGORY_PLAYER);
        loadTeamDriverTYpe(SingleplayerTeamSettings.DRIVER_CATEGORY_PLAYER, "playerMain");

        createTeamDriverCategory(SingleplayerTeamSettings.DRIVER_CATEGORY_AI);
        loadTeamDriverTYpe(SingleplayerTeamSettings.DRIVER_CATEGORY_AI, "aiClassic");
        loadTeamDriverTYpe(SingleplayerTeamSettings.DRIVER_CATEGORY_AI, "aiAdvanced");
        //loadTeamDriverTYpe(SingleplayerTeamSettings.DRIVER_CATEGORY_AI, "nnFirstDriver");
        //loadTeamDriverTYpe(SingleplayerTeamSettings.DRIVER_CATEGORY_AI, "aiEasy");
        //loadTeamDriverTYpe(SingleplayerTeamSettings.DRIVER_CATEGORY_AI, "aiMedium");
        //loadTeamDriverTYpe(SingleplayerTeamSettings.DRIVER_CATEGORY_AI, "aiHard");
        //loadTeamDriverTYpe(SingleplayerTeamSettings.DRIVER_CATEGORY_AI, "aiExperimental");

        //UI
        airForecsPU.Add(producingUnits["0_jet1"]);
        airForecsPU.Add(producingUnits["0_destroyer1"]);
        airForecsPU.Add(producingUnits["0_cyclone1"]);
        airForecsPU.Add(producingUnits["0_carrybus1"]);
        airForecsPU.Add(producingUnits["0_debris1"]);

        //Load finished!
        loadFinished = true;
    }

    #region Loading functions
    private static void loadUnit(string unitPath)
    {
        //print(unitPath);
        Unit unit = Resources.Load<Unit>("UnitsNew/" + unitPath);
        unit.onLoad();
        units.Add(unit.id, unit);
    }

    private static void loadBody(string bodyPath)
    {
        //print(bodyPath);
        UnitBody body = Resources.Load<UnitBody>("Bodies/" + bodyPath);
        bodies.Add(body.id, body);
    }

    private static void loadProducingUnit(string puPath)
    {
        //print(puPath);
        ProducingUnit pu = Resources.Load<ProducingUnit>("Producing Units/" + puPath);
        producingUnits.Add(pu.puId, pu);
    }

    private static void loadMapObject(string objectPath)
    {
        //print(objectPath);
        MapObject mapObject = Resources.Load<MapObject>("Map Objects/" + objectPath);
        mapObjects.Add(mapObject.objectId, mapObject);
    }

    private static void loadEventNode(string enPath)
    {
        //print(enPath);
        TTNode eventNode = Resources.Load<TTNode>("Event Nodes/" + enPath);
        eventNodes.Add(eventNode.nodeId, eventNode);
    }

    private static void createWeather(string id, string name, string iconPath)
    {
        WorldEnvironment.Weather weather = new WorldEnvironment.Weather(id, name, "Weather Icons/weather_" + iconPath);
        weathers.Add(id, weather);
    }
    #endregion

    #region Team drivers
    private static void createTeamDriverCategory(string id)
    {
        teamDrivers.Add(id, new Dictionary<string, TeamDriver>());
    }

    private static void loadTeamDriverTYpe(string id, string driverPath)
    {
        TeamDriver teamDriver = Resources.Load<TeamDriver>("Team Drivers/" + driverPath);
        teamDriver.driverCategoryId = id;
        teamDrivers[id].Add(teamDriver.id, teamDriver);
    }
    #endregion
}
