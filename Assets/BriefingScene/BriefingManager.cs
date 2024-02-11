using UnityEngine;
using UnityEngine.SceneManagement;

public class BriefingManager : MonoBehaviour
{
    //Planet map
    public Transform planet;
    public MissionCard missionCardPrefab;
    public Texture2D planetMissionAreas;

    public Material cardWood;
    private bool risingColor = true;
    private float cardWoodColorChange;

    void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            int randX = Random.Range(0, planetMissionAreas.width);
            int randY = Random.Range(0, planetMissionAreas.height);
        
            if (planetMissionAreas.GetPixel(randX, randY) == Color.white)
            {
                GameObject missionCardGameObject = Instantiate(missionCardPrefab.gameObject, planet);
                MissionCard missionCard = missionCardGameObject.GetComponent<MissionCard>();

                missionCardGameObject.transform.localPosition = new Vector3(0, 0.01f, 0);
                missionCardGameObject.transform.RotateAround(new Vector3(0, 0, 0), Vector3.right, (randY / (float)planetMissionAreas.height - 0.5f) * 180);
                missionCardGameObject.transform.RotateAround(new Vector3(0, 0, 0), Vector3.down, (randX / (float)planetMissionAreas.width - 0.5f) * 360);
            } else
            {
                i--;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }

        //Breathing cardWood
        if (cardWoodColorChange > 1f)
            risingColor = false;
        else if (cardWoodColorChange < 0f)
            risingColor = true;

        cardWoodColorChange += (risingColor ? Time.deltaTime : -Time.deltaTime) * 1.8f;
        cardWood.color = new Color((68 + 187 * cardWoodColorChange) / 255f, (33 + 222 * cardWoodColorChange) / 255f, (0 + 255 * cardWoodColorChange) / 255f);
    }
}
