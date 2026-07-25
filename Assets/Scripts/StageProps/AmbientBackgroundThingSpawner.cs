using UnityEngine;

public class AmbientBackgroundThingSpawner : MonoBehaviour
{
    [Header("Sea Dragons")]
    [SerializeField] GameObject RightSeaDragonPrefab;
    [SerializeField] Vector2 SeaDragon_SpawnInterval = new(20, 30);
    [SerializeField] Vector2 SeaDragon_SpawnHeight = new(-5, 5);

    [Header("Fishies")]
    [SerializeField] GameObject FishiesPrefab;
    [SerializeField] Vector2 Fishies_SpawnInterval = new Vector2(10, 15);
    [SerializeField] Vector2 Fishies_SpawnHeight = new(-5, 5);

    float timeToNextSeaDragon = 0;
    GameObject spawnedSeaDragon;

    float timeToNextFishy = 0;
    // Update is called once per frame
    void Update()
    {
        // Sea dragons
        if (spawnedSeaDragon == null) 
            timeToNextSeaDragon -= Time.deltaTime;
        if (timeToNextSeaDragon < 0)
        {
            SpawnSeaDragon();
            timeToNextSeaDragon = Random.Range(SeaDragon_SpawnInterval.x, SeaDragon_SpawnInterval.y);
        }

        // Fishies
        if (spawnedSeaDragon == null)
            timeToNextSeaDragon -= Time.deltaTime;
        if (timeToNextSeaDragon < 0)
        {
            SpawnSeaDragon();
            timeToNextSeaDragon = Random.Range(SeaDragon_SpawnInterval.x, SeaDragon_SpawnInterval.y);
        }
    }

    void SpawnSeaDragon()
    {
        spawnedSeaDragon = Instantiate(RightSeaDragonPrefab, new(transform.position.x, Random.Range(SeaDragon_SpawnHeight.x, SeaDragon_SpawnHeight.y)), Quaternion.identity);
        spawnedSeaDragon.transform.SetParent(transform, false);
    }
}
