using UnityEngine;

public class HPBar3D : MonoBehaviour
{
    public MeshRenderer hpBar;

    public float maxHP;

    private Transform playerCameraPivot;

    private void Start()
    {
        playerCameraPivot = GameObject.Find("PlayerCameraPivot").transform;
    }

    void Update()
    {
        //Face towards camera
        Vector3 camRot = playerCameraPivot.localRotation.eulerAngles;
        transform.rotation = Quaternion.Euler(new Vector3(0, camRot.y + 90, camRot.x - 90));
    }

    public void setHP(float hp)
    {
        //Update HP
        hpBar.transform.localScale = new Vector3(70, hp / maxHP * 100, 1);
        hpBar.transform.localPosition = new Vector3(0.6f, 0.005f, -1f + hp / maxHP);

        //Change color
        if (hp / maxHP >= 0.5f)
        {
            hpBar.material = Resources.Load<Material>("Materials/green");
        } else if (hp / maxHP >= 0.25f)
        {
            hpBar.material = Resources.Load<Material>("Materials/yellow");
        } else if (hp / maxHP >= 0)
        {
            hpBar.material = Resources.Load<Material>("Materials/red");
        }
    }
}
