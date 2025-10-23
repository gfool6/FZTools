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
using System.Threading.Tasks;
using nadena.dev.modular_avatar.core;

namespace FZTools
{
    public static class ExternalToolUtils
    {
        public static bool IsInstalledMA()
        {
            var packageName = "nadena.dev.modular-avatar";
            var packages = Client.List();
            while (!packages.IsCompleted) { }
            return packages.Result.FirstOrDefault(p => p.name == packageName) != null;
        }
    }
}