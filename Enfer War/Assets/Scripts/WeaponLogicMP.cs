using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WeaponLogicMP : NetworkBehaviour
{
    WeaponLogic m_weaponLogic;

    [SerializeField]
    Camera m_playerCamera;

    // Start is called before the first frame update
    void Start()
    {
        m_weaponLogic = GetComponentInChildren<WeaponLogic>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    [Command]
    public void CmdShoot()
    {
        if (!isServer || !m_weaponLogic)
        {
            return;
        }

        Ray ray = new Ray(m_playerCamera.transform.position, m_playerCamera.transform.forward);
        RaycastHit rayHit;

        if (Physics.Raycast(ray, out rayHit, 100.0f))
        {
            Debug.Log("Hit object: " + rayHit.collider.name);
            Debug.Log("Hit Pos: " + rayHit.point);

            bool hitPlayer = rayHit.collider.tag == "Player";
            if (hitPlayer)
            {
                PlayerLogic playerLogic = rayHit.collider.GetComponent<PlayerLogic>();
                if (playerLogic)
                {
                    playerLogic.TakeDamage(30);
                }
            }

            RpcShootEffect(rayHit.point, Quaternion.FromToRotation(Vector3.up, rayHit.normal) * Quaternion.Euler(-90, 0, 0), !hitPlayer);
        }
    }

    [ClientRpc]
    void RpcShootEffect(Vector3 impactPosition, Quaternion impactRotation, bool spawnImpactObj)
    {
        if (m_weaponLogic)
        {
            m_weaponLogic.ShootEffect(impactPosition, impactRotation, spawnImpactObj);
        }
    }
}
