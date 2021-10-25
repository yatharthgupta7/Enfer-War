using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance = null;

    [SerializeField]
    Text m_ammoText;

    [SerializeField]
    Text m_healthText;

    private void Awake()
    {
        if(Instance==null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetHealthText(int health)
    {
        if(m_healthText)
        {
            m_healthText.text = "Health: " + health;
        }
    }

    public void SetAmmoText(int ammo)
    {
        if(m_ammoText)
        {
            m_ammoText.text = "Ammo: " + ammo;
        }
    }
}
