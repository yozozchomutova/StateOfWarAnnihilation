using UnityEngine;

public class SMF_Effect : MonoBehaviour
{
    public SMF.State usage;
    public ParticleSystem ps;
    public Animation animation;
    public AnimationClip[] animationClips;

    public void begin()
    {
        if (animation != null)
        {
            animation.clip = animationClips[0];
            animation.Play();
        }
    }

    public void stop()
    {
        if (animation != null)
        {
            animation.Stop();
            animation.clip = animationClips[1];
            animation.Play();
        }

        var emmision = ps.emission;
        emmision.enabled = false;
        Destroy(gameObject, 2);
    }
}
