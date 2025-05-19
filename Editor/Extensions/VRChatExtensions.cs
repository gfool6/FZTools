using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using UnityEditor.Animations;

namespace FZTools
{
    public static class VRChatExtensions
    {
        public static AnimatorController GetFXController(this VRCAvatarDescriptor vrcad)
        {
            var runtimeController = vrcad.baseAnimationLayers.First(l => l.type == VRCAvatarDescriptor.AnimLayerType.FX).animatorController;
            var controllerPath = AssetDatabase.GetAssetPath(runtimeController);
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            return controller;
        }

        public static SkinnedMeshRenderer GetVRCAvatarFaceMeshRenderer(this VRCAvatarDescriptor avatarDescriptor)
        {
            // VisemeSkinnedMeshに設定されているメッシュか、一番シェイプ数の多いfaceもしくはbodyと名のつくメッシュを取得する
            var mesh = avatarDescriptor.VisemeSkinnedMesh;
            mesh = mesh ?? avatarDescriptor.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                                                .OrderByDescending(smr => smr.sharedMesh.blendShapeCount)
                                                .First(smr => smr.gameObject.name.ToLower().Contains("face") || smr.gameObject.name.ToLower().Contains("body"));
            return mesh;
        }
    }
}