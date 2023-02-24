using Codice.Client.BaseCommands;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;



namespace PLATEAU
{

    public class PlateauMapView : EditorWindow
    {
        private VisualElement map;

        private SelectMapRangeElement mapRange;

        private SliderInt scale;

        private Label scaleLabel;


        [MenuItem("PLATEAU/PlateauMapView-Test")]
        public static void ShowExample()
        {

            PlateauMapView wnd = GetWindow<PlateauMapView>();
            wnd.titleContent = new GUIContent("PlateauMapView");
        }





        public void CreateGUI()
        {
            var root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.synesthesias.plateau-unity-sdk/Editor/EditorWindow/MapView/PlateauMapView.uxml");
            rootVisualElement.Add(visualTree.Instantiate());

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.synesthesias.plateau-unity-sdk/Editor/EditorWindow/MapView/PlateauMapView.uss");
            root.styleSheets.Add(styleSheet);

            scale = root.Q<SliderInt>("Scale");
            scale.RegisterValueChangedCallback(ScaleChange);

            scaleLabel = root.Q<Label>("ScaleLabel");
            scaleLabel.text = scale.value.ToString();

            map = root.Q("Map");
            map.RegisterCallback<GeometryChangedEvent>(Init);
            map.RegisterCallback<WheelEvent>((e) => scale.value += Math.Sign(e.delta.y));
            map.RegisterCallback<ClickEvent>((e) =>
            {
                if (e.clickCount < 2 || mapRange.IsMouseOver) return;

                var center = mapRange.contentRect.center;
                mapRange.style.left = e.position.x - center.x;
                mapRange.style.top = e.position.y - center.y;
            });

            mapRange = root.Q<SelectMapRangeElement>("MapRange");
        }

        private void Init(GeometryChangedEvent e) 
        {
            map.UnregisterCallback<GeometryChangedEvent>(Init);
            map.style.width = position.width;
            map.style.height = position.height;

            // 高さが０になる、2021のバグで、2022で修正
            //var mapRangePosition = map.layout.center - mapRange.layout.center;
            scale.style.minHeight = position.height / 2;

            var mapRangePosition = (new Vector2(position.width, position.height) / 2) - mapRange.layout.center;
            mapRange.style.left = mapRangePosition.x;
            mapRange.style.top = mapRangePosition.y;
        }


        private void ScaleChange(ChangeEvent<int> e)
        {
            scaleLabel.text = e.newValue.ToString();

        }




        /**
スケール
・世界地図
・日本地図
・日本地図（拡大１）
・日本地図（拡大２）
・日本地図（拡大３）
・日本地図（拡大４）
・日本地図（拡大５）

         */





    }
}