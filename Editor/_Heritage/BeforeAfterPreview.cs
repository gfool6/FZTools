// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
// using UnityEditor;
// using UnityEditor.Animations;
// using VRC.SDK3.Avatars.Components;
// using EUI = FZTools.EditorUtils.UI;
// using ELayout = FZTools.EditorUtils.Layout;
// using FZTools;

// class BeforeAfterPreview : EditorWindow
// {
//     int selectionMeshIndex;
//     int selectedMode;
//     float beforeZoomValue = 0f;
//     float beforeRotateValue = 0f;
//     float beforePositionValue = 0f;
//     float totalAngleBefore = 0f;
//     float totalAngleAfter = 0f;
//     FZPreviewRenderer meshPreviewRendererBefore;
//     FZPreviewRenderer meshPreviewRendererAfter;
//     Renderer currentBeforePreviewObject;
//     Renderer currentAfterPreviewObject;
//     Vector3 beforePreviewCameraBasePosition;
//     Quaternion beforePreviewCameraBaseRotation;
//     Vector3 afterPreviewCameraBasePosition;
//     Quaternion afterPreviewCameraBaseRotation;
//     List<Material> SwapMaterialsBefore = new List<Material>();
//     List<Material> SwapMaterialsAfter = new List<Material>();

//     SkinnedMeshRenderer FaceMesh => AvatarDescriptor.GetVRCAvatarFaceMeshRenderer();

//     private void MeshPreviewUI()
//     {
//         ELayout.Vertical(() =>
//         {
//             var tempSI = selectionMeshIndex;
//             EUI.Popup(ref selectionMeshIndex, meshPaths.Select(p => p.Replace("/", "\u2215")).ToArray());
//             if (tempSI != selectionMeshIndex)
//             {
//                 ResetPreview();
//                 SwapMaterialsBefore = new List<Material>();
//                 SwapMaterialsAfter = new List<Material>();
//             }
//             EUI.Space(2);

//             MeshPreview();
//             MeshPreviewSlider();

//             MaterialsUI(Renderers[selectionMeshIndex]);
//             ReplaceMaterialUI(currentAfterPreviewObject);
//         }, GUILayout.ExpandWidth(true));
//         EUI.Space(2);
//     }

//     private void MeshPreview()
//     {
//         ELayout.Horizontal(() =>
//         {
//             EUI.Space();
//             ELayout.Vertical(() =>
//             {
//                 EUI.MiniLabel("Before", GUILayout.Width(ColumnSize), GUILayout.Height(24));
//                 PreviewBeforeMesh(Renderers[selectionMeshIndex]);

//             });
//             ELayout.Vertical(() =>
//             {
//                 EUI.MiniLabel("After", GUILayout.Width(ColumnSize), GUILayout.Height(24));
//                 PreviewAfterMesh(Renderers[selectionMeshIndex]);
//             });
//         }, GUILayout.Height(ColumnSize), GUILayout.ExpandWidth(true));
//         EUI.Space(2);
//     }

//     private void MeshPreviewSlider()
//     {
//         ELayout.Horizontal(() =>
//         {
//             EUI.Space();
//             ELayout.Vertical(() =>
//             {
//                 var tempbzv = beforeZoomValue;
//                 var tempbrv = beforeRotateValue;
//                 var tempbpv = beforePositionValue;
//                 ELayout.Horizontal(() =>
//                 {
//                     EUI.MiniLabel("拡縮", GUILayout.Width(24));
//                     EUI.Slider(ref beforeZoomValue, 0, 1, GUILayout.Width(ColumnSize * 2));
//                 }, GUILayout.Width(ColumnSize * 2 + 48));
//                 ELayout.Horizontal(() =>
//                 {
//                     EUI.MiniLabel("回転", GUILayout.Width(24));
//                     EUI.Slider(ref beforeRotateValue, -1, 1, GUILayout.Width(ColumnSize * 2));
//                 }, GUILayout.Width(ColumnSize * 2 + 48));
//                 ELayout.Horizontal(() =>
//                 {
//                     EUI.MiniLabel("高さ", GUILayout.Width(24));
//                     EUI.Slider(ref beforePositionValue, -10, 10, GUILayout.Width(ColumnSize * 2));
//                 }, GUILayout.Width(ColumnSize * 2 + 48));

//                 meshPreviewRendererBefore.Camera.transform.position = Zoom(meshPreviewRendererBefore.Camera.transform.position, currentBeforePreviewObject.transform.position, beforeZoomValue);
//                 meshPreviewRendererAfter.Camera.transform.position = Zoom(meshPreviewRendererAfter.Camera.transform.position, currentAfterPreviewObject.transform.position, beforeZoomValue);
//                 Rotate(meshPreviewRendererBefore.Camera.transform, currentBeforePreviewObject.transform.position, beforeRotateValue, ref totalAngleBefore);
//                 Rotate(meshPreviewRendererAfter.Camera.transform, currentAfterPreviewObject.transform.position, beforeRotateValue, ref totalAngleAfter);
//                 if (tempbpv != beforePositionValue)
//                 {
//                     meshPreviewRendererBefore.SetCameraPosition(beforePreviewCameraBasePosition + new Vector3(0, beforePositionValue, 0));
//                     meshPreviewRendererAfter.SetCameraPosition(afterPreviewCameraBasePosition + new Vector3(0, beforePositionValue, 0));
//                 }
//             });
//             EUI.Space();
//         }, GUILayout.ExpandWidth(true));
//         EUI.Space(2);
//     }

//     private void MaterialsUI(Renderer renderer)
//     {
//         ELayout.Horizontal(() =>
//         {
//             EUI.Space();
//             if (SwapMaterialsBefore.Count == 0)
//             {
//                 SwapMaterialsBefore = new List<Material>(renderer.sharedMaterials);
//             }
//             ELayout.Vertical(() =>
//             {
//                 EUI.MiniLabel("差し替え前");
//                 for (int i = 0; i < SwapMaterialsBefore.Count; i++)
//                 {
//                     ELayout.Horizontal(() =>
//                     {
//                         EUI.MiniLabel($"Material [{i}]", GUILayout.Width(60));
//                         SwapMaterialsBefore[i] = (Material)EditorGUILayout.ObjectField(SwapMaterialsBefore[i], typeof(Material), true, GUILayout.Width(ColumnSize * 2));
//                     }, GUILayout.Width(ColumnSize * 2 + 48));
//                 }
//             });
//             EUI.Space();
//         }, GUILayout.ExpandWidth(true));
//         EUI.Space();
//     }

//     private void ReplaceMaterialUI(Renderer renderer)
//     {
//         ELayout.Horizontal(() =>
//         {
//             EUI.Space();
//             if (SwapMaterialsAfter.Count == 0)
//             {
//                 SwapMaterialsAfter = new List<Material>(renderer.sharedMaterials);
//             }
//             ELayout.Vertical(() =>
//             {
//                 EUI.MiniLabel("差し替え後");
//                 for (int i = 0; i < SwapMaterialsAfter.Count; i++)
//                 {
//                     ELayout.Horizontal(() =>
//                     {
//                         EUI.MiniLabel($"Material [{i}]", GUILayout.Width(60));
//                         EUI.ChangeCheck(() =>
//                         {
//                             SwapMaterialsAfter[i] = (Material)EditorGUILayout.ObjectField(SwapMaterialsAfter[i], typeof(Material), true, GUILayout.Width(ColumnSize * 2));
//                         },
//                         () =>
//                         {
//                             Debug.LogWarning("Changed");
//                             meshPreviewRendererAfter.GetPreviewObjectComponent<Renderer>().sharedMaterials[i] = SwapMaterialsAfter[i];
//                             Debug.LogWarning(meshPreviewRendererAfter.GetPreviewObjectComponent<Renderer>().sharedMaterials[i].name);
//                             Debug.LogWarning(SwapMaterialsAfter[i].name);
//                         });
//                     }, GUILayout.Width(ColumnSize * 2 + 48));
//                 }
//             });
//             EUI.Space();
//         }, GUILayout.ExpandWidth(true));
//         EUI.Space();
//     }

//     private Vector3 Zoom(Vector3 cameraPosition, Vector3 objectPosition, float zoomVal)
//     {
//         var maxDistance = 10;
//         var targetDistance = maxDistance - (maxDistance * zoomVal); // 距離算出 zoomValは正規化した値
//         var positionDiffs = cameraPosition - objectPosition; // カメラとオブジェクトの距離（Vector計算）
//         return positionDiffs.normalized * targetDistance; // 正規化したベクトルに倍率かけてズームイン/アウトの計算
//     }

//     private void Rotate(Transform cameraTransform, Vector3 objectPosition, float rotateVal, ref float totalAngle)
//     {
//         // 必ず0~360°の値を取るよう計算　回転量 - totalがminusになる場合、total+=newは-=になるのでtotalは0~360の値を取れるようになる
//         var newAnglex = 360 * rotateVal - totalAngle;
//         totalAngle += newAnglex;
//         cameraTransform.RotateAround(objectPosition, Vector3.up, newAnglex); // これは普通にRotateAround　今回の場合はY軸
//     }

//     private void PreviewBeforeMesh(Renderer renderer)
//     {
//         if (meshPreviewRendererBefore == null)
//         {
//             meshPreviewRendererBefore = new FZPreviewRenderer(Instantiate(renderer.gameObject));

//             var headBone = avatar.GetBoneRootObject().GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name.ToLower().Contains("head"));
//             var headPosition = headBone.position + new Vector3(0, headBone.position.y * 0.04f, 1 * avatar.transform.localScale.z * headBone.localScale.z);
//             meshPreviewRendererBefore.SetCameraPosition(headPosition + new Vector3(0, -headPosition.y / 2.2f, headPosition.y * 6));

//             beforePreviewCameraBasePosition = meshPreviewRendererBefore.Camera.transform.position;
//             beforePreviewCameraBaseRotation = meshPreviewRendererBefore.Camera.transform.rotation;
//         }
//         Preview(meshPreviewRendererBefore, PreviewThumbnailSize);
//         currentBeforePreviewObject = meshPreviewRendererBefore.GetPreviewObjectComponent<Renderer>();

//     }

//     private void PreviewAfterMesh(Renderer renderer)
//     {
//         if (meshPreviewRendererAfter == null)
//         {
//             meshPreviewRendererAfter = new FZPreviewRenderer(Instantiate(renderer.gameObject));

//             var headBone = avatar.GetBoneRootObject().GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name.ToLower().Contains("head"));
//             var headPosition = headBone.position + new Vector3(0, headBone.position.y * 0.04f, 1 * avatar.transform.localScale.z * headBone.localScale.z);
//             meshPreviewRendererAfter.SetCameraPosition(headPosition + new Vector3(0, -headPosition.y / 2.2f, headPosition.y * 6));

//             afterPreviewCameraBasePosition = meshPreviewRendererAfter.Camera.transform.position;
//             afterPreviewCameraBaseRotation = meshPreviewRendererAfter.Camera.transform.rotation;
//         }
//         Preview(meshPreviewRendererAfter, PreviewThumbnailSize);
//         currentAfterPreviewObject = meshPreviewRendererAfter.GetPreviewObjectComponent<Renderer>();
//     }

//     int PreviewThumbnailSize => (int)(ColumnSize);

//     private void Preview(FZPreviewRenderer previewRenderer, int previewSize)
//     {
//         if (previewRenderer == null)
//         {
//             return;
//         }
//         previewRenderer.RenderPreview(previewSize, previewSize);
//         EditorGUILayout.LabelField(new GUIContent(previewRenderer.renderTexture), GUILayout.Width(previewSize), GUILayout.Height(previewSize));
//         Repaint();
//     }

//     private void ResetPreview()
//     {
//         if (meshPreviewRendererBefore != null)
//         {
//             meshPreviewRendererBefore.EndPreview();
//             meshPreviewRendererBefore = null;
//         }
//         beforeZoomValue = 0f;
//         beforeRotateValue = 0f;
//         beforePositionValue = 0f;
//         totalAngleBefore = 0;

//         if (meshPreviewRendererAfter != null)
//         {
//             meshPreviewRendererAfter.EndPreview();
//             meshPreviewRendererAfter = null;
//         }
//         totalAngleAfter = 0;
//     }

// }