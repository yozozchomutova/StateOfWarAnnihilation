
[System.Serializable]
public class TeamPlaySettings
{
    public int teamId;

    public string teamDriverCategoryId;
    public string teamDriverId;

    public TeamPlaySettings(int teamId, TeamDriver teamDriver)
    {
        this.teamId = teamId;
        this.teamDriverCategoryId = teamDriver.driverCategoryId;
        this.teamDriverId = teamDriver.id;
    }

    public TeamPlaySettings(int teamId, string teamDriverCategoryId, string teamDriverId)
    {
        this.teamId = teamId;
        this.teamDriverCategoryId = teamDriverCategoryId;
        this.teamDriverId = teamDriverId;
    }
}
