using System.Collections.Specialized;
using System.Collections.Generic;
using UnityEngine;
namespace FZTools
{
    public static class SkinnedMeshRendererExtensions
    {
        /// <summary>
        /// 名前とweightのdictionaryを返す
        /// </summary>
        /// <param name="skinnedMeshRenderer"></param>
        /// <returns></returns>
        public static OrderedDictionary BlendShapeInfo(this SkinnedMeshRenderer skinnedMeshRenderer)
        {
            if (skinnedMeshRenderer == null || skinnedMeshRenderer.sharedMesh == null)
            {
                return null;
            }
            var blendShapeInfo = new OrderedDictionary();
            for (int i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
            {
                var shapeName = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i);
                var shapeWeight = skinnedMeshRenderer.GetBlendShapeWeight(i);
                blendShapeInfo.Add(shapeName, shapeWeight);
            }
            return blendShapeInfo;
        }
    }
}
