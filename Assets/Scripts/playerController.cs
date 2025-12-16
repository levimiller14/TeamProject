//using NUnit.Framework; //causes CS0104 conflict with UnityEngine.RangeAttribute
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.DualShock;

public class playerController : MonoBehaviour, IDamage, IHeal, IPickup
{
    [Header("----- Component -----")]
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [Header("----- Stats -----")]
    // Levi edit
    // changing HP to public for access in cheatManager.cs
    [Range(1, 10)] public int HP;
    //[Range(1, 10)] [SerializeField] int HP;
    [Range(1, 5)] [SerializeField] int speed;
    [Range(2, 5)] [SerializeField] int sprintMod;
    [Range(5, 20)] [SerializeField] int jumpSpeed;
    [Range(1, 3)] [SerializeField] int jumpMax;
    [Range(15, 50)] [SerializeField] int gravity;

    [Header("----- Guns -----")]
    [SerializeField] List<gunStats> gunList = new List<gunStats>();
    [SerializeField] GameObject gunModel;
    [SerializeField] int shootDamage;
    [SerializeField] int shootDist;
    [SerializeField] float shootRate;
    [SerializeField] GameObject playerBullet;
    [SerializeField] Transform playerShootPos;

    private wallRun wallRun;
    bool wasWallRunning;

    int jumpCount;
    int speedOrig;
    // making HPOrig public for cheatManager.cs
    public int HPOrig;
    int gunListPos;

    // GODMODE
    public bool isGodMode = false;

    float shootTimer;

    // status effects
    private Coroutine poisoned;
    private bool tazed;

    Vector3 moveDir;
    Vector3 playerVel;
    Vector3 externalVelocity;

    [Header("----- Grapple Settings -----")]
    [SerializeField] float groundedVelocityDecay = 15f;
    [SerializeField] float airVelocityDecay = 0.5f;

    GrapplingHook grappleHook;
    bool grappleJumpedThisFrame;

    Camera mainCam;

    

    // launch pad control member - Aaron k
    //public Vector3 launchVelocity;

    void Start()
    {
        HPOrig = HP;
        speedOrig = speed;
        mainCam = Camera.main;
        grappleHook = GetComponent<GrapplingHook>();
        wallRun = GetComponent<wallRun>();
        //launchVelocity = Vector3.zero;
        if(wallRun != null)
        {
            wallRun.controller = controller;
            wallRun.orientation = transform;
            wallRun.cam = Camera.main.transform;
        }
        updatePlayerUI();
    }

    // Update is called once per frame
    void Update()
    {
        if(!gameManager.instance.isPaused)
        {
            movement();
        }
        sprint();

        // launch pad mechanic - NONFUNCTIONAL - Aaron K
        // ==================================================
        //if (controller.isGrounded && launchVelocity.y < 0)
        //{
        //    launchVelocity.y = -2f;
        //}
        //if (launchVelocity != Vector3.zero || !controller.isGrounded)
        //{
        //    launchVelocity.y += gravity * Time.deltaTime;
        //}
        //launchVelocity.y -= gravity * Time.deltaTime;
        //controller.Move(launchVelocity * Time.deltaTime);
    }

    void movement()
    {
        shootTimer += Time.deltaTime;

#if UNITY_EDITOR
        Debug.DrawRay(mainCam.transform.position, mainCam.transform.forward * shootDist, Color.yellow);
#endif

        if(Input.GetButton("Fire1") && shootTimer >= shootRate)
        {
            shoot();
        }

        // only block movement when actively being pulled by grapple (not during line extend)
        bool isGrappling = grappleHook != null && grappleHook.IsGrappling();

        // debug - remove after testing
        // if (grappleHook != null) Debug.Log($"isGrappling: {isGrappling}, lineExtending: {grappleHook.IsLineExtending()}");

        if(controller.isGrounded)
        {
            jumpCount = 0;
            if (playerVel.y <= 0)
            {
                playerVel.y = 0;
            }

            if (externalVelocity.magnitude > 0.1f)
            {
                externalVelocity = Vector3.MoveTowards(externalVelocity, Vector3.zero, groundedVelocityDecay * Time.deltaTime);
            }
            else
            {
                externalVelocity = Vector3.zero;
            }
        }
        else if (!isGrappling)
        {
            float decay = 1f - (airVelocityDecay * Time.deltaTime);
            externalVelocity *= Mathf.Max(decay, 0.9f);
        }

        if (!isGrappling)
        {
            moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;

            // calculate actual velocity for wall run (moveDir * speed + external + playerVel)
            Vector3 currentVelocity = (moveDir * speed) + externalVelocity + playerVel;

            // wall run start
            if (wallRun != null)
            {
                wallRun.ProcessWallRun(ref moveDir, ref playerVel, controller.isGrounded, currentVelocity);
            }
            bool isWallRunningNow = wallRun != null && wallRun.IsWallRunning;
            if (!isWallRunningNow && wasWallRunning)
            {
                externalVelocity = Vector3.zero;
                playerVel.x = 0f;
                playerVel.z = 0f;
            }
            wasWallRunning = isWallRunningNow;
            // wall run end

            controller.Move(moveDir * speed * Time.deltaTime);

            // Wall run start
            if(wallRun == null || !wallRun.IsWallRunning)
            {
                jump();

                if (externalVelocity.magnitude > 0.1f)
                {
                    controller.Move(externalVelocity * Time.deltaTime);
                }

                controller.Move(playerVel * Time.deltaTime);
                playerVel.y -= gravity * Time.deltaTime;
            }
            else
            {
                if (externalVelocity.magnitude > 0.1f)
                {
                    controller.Move(externalVelocity * Time.deltaTime);
                }

                controller.Move(playerVel * Time.deltaTime);
            } // Wall run end
        }
        else
        {
            playerVel.y -= gravity * 0.3f * Time.deltaTime;
        }

        // Wall run start
        if (wallRun != null)
        {
            wallRun.UpdateCameraTilt();
        } // Wall run end
    }

    void jump()
    {
        // skip if grapple jump already happened this frame
        if (grappleJumpedThisFrame)
            return;

        if(Input.GetButtonDown("Jump") && jumpCount < jumpMax && !tazed)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;
        }
    }

    void LateUpdate()
    {
        grappleJumpedThisFrame = false;
    }

    void sprint()
    {
        if(Input.GetButtonDown("Sprint") && !tazed)
        {
            speed *= sprintMod;
        }
        else if(Input.GetButtonUp("Sprint") && !tazed)
        {
            speed = speedOrig;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("FrostTrap"))
        {
            damage trapSlow = other.GetComponent<damage>();
            speed = Mathf.RoundToInt(speedOrig * trapSlow.slowedSpeed);
        }
        if(other.CompareTag("Launch Pad"))
        {
            launchPad padScript = other.GetComponent<launchPad>();
            if (padScript != null)
            {
                Vector3 launchVel = padScript.GetLaunchVelocity();
                externalVelocity = new Vector3(launchVel.x, 0f, launchVel.z);
                playerVel.y = launchVel.y;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("FrostTrap"))
        {
            speed = speedOrig;
        }
    }

    void shoot()
    {
        shootTimer = 0;
        
        // Levi addition, statTracking
        if(statTracker.instance != null)
        {
            statTracker.instance.IncrementShotsFired();
        }

        Instantiate(playerBullet, playerShootPos.position, mainCam.transform.rotation);

        //RaycastHit hit;
        //if (Physics.Raycast(mainCam.transform.position, mainCam.transform.forward, out hit, shootDist, ~ignoreLayer))
        //{

        //    IDamage dmg = hit.collider.GetComponent<IDamage>();
        //    if (dmg != null)
        //    {
        //        dmg.takeDamage(shootDamage);

        //        // stat tracking
        //        if(statTracker.instance != null)
        //        {
        //            statTracker.instance.IncrementShotsHit();
        //        }
        //    }
        //}
    }

    public void takeDamage(int amount)
    {
        // godmode
        if (isGodMode) return;

        if (amount > 0)
        {
            HP -= amount;
            StartCoroutine(flashRed());
            updatePlayerUI();
        }

        if(HP <= 0)
        {
            // You Died!
            gameManager.instance.youLose();
        }
    }


    public void updatePlayerUI()
    {
        gameManager.instance.playerHPBar.fillAmount = (float)HP / HPOrig;
    }

    IEnumerator flashRed()
    {
        gameManager.instance.playerDamageScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.instance.playerDamageScreen.SetActive(false);
    }
    IEnumerator flashGreen() //flash green for heal
    {
        gameManager.instance.playerHealScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f); //active flash time
        gameManager.instance.playerHealScreen.SetActive(false);
    }

    public void heal(int healAmount)
    {
        if(HP < HPOrig)
        {
            HP = Mathf.Min(HP +healAmount, HPOrig);
            updatePlayerUI();
            StartCoroutine(flashGreen());
        }
    }

    public void SetExternalVelocity(Vector3 velocity)
    {
        externalVelocity = velocity;
    }

    public Vector3 GetVelocity()
    {
        return externalVelocity + playerVel;
    }

    public void GrappleJump(Vector3 grappleVelocity)
    {
        // transfer full horizontal velocity from grapple
        externalVelocity = new Vector3(grappleVelocity.x, 0, grappleVelocity.z);

        // combine upward grapple momentum with jump - keep most of it
        float upwardMomentum = Mathf.Max(grappleVelocity.y, 0);
        playerVel.y = jumpSpeed + upwardMomentum;

        // reset jump count - grapple jump gives fresh jumps (allows double jump after)
        jumpCount = 1;

        // prevent normal jump from also triggering this frame
        grappleJumpedThisFrame = true;
    }

    // Launch Pad functionality NONFUNCTIONAL - per no rigidbody - Aaron K
    //public void Launch(Vector3 launchDirection, float launchForce)
    //{
    //    // Set the velocity directly to launch the player
    //    launchVelocity = launchDirection * launchForce;
    //}

    // poison routines- Aaron K
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

        yield return wait;

        while (timer < duration)
        {
            takeDamage(damage);
            timer += rate;
            yield return wait;
        }
        poisoned = null;
    }

    // Tazed Effect
    public void taze(/*int damage,*/ float duration)
    {
        StartCoroutine(TazeRoutine(duration));
    }

    private IEnumerator TazeRoutine(float duration)
    {
        tazed = true;

        yield return new WaitForSeconds(duration);

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            tazed = true;
            speed = 0;
        }
        tazed = false;
        speed = speedOrig;
    }

    public void getGunStats(gunStats gun)
    {

        gunList.Add(gun);
        gunListPos = gunList.Count - 1;

        changeGun();

    }

    void changeGun()
    {
        shootDamage = gunList[gunListPos].shootDamage;
        shootDist = gunList[gunListPos].shootDist;
        shootRate = gunList[gunListPos].shootRate;

        gunModel.GetComponent<MeshFilter>().sharedMesh = gunList[gunListPos].gunModel.GetComponent<MeshFilter>().sharedMesh; //xfer mesh filter on pickup
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = gunList[gunListPos].gunModel.GetComponent<MeshRenderer>().sharedMaterial; //xfer mesh renderer (shader, material, etc) on pinkup
    }

    void selectGun()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && gunListPos < gunList.Count - 1) //if bigger than zero and within list
        {
            gunListPos++; //increment
            changeGun(); //changegun
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && gunListPos > 0) //if smaller than zero and within list
        {
            gunListPos--; //decrement
            changeGun(); //changegun
        }
    }

    // UpgradeShop stuff - JC
    public void applyUpgrade(upgradeData upgrade)
    {
        switch (upgrade.type)
        {
            // Player upgrades
            case upgradeType.playerMaxHP:
                HPOrig += Mathf.RoundToInt(upgrade.amount);
                HP = HPOrig;
                updatePlayerUI();
                break;
            case upgradeType.playerMoveSpeed:
                speedOrig += Mathf.RoundToInt(upgrade.amount);
                if (speedOrig < 1)
                    speedOrig = 1;
                speed = speedOrig;
                break;
            case upgradeType.playerJumpMax:
                jumpMax += Mathf.RoundToInt(upgrade.amount);
                if (jumpMax < 1)
                    jumpMax = 1;
                break;
            // Gun upgrades
            case upgradeType.gunDamage:
                shootDamage = Mathf.RoundToInt(shootDamage * (1f + upgrade.amount));
                break;
            case upgradeType.gunFireRate:
                shootRate *= (1f - upgrade.amount);
                if (shootRate < 0.05f)
                    shootRate = 0.05f;
                break;
            case upgradeType.gunRange:
                shootDist += Mathf.RoundToInt(upgrade.amount);
                if (shootDist < 1)
                    shootDist = 1;
                break;
        }
    }
}
