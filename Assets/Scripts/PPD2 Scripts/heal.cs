using System;
using System.Collections;
using UnityEngine;



public class heal : MonoBehaviour
{
   public event Action<float, float> OnHealthChanged;
    enum HealType {HOT, still} //HOT capsule that disappears after healed
    [SerializeField] int healAmount; //total heal amount
    [SerializeField] int healTime; //how long it heals
    [SerializeField] int healSpeed; //rate it heals at


    [SerializeField] HealType _heal;

    bool healing;
    private float maxHealth = 100f;
    private float cHealth;
    private Coroutine healCoroutine;
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cHealth = maxHealth; //set hp to max on start
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter(Collider other) //capsule heals
    {
        if (other.isTrigger)
            return;

        IHeal health = other.GetComponent<IHeal>(); //does it have IHeal

        if(health != null && _heal == HealType.HOT) //if not null and is Heal over time
        {
           //health.heal(healAmount);
           if(cHealth < maxHealth)
            {
                healPlayer(healAmount); //heal for that amount
                Destroy(gameObject);
            }
            //healPlayer(healAmount); //heal for that amount
            Destroy(gameObject);
            
            
        }
        

     
    }
    private void OnTriggerStay(Collider other) //while in heal area
    {
        
        IHeal health = other.GetComponent<IHeal>();

        if(health != null && _heal == HealType.still && !healing) //if health isnt null and type is still
        {
            healCoroutine = StartCoroutine(healOther(health));
        }
    }
    private void OnTriggerExit(Collider other) //leaving heal area
    {
        IHeal health = other.GetComponent<IHeal>();

        if(health != null)
        {
            healing = false;
            if (healCoroutine != null)
            {
                StopCoroutine(healCoroutine);
                healCoroutine = null;
            }
        }
    }
    public void healPlayer(int healAmount)
    {
        healAmount = Mathf.Abs(healAmount); //get value 

        cHealth = Mathf.Min(cHealth + healAmount, maxHealth);

        OnHealthChanged?.Invoke(cHealth, maxHealth);
        
    }
    IEnumerator healOther(IHeal h)
    {
        healing = true; //heal
        h.heal(healAmount); //heal for that amount
        yield return new WaitForSeconds(healTime); //how fast is heal time
        healing = false; //stop
    }
    
}
    
