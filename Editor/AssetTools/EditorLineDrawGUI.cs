using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MyAssetTools
{
    internal static class EditorLineDrawGUI
    {
        public static void DrawSeparatorHorizontal(int thickness, int padding, Color color)
        {
            var rt = EditorGUILayout.GetControlRect(GUILayout.Height(thickness + padding));
            rt.height = thickness;
            rt.y += padding * 0.75f;
            rt.x += 2;
            rt.width -= 4;
            EditorGUI.DrawRect(rt, color);
        }
    }
}

