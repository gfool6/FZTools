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
    public class FZWheelChair : EditorWindow
    {
        [SerializeField] GameObject targetAvatar;
        [SerializeField] GameObject wheelChair;
        VRCAvatarDescriptor AvatarDescriptor => targetAvatar != null ? targetAvatar.GetComponent<VRCAvatarDescriptor>() : null;

        List<string> errors = new List<string>();
        List<string> warns = new List<string>();

        [MenuItem("FZTools/_WIP/FZWC")]
        private static void OpenWindow()
        {
            var window = GetWindow<FZWheelChair>();
            window.titleContent = new GUIContent("FZWC");
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
                            Validator();
                        });
                    EUI.Space(2);
                    EUI.Label("Target WheelChair");
                    EUI.ChangeCheck(
                        () => EUI.ObjectField<GameObject>(ref wheelChair),
                        () =>
                        {
                            Validator();
                        });
                    EUI.Space();
                    var text = "以下の機能を提供します\n"
                            + "・アバターに車椅子用座りモーションの組み込み\n"
                            + "・車椅子のオンオフメニューの追加";
                    EUI.InfoBox(text);
                    EUI.Space(2);
                    if (errors.Count() > 0)
                    {
                        foreach (var error in errors)
                        {
                            EUI.ErrorBox(error);
                            EUI.Space(2);
                        }
                    }
                    if (warns.Count() > 0)
                    {
                        foreach (var warn in warns)
                        {
                            EUI.InfoBox(warn);
                            EUI.Space(2);
                        }
                    }
                    EUI.Space(2);
                    EUI.Button("Combine", Combine);
                });
                EUI.Space();
            });
        }

        private void Validator()
        {
            errors.Clear();
            warns.Clear();
            if (targetAvatar == null)
            {
                errors.Add("アバターが設定されていません");
            }
            else if (AvatarDescriptor == null)
            {
                errors.Add("アバターにVRCAvatarDescriptorコンポーネントがありません");
            }
            else
            {
                var locomotion = AvatarDescriptor.GetPlayableLayerController(VRCAvatarDescriptor.AnimLayerType.Base);
                if (locomotion == null)
                {
                    warns.Add("アバターにLocomotion Controllerが設定されていません。\n"
                            + "本ツールプリセットのLocomotion Controllerを使用します");
                }
            }

            if (wheelChair == null)
            {
                warns.Add("車椅子のGameObjectが設定されていません。座りモーションの組み込みのみ行われます");
            }
        }

        private void Combine()
        {
            if (errors.Count() > 0)
            {
                return;
            }
            errors.Clear();
            warns.Clear();

            /**
                処理プロセス
                0. {アバター名}_{連番}でフォルダを作成し、プリセットのprefabをコピーして配置
                1. アバターのLocomotion Controllerを取得してコピーし車椅子用座りモーションのStateを追加、フォルダに保存
                2. prefabのMA Merge Animatorに1のLocomotion Controllerを設定
                3. 車椅子のGameObjectをアバターの子に設定し、MA Bone Proxyを追加
                4. prefabのメニュー側設定に車椅子Gameobejctを指定
                5. prefabを保存してアバターの子に設定
            */

            WCPrefabInitialize();
        }

        private void WCPrefabInitialize()
        {
            if (targetAvatar == null || AvatarDescriptor == null)
            {
                return;
            }

            // 自動生成先の作成
            var avatarName = targetAvatar.name;
            var index = 0;
            while (AssetDatabase.IsValidFolder($"{FZToolsConstants.OutputPath.WheelChair}/{avatarName}_{index}"))
            {
                index++;
            }
            AssetUtils.CreateDirectoryRecursive($"{FZToolsConstants.OutputPath.WheelChair}/{avatarName}_{index}");

            // package側で持っているprefabをコピー
            var prefabPath = AssetUtils.FindAssetPathFromObjectFileName("FZWheelChair.prefab");
            if (string.IsNullOrEmpty(prefabPath))
            {
                errors.Add("FZWheelChair.prefabが見つかりません。パッケージのインポートを確認してください");
                return;
            }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                errors.Add("FZWheelChair.prefabの読み込みに失敗しました。パッケージのインポートを確認してください");
                return;
            }
            var prefabCopyPath = $"{FZToolsConstants.OutputPath.WheelChair}/{avatarName}_{index}/FZWheelChair.prefab";
            AssetUtils.CreateAsset(prefab, prefabCopyPath);
            var prefabCopy = AssetDatabase.LoadAssetAtPath<GameObject>(prefabCopyPath);
            if (prefabCopy == null)
            {
                errors.Add("FZWheelChair.prefabのコピーに失敗しました。パッケージのインポートを確認してください");
                return;
            }

        }
    }
}
