using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace FZTools
{
    public static class GameObjectExtensions
    {
        public static string GetGameObjectPath(this GameObject obj, bool removeRootPath = false)
        {
            var currentTransform = obj.transform.parent;
            var path = obj.name;
            var rootPath = "";
            while (currentTransform != null)
            {
                rootPath = $"{currentTransform.name}/";
                path = $"{currentTransform.name}/{path}";
                currentTransform = currentTransform.parent;
            }
            return removeRootPath ? path.Replace(rootPath, "") : path;
        }

        public static GameObject GetBoneRootObject(this GameObject rootGameObject)
        {
            return rootGameObject.GetComponentsInChildren<Transform>().Where(t => t.GetComponent<Renderer>() == null).FirstOrDefault(t => t.name == "Hips").gameObject;
        }
    }
}