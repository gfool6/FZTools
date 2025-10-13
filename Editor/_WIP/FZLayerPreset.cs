using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace FZTools
{
    // [CreateAssetMenu(fileName = "FZLayerPreset", menuName = "FZTools/FZLayerPreset", order = 1)]
    public class FZLayerPreset : ScriptableObject
    {
        [Header("Layer Info")]
        [SerializeField]
        public string layerName;
        [SerializeField]
        public int layerIndex;

        [SerializeField]
        public List<FZAnimationPreset> animationPresets;
    }

    [System.SerializableAttribute]
    public class FZAnimationPreset
    {
        [SerializeField]
        public Motion motion; // animationclip or brendtree

        // exittime, writeEnable周りなども含め保持できるようにする
        [SerializeField]
        public ExitSetting exitSetting;

        [SerializeField]
        public List<FZPresetTransitionParam> transitionParams;
    }

    public enum ParameterVector
    {
        IN,
        OUT
    }
    public enum ParameterValueType
    {
        [InspectorName("bool")]
        BOOL = 0,
        [InspectorName("int")]
        INT = 1,
        [InspectorName("float")]
        FLOAT = 2
    }
    public enum ParameterCompareType
    {
        [InspectorName("Equal")]
        EQUAL = 0,
        [InspectorName("Not Equal")]
        NOTEQUAL = 1,
        [InspectorName("Greater")]
        GREATER = 2,
        [InspectorName("Less")]
        LESS = 3
    }

    [System.SerializableAttribute]
    public class ExitSetting
    {
        public bool hasExitTime;
        public float exitTime;
        public bool hasFixedDuration;
        public float duration;
        public float offset;
        public TransitionInterruptionSource interruptionSource = TransitionInterruptionSource.None;
        public bool orderedInterruption;
        public bool canTransitionToSelf;
    }

    [System.SerializableAttribute]
    public class FZPresetTransitionParam
    {
        [SerializeField]
        public string parameterName;
        [SerializeField]
        public ParameterVector inOut;
        [SerializeField]
        public ParameterValueType parameterValueType;
        [SerializeField]
        public ParameterCompareType parameterCompareType;
        [SerializeField]
        public string parameterValue;

        public FZPresetTransitionParam(
            string n,
            ParameterVector io,
            ParameterValueType vt,
            ParameterCompareType ct,
            string v
        )
        {
            parameterName = n;
            inOut = io;
            parameterValueType = vt;
            parameterCompareType = ct;
            parameterValue = v;
        }
    }
}