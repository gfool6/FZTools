using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Avatars.Components;
using EUI = FZTools.EditorUtils.UI;
using ELayout = FZTools.EditorUtils.Layout;
using static FZTools.FZToolsConstants;

namespace FZTools
{
    public class FZBulkMenuCreator : EditorWindow
    {
        [SerializeField] GameObject targetAvatar;
        private ReorderableList reorderableListOuter;
        private ReorderableList reorderableListInner;
        private SerializedObject menuItem;
        private SerializedProperty menuProp;
        private SerializedObject itemItem;

        private class ListMenuItem
        {

        }


        [MenuItem("FZTools/メニュー一括作成")]
        private static void OpenWindow()
        {
            var window = GetWindow<FZBulkMenuCreator>();
            window.titleContent = new GUIContent("メニュー一括作成");
        }

        void OnEnable()
        {
            menuItem = new SerializedObject(ScriptableObject.CreateInstance<FZBulkCreateMenu>());
            menuProp = menuItem.FindProperty("menuItems");
            reorderableListOuter = new ReorderableList(menuItem, menuProp, true, true, true, true);
            reorderableListOuter.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                EditorGUI.PropertyField(rect, menuItem.FindProperty("menuNamme"));
                
                var element = menuProp.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, element);
            };

            // itemItem = new SerializedObject(ScriptableObject.CreateInstance<FZBulkMenuCreatorMenuItem.FZBulkCreateMenu>());
            // reorderableListInner = new ReorderableList(itemItem, itemItem.FindProperty("menuItems"), true, true, true, true);
            // reorderableListInner.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            // {
            //     var element = reorderableListInner.serializedProperty.GetArrayElementAtIndex(index);
            //     EditorGUI.PropertyField(rect, element);
            // };
        }

        private void OnGUI()
        {
            if(menuItem == null)
            {
                return;
            }
            menuItem.Update();
            reorderableListOuter.DoLayoutList();
            menuItem.ApplyModifiedProperties();
            // reorderableListInner.DoLayoutList();
        }

        // TODO 前提となるMAとMA Menu Creator系がない場合、操作できないようにする
        // 
    }
}