using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFX_Mesh_FadeOut : MonoBehaviour
{
    [Header("Properties (Time is in seconds)")]
    public float timeToDestroy;
    public float timeToStartFading;
    public float fadeOutSpeedMultiplier;

    private float alpha = 1f;
    private MeshRenderer meshRenderer;
    private Material material;

    void Start()
    {
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        material = meshRenderer.materials[0];

        StartCoroutine(FadeOut());
    }

    void Update()
    {
        Color c = material.color;
        c = new Color(c.r, c.g, c.b, alpha);
        material.color = c;
    }

    IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(timeToStartFading);

        while (true)
        {
            alpha -= fadeOutSpeedMultiplier * Time.deltaTime;

            if (alpha <= 0) {
                Destroy(gameObject);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}
