using System.Diagnostics;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditorInternal;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Avatars.Components;
using EUI = FZTools.EditorUtils.UI;
using ELayout = FZTools.EditorUtils.Layout;
using static FZTools.FZToolsConstants;
using System.Threading.Tasks;
using nadena.dev.modular_avatar.core;

namespace FZTools
{
    public class FZBulkMenuCreator : EditorWindow
    {
        [SerializeField] GameObject targetAvatar;
        [SerializeField] List<FZBulkCreateMenu> menues = new List<FZBulkCreateMenu>();
        [SerializeField] UnityEngine.Object menuesTemplateFile;
        Vector2 scrollPos;

        string MenueOutputPath => $"{AssetUtils.OutputRootPath(targetAvatar?.gameObject?.name)}/BulkMenuCreator";

        bool? isInstalledMA;
        bool IsInstalledMA
        {
            get
            {
                if (isInstalledMA == null)
                {
                    isInstalledMA = ValidMA();
                }
                return (bool)isInstalledMA;
            }
        }

        GameObject menuObject;


        [MenuItem("FZTools/メニュー雛形一括作成(β版)")]
        private static void OpenWindow()
        {
            var window = GetWindow<FZBulkMenuCreator>();
            window.titleContent = new GUIContent("メニュー雛形一括作成(β版)");
        }

        private void OnGUI()
        {
            ELayout.Horizontal(() =>
            {
                EUI.Space();
                ELayout.Vertical(() =>
                {
                    EUI.Space(2);
                    EUI.Label("Target Avatar");
                    EUI.ChangeCheck(
                        () => EUI.ObjectField<GameObject>(ref targetAvatar),
                        () =>
                        {
                        });
                    EUI.Space();

                    var text = "以下の機能を提供します\n"
                            + "・MAを使用したExpression Menuの雛形を作成します。\n"
                            + "・基本的なオン/オフメニューであればこれだけで作成可能です。\n"
                            + "・[WIP]メニュー雛形のテンプレートを作成し、使いまわせるようにします。"
                            + "※Modular Avatarの導入が前提となります。\n"
                            + "※作成されるメニューは雛形です。必要に応じて調整してください。";
                    EUI.InfoBox(text);
                    if (!IsInstalledMA)
                    {
                        EUI.ErrorBox("Modular Avatarがインストールされていません。\nこのツールはModular Avatarが前提となります。");
                        return;
                    }
                    EUI.Space(2);
                    using (new EditorGUI.DisabledScope(menues.Count() == 0))
                    {
                        EUI.Button("以下の内容でメニューを作成する", Create);
                    }
                    EUI.ChangeCheck(
                        () => EUI.ObjectField(ref menuesTemplateFile),
                        () =>
                        {
                            restoreMenu();
                        });
                    EUI.Space();
                    ELayout.Scroll(ref scrollPos, () =>
                                {
                                    EUI.Space(2);
                                    var serializedObject = new SerializedObject(this);
                                    serializedObject.Update();
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("menues"), true);
                                    serializedObject.ApplyModifiedProperties();
                                });
                    EUI.Space(2);
                });
                EUI.Space(2);
            });
        }

        private bool ValidMA()
        {
            var packageName = "nadena.dev.modular-avatar";
            var packages = Client.List();
            while (!packages.IsCompleted) { }
            return packages.Result.FirstOrDefault(p => p.name == packageName) != null;
        }

        private void restoreMenu()
        {
            if (menuesTemplateFile == null)
            {
                return;
            }

            string path = AssetDatabase.GetAssetPath(menuesTemplateFile);
            string json = File.ReadAllText(path);
            var menusArray = json.Split(",\n");
            menues = menusArray.Select(menuJson => JsonUtility.FromJson<FZBulkCreateMenu>(menuJson)).ToList();
        }

        private void Create()
        {
            AddMAMenu();
            menues.ForEach(menu =>
            {
                if (menu.menuType == FZBulkCreateMenu.MenuType.Single)
                {
                    if (menu.menuItems.Count != 1)
                    {
                        UnityEngine.Debug.LogWarning($"SingleタイプのメニューはMenuItemを1つだけ指定してください。Menu:{menu.menuName}");
                        return;
                    }
                    var item = menu.menuItems[0];
                    AddMAMenuItem(item.itemType, item, menuObject);
                }
                else if (menu.menuType == FZBulkCreateMenu.MenuType.SubMenu)
                {
                    if (menu.menuItems.Count == 0)
                    {
                        UnityEngine.Debug.LogWarning($"SubMenuタイプのメニューはMenuItemを1つ以上指定してください。Menu:{menu.menuName}");
                        return;
                    }
                    var subMenu = AddSubMenu(menu.menuName);
                    menu.menuItems.ForEach(item =>
                    {
                        AddMAMenuItem(item.itemType, item, subMenu);
                    });
                }
            });

            string menuesJson = string.Join(",\n", menues.Select(menu => JsonUtility.ToJson(menu)).ToList());
            AssetUtils.CreateDirectoryRecursive(MenueOutputPath);
            var now = DateTime.Now;
            File.WriteAllText($"{MenueOutputPath}/{targetAvatar?.gameObject?.name}_{now.Year}{now.Month}{now.Day}{now.Hour}{now.Minute}{now.Second}.fzbulkmenu", menuesJson, System.Text.Encoding.UTF8);
            AssetDatabase.Refresh();
        }

        private void AddMAMenu()
        {
            if (targetAvatar == null)
            {
                UnityEngine.Debug.LogWarning("Target Avatarが指定されていません。");
                return;
            }

            menuObject = new GameObject("Expression Menu");
            menuObject.transform.SetParent(targetAvatar.transform);
            menuObject.AddComponent<ModularAvatarMenuInstaller>();
            menuObject.AddComponent<ModularAvatarMenuGroup>();
        }

        private GameObject  AddSubMenu(string name)
        {
            var subMenuObject = new GameObject(name);
            subMenuObject.transform.SetParent(menuObject.transform);
            var item = subMenuObject.AddComponent<ModularAvatarMenuItem>();
            item.Control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
            item.automaticValue = true;
            item.MenuSource = SubmenuSource.Children;

            return subMenuObject;
        }

        private void AddMAMenuItem(FZBulkCreateMenu.MenuItemType itemType, FZBulkCreateMenu.FZBulkCreateMenuItem item, GameObject parent)
        {
            string menuItemName = item.menuItemName.Replace("\n", "").Replace("\r", "");
            var menuItemObject = new GameObject(item.menuItemName);
            menuItemObject.transform.SetParent(parent.transform);

            var menuItem = menuItemObject.AddComponent<ModularAvatarMenuItem>();
            menuItem.Control.type = itemType switch
            {
                FZBulkCreateMenu.MenuItemType.Toggle => VRCExpressionsMenu.Control.ControlType.Toggle,
                FZBulkCreateMenu.MenuItemType.Choose => VRCExpressionsMenu.Control.ControlType.Toggle,
                FZBulkCreateMenu.MenuItemType.Radial => VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                _ => VRCExpressionsMenu.Control.ControlType.Button,
            };
            menuItem.isDefault = item.defaultValue;
            menuItem.automaticValue = true;
            if(item.paramName != null && item.paramName != "")
            {
                menuItem.Control.parameter = new VRCExpressionsMenu.Control.Parameter
                {
                    name = item.paramName,
                };
                menuItem.automaticValue = false;
                menuItem.Control.value = item.paramValue;
            }

            if (itemType == FZBulkCreateMenu.MenuItemType.Toggle && item.targetObject != null)
            {
                var objectToggle = menuItemObject.AddComponent<ModularAvatarObjectToggle>();
                var objRef = new AvatarObjectReference();
                objRef.Set(item.targetObject);
                objectToggle.Objects = new List<ToggledObject>
                {
                    new ToggledObject
                    {
                        Object = objRef,
                        Active = false,
                    }
                };
                objectToggle.Inverted = true;
            }
        }
    }
}