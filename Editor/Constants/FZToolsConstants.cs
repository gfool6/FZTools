using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace FZTools
{
    public class FZToolsConstants
    {
        public struct Package
        {
            public const string Name = "FZTools";
            public const string PackageName = "com.gfool6.fztools";
            public const string Version = "0.1.5";
        }

        public struct OutputPath
        {
            public const string AutoCreateRoot = "Assets/FZTools/AutoCreate";
            public const string WheelChair = AutoCreateRoot + "/WheelChair";

        }

        public struct LabelText
        {
            public const string CreateFaceAnimationTemplate = "表情テンプレートアニメーション作成";
            public const string CreateMeshOnOffAnimation = "個別パーツオンオフアニメーション作成";
        }

        public struct AnimClipParam
        {
            public enum Axis
            {
                x,
                y,
                z
            }
            public static readonly string GameObjectIsActive = "m_IsActive";
            public static readonly string MeshEnabled = "m_Enabled";
            public static readonly string PositionKeyBase = "m_LocalPosition";
            public static string Position(Axis a) => $"{PositionKeyBase}.{a.ToString()}";
            public static readonly string RotationKeyBase = "localEulerAnglesRaw";
            public static string Rotation(Axis a) => $"{RotationKeyBase}.{a.ToString()}";
            public static readonly string ScaleKeyBase = "m_LocalScale";
            public static string Scale(Axis a) => $"{ScaleKeyBase}.{a.ToString()}";
            public static readonly string MaterialKeyBase = "m_Materials.Array.data";
            public static string MaterialReference(int i) => $"{MaterialKeyBase}[{i}]";
            public static string BlendShape(string shapeName) => $"blendShape.{shapeName}";
        }

        public struct VRChat
        {
            public static readonly string GestureLeft = "GestureLeft";
            public static readonly string GestureRight = "GestureRight";

            public static readonly string FaceMenu = "FaceMenu";

            public static HandGesture[] HandGestures => Enum.GetNames(typeof(HandGesture)).Select(n => (HandGesture)Enum.Parse(typeof(HandGesture), n)).ToArray();
            public enum HandGesture
            {
                Neutral = 0,
                Fist = 1,
                Open = 2,
                Point = 3,
                Victory = 4,
                Rocknroll = 5,
                Gun = 6,
                Thumbsup = 7
            }
        }
    }
}