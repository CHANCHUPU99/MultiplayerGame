using Unity.Netcode;
using UnityEngine;
public class Projectile : NetworkBehaviour
{
    public float speed = 10f;
    public float lifeTime = 3f;
    public float damage = 55f;
    public PlayerController instigator;//quien disparo el proyectil
    public Vector3 direction;
    public GameObject impactPrefab;

    void Start()
    {
        
    }

    void Update()
    {
       
        //el servidor tiene la autoridad sobre el proyectil
        if (IsServer) {
            lifeTime -= Time.deltaTime;
            if (lifeTime < 0)
            {
                //Destroy(gameObject);
                GetComponent<NetworkObject>().Despawn();
            }

            transform.position  += direction * speed * Time.deltaTime; 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        PlayerController otherPlayer = other.GetComponent<PlayerController>();
        if (otherPlayer != null && otherPlayer != instigator)
        {
            otherPlayer.takeDamage((int)damage);
            onImpactRPC();
            GetComponent<NetworkObject>().Despawn();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void onImpactRPC()
    {
        //spawnear el efecto de impacto
        if (impactPrefab != null) { 
            GameObject impact = Instantiate(impactPrefab, transform.position,Quaternion.identity);
            Destroy(impact,2f);
        }
    }
}
