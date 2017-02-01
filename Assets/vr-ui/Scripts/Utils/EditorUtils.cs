using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
//using UnityEditor;
using UnityEngine;

public class EditorUtils : MonoBehaviour
{

    public static void SetGameObjectIcon(GameObject gameObject, int idx)
    {
        /*var largeIcons = GetTextures("sv_label_", string.Empty, 0, 8);
        var icon = largeIcons[idx];
        var egu = typeof(EditorGUIUtility);
        var flags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
        var args = new object[] { gameObject, icon.image };
        var setIcon = egu.GetMethod("SetIconForObject", flags, null, new Type[] { typeof(UnityEngine.Object), typeof(Texture2D) }, null);
        setIcon.Invoke(null, args);*/
    }

    private static GUIContent[] GetTextures(string baseName, string postFix, int startIndex, int count)
    {
        return null;
        /*GUIContent[] array = new GUIContent[count];
        for (int i = 0; i < count; i++)
        {
            array[i] = EditorGUIUtility.IconContent(baseName + (startIndex + i) + postFix);
        }
        return array;*/
    }
}
