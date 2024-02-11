using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SOWUtils
{
    public class GO
    {
        //-STATIC FUNCTIONS-
        #region Get UI from path(static functions)
        public static RawImage getRawImage(GameObject additionalProp, string uiName)
        {
            return GameObject.Find(GetGameObjectPath(additionalProp) + "/" + uiName).GetComponent<RawImage>();
        }

        public static Image getImage(GameObject additionalProp, string uiName)
        {
            return GameObject.Find(GetGameObjectPath(additionalProp) + "/" + uiName).GetComponent<Image>();
        }

        public static Toggle getToggle(GameObject additionalProp, string uiName)
        {
            return GameObject.Find(GetGameObjectPath(additionalProp) + "/" + uiName).GetComponent<Toggle>();
        }

        public static Text getText(GameObject additionalProp, string uiName)
        {
            return GameObject.Find(GetGameObjectPath(additionalProp) + "/" + uiName).GetComponent<Text>();
        }

        public static Slider getSlider(GameObject additionalProp, string uiName)
        {
            return GameObject.Find(GetGameObjectPath(additionalProp) + "/" + uiName).GetComponent<Slider>();
        }

        public static GameObject getGameObject(GameObject additionalProp, string uiName)
        {
            return GameObject.Find(GetGameObjectPath(additionalProp) + "/" + uiName);
        }

        public static string GetGameObjectPath(GameObject obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }
        #endregion
    }

    public class DataTypeUtils
    {
        public static string tryGet(Dictionary<string, string> data, string key, string defaultValue)
        {
            if (!data.ContainsKey(key))
                return defaultValue;
            return data[key];
        }
        public static void printDictionary(Dictionary<string, string> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                Debug.Log("K: " + data.Keys.ElementAt(i) + " V: " + data.Values.ElementAt(i));
            }
        }

        public static string arrayToString(string[] array)
        {
            return string.Join(";", array);
        }

        public static string[] stringToArray(string line)
        {
            return line.Split(new char[] { ';' });
        }
    }
}
