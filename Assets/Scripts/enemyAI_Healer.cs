using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class enemyAI_Healer : MonoBehaviour, IDamage
{
    [Header("Stats")]
    [SerializeField] int maxHP = 30;
    int currentHP;

    [Header("Healing")]
    [SerializeField] float healRadius = 12f;
    [SerializeField] int healAmount = 5;
    [SerializeField] float healInterval = 1f;

    [Header("Movement")]
    [SerializeField] float retreatDistance = 8f;
    [SerializeField] float moveSpeed = 3f;

    Transform player;
    float lockedY;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHP = maxHP;
        lockedY = transform.position.y;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        StartCoroutine(healLoop());
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
            return;
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (distToPlayer < retreatDistance)
        {
            Vector3 dir = (transform.position - player.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;

            if (dir != Vector3.zero)
            {
                transform.forward = Vector3.Lerp(transform.forward, -dir, Time.deltaTime * 5f);
            }
        }

        Vector3 p = transform.position;
        p.y = lockedY;
        transform.position = p;
    }

    IEnumerator healLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(healInterval);

        while (true)
        {
            healNearbyAllies();
            yield return wait;
        }
    }

    void healNearbyAllies()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, healRadius);

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == this.gameObject)
                continue;
            if (!hit.CompareTag("Enemy"))
                continue;

            IHeal healTarget = hit.GetComponentInParent<IHeal>();
            if(healTarget != null)
            {
                healTarget.heal(healAmount);
            }
        }
    }

    public void takeDamage(int amount)
    {
        if (amount <= 0) 
            return;

        currentHP -= amount;

        if (currentHP <= 0)
        {
            die();
        }
    }

    void die()
    {
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, healRadius);
    }

    public void poison(int damage, float rate, float duration)
    {
        return;
    }

    public void taze(int damage, float duration)
    {
        return;
    }
}
