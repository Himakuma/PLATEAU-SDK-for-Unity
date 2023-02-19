using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace PLATEAU
{

    public class SelectMapRangeElement : MouseOverElement
    {
        public new class UxmlFactory : UxmlFactory<SelectMapRangeElement> { }


        private Dictionary<string, Vector2> defaultPointPositions = new Dictionary<string, Vector2>();



        private float minSize = 20f;


        // TODO:borderの値が何故か取れないので、暫定で固定
        private float borderSize = 4f;


        public SelectMapRangeElement()
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.synesthesias.plateau-unity-sdk/Editor/EditorWindow/MapView/SelectMapRange.uss");
            styleSheets.Add(styleSheet);
            AddToClassList("range");

            var leftTop = CreatePoint("range-point-left-top", false, false);
            var leftBottom = CreatePoint("range-point-left-bottom", false, true);
            var rightTop = CreatePoint("range-point-right-top", true, false);
            var rightBottom = CreatePoint("range-point-right-bottom", true, true);
            this.AddManipulator(new FilterDragger(() =>
            {
                return !(leftTop.IsMouseOver || leftBottom.IsMouseOver || rightTop.IsMouseOver || rightBottom.IsMouseOver);
            }));
        }


        private MouseOverElement CreatePoint(string name, bool leftLock, bool topLock)
        {
            var point = new MouseOverElement();
            point.name = name;
            point.AddToClassList("range-point");
            point.AddManipulator(new MoveDragger((e, target) => Resize(target, leftLock, topLock)));

            Add(point);
            return point;
        }



        private void Resize(VisualElement target, bool leftLock = false, bool topLock = false)
        {
            float targetLeft = target.style.left.value.value;
            float targetTop = target.style.top.value.value;

            float x = layout.size.x - borderSize;
            float y = layout.size.y - borderSize;

            float width = layout.size.x - targetLeft;
            float height =layout.size.y - targetTop;

            if (leftLock) 
            {
                x = 0f ;
                width = targetLeft;
            }

            if (topLock) 
            {
                y = 0f ;
                height = targetTop;
            }


            if (Mathf.Approximately(x, targetLeft) || (!leftLock && x <= targetLeft) || (leftLock && targetLeft <= x))
            {
                targetLeft = x;
                width = 0f;
            }

            if (Mathf.Approximately(y, targetTop) || (!topLock && y <= targetTop) || (topLock && targetTop <= y))
            {
                targetTop = y;
                height = 0f;
            }

            if (!leftLock) 
            {
                style.left = targetLeft + style.left.value.value;
            }

            if (!topLock) 
            {
                style.top = targetTop + style.top.value.value;
            }
            
            style.width = width;
            style.height = height;

            target.style.left = StyleKeyword.Null;
            target.style.top = StyleKeyword.Null;
        }
    }
}
