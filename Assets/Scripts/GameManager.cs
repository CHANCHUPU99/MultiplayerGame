using UnityEngine;
using System.Collections.Generic;
public class GameManager : MonoBehaviour
{
    public List<Transform> spawnPoints= new List<Transform>();
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public Vector3 getSpawnPoint()
    {
        if (spawnPoints.Count > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Count);
            return spawnPoints[randomIndex].position;
        }
        else
        {
            Debug.Log("no spawnPoint available");
            return Vector3.zero;
        }
    }
}
