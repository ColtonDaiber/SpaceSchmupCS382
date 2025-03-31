using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Hero : MonoBehaviour
{
    static public Hero S; // Singleton // a
    [Header("Set in Inspector")]
    // These fields control the movement of the ship
    public float speed = 30;
    public float rollMult = -45;
    public float pitchMult = 30;
    public float gameRestartDelay = 2f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 40;
    public Weapon[] weapons;
    [Header("Set Dynamically")]
    [SerializeField]
    private float _shieldLevel = 1;

    public delegate void WeaponFireDelegate();
    // Create a WeaponFireDelegate field named fireDelegate.
    public WeaponFireDelegate fireDelegate;

    public float shieldLevel
    {
        get
        {
            return (_shieldLevel); // a
        }
        set
        {
            _shieldLevel = Mathf.Min(value, 4);
            // If the shield is going to be set to less than zero
            if (value < 0)
            {
                Destroy(this.gameObject);
                Main.S.DelayedRestart(gameRestartDelay);
                // SaveGold();
            }
        }
    }

    // void SaveGold()
    // {
    //     PlayerPrefs.SetInt("weapon0", (int)WeaponType.none);
    //     PlayerPrefs.SetInt("weapon1", (int)WeaponType.none);
    //     PlayerPrefs.SetInt("weapon2", (int)WeaponType.none);
    //     PlayerPrefs.SetInt("weapon3", (int)WeaponType.none);
    //     PlayerPrefs.SetInt("weapon4", (int)WeaponType.none);

    //     if(weapons[0].type == WeaponType.spreadGold || weapons[0].type == WeaponType.blasterGold) PlayerPrefs.SetInt("weapon0", (int)weapons[0].type);
    //     if(weapons[1].type == WeaponType.spreadGold || weapons[1].type == WeaponType.blasterGold) PlayerPrefs.SetInt("weapon1", (int)weapons[1].type);
    //     if(weapons[2].type == WeaponType.spreadGold || weapons[2].type == WeaponType.blasterGold) PlayerPrefs.SetInt("weapon2", (int)weapons[2].type);
    //     if(weapons[3].type == WeaponType.spreadGold || weapons[3].type == WeaponType.blasterGold) PlayerPrefs.SetInt("weapon3", (int)weapons[3].type);
    //     if(weapons[4].type == WeaponType.spreadGold || weapons[4].type == WeaponType.blasterGold) PlayerPrefs.SetInt("weapon4", (int)weapons[4].type);
    //     PlayerPrefs.Save();
    // }
    void LoadGold()
    {
        weapons[0].SetType((WeaponType)PlayerPrefs.GetInt("weapon0"));
        weapons[1].SetType((WeaponType)PlayerPrefs.GetInt("weapon1"));
        weapons[2].SetType((WeaponType)PlayerPrefs.GetInt("weapon2"));
        weapons[3].SetType((WeaponType)PlayerPrefs.GetInt("weapon3"));
        weapons[4].SetType((WeaponType)PlayerPrefs.GetInt("weapon4"));
        //clear
        PlayerPrefs.SetInt("weapon0", (int)WeaponType.none);
        PlayerPrefs.SetInt("weapon1", (int)WeaponType.none);
        PlayerPrefs.SetInt("weapon2", (int)WeaponType.none);
        PlayerPrefs.SetInt("weapon3", (int)WeaponType.none);
        PlayerPrefs.SetInt("weapon4", (int)WeaponType.none);
    }

    // This variable holds a reference to the last triggering GameObject
    private GameObject lastTriggerGo = null;
    void Awake()
    {
        if (S == null)
        {
            S = this; // Set the Singleton // a
        }
        else
        {
            Debug.LogError("Hero.Awake() - Attempted to assign second Hero.S!");
        }
    }
    bool init = false;
    void Update()
    {
        if(!init)
        {
            init = true;
            ClearWeapons();
            LoadGold();
            if(weapons[0].type == WeaponType.none) weapons[0].SetType(WeaponType.blaster);
        }

        // Pull in information from the Input class
        float xAxis = Input.GetAxis("Horizontal"); // b
        float yAxis = Input.GetAxis("Vertical"); // b
                                                 // Change transform.position based on the axes
        Vector3 pos = transform.position;
        pos.x += xAxis * speed * Time.deltaTime;
        pos.y += yAxis * speed * Time.deltaTime;
        transform.position = pos;
        // Rotate the ship to make it feel more dynamic // c
        transform.rotation = Quaternion.Euler(yAxis * pitchMult, xAxis * rollMult, 0);

        if (Input.GetAxis("Jump") == 1 && fireDelegate != null)
        {
            fireDelegate(); // e
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Transform rootT = other.gameObject.transform.root;
        GameObject go = rootT.gameObject;
        //print("Triggered: "+go.name);
        // Make sure it's not the same triggering go as last time
        if (go == lastTriggerGo)
        {
            return;
        }
        lastTriggerGo = go;
        if (go.tag == "Enemy")
        { // If the shield was triggered by an enemy
            shieldLevel--; // Decrease the level of the shield by 1
            Destroy(go); // … and Destroy the enemy
        }
        else if (go.tag == "PowerUp")
        {
            // If the shield was triggered by a PowerUp
            AbsorbPowerUp(go);
        }
        else
        {
            print("Triggered by non-Enemy: " + go.name);
        }
    }

    public void AbsorbPowerUp(GameObject go)
    {
        PowerUp pu = go.GetComponent<PowerUp>();
        switch (pu.type)
        {
            case WeaponType.shield:
                shieldLevel++;
                break;
            default:
                if (pu.type == weapons[0].type || 
                    (pu.type == WeaponType.blasterGold && weapons[0].type == WeaponType.blaster) || 
                    (pu.type == WeaponType.blaster && weapons[0].type == WeaponType.blasterGold) || 
                    (pu.type == WeaponType.spreadGold && weapons[0].type == WeaponType.spread) || 
                    (pu.type == WeaponType.spread && weapons[0].type == WeaponType.spreadGold))
                { // If it is the same type
                    int index;
                    Weapon w = GetEmptyWeaponSlot(out index);
                    if (w != null)
                    {
                        // Set it to pu.type
                        w.SetType(pu.type);

                        if(w.type == WeaponType.spreadGold || w.type == WeaponType.blasterGold) PlayerPrefs.SetInt("weapon"+index, (int)w.type);
                    }
                }
                else
                { // If this is a different weapon type
                    ClearWeapons();
                    weapons[0].SetType(pu.type);

                    //clear golds
                    PlayerPrefs.SetInt("weapon0", (int)WeaponType.none);
                    PlayerPrefs.SetInt("weapon1", (int)WeaponType.none);
                    PlayerPrefs.SetInt("weapon2", (int)WeaponType.none);
                    PlayerPrefs.SetInt("weapon3", (int)WeaponType.none);
                    PlayerPrefs.SetInt("weapon4", (int)WeaponType.none);

                }
                break;

        }
        pu.AbsorbedBy(this.gameObject);
        // SaveGold();
    }

    Weapon GetEmptyWeaponSlot()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].type == WeaponType.none)
            {
                return (weapons[i]);
            }
        }
        return (null);
    }
    Weapon GetEmptyWeaponSlot(out int index)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].type == WeaponType.none)
            {
                index = i;
                return (weapons[i]);
            }
        }
        index = -1;
        return (null);
    }
    void ClearWeapons()
    {
        foreach (Weapon w in weapons)
        {
            w.SetType(WeaponType.none);
        }
    }
}