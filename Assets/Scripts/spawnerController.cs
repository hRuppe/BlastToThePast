using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnerController : MonoBehaviour
{
    [Header("----- Spawner Stats -----")]
    [SerializeField] GameObject enemy;
    [SerializeField] int enemiesToSpawn;
    [SerializeField] int spawnDelay;

    [Header("----- Spawner Components -----")]
    [SerializeField] Transform spawnPos;

    private bool isSpawning;
    private bool startSpawning;
    private int enemiesSpawned;

    void Start()
    {
        gameManager.instance.updateUI(enemiesToSpawn);
    }

    void Update()
    {
        if (startSpawning && !isSpawning && enemiesSpawned < enemiesToSpawn)
            StartCoroutine(Spawn());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            startSpawning = true;
    }

    IEnumerator Spawn()
    {
        isSpawning = true;

        enemiesSpawned++;

        Instantiate(enemy, spawnPos.position, enemy.transform.rotation);

        yield return new WaitForSeconds(spawnDelay);

        isSpawning = false;
    }
}

