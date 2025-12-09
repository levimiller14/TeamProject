using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.DualShock;

public class playerController : MonoBehaviour, IDamage, IHeal
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

    // GODMODE
    public bool isGodMode = false;

    float shootTimer;
    private Coroutine poisoned;

    Vector3 moveDir;
    Vector3 playerVel;
    Vector3 externalVelocity;

    [Header("----- Grapple Settings -----")]
    [SerializeField] float groundedVelocityDecay = 15f;
    [SerializeField] float airVelocityDecay = 0.5f;

    GrapplingHook grappleHook;
    bool grappleJumpedThisFrame;

    Camera mainCam;

    void Start()
    {
        HPOrig = HP;
        speedOrig = speed;
        mainCam = Camera.main;
        grappleHook = GetComponent<GrapplingHook>();
        wallRun = GetComponent<wallRun>();
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
            playerVel.y = 0;

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
            // wall run start
            if (wallRun != null)
            {
                wallRun.ProcessWallRun(ref moveDir, ref playerVel, controller.isGrounded);
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

        if(Input.GetButtonDown("Jump") && jumpCount < jumpMax)
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
        if(Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
        }
        else if(Input.GetButtonUp("Sprint"))
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
    }

    private void OnTriggerExit(Collider other)
    {
        speed = speedOrig;
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

        RaycastHit hit;
        if (Physics.Raycast(mainCam.transform.position, mainCam.transform.forward, out hit, shootDist, ~ignoreLayer))
        {

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(shootDamage);

                // stat tracking
                if(statTracker.instance != null)
                {
                    statTracker.instance.IncrementShotsHit();
                }
            }
        }
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
            HP += healAmount;
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

    
//<<<<<<< HEAD
//=======
    // poison routines
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
//>>>>>>> 1bb9c2b6da50e57523c371620cff226582621ee7
}
}
