using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fire : MonoBehaviour
{
    [Header("----- Fire Stats -----")]
    [SerializeField] float fireDamageDelay;
    [SerializeField] int fireDuration;

    bool playerInFire;
    bool burning;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, fireDuration);
        burning = false;
    }

    private void Update()
    {
        if (playerInFire && !burning)
            StartCoroutine(Burn());
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInFire = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInFire = false;
    }

    public IEnumerator Burn()
    {
        burning = true;
        gameManager.instance.playerScript.damage(1);

        yield return new WaitForSeconds(fireDamageDelay);
        burning = false;
    }
}
