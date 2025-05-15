using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class DamageVolume : MonoBehaviour
{
    public float initialDamage = 20;
    public float damagePerStep = 10;
    public float damageRate = 0.5f;
    //
    public List<PlayerController> players;

    private float damageTimer;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        damageTimer += Time.deltaTime;
        if (damageTimer > 0) { 
            
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null) {
            Debug.Log(other + "ha entrado");
            players.Add(pc);
            pc.takeDamage((int)initialDamage);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        Debug.Log(other + "se salio");
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null) { 
            players.Remove(pc);
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (damageTimer > damageRate) { 
            foreach (PlayerController pc in players) { 
                pc.takeDamage((int)initialDamage);
            }
            damageTimer = 0;
        }
    }
}
