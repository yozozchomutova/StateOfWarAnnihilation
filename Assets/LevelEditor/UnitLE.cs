using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class UnitLE : MonoBehaviour
{

    /// <summary> Class used to store Units property data like health, team id, producing progress, etc...</summary>
    [System.Serializable]
    public class UnitCustomDataSer
    {
        public string[] keys;
        public string[] values;

        public UnitCustomDataSer(Dictionary<string, string> valueDictionary)
        {
            keys = valueDictionary.Keys.ToArray();
            values = valueDictionary.Values.ToArray();
        }
    }
    /// <summary> Class used to store Units all data about position.</summary>
    [System.Serializable]
    public class UnitLESer
    {
        public int unitId;
        public float health;
        public int unitTeamID;

        public MapLevelManager.Vector3Ser position;
        public MapLevelManager.Vector3Ser bodyRotation;
        public MapLevelManager.Vector3Ser unitRotation;

        public UnitCustomDataSer additionalData;

        public UnitLESer(float health, int unitTeamID, MapLevelManager.Vector3Ser position, MapLevelManager.Vector3Ser bodyRotation, MapLevelManager.Vector3Ser unitRotation)
        {
            this.health = health;
            this.unitTeamID = unitTeamID;

            this.position = position;
            this.bodyRotation = bodyRotation;
            this.unitRotation = unitRotation;
        }

        public void setAdditionalData(UnitCustomDataSer data)
        {
            this.additionalData = data;
        }
    }
}
