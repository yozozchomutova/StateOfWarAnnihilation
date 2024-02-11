using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Campaign
{
    public string campaignName;
    public Type type;

    public Mission[] missions;

    public Campaign(string campaignName, Type type, string missionPackPath)
    {
        this.campaignName = campaignName;
        this.type = type;
    }

    public void allocateMissions(int size)
    {
        missions = new Mission[size];
    }

    public void insertMission(int id, Mission mission)
    {
        missions[id] = mission;
    }

    public enum Type
    {
        BEGINNER,
        WORLD_RULER,
        CONQUEROR
    }

    public class Mission
    {
        public int x, y;
        public string missionName;

        public enum MissionType
        {
            DESTROY,
            CONVOY
        }
    }
}
