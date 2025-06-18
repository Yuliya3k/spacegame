using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using TMPro;

public class RebindActionUI : MonoBehaviour
{
    public InputActionReference actionReference; // Reference to the Input Action
    public int bindingIndex; // Index of the binding within the action
    public Button rebindButton; // Button to initiate rebinding
    public TextMeshProUGUI actionNameText; // Displays the action name
    public TextMeshProUGUI bindingDisplayNameText; // Displays the current binding

    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

    private void OnEnable()
    {
        LoadBindingOverride();
        UpdateBindingDisplay();
    }

    public void StartRebinding()
    {
        rebindButton.interactable = false;
        bindingDisplayNameText.text = "Press a key...";

        InputAction action = actionReference.action;

        rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Mouse>/position") // Exclude mouse position
            .WithControlsExcluding("<Mouse>/delta")    // Exclude mouse delta
            .OnMatchWaitForAnother(0.1f)              // Wait for stable input
            .OnComplete(operation => RebindingComplete())
            .Start();
    }

    private void RebindingComplete()
    {
        rebindingOperation.Dispose();
        rebindButton.interactable = true;

        SaveBindingOverride();
        UpdateBindingDisplay();
    }

    private void UpdateBindingDisplay()
    {
        InputAction action = actionReference.action;

        if (action != null)
        {
            var binding = action.bindings[bindingIndex];
            string displayString = action.GetBindingDisplayString(bindingIndex);

            bindingDisplayNameText.text = displayString;
            actionNameText.text = action.name;
        }
    }

    private void SaveBindingOverride()
    {
        InputAction action = actionReference.action;

        string bindingOverrideJson = action.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(action.actionMap.name + action.name, bindingOverrideJson);
        PlayerPrefs.Save();
    }

    public void LoadBindingOverride()
    {
        InputAction action = actionReference.action;

        string bindingOverrideJson = PlayerPrefs.GetString(action.actionMap.name + action.name, string.Empty);
        if (!string.IsNullOrEmpty(bindingOverrideJson))
        {
            action.LoadBindingOverridesFromJson(bindingOverrideJson);
        }
    }

    public void ResetBinding()
    {
        InputAction action = actionReference.action;
        action.RemoveAllBindingOverrides();
        SaveBindingOverride();
        UpdateBindingDisplay();
    }
}
