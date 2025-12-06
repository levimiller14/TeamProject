using System.Collections;
using UnityEngine;


//public event Action<float, float> OnHealthChanged;
public class heal : MonoBehaviour
{
   // public event Action<float, float> OnHealthChanged;
    enum HealType {HOT, still} //HOT capsule that disappears after healed
    [SerializeField] int healAmount; //total heal amount
    [SerializeField] int healTime; //how long it heals
    [SerializeField] int healSpeed; //rate it heals at


    [SerializeField] HealType _heal;

    bool healing;
    private float maxHealth = 100f;
    private float cHealth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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

        if(cHealth != maxHealth && _heal == HealType.HOT) //if not null and is Heal over time
        {
           //health.heal(healAmount);
            healPlayer(healAmount); //heal for that amount
            if(cHealth == maxHealth)
            {
                Destroy(gameObject);
            }
            
        }
        

     
    }
    private void OnTriggerStay(Collider other) //heal area
    {
        if (other.isTrigger)
            return;
        IHeal health = other.GetComponent<IHeal>();

        if(health != null && _heal == HealType.still && !healing) //if health isnt null and type is still
        {
            StartCoroutine(healOther(health)); 
        }
    }

    public void healPlayer(int healAmount)
    {
        healAmount = Mathf.Abs(healAmount);

        cHealth = Mathf.Min(cHealth + healAmount, maxHealth);

        
    }
    IEnumerator healOther(IHeal h)
    {
        healing = true;
        h.heal(healAmount);
        yield return new WaitForSeconds(healTime); //how fast is heal time
        healing = false;
    }
    
}
    
