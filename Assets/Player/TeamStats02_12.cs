using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TeamStats02_12
{
    public int teamId;
    public bool activeTeam; //If team was somehow partipiciated in game.
    [System.NonSerialized] public int commandCenterPenalty; //Counts penalty for not having any command center (HeadquarterLE resets this / LevelManager counts), if this value is high => there's high propability that team doesn't have any command center.

    public int money;
    public int research;
    [System.NonSerialized] public int researchAvailableCount; //Counts how much stuff can be researched
    [System.NonSerialized] public float lastEnergy;
    [System.NonSerialized] public float newEnergy;

    public int jets;
    public int destroyers;
    public int cyclones;
    public int carrybuses;
    public int debrises;

    public bool ant;
    public bool antiAircraft;
    public bool plasmatic;
    public bool machineGun;
    public bool granader;

    public TTNode.Ser[] nodes;
    [System.NonSerialized] public List<TTNode.Ser> nodesList = new List<TTNode.Ser>();

    //Final game stats (If you RMB on value -> Find All References -> , correctly all values should have exactly 3 references (Display, Clear, Add) )
    public int moneyEarned;
    public int moneySpent;
    public int researchEarned;
    public int researchSpent;
    public int buildingsCaptured;
    public int buildingsLost;
    public int unitsProduced;
    public int unitsLost;
    public int airForcesSent;
    public int airForcesProduced;
    public int airForcesLost;
    public int towersBuilt;
    public int towersLost;

    public void OnSerialize()
    {
        nodes = new TTNode.Ser[nodesList.Count];
        nodesList.CopyTo(nodes);
    }

    public void OnDeserialize()
    {
        nodesList = new List<TTNode.Ser>();
        if (nodes != null)
        {
            nodesList.AddRange(nodes);

            foreach (TTNode.Ser nodee in nodesList)
            {
                //Debug.Log(teamId + "] T: " + nodee.nodeId + " X: " + nodee.x + " Y: " + nodee.y);
            }
        }
    }

    public void clear()
    {
        money = 0;
        research = 0;

        jets = 0;
        destroyers = 0;
        cyclones = 0;
        carrybuses = 0;
        debrises = 0;

        ant = false;
        antiAircraft = false;
        plasmatic = false;
        machineGun = false;
        granader = false;
    }

    public void clearStats()
    {
        moneyEarned = 0;
        moneySpent = 0;
        researchEarned = 0;
        researchSpent = 0;
        buildingsCaptured = 0;
        buildingsLost = 0;
        unitsProduced = 0;
        unitsLost = 0;
        airForcesSent = 0;
        airForcesProduced = 0;
        airForcesLost = 0;
        towersBuilt = 0;
        towersLost = 0;
    }

    public void AddMoney(int amount)
    {
        money += amount;
        moneyEarned += amount;
    }

    public void RemoveMoney(int amount)
    {
        money -= amount;
        moneySpent += amount;
    }

    public void AddResearch(int amount)
    {
        research += amount;
        researchEarned += amount;
    }

    public void RemoveResearch(int amount)
    {
        research -= amount;
        researchSpent += amount;
    }

    public void AddAirForce(string productId)
    {
        AddAirForce(productId, 1);
    }

    public void AddAirForce(string productId, int count)
    {
        if (productId == "0_jet1")
        {
            jets += count;
        }
        else if (productId == "0_destroyer1")
        {
            destroyers += count;
        }
        else if (productId == "0_cyclone1")
        {
            cyclones += count;
        }
        else if (productId == "0_carrybus1")
        {
           carrybuses += count;
        }
        else if (productId == "0_debris1")
        {
            debrises += count;
        }

        airForcesProduced += count;
    }
}
