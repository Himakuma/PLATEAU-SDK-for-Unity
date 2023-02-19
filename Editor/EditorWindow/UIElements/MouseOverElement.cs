using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace PLATEAU
{
    public class MouseOverElement : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<SelectMapRangeElement> { }

        public bool IsMouseOver { get; private set; } = false;


        public MouseOverElement()
        {
            RegisterCallback<MouseOverEvent>((e) => 
            {
                IsMouseOver = true;
            });
            RegisterCallback<MouseOutEvent>((e) => 
            {
                IsMouseOver = false;
            });

        }

    }
}
