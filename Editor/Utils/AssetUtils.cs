using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace FZTools
{
    public static class AssetUtils
    {
        public static void CreateAsset(UnityEngine.Object obj, string path)
        {
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(obj, AssetDatabase.GenerateUniqueAssetPath(path));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void CreateDirectoryRecursive(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var pathParts = path.Split('/');
            var p = pathParts[0];
            for (int i = 0; i < pathParts.Length; i++)
            {
                if (i == 0)
                    continue;

                if (!AssetDatabase.IsValidFolder($"{p}/{pathParts[i]}"))
                {
                    AssetDatabase.CreateFolder(p, pathParts[i]);
                }
                p = $"{p}/{pathParts[i]}";
            }
        }

        public static void DeleteAndCreateDirectoryRecursive(string dirPath)
        {
            AssetDatabase.DeleteAsset(dirPath);
            CreateDirectoryRecursive(dirPath);
        }

        public static string FindAssetPathFromObjectFileName(string objFileName)
        {
            return Directory.GetFiles("Assets", "*", System.IO.SearchOption.AllDirectories)
                    .FirstOrDefault(path => System.IO.Path.GetFileName(path) == $"{objFileName}")
                    .convertWinPath2Path();
        }

        public static void AddAllObjectToAsset(IEnumerable<UnityEngine.Object> adding, UnityEngine.Object target)
        {
            adding.ToList().ForEach(a => AssetDatabase.AddObjectToAsset(a, target));
        }
    }
}