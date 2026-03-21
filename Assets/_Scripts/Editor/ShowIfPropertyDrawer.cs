using UnityEditor;
using UnityEngine;

/// <summary>
/// PropertyDrawer dla atrybutu ShowIf — pokazuje pole warunkowo w inspektorze.
/// </summary>
[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;

        // Znajdź pole warunkowe w tej samej klasie
        SerializedProperty conditionProperty = property.serializedObject.FindProperty(showIf.ConditionFieldName);

        if (conditionProperty != null)
        {
            // Porównaj wartość warunkową z wartością pola warunkowego
            bool shouldShow = CompareValues(conditionProperty, showIf.ConditionValue);

            if (shouldShow)
            {
                // Jeśli warunek spełniony, narysuj pole
                EditorGUI.PropertyField(position, property, label, true);
            }
        }
        else
        {
            // Jeśli nie znaleziono pola warunkowego, pokaż ostrzeżenie
            EditorGUI.LabelField(position, label.text, "Condition field not found!");
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;
        SerializedProperty conditionProperty = property.serializedObject.FindProperty(showIf.ConditionFieldName);

        if (conditionProperty != null)
        {
            bool shouldShow = CompareValues(conditionProperty, showIf.ConditionValue);
            if (shouldShow)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }
            else
            {
                return 0; // Ukryj pole — nie zajmuje miejsca
            }
        }

        return EditorGUIUtility.singleLineHeight;
    }

    private bool CompareValues(SerializedProperty conditionProperty, object conditionValue)
    {
        switch (conditionProperty.propertyType)
        {
            case SerializedPropertyType.Enum:
                return conditionProperty.enumValueIndex == (int)conditionValue;

            case SerializedPropertyType.Integer:
                return conditionProperty.intValue == (int)conditionValue;

            case SerializedPropertyType.Boolean:
                return conditionProperty.boolValue == (bool)conditionValue;

            case SerializedPropertyType.String:
                return conditionProperty.stringValue == (string)conditionValue;

            default:
                return false;
        }
    }
}
