using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class InteractableObjects : MonoBehaviour
{
    public CharacterController player;
    public GameObject interactableImage;
    public Collider interactableCollider;
    public TMP_Text interactableText;


    InputAction interactAction;
    public GameObject interactableObject;
    public static bool openChest = false;
    bool timerDone = false;

    private void Start()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
    }

    private void Update()
    {
        print(openChest);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (player == null)
        {
            player = other.GetComponent<CharacterController>();
        }

        if (player != null)
        {
            interactableImage.SetActive(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (interactAction.IsPressed())
        {
            openChest = true;

            StartCoroutine("timer");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (player == null)
        {
            player = other.GetComponent<CharacterController>();
        }

        if (player != null)
        {
            interactableImage.SetActive(false);
        }
    }

    public IEnumerator timer()
    {
        yield return new WaitForSecondsRealtime(3);

        interactableText.text = "I am now open";
        openChest = false;
    }
}
