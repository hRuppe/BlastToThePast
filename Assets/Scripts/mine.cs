using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mine : MonoBehaviour
{
    [Header("--- Mine Stats ---")]
    [SerializeField] int damage;

    [Header("--- Components ---")]
    [SerializeField] GameObject explosionEffect;
    [SerializeField] AudioSource audioSource;

    [Header("--- Audio ---")]
    [SerializeField] AudioClip audExplosion;
    [Range(0, 1)][SerializeField] float audExplosionVol;
    [SerializeField] AudioClip audBeeping;
    [Range(0, 1)][SerializeField] float audBeepingVol;

    bool isExploding;

    void Update()
    {
        if (gameObject.activeSelf && !audioSource.isPlaying)
        {
            audioSource.clip = audBeeping;
            audioSource.volume = audBeepingVol;
            audioSource.Play();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(BlowUp(other));
    }

    IEnumerator BlowUp(Collider other)
    {
        if (other.gameObject.tag == "Enemy" && other == other.GetComponent<CapsuleCollider>() && !isExploding)
        {
            isExploding = true;
            Instantiate(explosionEffect, transform.position, transform.rotation);
            audioSource.Stop();
            audioSource.PlayOneShot(audExplosion, audExplosionVol);
            other.GetComponent<IDamage>().TakeDamage(damage);
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            yield return new WaitForSeconds(audExplosion.length);
            isExploding = false;
            Destroy(gameObject);
        }
    }
}