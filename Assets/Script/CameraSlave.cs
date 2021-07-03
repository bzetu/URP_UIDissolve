/*游戏从相机,使用Camera.slave获得
 用于配合处理一些后期效果
 create by jiangcheng_m
 */

using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GameCore
{
    public abstract class ICameraSlave
    {
        public enum CamRenderType
        { 
            SameWithOldCamera = -1,
            Base = 0,
            Overlay = 1,
        }

        protected Camera mCamera;

        protected UniversalAdditionalCameraData mCameraData;
        protected abstract void OnCreate();
        protected abstract void OnUpdate();

        public void LateUpdate()
        {
            OnUpdate();
        }

        private RenderTexture mCameraTargetRT;

        private RenderTexture mCameraColorTargetRT;

        public ICameraSlave()
        {
            OnCreate();
            if (mCamera != null)
            {
                mCamera.tag = "Slave";
                if (mCamera.TryGetComponent<UniversalAdditionalCameraData>(out mCameraData))
                    mCameraData.SetRenderer(2);
                GameObject.DontDestroyOnLoad(mCamera.gameObject);
                mCamera.hideFlags = HideFlags.HideInHierarchy;
            }
        }

        // 从URP托管改为手动管理
        private void ManualURP()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        }

        // 取消手动管理
        private void UnManualURP()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera.cameraType == CameraType.SceneView || camera.cameraType == CameraType.Preview)
                return;

            var cameraData = camera.GetUniversalAdditionalCameraData();
            if (cameraData.renderType != CameraRenderType.Base)
                return;

            mCamera.targetTexture = mCameraTargetRT;
            UniversalRenderPipeline.RenderSingleCamera(context, mCamera);
        }


        public Camera camera
        {
            get 
            {
                return mCamera;
            }
        }

        public UniversalAdditionalCameraData cameraData
        {
            get
            {
                return mCameraData;
            }
        }

        protected abstract Camera GetCloneCamera();

        public void Use(CamRenderType camRenderType = CamRenderType.SameWithOldCamera)
        {
            if (mCamera != null && !mCamera.gameObject.activeSelf)
                mCamera.gameObject.SetActive(true);

            var oldCamera = GetCloneCamera();
            if (oldCamera == null)
                return;

            UniversalAdditionalCameraData cameraAdditionalData = null;
            oldCamera.TryGetComponent<UniversalAdditionalCameraData>(out cameraAdditionalData);
            if (cameraAdditionalData == null)
                cameraAdditionalData = oldCamera.GetComponent<UniversalAdditionalCameraData>();

            if (cameraAdditionalData != null)
            {
                mCamera.cameraType = oldCamera.cameraType;
                mCamera.orthographic = oldCamera.orthographic;
                mCamera.orthographicSize = oldCamera.orthographicSize;
                mCamera.fieldOfView = oldCamera.fieldOfView;
                mCamera.nearClipPlane = oldCamera.nearClipPlane;
                mCamera.farClipPlane = oldCamera.farClipPlane;
                mCamera.allowHDR = oldCamera.allowHDR;
                mCamera.allowMSAA = oldCamera.allowMSAA;
                mCamera.clearFlags = oldCamera.clearFlags;
                mCamera.backgroundColor = oldCamera.backgroundColor;

                if (camRenderType == CamRenderType.SameWithOldCamera)
                    mCameraData.renderType = cameraAdditionalData.renderType;
                else
                    mCameraData.renderType = (CameraRenderType)camRenderType;

                if (mCameraData.renderType == CameraRenderType.Overlay)
                {
                    if (Camera.main.TryGetComponent<UniversalAdditionalCameraData>(out var mainCameraAdditionalData))
                    {
                        if (!mainCameraAdditionalData.cameraStack.Contains(mCamera))
                        {
                            mainCameraAdditionalData.cameraStack.Add(mCamera);
                        }
                    }
                }
                else if (mCameraData.renderType == CameraRenderType.Base)
                {
                    mCamera.depth = oldCamera.depth + 1;
                }
            }
            else
            {
                Debug.LogErrorFormat("Get {0} 's UniversalAdditionalCameraData fail!", oldCamera);
            }
            OnUse();
        }

        public void UnUse()
        {
            if (mCameraTargetRT != null)
                ClearCameraTarget();
            if (mCameraColorTargetRT != null)
                ClearColorTarget();
            if (mCamera != null && mCamera.gameObject.activeSelf)
                mCamera.gameObject.SetActive(false);
            OnUnUse();
        }

        public RenderTexture GetCameraTarget()
        {
            if (mCameraTargetRT == null)
            {
                mCameraTargetRT = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
                ManualURP();
            }
            return mCameraTargetRT;
        }

        public void ClearCameraTarget()
        {
            UnManualURP();
            if (mCameraTargetRT != null)
            {
                RenderTexture.ReleaseTemporary(mCameraTargetRT);
                mCameraTargetRT = null;
            }
        }

        public RenderTexture GetColorTarget()
        {
            if (mCameraColorTargetRT == null)
            {
                mCameraColorTargetRT = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
                UnManualURP();
            }
            return mCameraColorTargetRT;
        }

        public void ClearColorTarget()
        {
            UnManualURP();
            if (mCameraColorTargetRT != null)
            {
                RenderTexture.ReleaseTemporary(mCameraColorTargetRT);
                mCameraColorTargetRT = null;
            }
        }

        protected virtual void OnUse(){ }

        protected virtual void OnUnUse() { }

    }


    public class CameraSlaveUI : ICameraSlave
    {
        protected override void OnCreate()
        {
            var uiCamera = UICamera.mainCamera;
            if (uiCamera == null)
                return;

            mCamera = new GameObject("Camera", typeof(Camera), typeof(UniversalAdditionalCameraData)).GetComponent<Camera>();
            mCamera.transform.SetParent(uiCamera.transform.parent);
            mCamera.transform.localScale = uiCamera.transform.localScale;
            mCamera.transform.localPosition = uiCamera.transform.localPosition;
            mCamera.transform.localRotation = uiCamera.transform.localRotation;
            mCamera.gameObject.name = "CameraSlaveUI";
        }

        protected override Camera GetCloneCamera()
        { 
            return UICamera.mainCamera;
        }

        protected override void OnUpdate()
        {
            
        }
    }


    public class CameraSlaveMain : ICameraSlave
    {
        private bool mUpdate = false;
        protected override void OnCreate()
        {
            var cloneCamera = GetCloneCamera();
            if (cloneCamera == null)
                return;

            mCamera = new GameObject("Camera", typeof(Camera), typeof(UniversalAdditionalCameraData)).GetComponent<Camera>();
            mCamera.gameObject.name = "CameraSlaveMain";
            mCamera.transform.SetParent(null);
            mCamera.transform.localScale = cloneCamera.transform.localScale;
            mCamera.transform.position = cloneCamera.transform.position;
            mCamera.transform.rotation = cloneCamera.transform.rotation;
        }

        protected override Camera GetCloneCamera()
        {
            return Camera.main;
        }

        protected override void OnUpdate()
        {
            if (mUpdate)
            {
                mCamera.transform.position = GetCloneCamera().transform.position;
                mCamera.transform.rotation = GetCloneCamera().transform.rotation;
            }
        }

        protected override void OnUse()
        {
            mUpdate = true;
        }

        protected override void OnUnUse()
        {
            mUpdate = false;
        }

    }


    public class CameraSlave
    {
        private static ICameraSlave mUI;
        public static ICameraSlave UI
        {
            get 
            {
                if (mUI == null)
                    mUI = new CameraSlaveUI();
                return mUI;
            }
        }

        private static ICameraSlave mMain;
        public static ICameraSlave Main
        {
            get
            {
                if (mMain == null)
                    mMain = new CameraSlaveMain();
                return mMain;
            }
        }

        public static void LateUpdate()
        {
            if (mUI != null)
                mUI.LateUpdate();
            if (mMain != null)
                mMain.LateUpdate();
        }

    }


    public static class CameraEx
    {
        public static void IgnoreCullingMask(this Camera camera, params string[] ignoreLayerNames)
        {
            int cullingMask = camera.cullingMask;
            for (int i = 0; i < ignoreLayerNames.Length; i++)
            {
                var ignore = LayerMask.NameToLayer(ignoreLayerNames[i]);
                cullingMask &= ~(1 << ignore);
            }
            camera.cullingMask = cullingMask;
        }

        public static void AddCullingMask(this Camera camera, params string[] addLayerNames)
        {
            int cullingMask = camera.cullingMask;
            for (int i = 0; i < addLayerNames.Length; i++)
            {
                var ignore = LayerMask.NameToLayer(addLayerNames[i]);
                cullingMask |= (1 << ignore);
            }
            camera.cullingMask = cullingMask;
        }

        public static void SetCullingMask(this Camera camera, params string[] layerNames)
        {
            int cullingMask = 0;
            for (int i = 0; i < layerNames.Length; i++)
            {
                var ignore = LayerMask.NameToLayer(layerNames[i]);
                cullingMask |= (1 << ignore);
            }
            camera.cullingMask = cullingMask;
        }
    }

}
