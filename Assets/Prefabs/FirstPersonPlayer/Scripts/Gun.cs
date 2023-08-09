using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Type
{
    Projectile,
    Hitscan
}

public class Gun : MonoBehaviour
{
    [SerializeField] private Type weaponType;
    [SerializeField] private float range;

    public Type WeaponType => weaponType;
    public float Range => range;
}
