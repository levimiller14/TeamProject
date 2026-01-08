using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static enemyAI_Guard;

public class enemyAI_Turrets : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [SerializeField] Renderer modelHead;
    [SerializeField] Renderer modelHead2;
    [SerializeField] Renderer modelHead3;
    [SerializeField] Transform head;

    [SerializeField] int HP;
    [SerializeField] int facePlayerSpeed;
    [SerializeField] float FOV;

    [SerializeField] GameObject bullet;
    [SerializeField] GameObject bullet2;
    [SerializeField] GameObject bullet3;
    [SerializeField] float fireRate;
    [SerializeField] Transform firePos;
    [SerializeField] Transform firePos2;
    [SerializeField] Transform firePos3;

    public enum turretWeaponNumber
    {
        One,
        Two,
        Three
    }
    public enum turretWeaponTyoe
    {
        Standard,
        Homing,
        Shock,
        Poison
    }

    public enum turretState
    {
        Idle,
        Aggro
    }

    public turretWeaponNumber weaponNumber = turretWeaponNumber.One;
    public turretState state = turretState.Idle;

    bool isOne => weaponNumber == turretWeaponNumber.One;
    bool isTwo => weaponNumber == turretWeaponNumber.Two;
    bool isThree => weaponNumber == turretWeaponNumber.Three;

    Color colorOrig;
    Color colorOrigHead;
    Color colorOrigHead2;
    Color colorOrigHead3;

    MaterialPropertyBlock propBlock;
    MaterialPropertyBlock propBlockHead;
    MaterialPropertyBlock propBlockHead2;
    MaterialPropertyBlock propBlockHead3;
    static readonly int colorId = Shader.PropertyToID("_BaseColor");

    float fireTimer;
    float angleToPlayer;
    bool isAggro;

    // status effects
    private Coroutine poisoned;
    private bool tazed;

    Vector3 playerDir;

    bool playerInRange;
    Transform playerTransform;

    void Start()
    {
        propBlock = new MaterialPropertyBlock();
        propBlockHead = new MaterialPropertyBlock();
        propBlockHead2 = new MaterialPropertyBlock();
        propBlockHead3 = new MaterialPropertyBlock();

        model.GetPropertyBlock(propBlock);
        colorOrig = propBlock.GetColor(colorId);
        if (colorOrig == Color.clear)
            colorOrig = model.sharedMaterial.color;

        modelHead.GetPropertyBlock(propBlockHead);
        colorOrigHead = propBlockHead.GetColor(colorId);
        if (colorOrigHead == Color.clear)
            colorOrigHead = modelHead.sharedMaterial.color;

        if (modelHead2 != null)
        {
            modelHead2.GetPropertyBlock(propBlockHead2);
            colorOrigHead2 = propBlockHead2.GetColor(colorId);
            if (colorOrigHead2 == Color.clear)
                colorOrigHead2 = modelHead2.sharedMaterial.color;
        }

        if (modelHead3 != null)
        {
            modelHead3.GetPropertyBlock(propBlockHead3);
            colorOrigHead3 = propBlockHead3.GetColor(colorId);
            if (colorOrigHead3 == Color.clear)
                colorOrigHead3 = modelHead3.sharedMaterial.color;
        }

        if (difficultyManager.instance != null)
        {
            HP = Mathf.RoundToInt(HP * difficultyManager.instance.GetHealthMultiplier());
        }

        if (gameManager.instance.player != null)
            playerTransform = gameManager.instance.player.transform;
    }
    void Update()
    {
        fireTimer += Time.deltaTime;

        switch(state)
        {
            case turretState.Idle:
                IdleBehavior();
                break;

            case turretState.Aggro:
                AggroBehavior();
                break;
        }
    }

    void IdleBehavior()
    {
        if (playerInRange && canSeePlayer())
        {
            facePlayer();
        }
    }

    void AggroBehavior()
    {
        if (isAggro == true)
        {
            facePlayer();
            canSeePlayer();
        }
        if (playerInRange && canSeePlayer())
        {
            facePlayer();
        }
    }
    bool canSeePlayer()
    {
        if (playerTransform == null) return false;

        playerDir = playerTransform.position - transform.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, playerDir, out hit))
        {
            if (angleToPlayer <= FOV && hit.collider.CompareTag("Player"))
            {

                if (fireTimer >= fireRate)
                {
                    fire();
                }

                return true;
            }
        }
        return false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
    void facePlayer()
    {
        if(head == null || playerTransform == null)
        {
            return;
        }
        Vector3 targetDir = playerTransform.position;
        targetDir.y = 0;

        playerDir = targetDir - head.position;

        if(playerDir.sqrMagnitude < 0.001f)
        {
            return;
        }
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, head.transform.position.y, playerDir.z));

        head.rotation = Quaternion.Slerp(head.rotation, rot, facePlayerSpeed * Time.deltaTime);
    }

    void fire()
    {
        if (!tazed)
        {
            fireTimer = 0;
            //if (difficultyManager.instance != null)
            //{
                if (isOne)// && difficultyManager.instance.currentDifficulty == difficultyManager.Difficulty.Easy)
                {
                    // Levi addition damage multiplier
                    GameObject bulletObj = Instantiate(bullet, firePos.position, head.transform.rotation);

                    // apply dmg mult
                    damage bulletDmg = bulletObj.GetComponent<damage>();

                    if (bulletDmg != null && difficultyManager.instance != null)
                    {
                        bulletDmg.ApplyDifficultyMultiplier(difficultyManager.instance.GetDamageMultiplier());
                    }
                }
                if (isTwo) //&& difficultyManager.instance.currentDifficulty == difficultyManager.Difficulty.Normal)
                {
                    // Levi addition damage multiplier
                    GameObject bulletObj = Instantiate(bullet, firePos.position, head.transform.rotation);

                    // apply dmg mult
                    damage bulletDmg = bulletObj.GetComponent<damage>();

                    if (bulletDmg != null && difficultyManager.instance != null)
                    {
                        bulletDmg.ApplyDifficultyMultiplier(difficultyManager.instance.GetDamageMultiplier());
                    }
                    // Levi addition damage multiplier
                    GameObject bulletObj2 = Instantiate(bullet2, firePos2.position, head.transform.rotation);

                    // apply dmg mult
                    damage bulletDmg2 = bulletObj2.GetComponent<damage>();

                    if (bulletDmg2 != null && difficultyManager.instance != null)
                    {
                        bulletDmg2.ApplyDifficultyMultiplier(difficultyManager.instance.GetDamageMultiplier());
                    }
                }
                else if(isThree)// && difficultyManager.instance.currentDifficulty == difficultyManager.Difficulty.Hard)
                {
                    // Levi addition damage multiplier
                    GameObject bulletObj = Instantiate(bullet, firePos.position, head.transform.rotation);

                    // apply dmg mult
                    damage bulletDmg = bulletObj.GetComponent<damage>();

                    if (bulletDmg != null && difficultyManager.instance != null)
                    {
                        bulletDmg.ApplyDifficultyMultiplier(difficultyManager.instance.GetDamageMultiplier());
                    }
                    // Levi addition damage multiplier
                    GameObject bulletObj2 = Instantiate(bullet2, firePos2.position, head.transform.rotation);

                    // apply dmg mult
                    damage bulletDmg2 = bulletObj2.GetComponent<damage>();

                    if (bulletDmg2 != null && difficultyManager.instance != null)
                    {
                        bulletDmg2.ApplyDifficultyMultiplier(difficultyManager.instance.GetDamageMultiplier());
                    }
                    // Levi addition damage multiplier
                    GameObject bulletObj3 = Instantiate(bullet3, firePos3.position, head.transform.rotation);

                    // apply dmg mult
                    damage bulletDmg3 = bulletObj3.GetComponent<damage>();

                    if (bulletDmg3 != null && difficultyManager.instance != null)
                    {
                        bulletDmg3.ApplyDifficultyMultiplier(difficultyManager.instance.GetDamageMultiplier());
                    }
                }
            //}
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        state = turretState.Aggro;
        isAggro = true;

        if (HP <= 0)
        {
            //gameManager.instance.UpdateGameGoal(-1);

            // incrementing enemies defeated in stats
            if (statTracker.instance != null)
            {
                statTracker.instance.IncrementEnemiesDefeated();
            }

            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(flashRed());
        }
    }

    IEnumerator flashRed()
    {
        propBlock.SetColor(colorId, Color.red);
        model.SetPropertyBlock(propBlock);
        propBlockHead.SetColor(colorId, Color.red);
        modelHead.SetPropertyBlock(propBlockHead);

        if (modelHead2 != null)
        {
            propBlockHead2.SetColor(colorId, Color.red);
            modelHead2.SetPropertyBlock(propBlockHead2);
        }
        if (modelHead3 != null)
        {
            propBlockHead3.SetColor(colorId, Color.red);
            modelHead3.SetPropertyBlock(propBlockHead3);
        }

        yield return new WaitForSeconds(0.1f);
        propBlock.SetColor(colorId, colorOrig);
        model.SetPropertyBlock(propBlock);
        propBlockHead.SetColor(colorId, colorOrigHead);
        modelHead.SetPropertyBlock(propBlockHead);

        if(modelHead2 != null)
        {
            propBlockHead2.SetColor(colorId, colorOrigHead2);
            modelHead2.SetPropertyBlock(propBlockHead2);
        }

        if (modelHead3 != null)
        {
            propBlockHead3.SetColor(colorId, colorOrigHead3);
            modelHead3.SetPropertyBlock(propBlockHead2);
        }
    }
    public void poison(int damage, float rate, float duration)
    {
        if (poisoned != null)
        {
            StopCoroutine(poisoned); // cuts off current poison, effective duration reset
        }
        poisoned = StartCoroutine(PoisonRoutine(damage, rate, duration));
    }

    private IEnumerator PoisonRoutine(int damage, float rate, float duration)
    {
        float timer = 0f;
        WaitForSeconds wait = new WaitForSeconds(rate);

        while (timer < duration)
        {
            takeDamage(damage);
            timer += rate;
            yield return wait;
        }
        poisoned = null;
    }

    // Tazed effect
    public void taze(/*int damage,*/ float duration)
    {
        //takeDamage(damage);
        if (!tazed)
        {
            StartCoroutine(StunRoutine(duration));
        }
    }

    private IEnumerator StunRoutine(float duration)
    {
        // no movement in turrets
        tazed = true;
        //if (agent != null)
        //{
        //    agent.isStopped = true;
        //}
        yield return new WaitForSeconds(duration);
        tazed = false;
        //if (agent != null)
        //{
        //    agent.isStopped = false;
        //}
    }

}
