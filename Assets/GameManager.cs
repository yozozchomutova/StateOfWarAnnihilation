using UnityEngine;

/// <summary>
/// Management
/// </summary>
public class GameManager
{
    public static string agreementFileName = "003.txt";
    public static string versionTag = "Alpha";

    public static (short, short, short) GetGameVersion()
    {
        string[] parts = Application.version.Split('.');
        return (short.Parse(parts[0]), short.Parse(parts[1]), short.Parse(parts[2]));
    }

    public static bool compareVersion(int bigPatch1, int smallPatch1, int buildCode1, int bigPatch2, int smallPatch2, int buildCode2)
    {
        if (bigPatch1 != bigPatch2)
        {
            return false;
        }

        if (smallPatch1 != smallPatch2)
        {
            return false;
        }

        if (buildCode1 != buildCode2)
        {
            return false;
        }

        return true;
    }
}
