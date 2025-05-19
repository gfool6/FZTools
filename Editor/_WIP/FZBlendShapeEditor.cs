using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Avatars.Components;
using EUI = FZTools.EditorUtils.UI;
using ELayout = FZTools.EditorUtils.Layout;

namespace FZTools
{
    public class FZBlendShapeEditor : EditorWindow
    {
        [SerializeField]
        private GameObject avatar;

        string TargetAvatarName => avatar?.name;
        VRCAvatarDescriptor AvatarDescriptor => avatar.GetComponent<VRCAvatarDescriptor>();
        List<Renderer> Renderers => AvatarDescriptor.GetComponentsInChildren<Renderer>(true).ToList();


        [MenuItem("FZTools/_WIP/BlendShape編集")]
        public static void OpenWindow()
        {
            var window = GetWindow<FZBlendShapeEditor>("BlendShape編集");
        }

        private void OnEnable()
        {

        }

        private void OnGUI()
        {

        }

        // BlendShape取得
        // リストにして表示（入力欄の初期値）
        // 編集を反映（ボタン）
        // Meshごとプルダウンにするか全部一括にするかは検討
        // リネームを非破壊にするためにMesh出力→自動セットとかにしたほうが良さそう？
    }
}