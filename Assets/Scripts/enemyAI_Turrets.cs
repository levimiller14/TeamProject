using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class enemyAI_Turrets : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [SerializeField] Transform head;

    [SerializeField] int HP;
    [SerializeField] int facePlayerSpeed;
    [SerializeField] float FOV;

    [SerializeField] GameObject bullet;
    [SerializeField] float fireRate;
    [SerializeField] Transform firePos;

    Color colorOrig;

    float fireTimer;
    float angleToPlayer;
    bool isAggro;

    Vector3 playerDir;

    bool playerInRange;
    void Start()
    {
        colorOrig = model.material.color;
        gameManager.instance.UpdateGameGoal(1);
    }
    void Update()
    {
        fireTimer += Time.deltaTime;

        if (isAggro == true)
        {
            facePlayer();
        }
        if (playerInRange && canSeePlayer())
        {
            
        }
    }

    bool canSeePlayer()
    {
        playerDir = gameManager.instance.player.transform.position - transform.position;
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
        if(head == null)
        {
            return;
        }
        Vector3 targetDir = gameManager.instance.player.transform.position;
        targetDir.y = 0;

        playerDir = targetDir - head.position;

        if(playerDir.sqrMagnitude < 0.00f)
        {
            return;
        }
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, head.transform.position.y, playerDir.z));

        head.rotation = Quaternion.Slerp(head.rotation, rot, facePlayerSpeed * Time.deltaTime);
    }

    void fire()
    {
        fireTimer = 0;
        Instantiate(bullet, firePos.position, transform.rotation);
    }
    public void takeDamage(int amount)
    {
        HP -= amount;
        isAggro = true;

        if (HP <= 0)
        {
            gameManager.instance.UpdateGameGoal(-1);
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(flashRed());
        }
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }
    public void poison(int damage, float rate, float duration)
    {

    }

}
