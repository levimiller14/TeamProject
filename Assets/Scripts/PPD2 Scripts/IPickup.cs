using NUnit.Framework.Interfaces;
using UnityEngine;

public interface IPickup
{
    public void getGunStats(gunStats gun);
    public void addKey(keyFunction key);
}