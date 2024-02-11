using UnityEngine;
using UnityEngine.UI;

public class LETeamConfiguration : MonoBehaviour
{
    public Text teamName;

    public InputField money;
    public InputField research;

    public InputField fighters;
    public InputField bombers;
    public InputField triplers;
    public InputField carryalls;
    public InputField meteors;

    public Toggle cannon;
    public Toggle antiair;
    public Toggle plasma;
    public Toggle rotary;
    public Toggle defragmentator;

    [HideInInspector] public int ts_id; //TeamStats ID

    public void loadFrom()
    {
        TeamStats02_12 ts = LevelData.teamStats[ts_id];

        money.text = ts.money.ToString();   
        research.text = ts.research.ToString();

        fighters.text = ts.jets.ToString();
        bombers.text = ts.destroyers.ToString();
        triplers.text = ts.carrybuses.ToString();
        carryalls.text = ts.carrybuses.ToString();
        meteors.text = ts.debrises.ToString();

        cannon.isOn = ts.ant;
        antiair.isOn = ts.antiAircraft;
        plasma.isOn = ts.plasmatic;
        rotary.isOn = ts.machineGun;
        defragmentator.isOn = ts.granader;   
    }

    public void saveTo()
    {
        TeamStats02_12 ts = LevelData.teamStats[ts_id];

        ts.money = int.Parse(money.text);
        ts.research = int.Parse(research.text);

        ts.jets = int.Parse(fighters.text);
        ts.destroyers = int.Parse(bombers.text);
        ts.cyclones = int.Parse(triplers.text);
        ts.carrybuses = int.Parse(carryalls.text);
        ts.debrises = int.Parse(meteors.text);

        ts.ant = cannon.isOn;
        ts.antiAircraft = antiair.isOn;
        ts.plasmatic = plasma.isOn;
        ts.machineGun = rotary.isOn;
        ts.granader = defragmentator.isOn;
    }

    public void bindTeamStats(TeamStats02_12 teamStats)
    {
        Team t = GlobalList.teams[teamStats.teamId];
        teamName.text = GlobalList.teams[teamStats.teamId].name;

        //Change UI colors
        RawImage[] rawImages = transform.GetComponentsInChildren<RawImage>(true);

        for (int i = 0; i < rawImages.Length; i++)
        {
            rawImages[i].color = new Color(t.minimapColor.r, t.minimapColor.g, t.minimapColor.b, rawImages[i].color.a);
        }

        ts_id = teamStats.teamId;
    }
}
