using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponLogic : MonoBehaviour
{
    const int MAX_AMMO = 30;
    int m_ammoCount = MAX_AMMO;

    const float SHOT_COOLDOWN = 0.15f;
    float m_cooldown = 0.0f;

    PlayerLogic m_playerLogic;
    FirstPersonLogic m_firstPersonLogic;

    ParticleSystem m_muzzleFlash;
    Light m_muzzleFlashLight;
    const float MAX_LIGHT_TIME = 0.2f;
    float m_lightTimer = 0.0f;

    [SerializeField]
    GameObject m_bulletImpactObj;

    bool m_isReloading = false;

    AudioSource m_audioSource;

    [SerializeField]
    AudioClip m_shootSound;

    [SerializeField]
    AudioClip m_emptyClipSound;

    [SerializeField]
    AudioClip m_reloadingSound;

    Vector3 m_startPosition;
    const float TIME_SCALE = 2.0f;

    WeaponLogicMP m_weaponLogicMP;

    Rigidbody m_rigidBody;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;

        m_playerLogic = GetComponentInParent<PlayerLogic>();
        m_firstPersonLogic = GetComponentInParent<FirstPersonLogic>();
        m_muzzleFlash = GetComponentInChildren<ParticleSystem>();
        m_muzzleFlashLight = GetComponentInChildren<Light>();

        m_audioSource = GetComponent<AudioSource>();

        m_weaponLogicMP = GetComponentInParent<WeaponLogicMP>();

        m_startPosition = transform.localPosition;

        m_rigidBody = GetComponent<Rigidbody>();

        SetAmmoText();
    }

    public void DropWeapon()
    {
        transform.parent = null;

        if (m_rigidBody)
        {
            m_rigidBody.isKinematic = false;
            m_rigidBody.useGravity = true;
        }
    }

    void SetAmmoText()
    {
        if (UIManager.Instance && m_playerLogic && m_playerLogic.IsLocalPlayer())
        {
            UIManager.Instance.SetAmmoText(m_ammoCount);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_lightTimer > 0.0f)
        {
            m_lightTimer -= Time.deltaTime;
        }
        else
        {
            m_muzzleFlashLight.enabled = false;
        }

        if (m_playerLogic && (!m_playerLogic.IsLocalPlayer() || m_playerLogic.IsDead()))
        {
            return;
        }

        transform.localPosition = m_startPosition + new Vector3(0.0f, Mathf.Sin(Time.time * TIME_SCALE) / 100.0f, 0.0f);

        if (m_cooldown > 0.0f)
        {
            m_cooldown -= Time.deltaTime;
        }
        else
        {
            if (Input.GetButton("Fire1") && !m_isReloading)
            {
                if (m_ammoCount > 0)
                {
                    Shoot();
                }
                else
                {
                    // Play empty clip sound
                    PlaySound(m_emptyClipSound);
                }

                m_cooldown = SHOT_COOLDOWN;
            }
        }

        if (Input.GetButtonDown("Fire2"))
        {
            Reload();
        }
    }

    void Shoot()
    {
        --m_ammoCount;
        SetAmmoText();

        if (m_playerLogic)
        {
            m_playerLogic.PlayShootAnimation();
        }

        PlaySound(m_shootSound);

        if (m_weaponLogicMP)
        {
            m_weaponLogicMP.CmdShoot();
        }
    }

    public void ShootEffect(Vector3 impactPosition, Quaternion impactRotation, bool spawnBulletImpact)
    {
        // Spawn Bullet Impact FX
        if (m_bulletImpactObj && spawnBulletImpact)
        {
            GameObject.Instantiate(m_bulletImpactObj, impactPosition, impactRotation);
        }

        if (m_firstPersonLogic)
        {
            m_firstPersonLogic.AddRecoil();
        }

        if (m_playerLogic)
        {
            m_playerLogic.AddRecoil();
        }

        if (m_muzzleFlash)
        {
            m_muzzleFlash.Play(true);
        }

        if (m_muzzleFlashLight)
        {
            m_muzzleFlashLight.enabled = true;
            m_lightTimer = MAX_LIGHT_TIME;
        }
    }

    void Reload()
    {
        m_isReloading = true;

        if (m_playerLogic)
        {
            m_playerLogic.PlayReloadAnimation();
        }

        PlaySound(m_reloadingSound, 0.5f);
    }

    public bool IsReloading()
    {
        return m_isReloading;
    }

    public void SetReloadingState(bool isReloading)
    {
        m_isReloading = isReloading;

        if (!m_isReloading)
        {
            m_ammoCount = MAX_AMMO;
            SetAmmoText();
        }
    }

    void PlaySound(AudioClip sound, float volume = 1.0f)
    {
        if (m_audioSource && sound)
        {
            m_audioSource.volume = volume;
            m_audioSource.PlayOneShot(sound);
        }
    }
}
