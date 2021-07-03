using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour
{
    public UIDissolve mDissolve;
    public Transform mUI;
    public UILabel mLabelScreen;
    void Start()
    {
        mLabelScreen.text = string.Format("Screen.width={0} Screen.height={1}", Screen.width, Screen.height);
    }


    public void Play(GameObject btn)
    {
        mDissolve.mDebug = System.Convert.ToInt32(btn.gameObject.name.Substring(btn.gameObject.name.Length - 1));
        mDissolve.Play(mUI, () =>
        {
            Debug.LogFormat("播放结束:{0}", mDissolve.mDebug);
        });
    }


}
