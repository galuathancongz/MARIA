using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Luzart
{
    /// <summary>
    /// Property drawer for conditional attributes (ShowIf, HideIf, EnableIf, DisableIf, ShowIfAny, ShowIfAll)
    /// Supports path navigation: "../" to go up levels, "." to go deeper into properties
    /// Examples:
    /// - "propertyName" - sibling property
    /// - "../propertyName" - property in parent object
    /// - "../../typeAnimation" - property two levels up
    /// - "subObject.propertyName" - nested property
    /// </summary>
    [CustomPropertyDrawer(typeof(ConditionalPropertyAttribute), true)]
    [CustomPropertyDrawer(typeof(ShowIfAnyAttribute), true)]
    [CustomPropertyDrawer(typeof(ShowIfAllAttribute), true)]
    public class ConditionalPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool conditionMet = EvaluateAllConditions(property);
            bool shouldShow = ShouldShowProperty(conditionMet);
            bool shouldEnable = ShouldEnableProperty(conditionMet);

            if (!shouldShow)
                return;

            var wasEnabled = GUI.enabled;
            GUI.enabled = shouldEnable;
            
            EditorGUI.PropertyField(position, property, label, true);
            
            GUI.enabled = wasEnabled;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            bool conditionMet = EvaluateAllConditions(property);
            bool shouldShow = ShouldShowProperty(conditionMet);

            if (!shouldShow)
                return -EditorGUIUtility.standardVerticalSpacing;

            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        private bool EvaluateAllConditions(SerializedProperty property)
        {
            if (attribute is ConditionalPropertyAttribute conditionalAttribute)
            {
                return EvaluateCondition(property, conditionalAttribute.SiblingPropertyPath, conditionalAttribute.ConditionValue);
            }
            else if (attribute is ShowIfAnyAttribute showIfAnyAttribute)
            {
                bool anyMet = false;
                for (int i = 0; i < showIfAnyAttribute.PropertyPaths.Length; i++)
                {
                    if (EvaluateCondition(property, showIfAnyAttribute.PropertyPaths[i], showIfAnyAttribute.ConditionValues[i]))
                    {
                        anyMet = true;
                        break;
                    }
                }
                return anyMet;
            }
            else if (attribute is ShowIfAllAttribute showIfAllAttribute)
            {
                bool allMet = true;
                for (int i = 0; i < showIfAllAttribute.PropertyPaths.Length; i++)
                {
                    if (!EvaluateCondition(property, showIfAllAttribute.PropertyPaths[i], showIfAllAttribute.ConditionValues[i]))
                    {
                        allMet = false;
                        break;
                    }
                }
                return allMet;
            }

            return true;
        }

        private bool EvaluateCondition(SerializedProperty property, string path, object conditionValue)
        {
            try
            {
                var targetProperty = GetTargetProperty(property, path);
                object currentValue = null;
                
                if (targetProperty != null)
                {
                    // Found as SerializedProperty
                    currentValue = GetPropertyValue(targetProperty);
                }
                else
                {
                    // Try to get as computed property using reflection
                    currentValue = GetComputedPropertyValue(property, path);
                }

                if (currentValue == null)
                {
                    Debug.LogWarning($"ConditionalPropertyDrawer: Property not found at path '{path}' from '{property.propertyPath}'");
                    return false;
                }

                bool isEqual = AreValuesEqual(currentValue, conditionValue);
                
                return isEqual;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"ConditionalPropertyDrawer: Failed to evaluate condition for path '{path}': {ex.Message}");
                return false;
            }
        }

        private object GetComputedPropertyValue(SerializedProperty sourceProperty, string path)
        {
            try
            {
                // Get the target object that contains the property
                var targetObject = sourceProperty.serializedObject.targetObject;
                var propertyPath = sourceProperty.propertyPath;
                
                // Navigate to the correct object level based on the property path
                var pathParts = propertyPath.Split('.');
                object currentObject = targetObject;
                
                // Navigate to the parent object of the current property
                for (int i = 0; i < pathParts.Length - 1; i++)
                {
                    var part = pathParts[i];
                    if (part.Contains("[") && part.Contains("]"))
                    {
                        currentObject = GetArrayElement(currentObject, part);
                    }
                    else
                    {
                        currentObject = GetFieldOrPropertyValue(currentObject, part);
                    }
                    
                    if (currentObject == null)
                        return null;
                }
                
                // Now try to get the computed property from the current object
                return GetFieldOrPropertyValue(currentObject, path);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"ConditionalPropertyDrawer: Failed to get computed property value for '{path}': {ex.Message}");
                return null;
            }
        }

        private SerializedProperty GetTargetProperty(SerializedProperty sourceProperty, string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            
            // Start from the current property's path
            string currentPath = sourceProperty.propertyPath;
            var pathParts = path.Split('/');
            
            // Process navigation parts
            for (int i = 0; i < pathParts.Length; i++)
            {
                var part = pathParts[i];
                
                if (part == "..")
                {
                    // Navigate up one level
                    currentPath = NavigateUpPath(currentPath);
                }
                else if (!string.IsNullOrEmpty(part))
                {
                    // This is a property name - it should be at the same level as currentPath
                    // Get the parent of currentPath and append the new property name
                    string parentPath = NavigateUpPath(currentPath);
                    
                    if (string.IsNullOrEmpty(parentPath))
                    {
                        // We're at root level
                        currentPath = part;
                    }
                    else
                    {
                        currentPath = parentPath + "." + part;
                    }
                }
            }

            // Find the property at the final path
            if (string.IsNullOrEmpty(currentPath))
            {
                return null;
            }

            var targetProperty = sourceProperty.serializedObject.FindProperty(currentPath);
            
            // If not found and currentPath doesn't contain dots, it might be a direct property of the serialized object
            if (targetProperty == null && !currentPath.Contains("."))
            {
                // Try to find it directly in the serialized object
                var so = sourceProperty.serializedObject;
                var iterator = so.GetIterator();
                if (iterator.NextVisible(true))
                {
                    do
                    {
                        if (iterator.name == currentPath)
                        {
                            targetProperty = iterator.Copy();
                            break;
                        }
                    }
                    while (iterator.NextVisible(false));
                }
            }
            if (targetProperty == null)
            {
                return null;
            }
            return targetProperty;
        }

        private string NavigateUpPath(string propertyPath)
        {
            if (string.IsNullOrEmpty(propertyPath))
                return string.Empty;

            int lastDotIndex = propertyPath.LastIndexOf('.');
            if (lastDotIndex == -1)
            {
                // Already at root level
                return string.Empty;
            }

            return propertyPath.Substring(0, lastDotIndex);
        }

        private object GetPropertyValue(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    return property.boolValue;
                case SerializedPropertyType.Integer:
                    return property.intValue;
                case SerializedPropertyType.Float:
                    return property.floatValue;
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Enum:
                    return property.enumValueIndex;
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue;
                case SerializedPropertyType.Vector2:
                    return property.vector2Value;
                case SerializedPropertyType.Vector3:
                    return property.vector3Value;
                case SerializedPropertyType.Vector4:
                    return property.vector4Value;
                case SerializedPropertyType.Color:
                    return property.colorValue;
                case SerializedPropertyType.LayerMask:
                    return property.intValue;
                case SerializedPropertyType.Rect:
                    return property.rectValue;
                case SerializedPropertyType.AnimationCurve:
                    return property.animationCurveValue;
                case SerializedPropertyType.Bounds:
                    return property.boundsValue;
                default:
                    // For complex types, try reflection approach
                    return GetComplexPropertyValue(property);
            }
        }

        private object GetComplexPropertyValue(SerializedProperty property)
        {
            try
            {
                var targetObject = property.serializedObject.targetObject;
                var propertyPath = property.propertyPath;
                
                return GetValueFromPath(targetObject, propertyPath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"ConditionalPropertyDrawer: Failed to get complex property value for '{property.propertyPath}': {ex.Message}");
                return null;
            }
        }

        private object GetValueFromPath(object obj, string path)
        {
            if (obj == null || string.IsNullOrEmpty(path))
                return null;

            var parts = path.Split('.');
            object current = obj;

            foreach (var part in parts)
            {
                if (current == null)
                    return null;

                // Handle array elements
                if (part.Contains("[") && part.Contains("]"))
                {
                    current = GetArrayElement(current, part);
                }
                else
                {
                    current = GetFieldOrPropertyValue(current, part);
                }
            }

            return current;
        }

        private object GetArrayElement(object obj, string propertyWithIndex)
        {
            try
            {
                var openBracket = propertyWithIndex.IndexOf('[');
                var closeBracket = propertyWithIndex.IndexOf(']');
                
                if (openBracket == -1 || closeBracket == -1)
                    return null;

                var propertyName = propertyWithIndex.Substring(0, openBracket);
                var indexStr = propertyWithIndex.Substring(openBracket + 1, closeBracket - openBracket - 1);
                
                if (!int.TryParse(indexStr, out int index))
                    return null;

                var arrayObj = GetFieldOrPropertyValue(obj, propertyName);
                if (arrayObj is IList list && index >= 0 && index < list.Count)
                    return list[index];

                return null;
            }
            catch
            {
                return null;
            }
        }

        private object GetFieldOrPropertyValue(object obj, string name)
        {
            if (obj == null || string.IsNullOrEmpty(name))
                return null;

            var type = obj.GetType();
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            // Try field first
            var field = type.GetField(name, flags);
            if (field != null)
                return field.GetValue(obj);

            // Try property
            var property = type.GetProperty(name, flags);
            if (property != null && property.CanRead)
                return property.GetValue(obj);

            return null;
        }

        private bool AreValuesEqual(object value1, object value2)
        {
            if (value1 == null && value2 == null)
                return true;
            if (value1 == null || value2 == null)
                return false;

            // Handle enum comparisons
            if (value1 is int enumValue1 && value2 is Enum enumValue2)
                return enumValue1 == Convert.ToInt32(enumValue2);
            if (value1 is Enum enumValue3 && value2 is int enumValue4)
                return Convert.ToInt32(enumValue3) == enumValue4;

            // Handle Unity Object references (including null checks)
            if (value1 is UnityEngine.Object unityObj1)
            {
                if (value2 == null)
                    return unityObj1 == null;
                if (value2 is UnityEngine.Object unityObj2)
                    return unityObj1 == unityObj2;
                return false;
            }

            // Special handling for checking null against UnityEngine.Object
            if (value2 is UnityEngine.Object unityObj3)
            {
                if (value1 == null)
                    return unityObj3 == null;
                return false;
            }

            // Handle string comparisons (case-sensitive by default)
            if (value1 is string str1 && value2 is string str2)
                return string.Equals(str1, str2);

            return value1.Equals(value2);
        }

        private bool ShouldShowProperty(bool conditionMet)
        {
            return attribute switch
            {
                ShowIfAttribute => conditionMet,
                ShowIfAnyAttribute => conditionMet,
                ShowIfAllAttribute => conditionMet,
                HideIfAttribute => !conditionMet,
                _ => true
            };
        }

        private bool ShouldEnableProperty(bool conditionMet)
        {
            return attribute switch
            {
                EnableIfAttribute => conditionMet,
                DisableIfAttribute => !conditionMet,
                _ => GUI.enabled
            };
        }
    }
}