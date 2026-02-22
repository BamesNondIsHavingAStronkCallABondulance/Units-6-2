using UnityEngine;

public class InteractableObjects : MonoBehaviour
{
    protected PlayerManager player;
    GameObject interactableImage;
    protected Collider interactableCollider;

    private void OnTriggerEnter(Collider other)
    {

        if (player == null)
        {
            player = other.GetComponent<PlayerManager>();
        }

        if (player != null)
        {
            interactableImage.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (player == null)
        {
            player = other.GetComponent<PlayerManager>();
        }

        if (player != null)
        {
            interactableImage.SetActive(false);
        }
    }
}
