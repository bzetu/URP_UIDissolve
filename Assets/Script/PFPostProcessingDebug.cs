using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace PF.URP.PostProcessing
{
    public class PFPostProcessingDebug : MonoBehaviour
    {
        public Image mImgNo1;
        public Image mImgNo2;
        public Image mImgNo3;

        private static PFPostProcessingDebug mThis;
        private static PFPostProcessingDebug _this
        {
            get
            {
                if (mThis == null)
                {
                    var res = Resources.Load("PFPostProcessingDebug");
                    var go = GameObject.Instantiate(res) as GameObject;
                    mThis = go.GetComponent<PFPostProcessingDebug>();
                    if(mThis == null)
                        mThis = go.AddComponent<PFPostProcessingDebug>();

                    mThis.mImgNo1.enabled = false;
                    mThis.mImgNo2.enabled = false;
                    mThis.mImgNo3.enabled = false;
                }
                return mThis;
            }
        }


        private static void ShowTex(Image img, RenderTexture rt)
        {
            img.enabled = true;
            int width = rt.width;
            int height = rt.height;
            Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, false);
            RenderTexture.active = rt;
            texture2D.ReadPixels(new Rect(0, 0, width, height), 1, 1);
            texture2D.Apply();
            img.sprite = Sprite.Create(texture2D, new Rect(0, 0, width, height), Vector2.zero);
        }


        public static void ShowNo1(RenderTexture rt)
        {
            ShowTex(_this.mImgNo1, rt);
        }


        public static void ShowNo2(RenderTexture rt)
        {
            ShowTex(_this.mImgNo2, rt);
        }

        public static void ShowNo3(RenderTexture rt)
        {
            ShowTex(_this.mImgNo3, rt);
        }


        public static void Close()
        {
            if (mThis != null)
            {
                Destroy(mThis.gameObject);
            }
        }
    }
}

