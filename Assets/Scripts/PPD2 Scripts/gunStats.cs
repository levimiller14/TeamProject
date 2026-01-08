using System;
using UnityEngine;

[CreateAssetMenu] //enables right click to add object
public class gunStats : ScriptableObject
{
    public GameObject gunModel; //model we want to xfer 

    [Range(1, 10)] public int shootDamage;
    [Range(3, 1000)] public int shootDist;
    [Range(0.1f, 3)] public float shootRate;

    public int ammoCur; //current ammo
    [Range(5, 50)] public int ammoMax; //max ammo

    public ParticleSystem hitEffect; //wherever the raycast hits, show hit effect
    public AudioClip[] shootSound;//when shooting make sound. array
    [Range(0, 1)] public float shootSoundVol; //how loud gun is
}
