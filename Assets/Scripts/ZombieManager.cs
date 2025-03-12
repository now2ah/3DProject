using UnityEngine;

public class ZombieManager : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerManager>(out PlayerManager player))
            {
                player.BeHit();
            }
        }
    }
}
