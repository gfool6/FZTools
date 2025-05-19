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
    public class FZFaceAnimationCreator : EditorWindow
    {
        [SerializeField] GameObject avatar;
        VRCAvatarDescriptor AvatarDescriptor => avatar != null ? avatar.GetComponent<VRCAvatarDescriptor>() : null;
        List<SkinnedMeshRenderer> SkinnedMeshRenderers => AvatarDescriptor != null ? AvatarDescriptor.GetComponentsInChildren<SkinnedMeshRenderer>(true).ToList() : new List<SkinnedMeshRenderer>();
        String[] MeshNames => SkinnedMeshRenderers.Select(smr => smr.gameObject.name).ToArray();
        string TargetAvatarName => avatar?.gameObject?.name;
        string AnimationClipOutputPath => $"{FZToolsConstants.FZTools.OutputRootPath(TargetAvatarName)}/AnimationClip/FaceAnimCreator";
        private SkinnedMeshRenderer selected;
        private int selectedIndex = -1;
        private int prevSelectedIndex = -1;
        private bool checkValid = true;
        private string animFileName;
        Vector2 scrollPos;


        [MenuItem("FZTools/今の顔から表情Animation作るやつ")]
        private static void OpenWindow()
        {
            var window = GetWindow<FZFaceAnimationCreator>();
            window.titleContent = new GUIContent("FaceAnimCreator");
        }

        private void OnGUI()
        {
            ELayout.Horizontal(() =>
            {
                EUI.Space();
                ELayout.Vertical(() =>
                {
                    EUI.Space(2);
                    EUI.Label("Target Avatar");
                    EUI.ChangeCheck(
                        () => EUI.ObjectField<GameObject>(ref avatar),
                        () =>
                        {
                            selectedIndex = 0;
                        });
                    EUI.Space();
                    var text = "以下の機能を提供します\n"
                            + "・AnimationClipの値をメッシュのBlendshapeに転写\n"
                            + "・メッシュのBlendshapeの値をAnimationClipに転写";
                    EUI.InfoBox(text);

                    EUI.Space(2);
                    EUI.Label("Skinned Mesh Renderer");
                    EUI.Popup(ref selectedIndex, MeshNames, null);
                    if (prevSelectedIndex != selectedIndex)
                    {
                        selected = SkinnedMeshRenderers[selectedIndex];
                        prevSelectedIndex = selectedIndex;
                    }
                    EUI.Space();
                    EUI.Label("Animation Clip Name");
                    EUI.TextField(ref animFileName);
                    EUI.Space(2);
                    if (!checkValid)
                    {
                        var warnText = "Meshに存在しないShapeがAnimationClipに含まれています\n"
                                    + "Mesh側に存在しないShapeについては無視して転写されます";
                        EUI.InfoBox(warnText);
                    }
                    EUI.Space(2);
                    EUI.Button("作成", MeshToAnim);
                });
                EUI.Space();
            });

        }

        public void MeshToAnim()
        {
            var ac = new AnimationClip();
            ac.AddBlendShape(selected);
            AssetUtils.DeleteAndCreateDirectoryRecursive(AnimationClipOutputPath);
            AssetUtils.CreateAsset(ac, $"{AnimationClipOutputPath}/{animFileName}.anim");
            // int count = selected.sharedMesh.blendShapeCount;
            // for (int i = 0; i < count; i++)
            // {
            //     var shapeName = selected.sharedMesh.GetBlendShapeName(i);

            // }
        }
    }
}