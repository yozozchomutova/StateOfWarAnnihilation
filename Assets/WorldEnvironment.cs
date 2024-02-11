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
    public const int TIME_NIGHT_START = 1320;
    public const int TIME_DAY_START = 480;
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
    #endregion

    [System.Serializable] public class Weather
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

    [System.Serializable] public class Event
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

    public int time;
    /// <summary> Static => time won't change.</summary>
    public bool timeStatic;
    public Weather weatherCurrent;
    public Weather weatherDefault;
    public List<Event> events = new List<Event>();

    public void init(Text linkClock, Image linkIcon)
    {
        init();

        this.linkClock = linkClock;
        this.linkIcon = linkIcon;
        linkIcon.sprite = weatherDefault.loadSprite();
    }

    public void init()
    {
        time = 540;
        weatherCurrent = GlobalList.weathers[WEATHER_PARTIALLY_SUNNY];
        weatherDefault = GlobalList.weathers[WEATHER_PARTIALLY_SUNNY];
        onEventCheck();
    }

    public int eventCheckCoooldown = 0;
    public void onFrameUpdate()
    {
        //Update clock
        if (!timeStatic)
        {
            time++;
            if (time >= MAX_TIME)
            {
                time = 0;
            }
        }

        //Check events every 10 game minutes
        if (time % 10 == 0)
        {
            onEventCheck();
        }

        //Update ui
        linkClock.text = toTextTimeOutput(time);
        linkIcon.sprite = weatherDefault.loadSprite();
    }

    public void updateDaylight(Light directionalLight)
    {
        //Update game environment depending on current weather
        float timeNormalized = (float)LevelData.environment.time / WorldEnvironment.MAX_TIME;

        //Different interpolations
        if (timeNormalized < 0.208333f) //0:00 - 5:00
        {
            float progress = timeNormalized / 0.208333f;
            timeNormalized = Mathf.Lerp(0, 0.02f, progress);
        }
        else if (timeNormalized < 0.4166667f) //5:00 - 10:00
        {
            float progress = (timeNormalized - 0.208333f) / (0.4166667f - 0.208333f);
            timeNormalized = Mathf.Lerp(0.02f, 0.2f, progress);
        }
        else if (timeNormalized < 0.58333333f) //10:00 - 14:00
        {
            float progress = (timeNormalized - 0.4166667f) / (0.58333333f - 0.4166667f);
            timeNormalized = Mathf.Lerp(0.2f, 1f, progress);
        }
        else if (timeNormalized < 0.70833333f) //14:00 - 17:00
        {
            float progress = (timeNormalized - 0.58333333f) / (0.70833333f - 0.58333333f);
            timeNormalized = Mathf.Lerp(1f, 0.75f, progress);
        }
        else if (timeNormalized < 0.875) //17:00 - 21:00
        {
            float progress = (timeNormalized - 0.70833333f) / (0.875f - 0.70833333f);
            timeNormalized = Mathf.Lerp(0.75f, 0.06f, progress);
        }
        else if (timeNormalized <= 1) //9:00 - 23:59
        {
            float progress = (timeNormalized - 0.875f) / (1f - 0.875f);
            timeNormalized = Mathf.Lerp(0.06f, 0f, progress);
        }

        float exposure = 0.05f + timeNormalized * 1.1f;
        float lightIntensity = 0.05f + timeNormalized * 2.0f;
        UnityEngine.RenderSettings.skybox.SetFloat("_Exposure", exposure);
        directionalLight.intensity = lightIntensity;
    }

    private bool isNight = false;
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
        //eventCheckCoooldown = 10;
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
}
