using UnityEngine;
using System.Collections.Generic;
using System.IO; // Add this line
using UnityEditor;
using JetBrains.Annotations;
using UnityEngine.SocialPlatforms; // Add this line

public class LevelTrigger : MonoBehaviour
{
    public string PrefabDirectory = "Assets/Stages/Normal";
    public List<GameObject> Prefabs = new List<GameObject>(); // Prefabs list now lives in the LevelManager
    public List<GameObject> PlacedLevels = new List<GameObject>(); // Placed levels list also lives here
    public GameObject TriggerPrefab;
    public GameObject LastGenerated;
    public StageConfig LastGeneratedConfig;
    GameObject[] itemPrefabs;

    public void GenerateLevel(Vector3 position)
    {
        if (Prefabs == null || Prefabs.Count == 0)
        {
            Debug.LogWarning("No prefabs found. Make sure you find the prefabs first.");
            return;
        }

        // Build a candidate list based on CanConnectWith (if it exists)
        List<GameObject> candidates = new List<GameObject>();

        if (LastGeneratedConfig && LastGeneratedConfig.CanConnectWith.Count > 0)
        {
            foreach (GameObject prefab in Prefabs)
            {
                if (LastGeneratedConfig.CanConnectWith.Contains(prefab.name)
                    && prefab.name != LastGenerated.name)
                {
                    Debug.Log("Fart" + LastGeneratedConfig.CanConnectWith.Contains(prefab.name) + " " + prefab.name != LastGenerated.name);
                    candidates.Add(prefab);
                }
            }
        }
        else if (LastGeneratedConfig && LastGeneratedConfig.CannotConnectWith.Count > 0)
        {
            foreach (GameObject prefab in Prefabs)
            {
                if (!LastGeneratedConfig.CannotConnectWith.Contains(prefab.name)
                    && prefab.name != LastGenerated.name)
                {
                    Debug.Log((!LastGeneratedConfig.CannotConnectWith.Contains(prefab.name)) + " - " + (prefab.name != LastGenerated.name));
                    candidates.Add(prefab);
                }
            }
        }
        else
        {
            foreach (GameObject prefab in Prefabs)
            {
                candidates.Add(prefab);
            }
        }

        // Safety check — no valid prefabs
        if (candidates.Count == 0)
        {
            Debug.LogWarning("No valid prefabs found that can connect with the last generated stage.");
            GenerateLevel(position); // Try again
            return;
        }

        int randomIndex = Random.Range(0, candidates.Count);
        GameObject prefabToSpawn = candidates[randomIndex];

        if (prefabToSpawn != null)
        {
            GameObject newLevel = Instantiate(prefabToSpawn, position, Quaternion.identity);
            PlacedLevels.Add(newLevel);
            LastGeneratedConfig = newLevel.GetComponent<StageConfig>();
            LastGenerated = prefabToSpawn;

            foreach (Transform child in newLevel.transform)
            {
                if (child.name == "ItemSpawn")
                {
                    // Pick a random prefab
                    GameObject randomItem = itemPrefabs[Random.Range(0, itemPrefabs.Length)];

                    // Spawn it at the child’s position/rotation
                    Instantiate(randomItem, child.position, child.rotation);

                    Debug.Log($"Spawned {randomItem.name} at {child.name}");
                }
            }
        }
        else
        {
            Debug.LogError("Prefab at chosen index is null.");
            GenerateLevel(position); // Try again
        }
    }


    public static List<GameObject> FindAllPrefabs(string prefabDirectory)
    {
        List<GameObject> prefabs = new List<GameObject>();

        if (!Directory.Exists(prefabDirectory))
        {
            Debug.LogError("Prefab directory not found: " + prefabDirectory);
            return prefabs;
        }

        string[] filePaths = Directory.GetFiles(prefabDirectory, "*.prefab", SearchOption.AllDirectories);

        foreach (string filePath in filePaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);

            if (prefab != null && prefab.GetComponent<StageConfig>().CanPlace)
            {
                prefabs.Add(prefab);
            }
            else
            {
                Debug.LogWarning("Failed to load prefab at: " + prefabDirectory);
            }
        }

        Debug.Log("Found " + prefabs.Count + " prefabs in " + prefabDirectory);
        return prefabs;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Prefabs = FindAllPrefabs(PrefabDirectory);
        itemPrefabs = Resources.LoadAll<GameObject>("Items");
        Debug.Log(itemPrefabs.Length + " item prefabs found in Resources/Items");
        if (Prefabs.Count == 0)
        {
            Debug.LogWarning("No prefabs found in the specified directory: " + PrefabDirectory);
        }

        Vector3 levelSpawnPos = new Vector3(0, transform.position.y, 0);
            
        for(int i = 0; i < 200; i++)
        {
            GenerateLevel(new Vector3(0, -9+10, -0) + (i * new Vector3(0, 10, 0))); // Spawn new level 20 units above the trigger
        }
       
        
    }
    // Update is called once per frame

    void OnTriggerEnter2D(Collider2D other)
    {
        // This is called by TriggerCollision when the player enters the trigger
        if (other.CompareTag("Player"))
        {
            Vector3 levelSpawnPos = new Vector3(0, transform.position.y, 0);
            
            GenerateLevel(levelSpawnPos + new Vector3(0, 10, 0)); // Spawn new level 20 units above the trigger

            Vector3 triggerPosition = new Vector3(0, TriggerPrefab.transform.position.y + 10, 0);
            TriggerPrefab.transform.position = triggerPosition;
        }
        
    }
}