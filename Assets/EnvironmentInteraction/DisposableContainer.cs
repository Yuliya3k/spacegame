using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DisposableContainer : MonoBehaviour, IInteractable
{
    [Header("Container Settings")]
    public List<ContainerItem> containerItems; // List of items with quantity stored in this container
    public float fadeDuration = 2f; // Duration for the fading effect

    private bool isFading = false; // To prevent multiple triggers

    public string objectName = "Default Object Name";
    public string locationText = "Default Location";
    public string containerID; // Assign a unique ID in the Editor

    private void Awake()
    {
        if (string.IsNullOrEmpty(containerID))
        {
            containerID = System.Guid.NewGuid().ToString();
        }
    }

    // Method called when the player interacts with the container
    public void Interact()
    {
        if (!isFading)
        {
            AddItemsToInventory();
            StartCoroutine(FadeOutAndDestroy());
        }
    }

    // Add all items in the container to the player's inventory
    private void AddItemsToInventory()
    {
        foreach (ContainerItem containerItem in containerItems)
        {
            PlayerInventory.instance.AddItem(containerItem.itemData, containerItem.quantity);
            Debug.Log($"Added {containerItem.quantity} of {containerItem.itemData.objectName} to Inventory.");
        }
    }

    // Coroutine to handle fading and destroying the container
    private IEnumerator FadeOutAndDestroy()
    {
        isFading = true;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Color[] originalColors = new Color[renderers.Length];

        // Store original colors
        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].material.color;
        }

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);

            for (int i = 0; i < renderers.Length; i++)
            {
                Color color = originalColors[i];
                color.a = alpha;
                renderers[i].material.color = color;
            }

            yield return null;
        }

        Destroy(gameObject); // Finally destroy the container
    }


    private void OnEnable()
    {
        SaveSystem.RegisterDisposableContainer(this);
    }

    private void OnDisable()
    {
        SaveSystem.UnregisterDisposableContainer(this);
    }

}
