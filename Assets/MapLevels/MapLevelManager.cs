#region [Libraries] All libraries
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#endregion

public class MapLevelManager
{
    #region [Structs] Serializable transforms (Vectors, Rotations, etc.)
    [System.Serializable] 
    public class Vector3Ser
    {
        public float x, y, z;

        public Vector3Ser(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3Ser(Vector3 pos)
        {
            this.x = pos.x;
            this.y = pos.y;
            this.z = pos.z;
        }

        public Vector3Ser(string rawPos)
        {
            parseFromString(rawPos);
        }

        public Vector3 toVector()
        {
            return new Vector3(x, y, z);
        }

        public string parseToString()
        {
            return x + "," + y + "," + z;
        }

        public void parseFromString(string value)
        {
            string[] values = value.Split(',');
            this.x = float.Parse(values[0]);
            this.y = float.Parse(values[1]);
            this.z = float.Parse(values[2]);
        }

        public static Vector3Ser fromV(Vector3 v)
        {
            return new Vector3Ser(v.x, v.y, v.z);
        }

        public static Vector3Ser empty()
        {
            return new Vector3Ser(0, 0, 0);
        }
    }

    [System.Serializable] 
    public class Transfornm3Ser
    {
        public float posX, posY, posZ;
        public float rotX, rotY, rotZ;

        public Transfornm3Ser(float posX, float posY, float posZ, float rotX, float rotY, float rotZ)
        {
            this.posX = posX;
            this.posY = posY;
            this.posZ = posZ;

            this.rotX = rotX;
            this.rotY = rotY;
            this.rotZ = rotZ;
        }

        public Transfornm3Ser(Vector3 pos, Vector3 rot)
        {
            this.posX = pos.x;
            this.posY = pos.y;
            this.posZ = pos.z;

            this.rotX = rot.x;
            this.rotY = rot.y;
            this.rotZ = rot.z;
        }

        public Vector3 toVectorPosition()
        {
            return new Vector3(posX, posY, posZ);
        }

        public Vector3 toVectorRotation()
        {
            return new Vector3(rotX, rotY, rotZ);
        }
    }

    #endregion
    #region [Structs] Unit serializable 03-05-003-__AV
    [System.Serializable]
    public class UnitSerializable
    {
        [System.NonSerialized] public static readonly string KEY_UNIT_ROTATION = "unitRotation";
        [System.NonSerialized] public static readonly string KEY_BODY_POSITION = "bodyPosition";
        [System.NonSerialized] public static readonly string KEY_BODY_ROTATION = "bodyRotation";

        [System.NonSerialized] public static readonly string KEY_UNIT_ID = "id";
        [System.NonSerialized] public static readonly string KEY_UNIT_HP = "hp";
        [System.NonSerialized] public static readonly string KEY_UNIT_TEAMID = "team";

        public string[] keys;
        public string[] values;

        public UnitSerializable(string id, float hp, int teamId, Vector3 unitRotation, Vector3 bodyPosition, Vector3 bodyRotation)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add(KEY_UNIT_ID, id);
            data.Add(KEY_UNIT_HP, "" + hp);
            data.Add(KEY_UNIT_TEAMID, "" + teamId);
            data.Add(KEY_UNIT_ROTATION, new Vector3Ser(unitRotation).parseToString());
            data.Add(KEY_BODY_POSITION, new Vector3Ser(bodyPosition).parseToString());
            data.Add(KEY_BODY_ROTATION, new Vector3Ser(bodyRotation).parseToString());
            setAdditionalData(data);
        }

        public UnitSerializable(Dictionary<string, string> data)
        {
            setAdditionalData(data);
        }

        public void setAdditionalData(Dictionary<string, string> data)
        {
            keys = data.Keys.ToArray();
            values = data.Values.ToArray();
        }

        /// <returns>String value from Key</returns>
        public string getS(string key)
        {
            return values[findValueIndex(key)];
        }
        /// <returns>Float value from Key</returns>
        public float getF(string key)
        {
            return float.Parse(values[findValueIndex(key)]);
        }
        /// <returns>Integer value from Key</returns>
        public int getI(string key)
        {
            return int.Parse(values[findValueIndex(key)]);
        }
        /// <returns>Vector Serializable value from Key</returns>
        public Vector3Ser getVS(string key)
        {
            return new Vector3Ser(values[findValueIndex(key)]);
        }
        /// <returns>Vector value from Key</returns>
        public Vector3 getV(string key)
        {
            return getVS(key).toVector();
        }

        /// <summary> Sets String value to key</summary>
        public void setS(string key, string value)
        {
            values[findValueIndex(key)] = value;
        }
        /// <summary> Sets Float value to key</summary>
        public void setF(string key, float value)
        {
            values[findValueIndex(key)] = "" + value;
        }
        /// <summary> Sets Integer value to key</summary>
        public void setI(string key, int value)
        {
            values[findValueIndex(key)] = "" + value;
        }
        /// <returns>Vector value from Key</returns>
        public void setV(string key, Vector3 vector)
        {
            values[findValueIndex(key)] = "" + new Vector3Ser(vector).parseToString();
        }

        /// <returns>Found index of value/returns>
        private int findValueIndex(string key)
        {
            int index = -1;
            for (int i = 0; i < values.Length; i++)
            {
                if (keys[i] == key)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
    }
    #endregion
}
