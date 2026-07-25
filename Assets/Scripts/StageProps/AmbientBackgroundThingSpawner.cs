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

    float timeToNextSeaDragon = 10;
    GameObject spawnedSeaDragon;

    float timeToNextFishy = 0;
    GameObject spawnedFishy;
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
        if (spawnedFishy == null)
            timeToNextFishy -= Time.deltaTime;
        if (timeToNextFishy < 0)
        {
            SpawnFishy();
            timeToNextFishy = Random.Range(Fishies_SpawnInterval.x, Fishies_SpawnInterval.y);
        }
    }

    void SpawnSeaDragon()
    {
        spawnedSeaDragon = Instantiate(RightSeaDragonPrefab, new(transform.position.x, Random.Range(SeaDragon_SpawnHeight.x, SeaDragon_SpawnHeight.y)), Quaternion.identity);
        spawnedSeaDragon.transform.SetParent(transform, false);
    }

    void SpawnFishy()
    {
        spawnedFishy = Instantiate(FishiesPrefab, new(transform.position.x, Random.Range(Fishies_SpawnHeight.x, Fishies_SpawnHeight.y)), Quaternion.identity);
        spawnedFishy.transform.SetParent(transform, false);
    }
}
