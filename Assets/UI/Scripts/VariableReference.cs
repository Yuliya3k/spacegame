using UnityEngine;
using System;
using System.Reflection;

[Serializable]
public class VariableReference
{
    public GameObject targetGameObject;  // The GameObject containing the variable
    public string componentName;         // The name of the script/component
    public string variableName;          // The name of the variable

    private Component cachedComponent;
    private FieldInfo cachedField;
    private PropertyInfo cachedProperty;

    public float GetValue()
    {
        if (cachedComponent == null || (cachedField == null && cachedProperty == null))
        {
            if (targetGameObject == null || string.IsNullOrEmpty(componentName) || string.IsNullOrEmpty(variableName))
            {
                Debug.LogWarning("VariableReference is not fully configured.");
                return 0f;
            }

            // Get the component by name
            cachedComponent = targetGameObject.GetComponent(componentName);
            if (cachedComponent == null)
            {
                Debug.LogWarning($"Component '{componentName}' not found on GameObject '{targetGameObject.name}'.");
                return 0f;
            }

            Type type = cachedComponent.GetType();

            // Try to get the field
            cachedField = type.GetField(variableName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (cachedField == null)
            {
                // Try to get the property if field is not found
                cachedProperty = type.GetProperty(variableName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (cachedProperty == null)
                {
                    Debug.LogWarning($"Variable '{variableName}' not found on component '{componentName}'.");
                    return 0f;
                }
            }
        }

        // Get the value of the variable
        if (cachedField != null)
        {
            object value = cachedField.GetValue(cachedComponent);
            if (value is float floatValue)
                return floatValue;
            else
            {
                Debug.LogWarning($"Variable '{variableName}' is not of type float.");
                return 0f;
            }
        }
        else if (cachedProperty != null)
        {
            object value = cachedProperty.GetValue(cachedComponent, null);
            if (value is float floatValue)
                return floatValue;
            else
            {
                Debug.LogWarning($"Variable '{variableName}' is not of type float.");
                return 0f;
            }
        }
        else
        {
            return 0f;
        }
    }
}
