using Unity.Netcode;
using UnityEngine;
using TMPro;
using Unity.Netcode.Transports.UTP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Unity.VisualScripting;


public struct NamesData
{
    public string[] names; //el nombre de este campo debe coincidir con el nombre de campo en JSON
}
public class UIMANAGER : MonoBehaviour
{
    [Header("Menus")]
    public RectTransform panelMainMenu;
    public TMP_Dropdown namesSelector;
    public RectTransform panelClient;

    [Header("HUD")]
    public RectTransform panelHUD;
    public TMP_Text labelHealth;
    public GameObject playerNameTemplate;

    //lista de los nombres permitidos
    public List<string> namesList = new List<string>();

    public int selectedNameIndex { 
        get { return namesSelector.value; } 
    }

    public int selectedSombrero;
    void Start()
    {
        panelMainMenu.gameObject.SetActive(true);
        panelClient.gameObject.SetActive(false);
        panelHUD.gameObject.SetActive(true);

        namesSelector.ToString();
        getNames();
    }

    //obtener la lista de nombres permitidos y ponerla en el dropdown
    public void getNames(){
        namesSelector.ClearOptions();
        StartCoroutine(getNamesFromServer());
    }

    //las peticiones web son asincronas por lo que debemos usar una coroutine
    IEnumerator getNamesFromServer() {
        string url = "http://monsterballgo.com/api/names";
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success) { 
            //convertir el cuerpo de la respuesta a un string JSON 
            string json = www.downloadHandler.text;
            NamesData namesData = JsonUtility.FromJson<NamesData>(json);
            namesList.AddRange(namesData.names);//agregar nombres a la lista
            //poner la lista de nombres en el dropdown
            namesSelector.AddOptions(namesList);
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onButtonStartHost()
    {
        //crear una partida hospedada
        NetworkManager.Singleton.StartHost();
        Debug.Log("Match created");
        panelMainMenu.gameObject.SetActive(false);
    }

    public void OnButtonClientConnect()
    {
        GameObject go = GameObject.Find("InputIP");
        string ip = go.GetComponent<TMP_InputField>().text;
        panelMainMenu.gameObject.SetActive(false);
        panelClient.gameObject.SetActive(false);

        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = ip;

        NetworkManager.Singleton.StartClient();
    }
    public void onButtonEndHost() { 
    
    }

    public void onButtonHat(int idx)
    {
        selectedSombrero = idx;
        Debug.Log("sombrero seleccionado: " + idx);
    }
}
