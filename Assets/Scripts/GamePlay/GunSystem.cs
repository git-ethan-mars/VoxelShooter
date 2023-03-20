using System;
using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Plugin;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class GunSystem : MonoBehaviour
{
    public int damage;
    public float timeBetweenShooting, baseRecoil, stepRecoil, resetTimeRecoil, range, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool isAutomatic;
    private int bulletsLeft, bulletsShot;
    private bool shooting, readyToShoot, reloading;

    private Camera fpsCam;
    public Transform attackPoint;
    public RaycastHit rayHit;
    public LayerMask whatIsEnemy;
    private float recoilModifier = 1;

    public GameObject muzzleFlash, bulletHoleGraphic;
    public TextMeshProUGUI text;

    private void Awake()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;
        fpsCam = Camera.main;
        text = GameObject.Find("Canvas/GamePlay/AmmoAmount").GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        MyInput();
        text.SetText($"{bulletsLeft} / {magazineSize}");
    }

    private void MyInput()
    {
        shooting = isAutomatic ? Input.GetKey(KeyCode.K) : Input.GetKeyDown(KeyCode.K);

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading)
            Reload();

        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = bulletsPerTap;
            Shoot();
        }
    }

    private void Shoot()
    {
        var x = Math.Abs(recoilModifier - 1) < 0.00001 ? 0 : Random.Range(-baseRecoil, baseRecoil) * recoilModifier;
        var y = Math.Abs(recoilModifier - 1) < 0.00001 ? 0 : Random.Range(-baseRecoil, baseRecoil) * recoilModifier;
        recoilModifier += stepRecoil;

        readyToShoot = false;

        var direction = new Vector3(0.5f, 0.5f);
        if (isAutomatic)
            direction += new Vector3(x, y);
        
        var ray = fpsCam.ViewportPointToRay(direction);
        var raycastResult = Physics.Raycast(ray, out rayHit, range);
        if (raycastResult)
        {
            Instantiate(bulletHoleGraphic, rayHit.point, Quaternion.Euler(rayHit.normal.y * -90, 
                rayHit.normal.x * 90 + rayHit.normal.z * -180, 0));
        }
        
        Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);

        bulletsLeft--;
        bulletsShot--;
            
        Invoke(nameof(ResetShot), timeBetweenShooting);
        Invoke(nameof(ResetRecoil), resetTimeRecoil);
        
        if (bulletsShot > 0 && bulletsLeft > 0)
            Invoke(nameof(Shoot), timeBetweenShots);
    }

    private void ResetRecoil()
    {
        recoilModifier -= stepRecoil;
    }

    private void ResetShot()
    {
        readyToShoot = true;
    }
    
    private void Reload()
    {
        reloading = true;
        Invoke(nameof(ReloadFinished), reloadTime);
    }

    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }
}
