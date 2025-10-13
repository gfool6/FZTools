using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace FZTools
{
    [Serializable]
    [CreateAssetMenu(fileName = "FZBulkMenuCreatorMenuItem", menuName = "FZTools/FZBulkMenuCreatorMenuItem", order = 0)]
    public class FZBulkCreateMenu : ScriptableObject
    {
        public string menuName;
        public List<FZBulkCreateMenuItem> menuItems;
        

        [Serializable]
        public class FZBulkCreateMenuItem
        {
            public string menuItemName;
            public MenuType menuType;
            public bool defaultValue = true;
            public GameObject targetObject;
        }

        public enum MenuType
        {
            Toggle,
            Choose,
            Radial,

        }
    }
}