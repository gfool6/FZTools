using System.Diagnostics;
using System.Collections.Specialized;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Editor;
using VRC.SDKBase.Editor;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Avatars.Components;
using EUI = FZTools.EditorUtils.UI;
using ELayout = FZTools.EditorUtils.Layout;
using static FZTools.AvatarUtils;

using nadena.dev.modular_avatar.core;
using nadena.dev.modular_avatar.core.editor;

namespace FZTools
{
    public class ChimeraCreateHelper : EditorWindow
    {
        [SerializeField] GameObject headAvatar;
        [SerializeField] GameObject bodyAvatar;

        bool? isInstalledMA;
        bool IsInstalledMA
        {
            get
            {
                if (isInstalledMA == null)
                {
                    isInstalledMA = ExternalToolUtils.IsInstalledMA();
                }
                return (bool)isInstalledMA;
            }
        }


        [MenuItem("FZTools/ChimeraCreateHelper(β)")]
        private static void OpenWindow()
        {
            var window = GetWindow<ChimeraCreateHelper>();
            window.titleContent = new GUIContent("ChimeraCreateHelper(β)");
        }

        private void OnGUI()
        {
            ELayout.Horizontal(() =>
            {
                EUI.Space();
                ELayout.Vertical(() =>
                {
                    var text = "以下の機能を提供します\n"
                            + "・指定された頭用アバターと胴体用アバターをModular Avatarでマージします\n"
                            + "・頭の位置を胴体側のView Positionを基準にざっくりと合わせます（調整は自ら行ってください）\n"
                            + "・Eye lookやvisemeの設定なども自動で修正します\n";
                    EUI.InfoBox(text);
                    if (!IsInstalledMA)
                    {
                        EUI.ErrorBox("Modular Avatarがインストールされていません。\nこのツールはModular Avatarが前提となります。");
                        return;
                    }
                    EUI.Label("Head Avatar");
                    EUI.ChangeCheck(
                        () => EUI.ObjectField<GameObject>(ref headAvatar),
                        () =>
                        {
                        });
                    EUI.Space();

                    EUI.Label("Body Avatar");
                    EUI.ChangeCheck(
                        () => EUI.ObjectField<GameObject>(ref bodyAvatar),
                        () =>
                        {
                        });
                    EUI.Space();

                    EUI.Space(2);
                    using (new EditorGUI.DisabledScope(!IsInstalledMA || headAvatar == null || bodyAvatar == null))
                    {
                        EUI.Button("合成", Fusion);
                    }
                });
            });
        }

        private void Fusion()
        {
            if (headAvatar == null || bodyAvatar == null)
            {
                UnityEngine.Debug.LogError("頭用アバターと胴体用アバターの両方を指定してください");
                return;
            }

            var headDescriptor = headAvatar.GetAvatarDescriptor();
            var bodyDescriptor = bodyAvatar.GetAvatarDescriptor();

            if (headDescriptor == null || bodyDescriptor == null)
            {
                UnityEngine.Debug.LogError("頭用アバターと胴体用アバターの両方にVRCAvatarDescriptorが必要です");
                return;
            }

            Combine(headDescriptor, bodyDescriptor);
        }

        private void AlignHeadFromBodyViewPosition(VRCAvatarDescriptor headDescriptor, VRCAvatarDescriptor bodyDescriptor)
        {
            Vector3 headViewPosition = headDescriptor.ViewPosition;
            Vector3 bodyViewPosition = bodyDescriptor.ViewPosition;

            // bodyとheadのViewPosition.yの比率を求めて、head側のScaleを調整
            float yRatio = bodyViewPosition.y / headViewPosition.y;
            headDescriptor.transform.localScale = headDescriptor.transform.localScale * yRatio;

            // neckのWorldPositionを基準にheadのPositionを調整した方がいいかも TODO
            // bodyとheadのViewPosition.zの差分を求めて、head側のPosition.zを調整
            // float zOffset = bodyViewPosition.z - headViewPosition.z;
            // headDescriptor.transform.position = headDescriptor.transform.position + new Vector3(0, 0, zOffset);
            Animator headAvatarAnimator = headAvatar.GetComponent<Animator>();
            var neckBoneHead = headAvatarAnimator.GetBoneTransform(HumanBodyBones.Head);
            Animator bodyAvatarAnimator = headAvatar.GetComponent<Animator>();
            var neckBoneBody = bodyAvatarAnimator.GetBoneTransform(HumanBodyBones.Head);

            float zOffset = neckBoneBody.position.z - neckBoneHead.position.z;
            headDescriptor.transform.position = headDescriptor.transform.position + new Vector3(0, 0, zOffset);
        }

        private void FixDescriptor(VRCAvatarDescriptor headDescriptor, VRCAvatarDescriptor bodyDescriptor)
        {
            // LipSyncで参照してるFaceMeshをhead→bodyにコピー
            var lipSyncUsesFaceMesh = bodyDescriptor.lipSync == VRCAvatarDescriptor.LipSyncStyle.VisemeBlendShape || bodyDescriptor.lipSync == VRCAvatarDescriptor.LipSyncStyle.JawFlapBlendShape;
            if (lipSyncUsesFaceMesh)
            {
                bodyDescriptor.VisemeSkinnedMesh = headDescriptor.VisemeSkinnedMesh;
            }

            // Eyelids側のFaceMeshコピー
            var eyelidUsesFaceMesh = bodyDescriptor.customEyeLookSettings.eyelidType == VRCAvatarDescriptor.EyelidType.Blendshapes;
            if (eyelidUsesFaceMesh)
            {
                bodyDescriptor.customEyeLookSettings.eyelidsSkinnedMesh = headDescriptor.customEyeLookSettings.eyelidsSkinnedMesh;
            }

            // Eyesで参照してる左右目ボーンをhead→bodyにコピー
            bodyDescriptor.customEyeLookSettings.leftEye = headDescriptor.customEyeLookSettings.leftEye;
            bodyDescriptor.customEyeLookSettings.rightEye = headDescriptor.customEyeLookSettings.rightEye;
        }

        private void Combine(VRCAvatarDescriptor headDescriptor, VRCAvatarDescriptor bodyDescriptor)
        {
            // 頭の位置を胴体のViewPosition基準でざっくり合わせる
            AlignHeadFromBodyViewPosition(headDescriptor, bodyDescriptor);

            // 顔側：
            // 全メッシュ取得→非表示にし、顔メッシュだけ表示する
            var faceMeshHead = headDescriptor.GetVRCAvatarFaceMeshRenderer();
            headDescriptor.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true).ToList().ForEach(smr => smr.gameObject.SetActive(false));
            faceMeshHead.gameObject.SetActive(true);
            // もしHairMeshがあればそれも表示にする。SkinnedMeshRendererを持つgameObjectのnameに"hair"が含まれるものを探す
            headDescriptor.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                                            .Where(smr => IsHeadParts(smr.gameObject.name)).ToList()
                                            .ForEach(smr => smr.gameObject.SetActive(true));

            // 体側：
            // 顔・髪などを非表示にする
            var faceMeshBody = bodyDescriptor.GetVRCAvatarFaceMeshRenderer();
            faceMeshBody.gameObject.SetActive(false);
            // もしHairMeshがあればそれも表示にする。SkinnedMeshRendererを持つgameObjectのnameに"hair"が含まれるものを探す
            bodyDescriptor.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                                            .Where(smr => IsHeadParts(smr.gameObject.name)).ToList()
                                            .ForEach(smr => smr.gameObject.SetActive(false));

            // body側のDescriptorへ各種設定をhead側からコピー
            FixDescriptor(headDescriptor, bodyDescriptor);

            // Head AvatarにModularAvatarコンポーネントを追加
            // HeadのDescriptorを削除
            DestroyImmediate(headAvatar.GetComponent<VRCAvatarDescriptor>());
            DestroyImmediate(headAvatar.GetComponent<VRC.Core.PipelineManager>());

            // setup 
            headAvatar.transform.SetParent(bodyAvatar.transform);
            SetupOutfit.SetupOutfitUI(headAvatar);
        }

        private bool IsHeadParts(string gameObjectName)
        {
            var headPartsKeywords = new string[] { "face", "head", "hair", "horn", "ear" };
            var name = gameObjectName.ToLower();
            foreach (var parts in headPartsKeywords)
            {
                if (name.Contains(parts))
                {
                    return true;
                }
            }
            return false;
        }
    }
}