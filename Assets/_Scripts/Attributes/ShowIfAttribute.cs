using UnityEngine;

/// <summary>
/// Atrybut warunkowy — pokazuje pole w inspektorze tylko gdy warunek jest spełniony.
/// Użycie: [ShowIf(nameof(fieldName), value)]
/// </summary>
public class ShowIfAttribute : PropertyAttribute
{
    public string ConditionFieldName { get; private set; }
    public object ConditionValue { get; private set; }

    public ShowIfAttribute(string conditionFieldName, object conditionValue)
    {
        ConditionFieldName = conditionFieldName;
        ConditionValue = conditionValue;
    }
}
