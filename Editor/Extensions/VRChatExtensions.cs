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
        public static VRCAvatarDescriptor GetAvatarDescriptor(this GameObject avatar){
            return avatar != null ? avatar.GetComponent<VRCAvatarDescriptor>() : null;
        }

        public static AnimatorController GetPlayableLayerController(this VRCAvatarDescriptor vrcad, VRCAvatarDescriptor.AnimLayerType lt)
        {
            var runtimeController = vrcad.baseAnimationLayers.First(l => l.type == lt).animatorController;
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