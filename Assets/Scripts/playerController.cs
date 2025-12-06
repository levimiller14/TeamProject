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

    int jumpCount;
    int speedOrig;
    // making HPOrig public for cheatManager.cs
    public int HPOrig;

    float shootTimer;
    private Coroutine poisoned;

    Vector3 moveDir;
    Vector3 playerVel;

    Camera mainCam;

    void Start()
    {
        HPOrig = HP;
        speedOrig = speed;
        mainCam = Camera.main;
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

        if(controller.isGrounded)
        {
            jumpCount = 0;
            playerVel = Vector3.zero;
        }

        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * speed * Time.deltaTime);

        jump();
        controller.Move(playerVel * Time.deltaTime);

        playerVel.y -= gravity * Time.deltaTime;
    }

    void jump()
    {
        if(Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;
        }
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
        Instantiate(playerBullet, playerShootPos.position, mainCam.transform.rotation);

        RaycastHit hit;
        if (Physics.Raycast(mainCam.transform.position, mainCam.transform.forward, out hit, shootDist, ~ignoreLayer))
        {

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(shootDamage);
            }
        }
    }

    public void takeDamage(int amount)
    {
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
