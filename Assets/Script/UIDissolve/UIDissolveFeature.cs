using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using GameCore;

namespace PF.URP.PostProcessing
{
    public class UIDissolveFeature : ScriptableRendererFeature
    {
        public enum UseType
        { 
            SetColorTarget,
            RenderDissolve,
        }

        public RenderPassEvent mEvent = RenderPassEvent.AfterRenderingPostProcessing;

        public UseType mUseType;

        private UIDissolvePass mPass;

        private UIDissolve mDissolve;

        private RenderTargetIdentifier mBaseCameraColorTarget;

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!Application.isPlaying)
                return;
            mDissolve = PFPostProcessingMgr.Instance.GetPostProcessing<UIDissolve>();
            if (mDissolve == null)//控制开关
                return;

            if (mUseType == UseType.RenderDissolve)
            {
                if (renderingData.cameraData.camera.tag != "Slave")
                    return;
            }
            else if (mUseType == UseType.SetColorTarget)
            {
                if (renderingData.cameraData.camera != UICamera.mainCamera)
                    return;
            }

            var cameraColorTarget = renderer.cameraColorTarget;
            //设置当前需要后期的画面
            mPass.Setup(cameraColorTarget, mDissolve, mUseType);
            //添加到渲染列表
            renderer.EnqueuePass(mPass);
        }




        public override void Create()
        {
            mPass = new UIDissolvePass(mEvent);
        }
    }
}