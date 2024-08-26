using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static string agreementFileName = "003.txt";

    public static int cur_bigPatch, cur_smallPatch, cur_buildCode;

    void Start()
    {
        cur_bigPatch = int.Parse(getBigPatchVer());
        cur_smallPatch = int.Parse(getSmallPatchVer());
        cur_buildCode = getTagVersionId();
    }

    void Update()
    {
        
    }


    //VERSIONING SYSTEM
    public static readonly string __AV = "__AV";
    public static readonly string __BV = "__BV";
    public static readonly string __EA = "__EA";
    public static readonly string __RB = "__RB";
    public static readonly string UNKN = "unk_tag_ver";

    public static string getBigPatchVer()
    {
        return Application.version.Substring(0, 2);
    }

    public static string getSmallPatchVer()
    {
        return Application.version.Substring(3, 2);
    }

    public static string getAgreementVer()
    {
        return Application.version.Substring(6, 3);
    }

    public static string getTagVersion()
    {
        return Application.version.Substring(10, 4);
    }

    public static string getTagVersionByID(int id)
    {
        if (id == 0)
        {
            return __AV;
        } else if (id == 1)
        {
            return __BV;
        } else if (id == 2)
        {
            return __EA;
        } else if (id == 3)
        {
            return __RB;
        } else
        {
            return UNKN;
        }
    }

    public static int getTagVersionId()
    {
        string tag = getTagVersion();

        if (tag == __AV)
        {
            return 1;
        }
        else if (tag == __BV)
        {
            return 2;
        }
        else if (tag == __EA)
        {
            return 3;
        }
        else if (tag == __RB)
        {
            return 4;
        }
        else
        {
            return 0;
        }
    }

    public static string getTagVersionFull()
    {
        string tagVer = getTagVersion();

        if (tagVer == __EA)
            return "Early Access";
        else if (tagVer == __AV)
            return "Alpha Version";
        else if (tagVer == __BV)
            return "Beta Version";
        else if (tagVer == __RB)
            return "Release Build";
        else 
            return UNKN;
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
