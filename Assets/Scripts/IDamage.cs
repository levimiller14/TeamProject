using UnityEngine;

public interface IDamage
{
    void takeDamage(int amount);
    void poison(int damage, float rate, float duration);
    void taze(int damage, float duration);
}
