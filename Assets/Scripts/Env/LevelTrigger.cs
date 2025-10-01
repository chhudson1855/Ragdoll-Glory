using UnityEngine;
using System.Collections.Generic;
using System.IO; // Add this line
using UnityEditor; // Add this line

public class LevelTrigger : MonoBehaviour
{
    public string PrefabDirectory = "Assets/Stages/Normal";
    public List<GameObject> Prefabs = new List<GameObject>(); // Prefabs list now lives in the LevelManager
    public List<GameObject> PlacedLevels = new List<GameObject>(); // Placed levels list also lives here
    public GameObject TriggerPrefab;
    public StageConfig LastGeneratedConfig;

    public void GenerateLevel(Vector3 position)
    {
        if (Prefabs == null || Prefabs.Count == 0)
        {
            Debug.LogWarning("No prefabs found. Make sure you find the prefabs first.");
            return;
        }

        // Build a candidate list based on CanConnectWith (if it exists)
        List<GameObject> candidates = new List<GameObject>();

        foreach (GameObject prefab in Prefabs)
        {
            if (LastGeneratedConfig.CanConnectWith.Contains(prefab.name) ||
            (LastGeneratedConfig.CanConnectWith.Count == 0 && !LastGeneratedConfig.CannotConnectWith.Contains(prefab.name))
            )
           {
                candidates.Add(prefab);
            }
        }

        // Safety check — no valid prefabs
        if (candidates.Count == 0)
        {
            Debug.LogWarning("No valid prefabs found that can connect with the last generated stage.");
            return;
        }

        int randomIndex = Random.Range(0, candidates.Count);
        GameObject prefabToSpawn = candidates[randomIndex];

        if (prefabToSpawn != null)
        {
            GameObject newLevel = Instantiate(prefabToSpawn, position, Quaternion.identity);
            PlacedLevels.Add(newLevel);
            LastGeneratedConfig = newLevel.GetComponent<StageConfig>();
        }
        else
        {
            Debug.LogError("Prefab at chosen index is null.");
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

    void Start()
    {
        Prefabs = FindAllPrefabs(PrefabDirectory);
        if (Prefabs.Count == 0)
        {
            Debug.LogWarning("No prefabs found in the specified directory: " + PrefabDirectory);
        }
    }

    public void MoveTrigger()
    {
        // Calculate the position for the new trigger
        Vector3 triggerPosition = new Vector3(0, TriggerPrefab.transform.position.y + 10, 0);
        TriggerPrefab.transform.position = triggerPosition;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // This is called by TriggerCollision when the player enters the trigger
        if (other.CompareTag("Player"))
        {
            Vector3 levelSpawnPos = new Vector3(0, transform.position.y, 0);

            GenerateLevel(levelSpawnPos);
            MoveTrigger();
        }
    }
}