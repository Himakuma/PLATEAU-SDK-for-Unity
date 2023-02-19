using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;



namespace PLATEAU
{

    public class PlateauMapView : EditorWindow
    {
        private SelectMapRangeElement mapRange;

        private VisualElement map;


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
            root.Add(visualTree.Instantiate());

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.synesthesias.plateau-unity-sdk/Editor/EditorWindow/MapView/PlateauMapView.uss");
            root.styleSheets.Add(styleSheet);

            map = root.Query("Map");
            map.RegisterCallback<ClickEvent>((e) =>
            {
                if (e.clickCount < 2 || mapRange.IsMouseOver) return;

                var center = mapRange.contentRect.center;
                mapRange.style.left = e.position.x - center.x;
                mapRange.style.top = e.position.y - center.y;
            });

            mapRange = root.Query<SelectMapRangeElement>("MapRange");
            mapRange.style.left = 100;
            mapRange.style.top = 100;




        }


        private void OnGUI()
        {
        }

        private void Update()
        {
            Repaint();
        }
    }
}