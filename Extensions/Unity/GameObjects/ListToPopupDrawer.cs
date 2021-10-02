#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Svelto.ECS;
using Svelto.ECS.Extensions.Unity;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ListToPopupAttribute))]
public class ListToPopupDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ListToPopupAttribute atb        = attribute as ListToPopupAttribute;
        List<Type>           stringList = null;

        if (atb.classType.GetField(atb.listName, BindingFlags.Static | BindingFlags.NonPublic) != null)
        {
            stringList = atb.classType.GetField(atb.listName, BindingFlags.Static | BindingFlags.NonPublic).GetValue(atb.classType) as List<Type>;
        }

        if (stringList != null && stringList.Count != 0)
        {
            int selectedIndex = Mathf.Max(0, stringList.FindIndex(t => t.Name == property.stringValue)); 
            
            selectedIndex = EditorGUI.Popup(position, property.name, selectedIndex, stringList.Select(t => t.Name).ToArray());

            property.stringValue = stringList[selectedIndex].Name;
            (property.serializedObject.targetObject as EntityDescriptorHolder).type =
                Activator.CreateInstance(stringList[selectedIndex]) as IEntityDescriptor;
        }
        else
        {
            EditorGUI.TextArea(position, "Error - no valid entity descriptors found");
        }
    }
}
#endif