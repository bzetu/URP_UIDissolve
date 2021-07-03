/*
NGUI的UI包围盒
    1.可以获得左上角与右下角的UV坐标
    create by jiangcheng_m
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameCore
{
    [System.Serializable]
    public class UIBoundingBox
    {
        /*------------可以被序列化的内部字段-----------------*/
        [SerializeField]
        private Vector2 mCenter;
        [SerializeField]
        private Vector2 mSize;
        [SerializeField]
        private Vector2 mLT;
        [SerializeField]
        private Vector2 mRB;
        [SerializeField]
        private Vector2 mLT_UV;
        [SerializeField]
        private Vector2 mRB_UV;

        /*------------不可被序列化的内部字段-----------------*/
        //UI矩阵
        private Matrix4x4 mMatrix;
        //本地坐标与实际中心点的偏移
        private Vector3 mCenterOffset;
        //包围盒大小偏移
        private Vector4 mBoundingBoxOffset;
        /*------------可以被外部访问的属性-----------------*/
        //包围盒中心点
        public Vector2 Center
        {
            get 
            { 
                return mCenter; 
            }
            set
            {
                UpdateCenter(value);
            }
        }
        //包围盒大小
        public Vector2 Size
        {
            get
            {
                return mCenter;
            }
            set
            {
                UpdateSize(value);
            }
        }
        //包围盒左上角顶点
        public Vector2 LT { get { return mLT; } }
        //包围盒右下角顶点
        public Vector2 RB { get { return mRB; } }
        //包围盒左上角顶点UV坐标
        public Vector2 LT_UV { get { return mLT_UV; } }
        //包围盒右下角顶点UV坐标
        public Vector2 RB_UV { get { return mRB_UV; } }
        //是否初始化
        public bool IsInit { private set; get; }


        public bool Init(Transform ui,Vector4? boundingBoxOffset = null)
        {
            if (UIRoot.list.Count > 0)
            {
                UIRoot root = UIRoot.list[0];
                if (root != null)
                {
                    if (boundingBoxOffset != null)
                        mBoundingBoxOffset = (Vector4)boundingBoxOffset;

                    mMatrix = ui.transform.localToWorldMatrix;
                    UIWidget[] widgets = ui.GetComponentsInChildren<UIWidget>();
                    //赋值第一个
                    if (widgets.Length > 0)
                    {
                        UIWidget w = widgets[0];
                        Vector3 localPos = root.transform.worldToLocalMatrix.MultiplyPoint(w.transform.position);
                        //左上角
                        mLT.x = localPos.x + (0 - w.pivotOffset.x) * w.width;
                        mLT.y = localPos.y + (1 - w.pivotOffset.y) * w.height;
                        //右下角
                        mRB.x = localPos.x + (1 - w.pivotOffset.x) * w.width;
                        mRB.y = localPos.y + (0 - w.pivotOffset.y) * w.height;
                    }
                    //和其他的比较
                    for (int i = 1; i < widgets.Length; i++)
                    {
                        UIWidget w = widgets[i];
                        Vector3 localPos = root.transform.worldToLocalMatrix.MultiplyPoint(w.transform.position);
                        //左上角
                        float LT_X = localPos.x + (0 - w.pivotOffset.x) * w.width;
                        float LT_Y = localPos.y + (1 - w.pivotOffset.y) * w.height;
                        //右下角
                        float RB_X = localPos.x + (1 - w.pivotOffset.x) * w.width;
                        float RB_Y = localPos.y + (0 - w.pivotOffset.y) * w.height;
                        //更新包围盒的左上角与右下角
                        mLT.x = LT_X < mLT.x ? LT_X : mLT.x;
                        mLT.y = LT_Y > mLT.y ? LT_Y : mLT.y;
                        mRB.x = RB_X > mRB.x ? RB_X : mRB.x;
                        mRB.y = RB_Y < mRB.y ? RB_Y : mRB.y;
                    }
                    //大小
                    mSize = new Vector2(mRB.x - mLT.x, mLT.y - mRB.y);
                    //中心点
                    Center = (RB + LT) / 2;
                    //本地坐标与实际中心点的偏移
                    mCenterOffset.x = Center.x -  ui.transform.localPosition.x;
                    mCenterOffset.y = Center.y - ui.transform.localPosition.y;
                    //更新偏移
                    UpdateBoundingBoxOffset();
                    //UV坐标
                    UpdateUV();
                    IsInit = true;

                    return true;
                }
            }
            Debug.Log("-----------UIRoot.list.Count = 0--------------");
            return false;
        }


        /// <summary>
        /// 更新包围盒
        /// </summary>
        /// <param name="uiLocalPos"></param>
        public void UpdateBoundingBox(Vector3 uiLocalPos)
        {
            mCenter.x = uiLocalPos.x + mCenterOffset.x;
            mCenter.y = uiLocalPos.y + mCenterOffset.y;
            UpdateCenter(mCenter);
        }


        /// <summary>
        /// 更新中心点
        /// </summary>
        /// <param name="center"></param>
        private void UpdateCenter(Vector2 center)
        {
            mCenter = center;
            float halfWidth = mSize.x / 2f;
            float halfHeight = mSize.y / 2f;
            mLT.x = Center.x - halfWidth;
            mLT.y = Center.y + halfHeight;
            mRB.x = Center.x + halfWidth;
            mRB.y = Center.y - halfHeight;
            UpdateBoundingBoxOffset();
            UpdateUV();
        }

        /// <summary>
        /// 更新大小
        /// </summary>
        /// <param name="size"></param>
        private void UpdateSize(Vector2 size)
        {
            mSize = size;
            mLT.x = mCenter.x - mSize.x / 2f;
            mLT.y = mCenter.y + mSize.y / 2f;
            mRB.x = mCenter.x + mSize.x / 2f;
            mRB.y = mCenter.y - mSize.y / 2f;
            UpdateBoundingBoxOffset();
            UpdateUV();
        }

        private void UpdateBoundingBoxOffset()
        {
            mLT.x = mLT.x + mBoundingBoxOffset.x;
            mLT.y = mLT.y + mBoundingBoxOffset.y;
            mRB.x = mRB.x + mBoundingBoxOffset.z;
            mRB.y = mRB.y + mBoundingBoxOffset.w;
        }



        /// <summary>
        /// 更新UV坐标
        /// </summary>
        private void UpdateUV()
        {
            UIRoot root = UIRoot.list[0];
            if (root != null)
            {
                var fullWidth = Mathf.Max(root.manualWidth, Screen.width);
                var fullHeight = Mathf.Max(root.manualHeight, Screen.height);
  
                mLT_UV = new Vector2(Mathf.Clamp((fullWidth / 2f + mLT.x) / fullWidth, 0,1), Mathf.Clamp((fullHeight / 2f + mLT.y) / fullHeight, 0,1));
                mRB_UV = new Vector2(Mathf.Clamp((fullWidth / 2f + mRB.x) / fullWidth, 0,1), Mathf.Clamp((fullHeight / 2f + mRB.y) / fullHeight, 0,1));
            }
        }





#if UNITY_EDITOR
        /// <summary>
        /// 画出线框(在MonoBehaviour脚本的OnDrawGizmos函数中调用)
        /// </summary>
        public void DrawGizmos()
        {
            if (IsInit)
            {
                Gizmos.matrix = this.mMatrix;
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(this.Center, this.Size);
                Gizmos.color = Color.clear;
            }
        }
#endif

    }
    

}