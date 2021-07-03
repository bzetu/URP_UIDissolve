using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using GameCore;

namespace PF.URP.PostProcessing
{
    public class UIDissolvePass : ScriptableRenderPass
    {
        private const string mCommandBufferName = "CommandBuffer_UIDissolve";
        private const string mTempTexName = "UIDissolve Temp Texture";

        private RenderTargetHandle mTempTex_Handle;
        private UIDissolve mDissolve;
        private FilterMode mFilterMode = FilterMode.Bilinear;

        private RenderTargetIdentifier mSourceRT_Id;
        private int mShaderId_ClipTex = Shader.PropertyToID("_ClipTex");
        private int mShaderId_BoundingBox = Shader.PropertyToID("_BoundingBox");
        private int mShaderId_DissolveValue = Shader.PropertyToID("_DissolveValue");
        private UIDissolveFeature.UseType mUseType;

        public UIDissolvePass(RenderPassEvent @event)
        {
            this.renderPassEvent = @event;
            mTempTex_Handle.Init(mTempTexName);
        }

        public void Setup(RenderTargetIdentifier sourceRT, UIDissolve dissolve, UIDissolveFeature.UseType useType)
        {
            mSourceRT_Id = sourceRT;
            mDissolve = dissolve;
            mUseType = useType;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get(mCommandBufferName);
            if (mUseType == UIDissolveFeature.UseType.RenderDissolve)
            {
                RenderImage(cmd, ref renderingData);
            }
            else if (mUseType == UIDissolveFeature.UseType.SetColorTarget)
            {
                Blit(cmd, mSourceRT_Id, CameraSlave.UI.GetColorTarget());
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }


        private RenderTexture mTest1;
        private int mShaderId_Debug = Shader.PropertyToID("_Debug");
        private void RenderImage(CommandBuffer cmd, ref RenderingData renderingData)
        {
            mDissolve.Material.SetFloat(mShaderId_DissolveValue, mDissolve.DissolveValue);
            mDissolve.Material.SetVector(mShaderId_BoundingBox, mDissolve.BoundingBox);
            mDissolve.Material.SetTexture(mShaderId_ClipTex, CameraSlave.UI.GetColorTarget());



            mDissolve.Material.SetInt(mShaderId_Debug, mDissolve.mDebug);


            if (mDissolve.mDebug == 3)
                PFPostProcessingDebug.ShowNo1(CameraSlave.UI.GetColorTarget());


            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            //获取临时RT
            cmd.GetTemporaryRT(mTempTex_Handle.id, opaqueDesc, mFilterMode);

            if (mDissolve.mDebug == 3)
            {
                mTest1 = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
                Blit(cmd, mSourceRT_Id, mTest1);
                PFPostProcessingDebug.ShowNo2(mTest1);
            }
            else
            {
                PFPostProcessingDebug.Close();
            }





            //将当前相机的RT经过处理后,存入临时RT
            Blit(cmd, mSourceRT_Id, mTempTex_Handle.Identifier(), mDissolve.Material);

            //将处理后的RT赋值给相机RT
            Blit(cmd, mTempTex_Handle.Identifier(), mSourceRT_Id);


        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            //释放临时RT
            cmd.ReleaseTemporaryRT(mTempTex_Handle.id);

            RenderTexture.ReleaseTemporary(mTest1);
            //RenderTexture.ReleaseTemporary(mTest2);
        }

    }
}