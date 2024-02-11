using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ImageSequence : MonoBehaviour
{
    public Sprite[] frames;
    public float framesPerSecond = 10;
    public bool playOnStart;
    public bool loop;
    private Image imageComponent;

    void Start()
    {
        if (playOnStart)
            play();
    }
        

    public void play()
    {
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        int frameIndex = 0;
        imageComponent = gameObject.GetComponent<Image>();
        imageComponent.enabled = true;
        do
        {
            frameIndex %= frames.Length;
            imageComponent.sprite = frames[frameIndex];
            frameIndex++;

            yield return new WaitForSeconds(1 / framesPerSecond);
        } while (loop || frameIndex != frames.Length);
    }
}
