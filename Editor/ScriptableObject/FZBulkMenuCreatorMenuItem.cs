using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace FZTools
{
    [Serializable]
    // [CreateAssetMenu(fileName = "FZBulkMenuCreatorMenuItem", menuName = "FZTools/FZBulkMenuCreatorMenuItem", order = 0)]
    public class FZBulkCreateMenu
    {
        [SerializeField]
        public string menuName;
        [SerializeField]
        public MenuType menuType;
        [SerializeField]
        public List<FZBulkCreateMenuItem> menuItems;


        [Serializable]
        public class FZBulkCreateMenuItem
        {

            [SerializeField]
            public string menuItemName;
            [SerializeField]
            public MenuItemType itemType;
            [SerializeField]
            public bool defaultValue = true;
            [SerializeField]
            public string paramName;
            [SerializeField]
            public int paramValue;
            [SerializeField]
            public GameObject targetObject;
        }

        public enum MenuType
        {
            SubMenu,
            Single,
        }

        public enum MenuItemType
        {
            Toggle,
            Choose,
            Radial,

        }
    }
}