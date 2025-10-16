using System.Diagnostics;
using System.Collections.Specialized;
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
using static FZTools.AvatarUtils;

namespace FZTools
{
    public class FZFaceAnimationCreator : EditorWindow
    {
        [SerializeField] GameObject avatar = null;
        [SerializeField] AnimationClip defaultAnim = null;
        VRCAvatarDescriptor AvatarDescriptor => avatar != null ? avatar.GetComponent<VRCAvatarDescriptor>() : null;
        List<SkinnedMeshRenderer> SkinnedMeshRenderers => AvatarDescriptor != null ? AvatarDescriptor.GetComponentsInChildren<SkinnedMeshRenderer>(true).ToList() : new List<SkinnedMeshRenderer>();
        String[] MeshNames => SkinnedMeshRenderers.Select(smr => smr.gameObject.name).ToArray();
        string TargetAvatarName => avatar?.gameObject?.name;
        string AnimationClipOutputPath => $"{AssetUtils.OutputRootPath(TargetAvatarName)}/AnimationClip/FaceAnimCreator";
        private SkinnedMeshRenderer selected;
        private int selectedIndex = -1;
        private int prevSelectedIndex = -1;
        private bool checkValid = true;
        private string animFileName;
        private int mode = 0; //0:通常モード, 1:WD Onモード
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
                            + "・指定したメッシュのBlendshapeを元にAnimationClipを作成\n"
                            + "・AnimationClipはAssets/Output/(アバター名)/AnimationClip/FaceAnimCreator以下に保存されます\n"
                            + "・vrc.v_silなどのlipSync・eyeLook系のBlendshapeは除外されます";
                    EUI.InfoBox(text);

                    EUI.Space(2);
                    EUI.Label("元になるSkinned Mesh Renderer");
                    EUI.Popup(ref selectedIndex, MeshNames, null);
                    if (prevSelectedIndex != selectedIndex)
                    {
                        selected = SkinnedMeshRenderers[selectedIndex];
                        prevSelectedIndex = selectedIndex;
                    }
                    EUI.Space();
                    EUI.RadioButton(ref mode, new string[] { "通常モード", "WD Onモード" }, null);
                    EUI.Space();
                    if (mode == 0)
                    {
                        var wdModeText = "通常モード\n"
                                        + "現在のBlendShapeをAnimationClipとして作成します。\n"
                                        + "すべてのBlendShapeがAnimationClipに追加されます。";
                        EUI.InfoBox(wdModeText);
                    }
                    if (mode == 1)
                    {
                        var wdModeText = "WD Onモード\n"
                                        + "指定したAnimationClipをデフォルトとし、\n"
                                        + "それと現在のBlendShapeとの差分のみをAnimationClipとして作成します"
                                        + "指定されない場合、0以上の値を持つBlendShapeのみがAnimationClipに追加されます。";
                        EUI.InfoBox(wdModeText);
                        EUI.Space();
                        EUI.Label("WD On用のデフォルトAnimationClip");
                        EUI.ChangeCheck(
                            () => EUI.ObjectField<AnimationClip>(ref defaultAnim),
                            () => { });
                    }
                    EUI.Space();
                    EUI.Label("AnimationClipの名前");
                    EUI.TextField(ref animFileName);
                    EUI.Space(2);
                    if (avatar == null)
                    {
                        var err1Text = "Avatarが指定されていません";
                        EUI.ErrorBox(err1Text);
                    }
                    if (mode == 1)
                    {
                        var err2Text = "WD OnモードではデフォルトAnimationClipを指定してください\n"
                                    + "指定しない場合、0以上の値を持つBlendShapeのみがAnimationClipに追加されます";
                        EUI.WarningBox(err2Text);
                    }
                    EUI.Space(2);
                    EUI.Button("作成", MeshToAnim);
                });
                EUI.Space();
            });

        }

        public void MeshToAnim()
        {
            if (avatar == null)
            {
                avatar = null;
                return;
            }

            switch (mode)
            {
                case 0:
                    Create();
                    break;
                case 1:
                    CreateWithWDOn();
                    break;
                default:
                    break;
            }
        }

        private void Create()
        {
            var ac = new AnimationClip();
            var ignoreShapes = GetIgnoreShapeNames(selected, AvatarDescriptor);
            ac.AddBlendShape(selected, ignoreShapes);

            AssetUtils.CreateDirectoryRecursive(AnimationClipOutputPath);
            AssetUtils.CreateAsset(ac, $"{AnimationClipOutputPath}/{animFileName}.anim");
        }

        private void CreateWithWDOn()
        {
            var ac = new AnimationClip();
            var ignoreShapes = GetIgnoreShapeNames(selected, AvatarDescriptor);

            if (defaultAnim != null)
            {
                // ignoreShapeに、defaultAnimとname・weightが一致するものを追加
                var bindingCurves = defaultAnim.GetBindingCurves();
                foreach (var curve in bindingCurves)
                {
                    var shapeName = curve.propertyName.Replace("blendShape.", "");
                    var shapeVal = selected.GetBlendShapeWeight(selected.sharedMesh.GetBlendShapeIndex(shapeName));
                    var defaultShapeVal = AnimationUtility.GetEditorCurve(defaultAnim, curve).keys[0].value;

                    if (shapeVal == defaultShapeVal && !ignoreShapes.Contains(shapeName))
                    {
                        ignoreShapes.Add(shapeName);
                    }
                }
            }
            else
            {
                // ignoreShapesに、BlendShapeの値が0のものを追加
                int count = selected.sharedMesh.blendShapeCount;
                for (int i = 0; i < count; i++)
                {
                    var shapeName = selected.sharedMesh.GetBlendShapeName(i);
                    var weight = selected.GetBlendShapeWeight(i);
                    if (weight <= 0 && !ignoreShapes.Contains(shapeName))
                    {
                        ignoreShapes.Add(shapeName);
                    }
                }
            }
            ac.AddBlendShape(selected, ignoreShapes);

            AssetUtils.CreateDirectoryRecursive(AnimationClipOutputPath);
            AssetUtils.CreateAsset(ac, $"{AnimationClipOutputPath}/{animFileName}.anim");
        }

        private List<String> GetIgnoreShapeNames(SkinnedMeshRenderer faceMesh, VRCAvatarDescriptor descriptor)
        {
            var ignoreShapeNames = new List<string>();
            var lipSyncBlendShapes = AvatarUtils.GetLipSyncShapeNames(faceMesh, descriptor);
            var eyelidsBlendShapes = AvatarUtils.GetEyelidsShapeNames(faceMesh, descriptor);
            if (lipSyncBlendShapes != null)
            {
                ignoreShapeNames.AddRange(lipSyncBlendShapes);
            }
            if (eyelidsBlendShapes != null)
            {
                ignoreShapeNames.AddRange(eyelidsBlendShapes);
            }
            return ignoreShapeNames;
        }
    }
}