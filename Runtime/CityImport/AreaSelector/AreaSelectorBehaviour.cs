﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PLATEAU.CityImport.AreaSelector.SceneObjs;
using PLATEAU.Geometries;
using PLATEAU.Dataset;
using PLATEAU.Native;
using PLATEAU.Util;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PLATEAU.CityImport.AreaSelector
{
    /// <summary>
    /// 範囲選択画面の進行を担当するコンポーネントです。
    /// 別途 AreaSelectorBehaviourEditor も参照してください。
    /// </summary>
    [ExecuteAlways]
    internal class AreaSelectorBehaviour : MonoBehaviour
    {
        [SerializeField] private string prevScenePath;
        [SerializeField] private DatasetSourceConfig datasetSourceConfig;
        private AreaSelectGizmosDrawer gizmosDrawer;
        private IAreaSelectResultReceiver areaSelectResultReceiver;
        private PredefinedCityModelPackage availablePackageFlags;
        private int coordinateZoneID;
        private GeoReference geoReference;
        private bool prevSceneCameraRotationLocked;
        private GSIMapLoaderZoomSwitch mapLoader;
        #if UNITY_EDITOR
        private EditorWindow prevEditorWindow;
        #endif


        public static bool IsAreaSelectEnabled { get; set; }

#if UNITY_EDITOR
        public void Init(string prevScenePathArg, DatasetSourceConfig datasetSourceConfigArg, IAreaSelectResultReceiver areaSelectResultReceiverArg, int coordinateZoneIDArg, EditorWindow prevEditorWindowArg)
        {
            IsAreaSelectEnabled = true;
            this.prevScenePath = prevScenePathArg;
            this.datasetSourceConfig = datasetSourceConfigArg;
            this.areaSelectResultReceiver = areaSelectResultReceiverArg;
            this.coordinateZoneID = coordinateZoneIDArg;
            this.prevSceneCameraRotationLocked = SceneView.lastActiveSceneView.isRotationLocked;
            this.prevEditorWindow = prevEditorWindowArg;
        }
#endif

        private void Start()
        {
            AreaSelectorGUI.Enable(this);
#if UNITY_EDITOR
            EditorUtility.DisplayProgressBar("", "データファイルを検索中です...", 0f);
#endif
            ReadOnlyCollection<MeshCode> meshCodes;
            PredefinedCityModelPackage packageFlags;
            try
            {
                GatherMeshCodes(this.datasetSourceConfig, out meshCodes, out packageFlags);
            }
            catch (Exception e)
            {
                const string errorMessage = "メッシュコードの取得に失敗しました。";
                Debug.LogError($"{errorMessage}\n{e}");
                #if UNITY_EDITOR
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("PLATEAU", errorMessage, "OK");
                #endif
                CancelAreaSelection();
                return;
            }

            var drawerObj = new GameObject($"{nameof(AreaSelectGizmosDrawer)}");
            this.gizmosDrawer = drawerObj.AddComponent<AreaSelectGizmosDrawer>();
            this.gizmosDrawer.Init(meshCodes, this.datasetSourceConfig, this.coordinateZoneID, out this.geoReference);
            this.availablePackageFlags = packageFlags;
            var entireExtent = CalcExtentCoversAllMeshCodes(meshCodes);
            this.mapLoader = new GSIMapLoaderZoomSwitch(this.geoReference, entireExtent);
            SetInitialCamera(entireExtent);
        }

        private void SetInitialCamera(Extent entireExtent)
        {
            #if UNITY_EDITOR
            RotateSceneViewCameraDown();
            var centerPos = this.geoReference.Project(entireExtent.Center).ToUnityVector();
            var initialCameraPos = new Vector3(centerPos.x, 0, centerPos.z);
            SceneView.lastActiveSceneView.pivot = initialCameraPos;
            // シーンビューのカメラが全体を映すようにします。
            SceneView.lastActiveSceneView.size = Mathf.Abs((float)(this.geoReference.Project(entireExtent.Max).Z - this.geoReference.Project(entireExtent.Min).Z) / 2f);
            #endif
        }

        private void Update()
        {
            // カメラを下に向けます。
            RotateSceneViewCameraDown();

#if UNITY_EDITOR
            this.mapLoader.Update(SceneView.lastActiveSceneView.camera);
#endif
        }

        private void OnRenderObject()
        {
            // [ExecuteAlways] を付けたクラスは Editモードでも Updateが呼ばれるとはいえ、
            // その呼び出し頻度は低い（何か変更しないと呼び出されない）ため、地図読込に悪影響があります。
            // そこで描画のたびに Update を回すようにします。
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            SceneView.lastActiveSceneView.isRotationLocked = this.prevSceneCameraRotationLocked;
#endif
            this.mapLoader?.Dispose();
        }

        private static Extent CalcExtentCoversAllMeshCodes(IEnumerable<MeshCode> meshCodes)
        {
            var entireMin = new GeoCoordinate(90, 180, 9999);
            var entireMax = new GeoCoordinate(-90, -180, -9999);
            foreach (var meshCode in meshCodes)
            {
                var areaMin = meshCode.Extent.Min;
                var areaMax = meshCode.Extent.Max;
                entireMin = GeoCoordinate.Min(entireMin, areaMin);
                entireMax = GeoCoordinate.Max(entireMax, areaMax);
            }

            var entireExtent = new Extent(entireMin, entireMax);
            return entireExtent;
        }

        private static void GatherMeshCodes(DatasetSourceConfig datasetSourceConfig, out ReadOnlyCollection<MeshCode> meshCodes, out PredefinedCityModelPackage availablePackageFlags)
        {
            using var datasetSource = DatasetSource.Create(datasetSourceConfig);
            using var accessor = datasetSource.Accessor;
            // ローカルモードではうまくいきますが、サーバーモードでは Packages は None になります。
            availablePackageFlags = accessor.Packages;
            meshCodes = new ReadOnlyCollection<MeshCode>(accessor.MeshCodes.Where(code => code.IsValid).ToArray());
            if (meshCodes.Count <= 0)
            {
                Debug.LogError("No MeshCode found.");
            }
        }

        

        internal void EndAreaSelection()
        {
            IsAreaSelectEnabled = false;
            AreaSelectorGUI.Disable();
            var areaSelectResult = this.gizmosDrawer.SelectedMeshCodes;
            var selectedExtent = this.gizmosDrawer.CursorExtent(this.coordinateZoneID, this.geoReference.ReferencePoint);
            // 無名関数のキャプチャを利用して、シーン終了後も必要なデータが渡るようにします。
            #if UNITY_EDITOR
            AreaSelectorDataPass.Exec(this.prevScenePath, areaSelectResult, this.areaSelectResultReceiver, this.availablePackageFlags, selectedExtent, this.prevEditorWindow);
            #endif
        }

        internal void CancelAreaSelection()
        {
            AreaSelectorGUI.Disable();
            IsAreaSelectEnabled = false;
            var emptyAreaSelectResult = new MeshCode[] { };
            var dummyExtent = Extent.All;
            #if UNITY_EDITOR
            AreaSelectorDataPass.Exec(this.prevScenePath, emptyAreaSelectResult, this.areaSelectResultReceiver, this.availablePackageFlags, dummyExtent, this.prevEditorWindow);
            #endif
        }

        private static void RotateSceneViewCameraDown()
        {
            #if UNITY_EDITOR
            SceneView.lastActiveSceneView.in2DMode = false; // 2Dモードだと上から見下ろすカメラにできないため
            var scene = SceneView.lastActiveSceneView;
            scene.isRotationLocked = true;
            scene.rotation = Quaternion.Euler(90, 0, 0);
            #endif
        }
    }
}
