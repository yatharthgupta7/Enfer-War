using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum Team
{
    Blue,
    Red
}

public class PlayerLogic : NetworkBehaviour
{
    float m_rotationY;
    const float ROTATION_SPEED = 2.0f;

    float m_horizontalInput;
    float m_verticalInput;

    const float MOVEMENT_SPEED = 5.0f;

    Vector3 m_horizontalMovement;
    Vector3 m_verticalMovement;
    Vector3 m_heightMovement;

    float m_jumpHeight = 0.25f;
    float m_gravity = 0.981f;
    bool m_jump = false;

    CharacterController m_characterController;
    Animator m_animator;

    [SerializeField]
    Transform m_leftHandTarget;

    [SerializeField]
    Transform m_rightHandTarget;

    bool m_isCrouching = false;

    WeaponLogic m_weaponLogic;

    AudioSource m_audioSource;

    [SerializeField]
    List<AudioClip> m_footstepSounds;

    [SerializeField]
    GameObject m_camera;

    [SerializeField]
    SkinnedMeshRenderer m_headRenderer;

    [SerializeField]
    SkinnedMeshRenderer m_bodyRenderer;

    NetworkAnimator m_networkAnimator;

    const int MAX_HEALTH = 100;
    int m_health = MAX_HEALTH;

    bool m_isDead = false;

    [SyncVar]
    Team m_team = Team.Blue;

    [SerializeField]
    Material m_blueMaterial;

    [SerializeField]
    Material m_redMaterial;

    // Start is called before the first frame update
    void Start()
    {
        SetupCamera();

        m_characterController = GetComponent<CharacterController>();
        m_animator = GetComponent<Animator>();
        m_weaponLogic = GetComponentInChildren<WeaponLogic>();
        m_audioSource = GetComponent<AudioSource>();
        m_networkAnimator = GetComponent<NetworkAnimator>();

        SetupHeadRendering();

        SetHealthText();
    }

    public override void OnStartClient()
    {
        SetColor(m_team);
    }

    public void SetTeam(Team team)
    {
        RpcSetTeam(team);
    }

    [ClientRpc]
    void RpcSetTeam(Team team)
    {
        m_team = team;
        SetColor(m_team);
    }

    void SetColor(Team team)
    {
        if (m_team == Team.Blue)
        {
            SetMaterial(m_blueMaterial);
        }
        else if (m_team == Team.Red)
        {
            SetMaterial(m_redMaterial);
        }
    }

    void SetMaterial(Material material)
    {
        // Body
        Material[] mats = m_bodyRenderer.materials;
        mats[0] = material;
        m_bodyRenderer.materials = mats;

        // Head
        mats = m_headRenderer.materials;
        mats[0] = material;
        m_headRenderer.materials = mats;
    }

    void SetHealthText()
    {
        if (UIManager.Instance && isLocalPlayer)
        {
            UIManager.Instance.SetHealthText(m_health);
        }
    }

    void SetupCamera()
    {
        if (Camera.main)
        {
            Camera.main.enabled = false;
        }

        if (m_camera && isLocalPlayer)
        {
            m_camera.SetActive(true);
        }
    }

    void SetupHeadRendering()
    {
        if (m_headRenderer)
        {
            if (isLocalPlayer)
            {
                m_headRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
            else
            {
                m_headRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer || m_isDead)
        {
            return;
        }

        m_rotationY += Input.GetAxis("Mouse X") * ROTATION_SPEED;
        transform.rotation = Quaternion.Euler(0, m_rotationY, 0);

        m_horizontalInput = Input.GetAxis("Horizontal");
        m_verticalInput = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.C))
        {
            m_isCrouching = !m_isCrouching;
            if (m_animator)
            {
                m_animator.SetBool("IsCrouching", m_isCrouching);
            }
        }

        if (Input.GetButtonDown("Jump") && m_characterController.isGrounded)
        {
            m_jump = true;
        }
    }

    public bool IsCrouching()
    {
        return m_isCrouching;
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer || m_isDead)
        {
            return;
        }

        if (m_jump)
        {
            m_heightMovement.y = m_jumpHeight;
            m_jump = false;
        }

        m_heightMovement.y -= m_gravity * Time.deltaTime;

        m_horizontalMovement = transform.right * m_horizontalInput * MOVEMENT_SPEED * Time.deltaTime;
        m_verticalMovement = transform.forward * m_verticalInput * MOVEMENT_SPEED * Time.deltaTime;

        if (m_characterController && !m_isCrouching)
        {
            m_characterController.Move(m_horizontalMovement + m_verticalMovement + m_heightMovement);
        }

        if (m_animator)
        {
            m_animator.SetFloat("HorizontalInput", m_horizontalInput);
            m_animator.SetFloat("VerticalInput", m_verticalInput);
        }
    }

    public float GetRotationY()
    {
        return m_rotationY;
    }

    public void AddRecoil()
    {
        m_rotationY += Random.Range(-1.0f, 1.0f);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (m_animator && !m_isDead)
        {
            if (m_weaponLogic && !m_weaponLogic.IsReloading())
            {
                SetHandIK(AvatarIKGoal.LeftHand, m_leftHandTarget);
            }

            SetHandIK(AvatarIKGoal.RightHand, m_rightHandTarget);
        }
    }

    void SetHandIK(AvatarIKGoal avatarIKGoal, Transform target)
    {
        if (target)
        {
            m_animator.SetIKPosition(avatarIKGoal, target.position);
            m_animator.SetIKRotation(avatarIKGoal, target.rotation);
            m_animator.SetIKPositionWeight(avatarIKGoal, 1.0f);
            m_animator.SetIKRotationWeight(avatarIKGoal, 1.0f);
        }
    }

    public void PlayFootstepSound()
    {
        int soundIndex = Random.Range(0, m_footstepSounds.Count);
        PlaySound(m_footstepSounds[soundIndex]);
    }

    void PlaySound(AudioClip sound, float volume = 1.0f)
    {
        if (m_audioSource && sound)
        {
            m_audioSource.volume = volume;
            m_audioSource.PlayOneShot(sound);
        }
    }

    public bool IsLocalPlayer()
    {
        return isLocalPlayer;
    }

    public void PlayShootAnimation()
    {
        if (m_animator)
        {
            m_animator.SetTrigger("Shoot");
        }

        if (m_networkAnimator)
        {
            m_networkAnimator.SetTrigger("Shoot");
        }
    }

    public void PlayReloadAnimation()
    {
        if (m_animator)
        {
            m_animator.SetTrigger("Reload");
        }

        if (m_networkAnimator)
        {
            m_networkAnimator.SetTrigger("Reload");
        }
    }

    public void TakeDamage(int damage)
    {
        if (!isServer)
        {
            return;
        }

        m_health -= damage;
        m_health = Mathf.Clamp(m_health, 0, MAX_HEALTH);
        RpcSetHealth(m_health);

        if (m_health == 0)
        {
            RpcDie();
        }
    }

    [ClientRpc]
    void RpcDie()
    {
        if (m_animator)
        {
            m_animator.SetTrigger("Die");
        }

        if (m_networkAnimator)
        {
            m_networkAnimator.SetTrigger("Die");
        }

        if (m_weaponLogic)
        {
            m_weaponLogic.DropWeapon();
        }

        m_isDead = true;
    }

    [ClientRpc]
    void RpcSetHealth(int health)
    {
        m_health = health;

        SetHealthText();
    }

    public bool IsDead()
    {
        return m_isDead;
    }
}
