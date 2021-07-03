using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PF.URP.PostProcessing
{
    public class PFPostProcessingMgr
    {
        private Dictionary<System.Type, MonoBehaviour> mPostProcessDic;

        private PFPostProcessingMgr() { }

        private static PFPostProcessingMgr mInstance;
        public static PFPostProcessingMgr Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new PFPostProcessingMgr();
                    mInstance.mPostProcessDic = new Dictionary<System.Type, MonoBehaviour>();
                }
                return mInstance;
            }
        }

        public T GetPostProcessing<T>() where T : MonoBehaviour
        {
            mPostProcessDic.TryGetValue(typeof(T), out var postProcessing);
            return (T)postProcessing;
        }

        public bool AddPostProcessing(MonoBehaviour postProcessing)
        {
            System.Type postProcessingType = postProcessing.GetType();
            if (mPostProcessDic.ContainsKey(postProcessingType))
            {
                Debug.LogErrorFormat("can not addition same postprocessing!({0} has exist!)", postProcessingType);
                return false;
            }
            mPostProcessDic.Add(postProcessingType, postProcessing);
            return true;
        }

        public bool RemovePostProcessing(MonoBehaviour postProcessing)
        {
            System.Type postProcessingType = postProcessing.GetType();
            if (!mPostProcessDic.ContainsKey(postProcessingType))
            {
                Debug.LogWarningFormat("the postprocessing {0} is not exist,remove fail!", postProcessingType);
                return false;
            }
            return mPostProcessDic.Remove(postProcessingType);
        }
    }
}

