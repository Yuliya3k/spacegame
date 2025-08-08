using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Transform responsesParent;
    public Button responseButtonPrefab;
    public TMP_InputField playerInputField;
    private DialogueLine currentLine;
    private NPCController currentNPC;
    private Dictionary<string, float> savedBlendShapes = new Dictionary<string, float>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(DialogueLine startingLine, NPCController npc)
    {
        if (startingLine == null)
            return;
        currentNPC = npc;
        currentLine = startingLine;
        
        ShowCurrentLine();
        if (currentNPC != null)
        {
            currentNPC.SetInteractionAnimation(currentLine.animationTrigger);
        }
        dialoguePanel.SetActive(true);

        if (playerInputField != null)
        {
            playerInputField.gameObject.SetActive(true);
            playerInputField.text = string.Empty;
            playerInputField.onSubmit.AddListener(HandlePlayerInput);
            playerInputField.ActivateInputField();
        }
        if (InputFreezeManager.instance != null && !InputFreezeManager.instance.IsFrozen)
        {
            InputFreezeManager.instance.FreezePlayerAndCursor();
        }
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        foreach (Transform child in responsesParent)
            Destroy(child.gameObject);
        if (currentNPC != null)
        {
            if (currentNPC.characterStats != null)
            {
                foreach (var kvp in savedBlendShapes)
                {
                    currentNPC.characterStats.SetFacialExpression(kvp.Key, kvp.Value, 0f);
                }
            }
            currentNPC.Unfreeze();
            currentNPC.ReturnToDefaultAnimation();
            currentNPC = null;
            savedBlendShapes.Clear();
        }

        if (InputFreezeManager.instance != null)
        {
            InputFreezeManager.instance.UnfreezePlayerAndCursor();
            // InputFreezeManager.instance.UnfreezePlayerAndCursor();
        }

        if (playerInputField != null)
        {
            playerInputField.onSubmit.RemoveListener(HandlePlayerInput);
            playerInputField.text = string.Empty;
            playerInputField.gameObject.SetActive(false);
        }

    }

    private void HandlePlayerInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return;

        var inventory = currentNPC != null ? currentNPC.GetComponent<NPCInventory>() : null;
        if (inventory == null)
            return;

        string lower = input.ToLower().Trim();
        string[] parts = lower.Split(' ', 2);
        if (parts.Length < 2)
        {
            dialogueText.text = "Sorry, I do not have this item.";
            playerInputField.text = string.Empty;
            return;
        }

        string verb = parts[0];
        string keyword = parts[1];
        ItemData item = inventory.FindItemByKeyword(keyword);

        if (item == null)
        {
            dialogueText.text = "Sorry, I do not have this item.";
        }
        else
        {
            if (verb == "wear")
            {
                inventory.EquipItem(item);
                dialogueText.text = $"Wearing {item.objectName}.";
            }
            else if (verb == "eat")
            {
                inventory.EatItem(item);
                dialogueText.text = $"Eating {item.objectName}.";
            }
            else
            {
                dialogueText.text = "Sorry, I do not have this item.";
            }
        }

        playerInputField.text = string.Empty;
        playerInputField.ActivateInputField();

    }

    private void ShowCurrentLine()
    {
        dialogueText.text = currentLine.lineText;

        if (currentNPC != null)
        {
            currentNPC.SetInteractionAnimation(currentLine.animationTrigger);
            if (currentLine.facialExpressions != null)
            {
                foreach (var exp in currentLine.facialExpressions)
                {
                    SaveBlendShape(exp.blendShapeName);
                    currentNPC.characterStats.SetFacialExpression(exp.blendShapeName, exp.value, exp.durationInGameMinutes);
                }
            }
        }

        foreach (Transform child in responsesParent)
            Destroy(child.gameObject);

        if (currentLine.responses == null || currentLine.responses.Length == 0)
        {
            Button endButton = Instantiate(responseButtonPrefab, responsesParent);
            endButton.GetComponentInChildren<TextMeshProUGUI>().text = "Close";
            endButton.onClick.AddListener(EndDialogue);
            return;
        }

        foreach (DialogueResponse response in currentLine.responses)
        {
            DialogueResponse localResponse = response;
            Button button = Instantiate(responseButtonPrefab, responsesParent);
            button.GetComponentInChildren<TextMeshProUGUI>().text = localResponse.responseText;
            button.onClick.AddListener(() => OnResponseSelected(localResponse));
        }
    }

    private void OnResponseSelected(DialogueResponse response)
    {
        if (currentNPC != null)
        {
            if (!string.IsNullOrEmpty(response.animationTrigger))
            {
                currentNPC.SetInteractionAnimation(response.animationTrigger);
            }

            if (response.facialExpressions != null)
            {
                foreach (var exp in response.facialExpressions)
                {
                    SaveBlendShape(exp.blendShapeName);
                    currentNPC.characterStats.SetFacialExpression(exp.blendShapeName, exp.value, exp.durationInGameMinutes);
                }
            }
        }

        if (response.nextLine == null)
        {
            EndDialogue();
        }
        else
        {
            currentLine = response.nextLine;
            ShowCurrentLine();
        }
    }

    private void SaveBlendShape(string blendShapeName)
    {
        if (currentNPC == null || currentNPC.characterStats == null)
            return;

        if (!savedBlendShapes.ContainsKey(blendShapeName))
        {
            float value = currentNPC.characterStats.GetBlendShapeValue(blendShapeName);
            savedBlendShapes[blendShapeName] = value;
        }
    }

}