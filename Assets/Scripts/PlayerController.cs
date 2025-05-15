using UnityEngine;
using Unity.Netcode;
using TMPro;
using NUnit.Framework;
using System.Collections.Generic;
public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    //hacia donde apunta el character
    Vector3 desiredDirection;
    public float speed = 1.0f;

    //salud del personaje
    //public int health = 100;

    [Header("Camera")]
    public Vector3 cameraOffset = new Vector3(0, 4f, -3);
    public Vector3 cameraViewOffset = new Vector3(0, 1.5f, 0);
    Camera cam;

    [Header("Weapon")]
    public GameObject projectilePrefab;
    public Transform weaponSocket;
    public float weaponCadence = 0.8f;
    float lastShotTimer = 0;

    [Header("Accesories")]
    public Transform hatSocket;
    public List<GameObject> prefabsHat = new List<GameObject>();
    bool hatSpawned = false;

    //networkVariable para poder replicar health
    NetworkVariable<int> health = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private UIMANAGER hud;
    private GameManager gameManager;
    private TMP_Text playerName;


    [Header("SFX")]
    public AudioClip DamageSound;
    public AudioClip DeathSound;
    AudioSource audioSource;
    Animator animator;


    //public int nameId = 0; //id del nombre seleccionado
    NetworkVariable<int> nameId = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,//permiso de lectura
        NetworkVariableWritePermission.Server);//perimiso de escritura

    //similar al nombre , un ID de accesorio
    NetworkVariable<int> sombreroId = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    public override void OnNetworkSpawn()
    {
        Debug.Log("holaaaaaaaaaaaa " + (IsClient ? "cliente" : "servidor"));
        Debug.Log("isClient=" + IsClient + ",Server=" + IsServer + ",Hosts=" + IsHost);
        Debug.Log(name + " is owner =" + IsOwner);

        sombreroId.OnValueChanged += spawnSombrero;
    }
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    void Start()
    {

        hud = GameObject.Find("GameManager").GetComponent<UIMANAGER>();
        audioSource = GetComponent<AudioSource>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        transform.position = gameManager.getSpawnPoint();
        //asiganr camera


        if (IsOwner)
        {
            cam = GameObject.Find("Main Camera").GetComponent<Camera>();
            cam.transform.position = transform.position + cameraOffset;
            cam.transform.LookAt(transform.position + cameraViewOffset);
            setNameIDRPC(hud.selectedNameIndex);//enviar el ide del nombre al servidor
            setSombreroIDRPC(hud.selectedSombrero);
        }

        if (!IsOwner) {
            spawnSombrero(0, sombreroId.Value);
        }
        //spawnear el sombrero
        //nameId = hud.selectedNameIndex;
        createPlayerNameHUD();
    }

    void spawnSombrero(int old, int newval)
    {
        if (hatSpawned)
        {
            if(newval != old)
            {
                Destroy(hatSocket.GetChild(0).gameObject);
                hatSpawned = false;
            }
        }
        ;
        GameObject hat = Instantiate(prefabsHat[sombreroId.Value], hatSocket.position, hatSocket.rotation, hatSocket);
        hatSpawned = true;
    }

    void createPlayerNameHUD()
    {
        if (IsClient)
        {
            playerName = Instantiate(hud.playerNameTemplate, hud.panelHUD).GetComponent<TMP_Text>();
            playerName.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (IsOwner)
        {
            desiredDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            desiredDirection.Normalize();



            if (isAlive()) {
                if (Input.GetButtonDown("Fire1"))
                {
                    fireWeaponRpc();
                }

                //esta variable es la que tiene que activar la animacion de movimientoooooo
                float mag = desiredDirection.magnitude;
                animator.SetBool("isWalking", mag > 0);
                if (mag > 0) {
                    //interpolar entre la rotacion actual y la deseada
                    Quaternion q = Quaternion.LookRotation(desiredDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * 10);


                    //transform.forward = desiredDirection;
                    transform.Translate(0, 0, speed * Time.deltaTime);

                }

                //temporal para probar el sistema de vida
                if (Input.GetKeyDown(KeyCode.T)) {
                    takeDamage(50);
                }
            }

            cam.transform.position = transform.position + cameraOffset;
            cam.transform.LookAt(transform.position + cameraViewOffset);
            hud.labelHealth.text = health.Value + "";

        }

        if (IsClient) {
            Camera maincam = GameObject.Find("Main Camera").GetComponent<Camera>();

            playerName.text = hud.namesList[nameId.Value];
            playerName.transform.position = maincam.WorldToScreenPoint(transform.position + new Vector3(0, 0.4f, 0));

        }

        if (IsServer)
        {
            lastShotTimer += Time.deltaTime;
        }
    }

    //sirve para llamar funciones
    [Rpc(SendTo.Server)]
    public void takeDamageRpc(int amount)
    {
        Debug.Log("rcp recibido takedamage");
        takeDamage(amount);
    }

    
    public void takeDamage(int amount)
    {
        if (!isAlive()) return;

        if (!IsServer)
        {
            takeDamageRpc(amount);
        }
        else {
            health.Value -= amount;
            if (health.Value <= 0) {
                health.Value = 0;
                onDeath();
                Debug.LogWarning("entrooooooooooooooooooo");
            } else
            {
                audioSource.clip = DamageSound;
                audioSource.Play();
            }
        }

    }

    public void onDeath()
    {
        Debug.LogWarning(name + "se murio");
        audioSource.clip = DeathSound;
        audioSource.Play();
    }

    public bool isAlive()
    {
        return health.Value > 0;
    }

    [Rpc(SendTo.Server)]
    public void fireWeaponRpc()
    {
        if (lastShotTimer < weaponCadence) return;

        if (projectilePrefab != null) {
            Projectile proj = Instantiate(projectilePrefab, weaponSocket.position,
                weaponSocket.rotation).GetComponent<Projectile>();

            proj.direction = transform.forward;//sale en la direccion que apunta el personaje
            proj.instigator = this; //quien dispario el proyectil
            proj.GetComponent<NetworkObject>().Spawn();//spawnear el poreycetil en la red para que se replique
            lastShotTimer = 0;
        }
    }

    //enviarle al servidor el ide del nombre seleccionado
    [Rpc(SendTo.Server)]
    public void setNameIDRPC(int idx)
    {
        nameId.Value = idx;
    }

    [Rpc(SendTo.Server)]
    public void setSombreroIDRPC(int idx)
    {
        sombreroId.Value = idx;
    }
}
