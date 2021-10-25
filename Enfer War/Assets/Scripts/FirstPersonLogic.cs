using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonLogic : MonoBehaviour
{
    float m_rotationX = 0.0f;
    float m_targetRotationX = 0.0f;
    float m_startRotationX = 0.0f;
    const float MIN_X = -50.0f;
    const float MAX_X = 50.0f;
    const float ROTATION_SPEED = 2.0f;
    const float CROUCH_LERP_SPEED = 4.5f;
    const float DEFAULT_LERP_SPEED = 10.0f;
    PlayerLogic m_playerLogic;

    bool m_recoilAnim;
    float m_recoilAnimProgress;

    [SerializeField]
    Vector3 m_crouchingPosition;

    Vector3 m_defaultPosition;
    void Start()
    {
        m_playerLogic = GetComponentInParent<PlayerLogic>();
        m_defaultPosition = transform.localPosition;
    }
    void Update()
    {
        if (m_playerLogic && (!m_playerLogic.IsLocalPlayer() || m_playerLogic.IsDead()))
        {
            return;
        }
        m_rotationX -= Input.GetAxis("Mouse Y") * ROTATION_SPEED;
        m_rotationX = Mathf.Clamp(m_rotationX, MIN_X, MAX_X);

        if(m_recoilAnim)
        {
            m_recoilAnimProgress += Time.deltaTime;
            m_rotationX = Mathf.Lerp(m_startRotationX, m_targetRotationX, m_recoilAnimProgress);

            if(Mathf.Abs(m_rotationX-m_targetRotationX)<0.1f)
            {
                m_rotationX = m_targetRotationX;
                m_recoilAnim = false;
                m_recoilAnimProgress = 0.0f;
            }    
        }

        if(m_playerLogic)
        {
            if(m_playerLogic.IsCrouching())
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, m_crouchingPosition, Time.deltaTime * CROUCH_LERP_SPEED);
            }
            else
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, m_defaultPosition, Time.deltaTime * DEFAULT_LERP_SPEED);
            }
        }
    }

    private void LateUpdate()
    {
        if (m_playerLogic && (!m_playerLogic.IsLocalPlayer() || m_playerLogic.IsDead()))
        {
            return;
        }
        transform.rotation = Quaternion.Euler(m_rotationX, m_playerLogic.GetRotationY(), 0);
    }

    public void AddRecoil()
    {
        if(!m_recoilAnim)
        {
            m_targetRotationX = m_rotationX;
        }
        m_recoilAnimProgress = 0.0f;
        m_recoilAnim = true;
        m_startRotationX = m_rotationX;

        m_rotationX -= 2.0f;
    }
}
