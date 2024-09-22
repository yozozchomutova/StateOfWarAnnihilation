using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;

public class WorldEnvironment
{
    #region [Variables] Static general parameters
    public const float FRAME_UPDATE_TIME = 1.0f; //Default: 1.0f
    public const float CHECK_EVENTS_TIME = 10.0f;
    public const int TIME_NIGHT_START = 1040;
    public const int TIME_DAY_START = 360;
    #endregion
    #region [Variables] Weather IDs
    public static readonly string WEATHER_SUNNY = "0_sunny";
    public static readonly string WEATHER_PARTIALLY_SUNNY = "0_partiallySunny";
    public static readonly string WEATHER_CLOUDY = "0_cloudy";
    public static readonly string WEATHER_WINDY = "0_windy";
    public static readonly string WEATHER_FOGGY = "0_foggy";
    public static readonly string WEATHER_RAINY = "0_rainy";
    public static readonly string WEATHER_SNOWY = "0_snowy";
    public static readonly string WEATHER_THUNDER = "0_thunder";
    public static readonly string WEATHER_STORMY = "0_stormy";
    #endregion
    #region [Variables] Weather constants
    public const float MAX_TIME = 1440f;
    #endregion
    #region [Variables] Private Game meta
    public Text linkClock;
    public Image linkIcon;

    public ReflectionProbe reflectionProbe;
    public Light sun;
    #endregion

    /// <summary>Actuall game time</summary>
    public int time;
    /// <summary> Static => time won't change.</summary>
    public bool timeStatic = true;
    public Weather weatherCurrent;
    public Weather weatherDefault;
    public List<Event> events = new List<Event>();

    private bool isNight = false;

    public void init(ReflectionProbe reflectionProbe, Light sun)
    {
        this.reflectionProbe = reflectionProbe;
        this.sun = sun;

        weatherCurrent = GlobalList.weathers[WEATHER_SUNNY];
        weatherDefault = GlobalList.weathers[WEATHER_SUNNY];
        setTime(540);
    }

    public void LinkUI(Text linkClock, Image linkIcon)
    {
        this.linkClock = linkClock;
        this.linkIcon = linkIcon;
        linkIcon.sprite = weatherDefault.loadSprite();
    }

    public int eventCheckCoooldown = 0;
    public void onFrameUpdate()
    {
        //Update clock
        if (!timeStatic)
        {
            time++;
            if (time >= MAX_TIME)
                time = 0;

            OnTimeUpdate();
        }        

        //Update ui
        linkClock.text = toTextTimeOutput(time);
        if (!linkIcon.sprite)
            linkIcon.sprite = weatherDefault.loadSprite();
    }

    public void OnTimeUpdate()
    {
        //Update game environment depending on current weather
        float timeNormalized = (float)LevelData.environment.time / WorldEnvironment.MAX_TIME;
        float sunAngle = timeNormalized * 360 - 90;
        sun.transform.eulerAngles = new Vector3(sunAngle, -70, -70);
    }

    private void onEventCheck()
    {
        if (time > TIME_NIGHT_START || time <= TIME_DAY_START)
        {
            if (!isNight)
            {
                isNight = true; 
                foreach (Unit u in LevelData.units)
                {
                    if (u.body != null && u.virtualSpace == Unit.VirtualSpace.NORMAL)
                        u.body.onNightStart();
                }
            }
        } else if (time > TIME_DAY_START)
        {
            if (isNight)
            {
                isNight = false;
                foreach (Unit u in LevelData.units)
                {
                    if (u.body != null && u.virtualSpace == Unit.VirtualSpace.NORMAL)
                        u.body.onDayStart();
                }
            }
        }
    }

    /// <summary> Calls onNightStart or onDayStart, depending on time</summary><param name="body"></param>
    public void callBodyTimeCallback(UnitBody body)
    {
        if (time > TIME_NIGHT_START || time <= TIME_DAY_START)
            body.onNightStart();
        else
            body.onDayStart();
    }

    public void setTime(int newTime)
    {
        this.time = newTime;
        OnTimeUpdate();
        onEventCheck();
    }

    ///<summary> Returns in format: "9:30" </summary>
    public static string toTextTimeOutput(int time)
    {
        int hours = time / 60;
        int minutes = time % 60;

        string additiveMinutesText = (minutes < 10) ? "0" : "";

        return hours + ":" + additiveMinutesText + minutes;
    }

    [System.Serializable]
    public class Weather
    {
        public string id, name, iconPath;

        public Weather(string id, string name, string iconPath)
        {
            this.id = id;
            this.name = name;
            this.iconPath = iconPath;
        }

        public Sprite loadSprite()
        {
            return Resources.Load<Sprite>(iconPath);
        }
    }

    [System.Serializable]
    public class Event
    {
        ///<summary> Weather type to happen </summary>
        public Weather weather;
        ///<summary> Time ranging from 0 to 1440, indicating START of the event </summary>
        public int startTime;
        ///<summary> Time ranging from 0 to 1440, indicating END of the event </summary>
        public int endTime;

        public Event(Weather weather, int startTime, int endTime)
        {
            this.weather = weather;
            this.startTime = startTime;
            this.endTime = endTime;
        }

        ///<summary> Returns in format: "09:30 - 12:30" (startTime - endTime) </summary>
        public string toTextTimeRangeOutput()
        {
            return toTextTimeOutput(startTime) + " - " + toTextTimeOutput(endTime);
        }
    }
}
