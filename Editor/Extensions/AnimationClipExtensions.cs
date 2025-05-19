using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Cysharp.Threading.Tasks;

namespace FZTools
{
    public static class AnimationClipExtensions
    {
        public static void AddAnimationCurve(this AnimationClip ac, Keyframe keyframe, string animPathName, string propertyName, Type type)
        {
            var animationCurve = new AnimationCurve(keyframe);
            EditorCurveBinding binding = new EditorCurveBinding()
            {
                path = animPathName,
                type = type,
                propertyName = propertyName
            };

            AnimationUtility.SetEditorCurve(ac, binding, animationCurve);
        }

        public static void AddBlendShape(this AnimationClip ac, SkinnedMeshRenderer mesh, List<string> ignoreShapeNames = null)
        {
            int count = mesh.sharedMesh.blendShapeCount;
            for (int i = 0; i < count; i++)
            {
                var shapeName = mesh.sharedMesh.GetBlendShapeName(i);
                if (ignoreShapeNames != null && ignoreShapeNames.Contains(shapeName))
                {
                    continue;
                }
                var shapeVal = mesh.GetBlendShapeWeight(i);
                ac.AddAnimationCurve(
                    new Keyframe(0, shapeVal),
                    mesh.gameObject.GetGameObjectPath(true),
                    FZToolsConstants.AnimClipParam.BlendShape(shapeName),
                    typeof(SkinnedMeshRenderer)
                );
            };
        }

        public static List<EditorCurveBinding> GetBindingCurves(this AnimationClip ac)
        {
            var curves = AnimationUtility.GetCurveBindings(ac).ToList();
            return curves;
        }
    }
}