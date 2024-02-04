using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_02_Shoot : MonoBehaviour
{
    public Transform ShootingPoint;
    public GameObject DamageOrb;
    private character cc;
    private void Awake() 
    {
        cc = GetComponent<character>();
    }
    public void ShootTheDamageOrb()
    {
        Instantiate(DamageOrb,ShootingPoint.position,Quaternion.LookRotation(ShootingPoint.forward));
    }
    private void Update() 
    {
        cc.RotateToTarget();
    }
}
