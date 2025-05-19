using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace FZTools
{
    [CreateAssetMenu(fileName = "FZPreset", menuName = "FZTools/FZPreset", order = 0)]
    public class FZPreset : ScriptableObject
    {
        [SerializeField]
        public List<FZLayerPreset> layerPresets;
    }


}