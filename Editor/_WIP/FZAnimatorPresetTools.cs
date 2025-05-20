using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using EUI = FZTools.EditorUtils.UI;
using ELayout = FZTools.EditorUtils.Layout;

namespace FZTools
{
    public class FZAnimatorPresetTools : EditorWindow
    {
        [SerializeField]
        private VRCAvatarDescriptor avatarDescripter;
        [SerializeField]
        private string fileNamePrefix = "";
        [SerializeField]
        private string fileNameSuffix = "";
        [SerializeField]
        private AnimatorController convertAnimatorController;
        [SerializeField]
        private FZPreset fzPreset;

        string TargetAvatarName => avatarDescripter?.gameObject?.name;
        string OutputDirPath => $"{AssetUtils.OutputRootPath(TargetAvatarName)}/AnimatorController";


        [MenuItem("FZTools/_WIP/FZAnimatorPresetTools")]
        private static void OpenWindow()
        {
            var window = GetWindow<FZAnimatorPresetTools>();
            window.titleContent = new GUIContent("FZAnimatorPresetTools");
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
                    EUI.ObjectField<VRCAvatarDescriptor>(ref avatarDescripter);
                    EUI.Space(2);
                    EUI.Label("Animator Controller File Prefix");
                    EUI.TextField(ref fileNamePrefix);
                    EUI.Space(2);
                    EUI.Label("Animator Controller File Suffix");
                    EUI.TextField(ref fileNameSuffix);
                    EUI.Space(2);
                    EUI.Button("AnimatorControllerをプリセットに変換", () =>
                    {
                        FXLayer2Preset(avatarDescripter);
                    });
                    EUI.Space(2);

                    EUI.Label("Convert Animator Controller");
                    fzPreset = (FZPreset)EditorGUILayout.ObjectField(fzPreset, typeof(FZPreset), true);
                    if (GUILayout.Button("プリセットをAnimatorControllerに変換"))
                    {
                    }
                });
                EUI.Space();
            });
        }

        private void FXLayer2Preset(VRCAvatarDescriptor descriptor)
        {
            var dirPath = $"{OutputDirPath}/Preset";
            AssetDatabase.DeleteAsset(dirPath);
            AssetUtils.CreateDirectoryRecursive(dirPath);

            var controller = descriptor.GetFXController();

            var newPreset = ScriptableObject.CreateInstance<FZPreset>();
            newPreset.layerPresets = new List<FZLayerPreset>();
            var layers = controller.layers;
            for (int i = 0; i < layers.Length; i++)
            {
                var layer = layers[i];

                var layerPresetModel = ScriptableObject.CreateInstance<FZLayerPreset>();
                layerPresetModel.animationPresets = new List<FZAnimationPreset>();
                layerPresetModel.layerName = layer.name;
                layerPresetModel.layerIndex = i;

                var layerStateMachineStates = layer.stateMachine.states.ToList();
                var layerEntryTransitions = layer.stateMachine.entryTransitions.ToList();

                layerStateMachineStates.ForEach(cas =>
                {
                    var transitionParams = new List<FZPresetTransitionParam>();
                    var state = cas.state;
                    var exitTransition = state.transitions.FirstOrDefault(t => t.isExit);
                    var entryTransition = layerEntryTransitions.FirstOrDefault(et => et.destinationState.name.Equals(state.name));

                    // 対応する形式ではない
                    if (exitTransition == null || entryTransition == null)
                        return;

                    entryTransition.conditions.ToList().ForEach(etco =>
                    {
                        transitionParams.Add(GetTransitionParam(etco, ParameterVector.IN));
                    });
                    exitTransition.conditions.ToList().ForEach(etco =>
                    {
                        transitionParams.Add(GetTransitionParam(etco, ParameterVector.OUT));
                    });

                    layerPresetModel.animationPresets.Add(new FZAnimationPreset()
                    {
                        exitSetting = new ExitSetting()
                        {
                            hasExitTime = exitTransition.hasExitTime,
                            exitTime = exitTransition.exitTime,
                            hasFixedDuration = exitTransition.hasFixedDuration,
                            duration = exitTransition.duration,
                            offset = exitTransition.offset,
                            interruptionSource = exitTransition.interruptionSource,
                            orderedInterruption = exitTransition.orderedInterruption,
                            canTransitionToSelf = exitTransition.canTransitionToSelf
                        },
                        motion = state.motion,
                        transitionParams = transitionParams
                    });
                });
                AssetDatabase.CreateAsset(layerPresetModel, $"{dirPath}/{layer.name}_preset.asset");
                newPreset.layerPresets.Add(layerPresetModel);
            }
            AssetDatabase.CreateAsset(newPreset, $"{dirPath}/Preset.asset");

        }

        private FZPresetTransitionParam GetTransitionParam(AnimatorCondition condition, ParameterVector inOut)
        {
            bool isBool = (condition.mode == AnimatorConditionMode.If || condition.mode == AnimatorConditionMode.IfNot);
            bool isFloat = condition.threshold - Math.Floor(condition.threshold) != 0;
            ParameterValueType vt = isBool ? ParameterValueType.BOOL : isFloat ? ParameterValueType.FLOAT : ParameterValueType.INT;
            ParameterCompareType ct = ParameterCompareType.EQUAL;
            switch (condition.mode)
            {
                case AnimatorConditionMode.Greater: ct = ParameterCompareType.GREATER; break;
                case AnimatorConditionMode.Less: ct = ParameterCompareType.LESS; break;
                case AnimatorConditionMode.NotEqual: ct = ParameterCompareType.NOTEQUAL; break;
                case AnimatorConditionMode.Equals:
                default: ct = ParameterCompareType.EQUAL; break;
            }

            string val = isBool ? $"{condition.mode == AnimatorConditionMode.If}" : $"{condition.threshold}";
            return new FZPresetTransitionParam(condition.parameter, inOut, vt, ct, val);
        }

        private void Preset2AnimatorController(FZPreset preset)
        {

        }

        private string LoadPresetJsonString(FZPreset preset)
        {
            return "";
        }

        // private void CreateAnimatorControllerTest(string controllerOutputPath)
        // {
        //     var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerOutputPath);

        //     // https://docs.unity3d.com/ScriptReference/Animations.AnimatorController.html
        //     // やはり参考にすべきは公式のドキュメント
        //     // コントローラー作成→レイヤー追加・パラメータ追加→レイヤーからステートマシン取得→ステートマシンにステート追加→ステートマシン.Entry追加とステート.Exit追加
        //     // デフォルト指定は多分State側かな
        //     // これをデータの個数分やる　簡単？　スマートな実装だるそう
        //     controller.AddLayer("Left Hand");
        //     controller.AddParameter("GestureLeft", AnimatorControllerParameterType.Int);
        //     controller.AddParameter("IsGestureLeft", AnimatorControllerParameterType.Bool);
        //     var stateMachine = controller.layers.FirstOrDefault(l => l.name == "Left Hand").stateMachine;
        //     var face1 = stateMachine.AddState("FaceBool");
        //     var face2 = stateMachine.AddState("FaceInt");
        //     var face1EntryTransition = stateMachine.AddEntryTransition(face1);
        //     face1EntryTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "IsGestureLeft");
        //     var face1ExitTransition = face1.AddExitTransition();
        //     face1ExitTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.IfNot, 0, "IsGestureLeft");

        //     var face2EntryTransition = stateMachine.AddEntryTransition(face2);
        //     face2EntryTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1, "GestureLeft");
        //     var face2ExitTransition = face2.AddExitTransition();
        //     face2ExitTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.NotEqual, 0, "GestureLeft");
        // }
    }
}

// 機能
// 既存AnimatorControllerからプリセットを作成する（Controller→Json→プリセット？）
// あらかじめ決められた形式でプリセットを作成する（Jsonからロード）
// 