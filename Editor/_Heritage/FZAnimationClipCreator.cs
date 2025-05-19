// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
// using UnityEditor;
// using VRC.SDK3.Avatars.Components;
// using EUI = FZTools.EditorUtils.UI;
// using ELayout = FZTools.EditorUtils.Layout;

// namespace FZTools
// {
//     public class FZAnimationClipCreator : EditorWindow
//     {
//         #region Variable
//         [SerializeField]
//         private VRCAvatarDescriptor avatarDescripter;

//         private enum AnimationCreateorKind
//         {
//             [InspectorName(FZToolsConstants.LabelText.CreateFaceAnimationTemplate)]
//             FaceTemplate = 0,
//             [InspectorName(FZToolsConstants.LabelText.CreateMeshOnOffAnimation)]
//             MeshOnOff
//         };

//         string TargetAvatarName => avatarDescripter?.gameObject?.name;
//         string AnimationClipOutputPath => $"{FZToolsConstants.FZTools.OutputRootPath(TargetAvatarName)}/AnimationClip";
//         SkinnedMeshRenderer FaceMesh
//         {
//             get
//             {
//                 if (avatarDescripter == null) return null;

//                 var mesh = avatarDescripter.VisemeSkinnedMesh;
//                 mesh = mesh ?? avatarDescripter.GetComponentsInChildren<SkinnedMeshRenderer>(true)
//                                             .OrderByDescending(smr => smr.sharedMesh.blendShapeCount)
//                                             .First(smr => smr.gameObject.name.ToLower().Contains("face") || smr.gameObject.name.ToLower().Contains("body"));
//                 return mesh;
//             }
//         }
//         List<Renderer> Renderers => avatarDescripter.GetComponentsInChildren<Renderer>(true).ToList();
//         List<GameObject> ClothAndAccessoryRootObject => RenderersObjPath
//                                 .Select(n => n.Split('/')).Where(n => n.Count() >= 2).Select(n => string.Join("/", n.Take(n.Length - 1)))
//                                 .Distinct().Select(o => avatarDescripter.transform.Find(o).gameObject).ToList();
//         List<string> RenderersObjPath => Renderers.Select(e => e.gameObject.GetGameObjectPath(true)).ToList();
//         List<string> ClothAndAccessoryRootObjPath => ClothAndAccessoryRootObject.Select(o => o.GetGameObjectPath(true)).ToList();
//         List<string> VisemeBlendShapes => avatarDescripter.lipSync == VRCAvatarDescriptor.LipSyncStyle.VisemeBlendShape ? avatarDescripter?.VisemeBlendShapes?.ToList() : null;

//         AnimationCreateorKind kind;
//         List<bool> ignoreShapeFlags = new List<bool>();
//         List<bool> onOffAnimationCreateTarget = new List<bool>();
//         #endregion

//         #region EditorUI
//         [MenuItem("FZTools/AnimationClipCreator")]
//         private static void OpenWindow()
//         {
//             var window = GetWindow<FZAnimationClipCreator>();
//             window.titleContent = new GUIContent("FZAnimationClipCreator");
//         }

//         private void OnGUI()
//         {
//             ELayout.Horizontal(() =>
//             {
//                 EUI.Space();
//                 ELayout.Vertical(() =>
//                 {
//                     EUI.Space(2);
//                     EUI.Label("Target Avatar");
//                     EUI.Space();
//                     EUI.ObjectField<VRCAvatarDescriptor>(ref avatarDescripter);
//                     EUI.Space(2);
//                     string buttonLabel = "作成";
//                     Action onClick = () => { };
//                     EUI.Popup<AnimationCreateorKind>(ref kind);
//                     switch (kind)
//                     {
//                         case AnimationCreateorKind.FaceTemplate:
//                             FaceAnimationCreateGUI();
//                             buttonLabel = FZToolsConstants.LabelText.CreateFaceAnimationTemplate;
//                             onClick = CreateTemplateFaceAnimations;
//                             break;
//                         case AnimationCreateorKind.MeshOnOff:
//                             MeshOnOffAnimationCreateGUI();
//                             buttonLabel = FZToolsConstants.LabelText.CreateMeshOnOffAnimation;
//                             onClick = CreateMeshOnOffAnimations;
//                             break;
//                         default:
//                             break;
//                     }

//                     EUI.Space(2);
//                     EUI.Button(buttonLabel, onClick);
//                     EUI.Space(2);
//                 });
//                 // EUI.Space();
//             });
//         }

//         Vector2 FaceAnimScrollPos;
//         private void FaceAnimationCreateGUI()
//         {
//             if (avatarDescripter != null)
//             {
//                 EUI.Space(2);
//                 EUI.Label("Face Preset");
//                 EUI.Space();
//                 ELayout.Scroll(ref FaceAnimScrollPos, () =>
//                 {
//                     int count = FaceMesh.sharedMesh.blendShapeCount;
//                     for (int i = 0; i < count; i++)
//                     {
//                         var shapeName = FaceMesh.sharedMesh.GetBlendShapeName(i);
//                         if (ignoreShapeFlags.Count() < i + 1)
//                         {
//                             ignoreShapeFlags.Add(VisemeBlendShapes.Contains(shapeName));
//                         }
//                         ELayout.Horizontal(() =>
//                         {
//                             var temp = !ignoreShapeFlags[i];
//                             EUI.Toggle(ref temp);
//                             ignoreShapeFlags[i] = !temp;
//                             EUI.Label(shapeName);
//                             float bsw = FaceMesh.GetBlendShapeWeight(i);
//                             EUI.Slider(ref bsw);
//                             FaceMesh.SetBlendShapeWeight(i, bsw);
//                         });
//                     }
//                 });
//             }
//         }

//         Vector2 MeshOnOffScrollPos;
//         private void MeshOnOffAnimationCreateGUI()
//         {
//             if (avatarDescripter != null)
//             {
//                 EUI.Space(2);
//                 EUI.Label("Face Preset");
//                 EUI.Space();
//                 ELayout.Scroll(ref MeshOnOffScrollPos, () =>
//                 {
//                     EUI.Label("Meshes");
//                     EUI.Space();
//                     var countAcc = 0;
//                     var meshCount = Renderers.Count();
//                     for (int i = countAcc; i < meshCount; i++)
//                     {
//                         var mesh = Renderers[i];
//                         if (onOffAnimationCreateTarget.Count() < i + 1)
//                         {
//                             onOffAnimationCreateTarget.Add(mesh.gameObject.activeSelf);
//                         }
//                         ELayout.Horizontal(() =>
//                         {
//                             var temp = onOffAnimationCreateTarget[i];
//                             EUI.Toggle(ref temp);
//                             onOffAnimationCreateTarget[i] = temp;
//                             EUI.LabelButton(mesh.gameObject.GetGameObjectPath(true), () =>
//                             {
//                                 onOffAnimationCreateTarget[i] = !onOffAnimationCreateTarget[i];
//                             });
//                         });
//                     }
//                     countAcc += meshCount;

//                     EUI.Space();
//                     EUI.Label("GameObjects");
//                     EUI.Space();
//                     var carObj = ClothAndAccessoryRootObject;
//                     var carObjCount = carObj.Count();
//                     for (int i = countAcc; i < countAcc + carObjCount; i++)
//                     {
//                         var gameObject = carObj[i - countAcc];
//                         if (onOffAnimationCreateTarget.Count() < i + 1)
//                         {
//                             onOffAnimationCreateTarget.Add(gameObject.activeSelf);
//                         }
//                         ELayout.Horizontal(() =>
//                         {
//                             var temp = onOffAnimationCreateTarget[i];
//                             EUI.Toggle(ref temp);
//                             onOffAnimationCreateTarget[i] = temp;
//                             EUI.LabelButton(gameObject.GetGameObjectPath(true), () =>
//                             {
//                                 onOffAnimationCreateTarget[i] = !onOffAnimationCreateTarget[i];
//                             });
//                         });
//                     }
//                     countAcc += carObjCount;
//                 });
//             }
//         }
//         #endregion

//         #region  Creator
//         private void CreateTemplateFaceAnimations()
//         {
//             if (FaceMesh == null)
//             {
//                 Debug.LogError("対象のアバターをセットしてください");
//                 return;
//             }

//             var dirPath = $"{AnimationClipOutputPath}/Face";
//             AssetDatabase.DeleteAsset(dirPath);
//             AssetUtils.CreateDirectoryRecursive(dirPath);

//             var ignoreShapeNames = ignoreShapeFlags.Select((ignore, index) => ignore ? FaceMesh.sharedMesh.GetBlendShapeName(index) : null).Where(e => e != null).ToList();
//             FZToolsConstants.VRChat.HandGestures.ToList().ForEach(gesture =>
//             {
//                 new List<String>() { "L", "R" }.ForEach(hand =>
//                 {
//                     if (gesture == FZToolsConstants.VRChat.HandGesture.Neutral && hand.Equals("R")) return;
//                     var ac = new AnimationClip();
//                     ac.AddBlendShape(FaceMesh, ignoreShapeNames);
//                     var fileName = $"{gesture.ToString()}" + (gesture == FZToolsConstants.VRChat.HandGesture.Neutral ? $".anim" : $"_{hand}.anim");
//                     AssetUtils.CreateAsset(ac, $"{dirPath}/{fileName}");
//                 });
//             });
//         }

//         private void CreateMeshOnOffAnimations()
//         {
//             var dirPath = $"{AnimationClipOutputPath}/Expression";
//             AssetDatabase.DeleteAsset(dirPath);
//             AssetUtils.CreateDirectoryRecursive(dirPath);

//             var countAcc = 0;
//             countAcc += ExecCreateProcIfChecked(RenderersObjPath, dirPath, typeof(Renderer), countAcc);
//             countAcc += ExecCreateProcIfChecked(ClothAndAccessoryRootObjPath, dirPath, typeof(GameObject), countAcc);
//             CreateAllOnOffAnimationClip(dirPath);
//         }

//         private int ExecCreateProcIfChecked(List<string> meshPathList, string outputDir, Type type, int beginIndex)
//         {
//             var mplCount = meshPathList.Count();
//             for (int i = beginIndex; i < beginIndex + mplCount; i++)
//             {
//                 var index = beginIndex > 0 ? i - beginIndex : i;
//                 if (onOffAnimationCreateTarget[i])
//                 {
//                     CreateOnOffAnimationClip(outputDir, meshPathList[index], type);
//                 }
//             }
//             return mplCount;
//         }

//         private void CreateOnOffAnimationClip(string dirPath, string meshObjName, Type meshType)
//         {
//             new List<(float kfVal, string onOff)>() { (1, "on"), (0, "off") }.ForEach(o =>
//             {
//                 var ac = new AnimationClip();
//                 var tp = new List<(Type type, string propName)>()
//                 {
//                     (typeof(GameObject), FZToolsConstants.AnimClipParam.GameObjectIsActive)
//                 };
//                 if (meshType == typeof(Renderer))
//                     tp.Add((meshType, FZToolsConstants.AnimClipParam.MeshEnabled));

//                 tp.ForEach(t =>
//                 {
//                     ac.AddAnimationCurve(
//                         new Keyframe(0, o.kfVal),
//                         meshObjName,
//                         t.propName,
//                         t.type
//                     );
//                 });

//                 var parts = meshObjName.Split('/');
//                 var subDir = string.Join("_", parts.Take(parts.Count() - 1));
//                 var fileBaseName = parts[parts.Count() - 1];
//                 if (meshType != typeof(Renderer))
//                 {
//                     subDir = fileBaseName;
//                 }
//                 if (subDir.isNullOrEmpty())
//                 {
//                     subDir = TargetAvatarName;
//                 }
//                 Debug.LogWarning($"{dirPath}/{subDir}/{fileBaseName}_{o.onOff}.anim");
//                 AssetUtils.CreateDirectoryRecursive($"{dirPath}/{subDir}");
//                 AssetUtils.CreateAsset(ac, $"{dirPath}/{subDir}/{fileBaseName}_{o.onOff}.anim");
//             });
//         }

//         private void CreateAllOnOffAnimationClip(string dirPath)
//         {
//             var propPair = new List<(Type type, string propName)>()
//             {
//                 (typeof(GameObject), FZToolsConstants.AnimClipParam.GameObjectIsActive),
//                 (typeof(Renderer), FZToolsConstants.AnimClipParam.MeshEnabled)
//             };
//             var onOffPair = new List<(float kfVal, string onOff)>() { (1, "on"), (0, "off") };

//             onOffPair.ForEach(o =>
//             {
//                 var ac = new AnimationClip();
//                 var meshPaths = new List<List<string>>(){
//                     RenderersObjPath,
//                     ClothAndAccessoryRootObjPath
//                 };

//                 for (int i = 0; i < meshPaths.Count(); i++)
//                 {
//                     var pathList = meshPaths[i];
//                     pathList.ForEach(meshObjPath =>
//                     {
//                         propPair.ForEach(t =>
//                         {
//                             if (i == meshPaths.Count() - 1 && t.propName.Equals(FZToolsConstants.AnimClipParam.MeshEnabled)) return;
//                             ac.AddAnimationCurve(
//                                 new Keyframe(0, o.kfVal),
//                                 meshObjPath,
//                                 t.propName,
//                                 t.type
//                             );
//                         });
//                     });
//                 }
//                 AssetUtils.CreateAsset(ac, $"{dirPath}/AUTOCREATE_ALL_{o.onOff}.anim");
//             });
//         }
//         #endregion
//     }
// }