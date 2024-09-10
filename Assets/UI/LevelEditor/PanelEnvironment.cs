#region [Libraries] All libraries
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#endregion

public class PanelEnvironment : MonoBehaviour, SimpleListView1_Item.IDeleteRequest
{
    #region [Variables] Main environemt settings/properties
    [Header("Basic environments")]
    public InputField startingHours;
    public InputField startingMinutes;
    public Dropdown defaultWeather;
    public Toggle weatherIsStatic;
    #endregion
    #region [Variabless] Event properties (Adding event tab)
    [Header("Add event properties")]
    public Dropdown eventWeatherType;
    public Slider eventTimeStart;
    public Slider eventTimeEnd;
    public TMP_Text eventTimeText;
    #endregion
    #region [Variables] Event list UI
    [Header("Event list")]
    public SimpleListView1_Item eventListItem;
    public RectTransform eventListContent;
    private List<SimpleListView1_Item> eventItemList = new List<SimpleListView1_Item>();
    #endregion
    #region [Variables] VFX Light
    [Header("VFX Light")]
    public Light directionalLight;
    #endregion

    #region [Functions] Unity's Start / Enable / Disable events
    public void Start()
    {
        //Load hungry weather-type dropdowns
        List<Dropdown.OptionData> data = new List<Dropdown.OptionData>();
        for (int i = 0; i < GlobalList.weathers.Count; i++)
        {
            WorldEnvironment.Weather weather = GlobalList.weathers.Values.ElementAt(i);
            data.Add(new Dropdown.OptionData(weather.name, weather.loadSprite()));
        }

        eventWeatherType.options = data;
    }

    public void OnEnable()
    {
        refreshList();
        onEventTimeSliderChanged(); //Update text, so it doesn't look default

        //Load data
        weatherIsStatic.isOn = LevelData.environment.timeStatic;
        startingHours.text = "" + (LevelData.environment.time / 60);
        startingMinutes.text = "" + (LevelData.environment.time % 60);
        defaultWeather.value = GlobalList.weathers.Keys.ToList().IndexOf(LevelData.environment.weatherDefault.id);
    }

    public void OnDisable()
    {
        clearList();
    }
    #endregion
    #region [Functions] Button-handled functions
    public void ok() //Button controlled
    {
        //Save to data
        LevelData.environment.timeStatic = weatherIsStatic.isOn;
        LevelData.environment.weatherDefault = GlobalList.weathers.Values.ElementAt(defaultWeather.value);
        LevelData.environment.setTime(int.Parse(startingHours.text) * 60 + int.Parse(startingMinutes.text));

        LevelData.environment.OnTimeUpdate();

        //Hide panel
        gameObject.SetActive(false);
    }

    public void addEvent() //Button controlled
    {
        //Gather data
        WorldEnvironment.Weather weather = GlobalList.weathers.Values.ElementAt(eventWeatherType.value);
        int startTime = (int) eventTimeStart.value * 10;
        int endTime = (int) eventTimeEnd.value * 10;

        //Add
        WorldEnvironment.Event envEvent = new WorldEnvironment.Event(weather, startTime, endTime);
        LevelData.environment.events.Add(envEvent);
        addToEventList(envEvent);
    }
    #endregion
    #region [Functions] Slider-handled functions
    public void onEventTimeSliderChanged() //Slider controlled
    {
        //Update event time text
        eventTimeText.text = "Time -> " + WorldEnvironment.toTextTimeOutput((int)eventTimeStart.value * 10) + " - " + WorldEnvironment.toTextTimeOutput((int)eventTimeEnd.value * 10);
    }
    #endregion
    #region [Functions] Event list UI
    private void clearList()
    {
        for (int i = 0; i <  eventListContent.childCount; i++)
        {
            Destroy(eventListContent.GetChild(i).gameObject);
        }
        eventItemList.Clear();
    }

    private void refreshList()
    {
        for (int i = 0; i < LevelData.environment.events.Count; i++)
        {
            addToEventList(LevelData.environment.events[i]);
        }
    }

    private void addToEventList(WorldEnvironment.Event envEvent)
    {
        SimpleListView1_Item item = Instantiate(eventListItem, eventListContent);
        item.init(envEvent.weather.name, envEvent.toTextTimeRangeOutput(), envEvent.weather.loadSprite(), eventItemList, this);
    }

    public ref List<SimpleListView1_Item> onDeleteRequest(int itemId)
    {
        LevelData.environment.events.RemoveAt(itemId);
        return ref eventItemList;
    }
    #endregion
}
