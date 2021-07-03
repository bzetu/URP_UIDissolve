/*
 * 卡牌溶解效果
 * create by jiangcheng_m
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCore;
using PF.URP.PostProcessing;
[ExecuteInEditMode]
public class UIDissolve : MonoBehaviour
{
    [SerializeField]
    private float Duration = 1f;
    [SerializeField]
    private AnimationCurve Curve = new AnimationCurve(new Keyframe(), new Keyframe(1f, 1f, 2f, 2f, 0, 0));

    //四个值 x:左上角.x   y:左上角.y   z:右下角.x   w:右下角.y
    [HideInInspector]
    public Vector4 BoundingBox;
    //溶解值
    public float DissolveValue { private set; get; }
    //是否在播放中
    public bool IsPlaying { private set; get; }
    //包围盒(实时计算,不需要序列化)
    private UIBoundingBox mBox = new UIBoundingBox();
    //溶解材质
    private Material mDissolveMat;
    //开始时间
    private float mStartTime;
    //回调
    private System.Action mOnFinish;
    //包围盒偏移调整
    //private Vector4 mBoundingBoxOffset = new Vector4(-5, 5, 15, -15);
    private Vector4 mBoundingBoxOffset = Vector4.zero;
    //当前应用的UI
    private Transform mUI;

    public Material Material
    {
        get
        {
            if (mDissolveMat == null)
                mDissolveMat = Resources.Load<Material>("UIDissolve");
            return mDissolveMat;
        }
    }


    /// <summary>
    /// 播放溶解动画
    /// </summary>
    /// <param name="ui"></param>
    /// <param name="onFinish"></param>
    /// <returns></returns>
    public bool Play(Transform ui,System.Action onFinish = null)
    {
        if (mBox.Init(ui, mBoundingBoxOffset))
        {
            mUI = ui;
            mOnFinish = onFinish;
            mStartTime = Time.realtimeSinceStartup;
            BoundingBox = new Vector4(mBox.LT_UV.x, mBox.LT_UV.y, mBox.RB_UV.x, mBox.RB_UV.y);
            PFPostProcessingMgr.Instance.AddPostProcessing(this);
            CameraSlave.UI.Use();
            CameraSlave.UI.camera.SetCullingMask("Temp3");
            SetLayer(ui, LayerMask.NameToLayer("Temp3"));
            IsPlaying = true;
            return true;
        }
        return false;
    }


    /// <summary>
    /// 强行结束播放
    /// </summary>
    public void ForceFinsh(bool callOnFinish = true)
    {
        if (mUI == null)
            return;
        IsPlaying = false;
        PFPostProcessingMgr.Instance.RemovePostProcessing(this);
        SetLayer(mUI, LayerMask.NameToLayer("UI"));
        CameraSlave.UI.UnUse();
        if (callOnFinish && mOnFinish != null)
            mOnFinish();
    }




    /// <summary>
    /// 更新UI包围盒
    /// </summary>
    /// <param name="uiLocalPos"></param>
    public void UpdateBoundingBox(Vector3 uiLocalPos)
    {
        mBox.UpdateBoundingBox(uiLocalPos);
        BoundingBox.x = mBox.LT_UV.x;
        BoundingBox.y = mBox.LT_UV.y;
        BoundingBox.z = mBox.RB_UV.x;
        BoundingBox.w = mBox.RB_UV.y;
    }





    void Update()
    {
        if (IsPlaying)
        {
            float passTime = Time.realtimeSinceStartup - mStartTime;
            if (passTime > Duration)
            {
                OnPlay(passTime);
                ForceFinsh();
            }
            else
            {
                OnPlay(passTime);
            }
            
        }

    }



    private void OnPlay(float passTime)
    {
        float progress = Mathf.Clamp(passTime / Duration, 0, 1f);
        DissolveValue = Curve.Evaluate(passTime / Duration);
    }

    private void SetLayer(Transform ui, int layer)
    {
        if (ui == null)
            return;
        ui.gameObject.layer = layer;
        ui.SetChildLayer(layer);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        mBox.DrawGizmos();
    }
#endif

    [HideInInspector]
    public int mDebug;

   



}
