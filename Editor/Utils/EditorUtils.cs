using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace FZTools
{
    public class EditorUtils
    {
        public class UI
        {
            public static void Label(string label = "Label", params GUILayoutOption[] opt)
            {
                EditorGUILayout.LabelField(label, EditorStyles.wordWrappedLabel, opt);
            }

            public static void MiniLabel(string label = "Label", params GUILayoutOption[] opt)
            {
                EditorGUILayout.LabelField(label, EditorStyles.wordWrappedMiniLabel, opt);
            }

            public static void BoxLabel(string label = "Label")
            {
                EditorGUILayout.HelpBox(label, MessageType.None);
            }

            public static void ScrollableBoxLabel(ref Vector2 scrollPos, Action uiCalling, params GUILayoutOption[] opt)
            {
                using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos, "box", opt))
                {
                    scrollPos = scrollView.scrollPosition;
                    uiCalling();
                }
            }

            public static void TextField(ref string text, params GUILayoutOption[] opt)
            {
                text = EditorGUILayout.TextField(text, opt);
            }

            public static void ObjectField<T>(ref T target, params GUILayoutOption[] opt) where T : UnityEngine.Object
            {
                target = (T)EditorGUILayout.ObjectField(target, typeof(T), true, opt);
            }

            public static void UpdateDetectObjectField<T>(T target, Action onDetect, params GUILayoutOption[] opt) where T : UnityEngine.Object
            {
                ChangeCheck(() => { ObjectField<T>(ref target, opt); }, onDetect);
            }

            public static void Button(string label = "Button", Action onClick = null, params GUILayoutOption[] opt)
            {
                if (GUILayout.Button(label, opt))
                {
                    onClick?.Invoke();
                }
            }

            public static void LabelButton(string label = "Button", Action onClick = null, params GUILayoutOption[] opt)
            {
                if (GUILayout.Button(label, EditorStyles.label, opt))
                {
                    onClick?.Invoke();
                }
            }

            public static void BoxButton(string label = "Button", Action onClick = null, params GUILayoutOption[] opt)
            {
                if (GUILayout.Button(label, "box", opt))
                {
                    onClick?.Invoke();
                }
            }

            public static void Space(int spaces = 1)
            {
                Layout.Vertical(() =>
                {
                    for (int i = 0; i < spaces; i++)
                    {
                        EditorGUILayout.Space();
                    }
                });
            }

            public static void FlexibleSpace(int spaces = 1)
            {
                Layout.Vertical(() =>
                {
                    for (int i = 0; i < spaces; i++)
                    {
                        GUILayout.FlexibleSpace();
                    }
                });
            }

            public static void Toggle(ref bool val, params GUILayoutOption[] opt)
            {
                val = EditorGUILayout.Toggle(val, opt);
            }

            public static void ToggleWithLabel(ref bool val, string labelText, params GUILayoutOption[] opt)
            {
                val = EditorGUILayout.ToggleLeft(labelText, val, opt);
            }

            public static void Slider(ref float value, float min = 0, float max = 100, params GUILayoutOption[] opt)
            {
                value = EditorGUILayout.Slider(value, min, max, opt);
            }

            public static void Popup<T>(ref T selection, params GUILayoutOption[] opt) where T : Enum
            {
                selection = (T)EditorGUILayout.EnumPopup(selection, opt);
            }

            public static void Popup(ref int selectedIndex, string[] elements, params GUILayoutOption[] opt)
            {
                selectedIndex = EditorGUILayout.Popup(selectedIndex, elements, opt);
            }

            public static void InfoBox(string text)
            {
                EditorGUILayout.HelpBox(text, MessageType.Info);
            }

            public static void CustomBox(Action uiCalling, params GUILayoutOption[] opt)
            {
                using (new EditorGUILayout.VerticalScope("HelpBox", opt))
                {
                    uiCalling();
                }
            }

            public static void ErrorBox(string text)
            {
                EditorGUILayout.HelpBox(text, MessageType.Error);
            }

            public static void RadioButton(ref int selection, string[] labels, params GUILayoutOption[] opt)
            {
                selection = GUILayout.SelectionGrid(selection, labels, 1, EditorStyles.radioButton, opt);
            }

            public static void BoxRadioButton(ref int selection, string[] labels, params GUILayoutOption[] opt)
            {
                selection = GUILayout.SelectionGrid(selection, labels, 1, EditorStyles.miniButton, opt);
            }

            public static void SerializedPropertyField(string varName, SerializedObject serializedObject)
            {
                serializedObject.Update();
                EditorGUILayout.PropertyField(serializedObject.FindProperty(varName), true);
                serializedObject.ApplyModifiedProperties();
            }

            public static void ChangeCheck(Action uiCalling, Action callback)
            {
                using (var checkState = new EditorGUI.ChangeCheckScope())
                {
                    uiCalling();
                    if (checkState.changed)
                    {
                        callback();
                    }
                }
            }

            public static void DisableGroup(bool disabled, Action uiCalling)
            {
                using (new EditorGUI.DisabledScope(disabled))
                {
                    uiCalling();
                }
            }

            public static void ShowProgress(float progress, string title = "", string infoText = "")
            {
                EditorUtility.DisplayProgressBar(title, infoText, progress);
            }
            public static void DismissProgress()
            {
                EditorUtility.ClearProgressBar();
            }
        }
        public class Layout
        {
            public static void Horizontal(Action uiCalling, params GUILayoutOption[] opt)
            {
                using (new EditorGUILayout.HorizontalScope(opt))
                {
                    uiCalling();
                }
            }

            public static void Vertical(Action uiCalling, params GUILayoutOption[] opt)
            {
                using (new EditorGUILayout.VerticalScope(opt))
                {
                    uiCalling();
                }
            }

            public static void Scroll(ref Vector2 scrollPos, Action uiCalling, float height = 0, float width = 0)
            {
                var option = new List<GUILayoutOption>() { GUILayout.ExpandWidth(true) };
                if (height > 0)
                {
                    option.Add(GUILayout.Height(height));
                }
                if (width > 0)
                {
                    option.Remove(GUILayout.ExpandWidth(true));
                    option.Add(GUILayout.Width(width));
                }
                using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos, option.ToArray()))
                {
                    scrollPos = scrollView.scrollPosition;
                    uiCalling();
                }
            }
        }
    }
}