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
    [Header("Set Dynamically")]
    [SerializeField]
    private float _shieldLevel = 1;

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
                Main.S.DelayedRestart( gameRestartDelay );
            }
        }
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
    void Update()
    {
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
        else
        {
            print("Triggered by non-Enemy: " + go.name);
        }
    }
}