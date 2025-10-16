using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Avatars.Components;

namespace FZTools
{
    public static class AvatarUtils
    {
        public static List<String> GetLipSyncShapeNames(SkinnedMeshRenderer faceMesh, VRCAvatarDescriptor descriptor)
        {
            var usesViseme = descriptor.lipSync == VRCAvatarDescriptor.LipSyncStyle.VisemeBlendShape;
            var visemeBlendShapes = usesViseme ? descriptor?.VisemeBlendShapes?.ToList() : null;
            return visemeBlendShapes;
        }

        public static List<String> GetEyelidsShapeNames(SkinnedMeshRenderer faceMesh, VRCAvatarDescriptor descriptor)
        {
            var isBlendshapeEyelids = descriptor.customEyeLookSettings.eyelidType == VRCAvatarDescriptor.EyelidType.Blendshapes;
            var eyelidsBlendShapes = isBlendshapeEyelids ? descriptor.customEyeLookSettings.eyelidsBlendshapes.Select(index => faceMesh.sharedMesh.GetBlendShapeName(index)).ToList() : null;
            return eyelidsBlendShapes;
        }
    }
}
