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
    public class FZAnimationClipEditor : EditorWindow
    {
        [SerializeField]
        private GameObject avatar;
        [SerializeField]
        private AnimationClip animationClip;

        string TargetAvatarName => avatar?.name;
        VRCAvatarDescriptor AvatarDescriptor => avatar.GetComponent<VRCAvatarDescriptor>();
        List<Renderer> Renderers => AvatarDescriptor.GetComponentsInChildren<Renderer>(true).ToList();
        List<GameObject> ClothAndAccessoryRootObject => RenderersObjPath.Select(n => n.Split('/')).Where(n => n.Count() >= 2)
                                                                        .Select(n => string.Join("/", n.Take(n.Length - 1)))
                                                                        .Distinct().Select(o => AvatarDescriptor.transform.Find(o).gameObject).ToList();
        List<string> RenderersObjPath => Renderers.Select(e => e.gameObject.GetGameObjectPath(true)).ToList();
        List<string> ClothAndAccessoryRootObjPath => ClothAndAccessoryRootObject.Select(o => o.GetGameObjectPath(true)).ToList();
        int ColumnSize => (int)Math.Round(position.size.x / 3);
        int RowSize => (int)Math.Round(position.size.y / 4);
        bool IsCreate => animationClip == null;
        string AnimationClipOutputPath => $"{FZToolsConstants.FZTools.OutputRootPath(TargetAvatarName)}/AnimationClip";

        List<string> meshAndObjPaths = new List<string>();
        List<string> meshPaths = new List<string>();
        Dictionary<string, List<string>> meshAndObjAnims = new Dictionary<string, List<string>>();
        Dictionary<string, Dictionary<string, string>> animParams = new Dictionary<string, Dictionary<string, string>>();
        List<bool> addingParam = new List<bool>();
        int selectionIndex;

        Vector2 paramsScrollPos;
        Vector2 addingParamsPreviewLabelScrollPos;

        // これに記録
        // ObjectPath/AnimationKeyName, value
        // ex) Kikyo_Body/blendShape.あ, 100　など
        Dictionary<string, string> recordingParam = new Dictionary<string, string>();


        [MenuItem("FZTools/_WIP/アニメーション作成・編集")]
        private static void OpenWindow()
        {
            var window = GetWindow<FZAnimationClipEditor>();
            window.titleContent = new GUIContent("アニメーション作成・編集");
        }

        private void OnEnable()
        {
            if (avatar != null)
            {
                InitVariables();
            }
            ResetParams();
            // ResetPreview();
        }

        private void OnGUI()
        {
            ELayout.Horizontal(() =>
            {
                EUI.FlexibleSpace();
                ELayout.Vertical(() =>
                {
                    BaseUI();
                    if (avatar == null) return;

                    ParamsUI();
                    AddingParameterPreviewUI();
                    EUI.Button(IsCreate ? "作成" : "更新(開発中)", () =>
                    {
                        CreateAndUpdateAnimationClip();
                    });
                });
                EUI.FlexibleSpace();
            });
        }

        private void BaseUI()
        {
            EUI.Space(2);
            EUI.Label("Target Avatar");
            EUI.ChangeCheck(() =>
            {
                EUI.ObjectField<GameObject>(ref avatar);
            }, () =>
            {
                if (avatar != null)
                {
                    InitVariables();
                }
                ResetParams();
            });
            EUI.Space();
            EUI.Label("Target AnimationClip");
            EUI.ChangeCheck(() =>
            {
                EUI.ObjectField<AnimationClip>(ref animationClip);
            }, () =>
            {
                ResetParams();
                if (animationClip != null)
                {
                    LoadRecordingParams();
                }
            });
            EUI.Space(2);
            var infoText = "AnimationClipをセットしているかどうかで動作が変わります\n";
            infoText += "・セットしている場合:セットしたAnimationClipにパラメータを追加・削除します\n"
                        + "・セットしてない場合:選んだパラメータで新規にアニメーションを作成します";
            EUI.InfoBox(infoText);
            EUI.Space(2);


            // TODO メモ
            // マテリアル差し替えもシェイプ追加とかあの辺のメニューに統合した方が楽では？
            // パラメーターのところにObjectField出してさぁ…
            // 単純なシェイプ・オンオフの追加に関しては結構使いやすさはあると思うので
            // なんならいっそ全項目いい感じにやれたりしないもんかな…
            // 既存機能も諸々統合しちゃうとかは楽な気はするんだけどね
            // AnimationClip編集にプレビューつけた方が良さげ？
            // 顔カメラと全体カメラの両プレビューでアニメーションと表情などをチェック…的な
            // あーだからあれだよな　ジェスチャ用のアレとかもチェックできるよう必要か
            // transformとかその辺もだな
        }

        private void ParamsUI()
        {
            var tempSI = selectionIndex;
            EUI.Popup(ref selectionIndex, meshAndObjPaths.Select(p => p.Replace("/", "\u2215")).ToArray());
            if (tempSI != selectionIndex)
            {
                addingParam = new List<bool>();
            }
            EUI.Space(2);
            EUI.Button("チェックのついたパラメータを全て追加", () => // TODO 個別だとできるけどこっちだとエラーになる　Transform系/Mat系対応
            {
                meshAndObjAnims[meshAndObjPaths[selectionIndex]].ForEach(p =>
                {
                    var canBeAdd = false;
                    if (p.Equals(FZToolsConstants.AnimClipParam.GameObjectIsActive))
                    {
                        canBeAdd = animParams[meshAndObjPaths[selectionIndex]][p] == $"{true}";
                    }
                    else
                    {
                        var index = avatar.transform.Find(meshAndObjPaths[selectionIndex]).gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh.GetBlendShapeIndex(p);
                        canBeAdd = addingParam[index];
                    }
                    var key = $"{meshAndObjPaths[selectionIndex]}/{p}";
                    if (!recordingParam.ContainsKey(key) && canBeAdd)
                    {
                        recordingParam.Add($"{key}", $"{animParams[meshAndObjPaths[selectionIndex]][p]}");
                    }
                });
                addingParam = new List<bool>();
                Repaint();
            });
            EUI.Space(2);
            ELayout.Scroll(ref paramsScrollPos, () =>
            {
                int addingIndex = 0;
                meshAndObjAnims[meshAndObjPaths[selectionIndex]].ForEach(p =>
                {
                    if (addingParam.Count < addingIndex + 1)
                    {
                        addingParam.Add(true);
                    }
                    ELayout.Horizontal(() =>
                    {
                        var key = $"{meshAndObjPaths[selectionIndex]}/{p}";
                        if (recordingParam.ContainsKey(key))
                        {
                            return;
                        }

                        var tempParam = addingParam[addingIndex];
                        EUI.ToggleWithLabel(ref tempParam, p, GUILayout.Width(ColumnSize));
                        addingParam[addingIndex] = tempParam;

                        switch (p)
                        {
                            case var isGO when p.Equals(FZToolsConstants.AnimClipParam.GameObjectIsActive):
                                // toggle
                                break;
                            case var isPos when p.Contains(FZToolsConstants.AnimClipParam.PositionKeyBase):
                                // float input
                                break;
                            case var isRot when p.Contains(FZToolsConstants.AnimClipParam.RotationKeyBase):
                                // float input
                                break;
                            case var isSca when p.Contains(FZToolsConstants.AnimClipParam.ScaleKeyBase):
                                // float input
                                break;
                            case var isMat when p.Contains(FZToolsConstants.AnimClipParam.MaterialKeyBase):
                                // objectField
                                break;
                            default:
                                var path = meshAndObjPaths[selectionIndex];
                                var smr = avatar.transform.Find(path).gameObject.GetComponent<SkinnedMeshRenderer>();
                                var shapeIndex = smr.sharedMesh.GetBlendShapeIndex(p);

                                float bsw = float.Parse(animParams[meshAndObjPaths[selectionIndex]][p]);
                                EUI.Slider(ref bsw, min: 0, max: 100);
                                // smr.SetBlendShapeWeight(shapeIndex, bsw);
                                animParams[meshAndObjPaths[selectionIndex]][p] = $"{bsw}";
                                addingIndex++;
                                break;
                        }
                        EUI.Button("追加", () =>
                        {
                            if (!recordingParam.ContainsKey(key))
                            {
                                recordingParam.Add($"{key}", $"{animParams[meshAndObjPaths[selectionIndex]][p]}");
                            }
                        }, GUILayout.Width(48));
                    });
                });
            }, RowSize);
            EUI.Space(2);
        }

        private void AddingParameterPreviewUI()
        {
            EUI.ScrollableBoxLabel(ref addingParamsPreviewLabelScrollPos, () =>
            {
                recordingParam.ToList().ForEach(kv =>
                {
                    ELayout.Horizontal(() =>
                    {
                        var baseWidth = (position.size.x - 64) / 8;
                        EUI.Label(kv.Key, GUILayout.Width(baseWidth * 6.5f));
                        EUI.Label(kv.Value, GUILayout.Width(baseWidth));
                        EUI.Button("戻す", () =>
                        {
                            recordingParam.Remove(kv.Key);
                        }, GUILayout.Width(48));
                    });
                });
            }, GUILayout.Height(RowSize));
            EUI.Space(2);
        }

        private void InitVariables()
        {
            meshPaths = new List<string>();
            meshAndObjPaths = new List<string>();
            meshPaths = RenderersObjPath;
            meshAndObjPaths = RenderersObjPath.Concat(ClothAndAccessoryRootObjPath).ToList();

            meshAndObjAnims = new Dictionary<string, List<string>>();
            animParams = new Dictionary<string, Dictionary<string, string>>();

            meshAndObjPaths.ForEach(maop =>
            {
                animParams.Add(maop, new Dictionary<string, string>());
                var obj = avatar.transform.Find(maop).gameObject;

                if (obj.GetComponent<SkinnedMeshRenderer>() != null)
                {
                    var smr = obj.GetComponent<SkinnedMeshRenderer>();
                    var editParams = new List<string>();
                    if (smr.sharedMesh.blendShapeCount > 0)
                    {
                        editParams.AddRange(Enumerable.Range(0, smr.sharedMesh.blendShapeCount).Select(i => smr.sharedMesh.GetBlendShapeName(i)).ToList());
                    }
                    for (int i = smr.sharedMaterials.Length - 1; i >= 0; i--)
                    {
                        editParams.Insert(0, FZToolsConstants.AnimClipParam.MaterialReference(i));
                    }
                    editParams.Insert(0, FZToolsConstants.AnimClipParam.GameObjectIsActive);
                    meshAndObjAnims.Add(maop, editParams);

                    editParams.ForEach(ep =>
                    {
                        if (!recordingParam.ContainsKey(ep))
                            animParams[maop].Add(ep, ep.Equals(FZToolsConstants.AnimClipParam.GameObjectIsActive) ? $"{true}" : "0");
                    });
                }
                else // gameObject or mesh renderer
                {
                    meshAndObjAnims.Add(maop, new List<string>() {
                        FZToolsConstants.AnimClipParam.GameObjectIsActive,
                        FZToolsConstants.AnimClipParam.Position(FZToolsConstants.AnimClipParam.Axis.x),
                        FZToolsConstants.AnimClipParam.Position(FZToolsConstants.AnimClipParam.Axis.y),
                        FZToolsConstants.AnimClipParam.Position(FZToolsConstants.AnimClipParam.Axis.z),
                        FZToolsConstants.AnimClipParam.Rotation(FZToolsConstants.AnimClipParam.Axis.x),
                        FZToolsConstants.AnimClipParam.Rotation(FZToolsConstants.AnimClipParam.Axis.y),
                        FZToolsConstants.AnimClipParam.Rotation(FZToolsConstants.AnimClipParam.Axis.z),
                        FZToolsConstants.AnimClipParam.Scale(FZToolsConstants.AnimClipParam.Axis.x),
                        FZToolsConstants.AnimClipParam.Scale(FZToolsConstants.AnimClipParam.Axis.y),
                        FZToolsConstants.AnimClipParam.Scale(FZToolsConstants.AnimClipParam.Axis.z)
                    });

                    meshAndObjAnims[maop].ForEach(v =>
                    {
                        animParams[maop].Add(v, v.Equals(FZToolsConstants.AnimClipParam.GameObjectIsActive) ? $"{true}" : "0");
                    });
                }
            });
        }

        private void LoadRecordingParams()
        {
            var curves = AnimationUtility.GetCurveBindings(animationClip);
            curves.ToList().ForEach(c =>
            {
                var curve = AnimationUtility.GetEditorCurve(animationClip, c);
                var value = curve[0].value;
                var recValue = c.propertyName.Equals(FZToolsConstants.AnimClipParam.GameObjectIsActive) ? value == 0 ? $"{false}" : $"{true}" : $"{value}";
                recordingParam.Add($"{c.path}/{c.propertyName.Replace(FZToolsConstants.AnimClipParam.BlendShape(""), "")}", recValue);
            });
        }

        private void ResetParams()
        {
            recordingParam = new Dictionary<string, string>();
            addingParam = new List<bool>();
        }

        private void CreateAndUpdateAnimationClip()
        {
            var ac = new AnimationClip();
            var meshPaths = new List<List<string>>() { RenderersObjPath, ClothAndAccessoryRootObjPath };

            for (int i = 0; i < meshPaths.Count(); i++)
            {
                var pathList = recordingParam.ToList();
                pathList.ForEach(kv =>
                {
                    var isGameObject = kv.Key.Contains(FZToolsConstants.AnimClipParam.GameObjectIsActive);
                    var isRendererEnabled = kv.Key.Contains(FZToolsConstants.AnimClipParam.MeshEnabled);

                    var sprKey = kv.Key.Split('/');
                    var objPath = string.Join("/", sprKey.Take(sprKey.Length - 1));
                    var paramName = sprKey.Last();
                    if (!isGameObject && !isRendererEnabled)
                    {
                        paramName = FZToolsConstants.AnimClipParam.BlendShape(paramName);
                    }
                    var paramVal = !isGameObject && !isRendererEnabled ? int.Parse(kv.Value) : (kv.Value == $"{true}" ? 1 : 0);
                    var paramType = isGameObject ? typeof(GameObject) : isRendererEnabled ? typeof(Renderer) : typeof(SkinnedMeshRenderer);

                    ac.AddAnimationCurve(
                        new Keyframe(0, paramVal),
                        objPath,
                        paramName,
                        paramType
                    );
                });
            }

            var isCreate = animationClip == null;
            var saveFilePath = isCreate ? $"{AnimationClipOutputPath}/AnimationClipEditor/autocreate001.anim" : AssetDatabase.GetAssetPath(animationClip);
            Debug.LogWarning(string.Join("/", saveFilePath.Split('/').Take(saveFilePath.Split('/').Length - 1)));
            AssetUtils.CreateDirectoryRecursive(string.Join("/", saveFilePath.Split('/').Take(saveFilePath.Split('/').Length - 1)));
            AssetUtils.CreateAsset(ac, saveFilePath);
        }
    }
}