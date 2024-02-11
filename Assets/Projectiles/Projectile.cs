using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Global properties")]
    public float jerk;
    public float acceleration;
    public float maxspeed;

    public float lifetime;

    public float randomSpinRate;
    private Vector3 randomSpinVector;

    public bool immediatlyExplodes;
    public bool collidesWithUnits;
    public bool collidesWithSourceUnit;
    public bool collidesWithGround;
    public bool ignoresFriendlyUnits;

    public GameObject explosionEffect;
    public float explosionEffectLength;

    public int spreadSteps = 1;
    public float spreadStepLengthTime;

    [Header("SFX")]
    public AudioSource sfxOnHit;
    private AudioSource sfxOnLaunch;

    [Header("VFX")]
    public GameObject vfxGroup;

    //Private
    private TargetMode targetMode;

    private Unit sourceUnit;
    private Team sourceUnitTeam;

    private Unit targetUnit;
    private Vector3 targetPos;
    private float blastRadius;
    private float coreRadius;
    private float coreDamage;
    private bool friendlyFire;

    //temp
    private float phDistance;
    private float peakHeight_func_y;
    private float peakHeight_func_y2;

    private bool peakHeightEnabled;
    private float peakHeight;
    private Vector3 peakHeightTarget;
    private Vector3 peakHeightSource;
    private float peakHeightDistanceStart;
    private float peakHeight_lastFunc_y;

    private bool launched = false;
    private Vector3 velocity = Vector3.zero;
    private float lastRecordedDistance;

    private bool isErasing = false;

    void Start()
    {
        randomSpinVector = new Vector3(Random.Range(-randomSpinRate, randomSpinRate), Random.Range(-randomSpinRate, randomSpinRate), Random.Range(-randomSpinRate, randomSpinRate));
    }

    private void Update()
    {
        if (launched && !isErasing)
        {
            if (immediatlyExplodes)
            {
                OnExplode();
                erase();
            }

            //Lifetime 
            lifetime -= Time.deltaTime;
            if (lifetime <= 0)
                erase();

            if (targetMode == TargetMode.FREEFALL) //Just fall down the projectile
            {
                Vector3 direction = new Vector3(transform.position.x, 0, transform.position.z) - transform.position;
                if (Mathf.Abs(direction.x) > 1f || Mathf.Abs(direction.y) > 1f || Mathf.Abs(direction.z) > 1)
                    direction = direction.normalized;

                acceleration += jerk * Time.deltaTime;
                velocity += direction * acceleration * Time.deltaTime;

                if (velocity.magnitude > maxspeed)
                {
                    velocity = velocity.normalized * maxspeed;
                }

                peakHeight_func_y = 0f;
                if (peakHeightEnabled)
                {
                    phDistance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(peakHeightTarget.x, peakHeightTarget.z));
                    float func_x = phDistance / peakHeightDistanceStart * 2 - 1;

                    peakHeight_func_y = -func_x * func_x * peakHeight + peakHeight;
                    peakHeight_func_y2 = peakHeight_func_y;
                    peakHeight_func_y -= peakHeight_lastFunc_y;
                    peakHeight_lastFunc_y = peakHeight_func_y2;

                    if (func_x < -0.9f) //It's starting to increase again
                    {
                        velocity += Vector3.up * peakHeight_func_y / Time.deltaTime * 1.3f;
                        peakHeightEnabled = false;
                    }
                }

                transform.position += (velocity * Time.deltaTime) + (Vector3.up * peakHeight_func_y);

                if (randomSpinRate != 0f)
                {
                    transform.Rotate(randomSpinVector * Time.deltaTime);
                } else
                {
                    Vector3 rot = direction * 180;
                    transform.rotation = Quaternion.Euler(rot.y, rot.x, rot.z);
                    //transform.LookAt(direction, Vector3.up);
                    //transform.Rotate(new Vector3(90, 0, 0));
                }
            } else if (targetPos != Vector3.zero) //Projectile has specific position to land on
            {
                Vector3 direction = targetPos - transform.position;
                if (Mathf.Abs(direction.x) > 1f || Mathf.Abs(direction.y) > 1f || Mathf.Abs(direction.z) > 1)
                    direction = direction.normalized;

                acceleration += jerk * (Time.deltaTime * 10f);
                velocity += direction * acceleration * (Time.deltaTime * 10f);

                if (velocity.magnitude > maxspeed)
                {
                    velocity = velocity.normalized * maxspeed;
                }

                transform.position += velocity * maxspeed * (Time.deltaTime * 10f);
                transform.LookAt(targetPos, Vector3.up);
                transform.Rotate(new Vector3(90, 0, 0));

                //Check for overshoot
                float recordedDistance = Vector3.Distance(targetPos, transform.position);
                if (recordedDistance > lastRecordedDistance)
                {
                    transform.position = targetPos;
                    OnExplode();
                }
                lastRecordedDistance = recordedDistance;
            } else if (targetUnit != null) //Projectile has specific unit to follow
            {
                //TODO, DO SOMETHING ABOUT IT!!! : Vector3 direction = targetUnit.transform.position + new Vector3(0, targetUnit.targetHeightOffset, 0) - transform.position;
                Vector3 direction = targetUnit.transform.position - transform.position;
                if (Mathf.Abs(direction.x) > 1f || Mathf.Abs(direction.y) > 1f || Mathf.Abs(direction.z) > 1)
                    direction = direction.normalized;

                acceleration += jerk * Time.deltaTime;
                velocity += direction * acceleration * Time.deltaTime;

                if (velocity.magnitude > maxspeed)
                {
                    velocity = velocity.normalized * maxspeed;
                }

                transform.position += velocity * Time.deltaTime;
                transform.LookAt(targetUnit.transform, Vector3.up);
                transform.Rotate(new Vector3(90, 0, 0));
            }
            //else //Projectile doesn't have anything to follow -> it can die
            else //Projectile doesn't have anything to follow -> it becomes freefall
            {
                targetMode = TargetMode.FREEFALL;
                explode_and_erase();
            }
        }
    }

    public void setSFX(AudioSource sfxOnLaunch)
    {
        this.sfxOnLaunch = sfxOnLaunch;
    }

    public void prepare(Unit source, Unit target, float blastRadius, float coreRadius, float coreDamage, bool friendlyFire)
    {
        targetMode = TargetMode.TARGET_UNIT;
        sourceUnit = source;
        sourceUnitTeam = source.team;
        targetUnit = target;

        this.blastRadius = blastRadius;
        this.coreRadius = coreRadius;
        this.coreDamage = coreDamage;
        this.friendlyFire = friendlyFire;
    }

    public void prepare(Unit source, Vector3 target, float blastRadius, float coreRadius, float coreDamage, bool friendlyFire, bool freeFall)
    {
        targetMode = freeFall ? TargetMode.FREEFALL : TargetMode.TARGET_POSITION;
        sourceUnit = source;
        sourceUnitTeam = source.team;
        targetPos = target;
        lastRecordedDistance = Vector3.Distance(target, transform.position);

        this.blastRadius = blastRadius;
        this.coreRadius = coreRadius;
        this.coreDamage = coreDamage;
        this.friendlyFire = friendlyFire;
    }

    public void launch(float delay)
    {
        launch(Vector3.zero, delay);
    }

    public void launch(Vector3 startingVelocity, float delay)
    {
        velocity = startingVelocity;

        if (delay <= 0.01f) //Launch immediatly!
        {
            gameObject.transform.parent = null;
            launched = true;

            if (sfxOnLaunch != null)
                sfxOnLaunch.Play();
        } else
            StartCoroutine(launchProjectile(delay));
    }

    IEnumerator launchProjectile(float delay)
    {
        yield return new WaitForSeconds(delay);

        gameObject.transform.parent = null;
        launched = true;

        if (sfxOnLaunch != null)
            sfxOnLaunch.Play();
    }

    public void enableZOffsetting(float projectilePeakHeight, Vector3 source, Vector3 target)
    {
        this.peakHeightEnabled = true;
        this.peakHeight = projectilePeakHeight;
        this.peakHeightTarget = target;
        this.peakHeightSource = source;
        this.peakHeightDistanceStart = Vector2.Distance(new Vector2(source.x, source.z), new Vector2(target.x, target.z));
    }

    private void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isErasing)
            return;

        if (collidesWithGround && other.gameObject.layer == 13)
        {
            explode_and_erase();
        } else if (collidesWithUnits && other.gameObject.layer == 14)
        {
            Unit hitUnit = Unit.getUnit(other);
            if (hitUnit != null && (collidesWithSourceUnit || sourceUnit != hitUnit) && (!ignoresFriendlyUnits || hitUnit.team != sourceUnit.team))
            {
                OnExplode();
                explode_and_erase();
            }
        }
    }

    private void OnExplode()
    {
        if (isErasing)
            return;

        if (targetUnit != null) {
            targetUnit.damage(sourceUnit.team.id, coreDamage);
            explode_and_erase();
        }
        
        if (spreadStepLengthTime > 0.001f)
        {
            float baseSSLengthTime = spreadStepLengthTime / (float)spreadSteps;
            float radiusPerStep = blastRadius / (float)spreadSteps;
            for (int i = 0; i < spreadSteps; i++)
            {
                StartCoroutine(launchStepSphereDamage(radiusPerStep*i, radiusPerStep*(i+1), transform.position, baseSSLengthTime * i));
            }
        }
    }

    IEnumerator launchStepSphereDamage(float minRadius, float maxRadius, Vector3 originPoint, float delay)
    {
        yield return new WaitForSeconds(float.IsNaN(delay) ? 0 : delay);

        Collider[] hits = Physics.OverlapSphere(originPoint, maxRadius, 1 << 14);
        foreach (Collider hit in hits)
        {
            Unit hitUnit = Unit.getUnit(hit);

            if (hitUnit != null && (!friendlyFire || hitUnit.team.id != sourceUnitTeam.id))
            {
                //Try to get collider from unit... if body doesn't exist, get collider from body
                Collider collider = hitUnit.collider;
                if (collider == null)
                    collider = hitUnit.body.collider;

                float distanceBetweemCore = Vector3.Distance(transform.position, collider.ClosestPointOnBounds(transform.position));
                if (distanceBetweemCore < minRadius)
                    continue;

                if (distanceBetweemCore <= coreRadius)
                {
                    hitUnit.damage(sourceUnit.team.id, coreDamage);
                } else
                {
                    float blastCoreDistance = blastRadius - coreRadius;
                    float calculatedDamageMultiplier = (distanceBetweemCore - coreRadius) / blastCoreDistance;
                    calculatedDamageMultiplier = 1 - Mathf.Clamp01(calculatedDamageMultiplier);

                    hitUnit.damage(sourceUnit.team.id, calculatedDamageMultiplier * coreDamage);
                }
            }
        }
    }

    private void explode_and_erase()
    {
        if (sfxOnHit != null)
        {
            GameObject ball = Instantiate(sfxOnHit.gameObject, transform.position, Quaternion.identity);
            ball.GetComponent<AudioSource>().Play();
            Destroy(ball, explosionEffectLength);
        }

        if (explosionEffect != null)
            Destroy(Instantiate(explosionEffect, transform.position, Quaternion.identity), explosionEffectLength);

        erase();
    }

    private void erase()
    {
        isErasing = true;
        Destroy(gameObject, spreadStepLengthTime);

        if (vfxGroup != null)
            vfxGroup.transform.SetParent(null, false);
        Destroy(vfxGroup, explosionEffectLength);
    }

    public enum TargetMode
    {
        TARGET_UNIT,
        TARGET_POSITION,
        FREEFALL
    }
}
