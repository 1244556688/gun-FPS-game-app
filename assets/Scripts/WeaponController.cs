using UnityEngine;
using Photon.Pun;
using TMPro;

public class WeaponController : MonoBehaviourPun
{
    public enum WeaponType { Pistol, Rifle }

    [System.Serializable]
    public struct WeaponStats
    {
        public string weaponName;
        public float damage;
        public float fireRate;
        public int magSize;
        public float range;
    }

    public WeaponStats pistolStats = new WeaponStats { weaponName = "Pistol", damage = 15f, fireRate = 3f, magSize = 12, range = 50f };
    public WeaponStats rifleStats = new WeaponStats { weaponName = "Rifle", damage = 25f, fireRate = 8f, magSize = 30, range = 100f };

    private WeaponStats currentWeapon;
    private int currentAmmo;
    private float nextFireTime;
    private bool isReloading;

    [Header("References")]
    public Transform shootPoint;
    public ParticleSystem muzzleFlash;
    public GameObject impactPrefab;
    
    // UI can be assigned or found
    private TMP_Text ammoText;

    private void Start()
    {
        // Initial setup
        EquipWeapon(WeaponType.Pistol);
        
        // Find UI in scene
        GameObject ui = GameObject.Find("AmmoText");
        if (ui != null) ammoText = ui.GetComponent<TMP_Text>();
        
        UpdateUI();
    }

    public void EquipWeapon(WeaponType type)
    {
        currentWeapon = (type == WeaponType.Pistol) ? pistolStats : rifleStats;
        currentAmmo = currentWeapon.magSize;
        UpdateUI();
    }

    public void TriggerShoot()
    {
        if (!photonView.IsMine || isReloading) return;

        if (currentAmmo > 0)
        {
            if (Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + (1f / currentWeapon.fireRate);
                Shoot();
            }
        }
        else
        {
            // Auto reload or play empty sound
            Reload();
        }
    }

    private void Shoot()
    {
        currentAmmo--;
        UpdateUI();

        // RPC for visuals across network
        photonView.RPC("RPC_ShootFX", RpcTarget.All);

        // Raycast logic
        Ray ray = new Ray(shootPoint.position, shootPoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, currentWeapon.range))
        {
            Debug.Log("Hit: " + hit.collider.name);
            
            // Damage player
            PlayerHealth health = hit.collider.GetComponentInParent<PlayerHealth>();
            if (health != null)
            {
                health.photonView.RPC("TakeDamage", RpcTarget.All, currentWeapon.damage, PhotonNetwork.LocalPlayer.ActorNumber);
            }

            // Impact FX
            if (impactPrefab != null)
            {
                photonView.RPC("RPC_ImpactFX", RpcTarget.All, hit.point, hit.normal);
            }
        }
    }

    [PunRPC]
    void RPC_ShootFX()
    {
        if (muzzleFlash != null) muzzleFlash.Play();
    }

    [PunRPC]
    void RPC_ImpactFX(Vector3 pos, Vector3 rot)
    {
        GameObject impact = Instantiate(impactPrefab, pos, Quaternion.LookRotation(rot));
        Destroy(impact, 2f);
    }

    public void Reload()
    {
        if (isReloading || currentAmmo == currentWeapon.magSize) return;
        
        // Simple reload logic
        isReloading = true;
        Invoke("FinishReload", 1.5f); // 1.5s reload time
    }

    void FinishReload()
    {
        currentAmmo = currentWeapon.magSize;
        isReloading = false;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (photonView.IsMine && ammoText != null)
        {
            ammoText.text = $"{currentAmmo} / {currentWeapon.magSize}";
        }
    }
}
