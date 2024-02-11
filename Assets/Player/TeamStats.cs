[System.Serializable]
public class TeamStats
{
    public int teamId;
    [System.NonSerialized] public bool hasCommandCenter;

    public int money;
    public int research;
    [System.NonSerialized] public float lastEnergy;
    [System.NonSerialized] public float newEnergy;

    public int fighters;
    public int bombers;
    public int triplers;
    public int carryalls;
    public int meteors;

    public bool cannon;
    public bool antiair;
    public bool plasm;
    public bool rotary;
    public bool defragmentator;

    public void clear()
    {
        money = 0;
        research = 0;
        lastEnergy = 0;
        newEnergy = 0;

        fighters = 0;
        bombers = 0;
        triplers = 0;
        carryalls = 0;
        meteors = 0;

        cannon = false;
        antiair = false;
        plasm = false;
        rotary = false;
        defragmentator = false;
    }

    
}
