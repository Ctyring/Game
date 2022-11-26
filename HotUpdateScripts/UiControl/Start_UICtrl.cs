namespace HotUpdateScripts.UiControl;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using GlobalData;
public class Start_UICtrl : UI_ctrl {

    public override void Awake() {
        base.Awake();
    }

    void Start() {
        // UIMgr.Instance.ShowUIView("Game");
        UIMgr.Instance.ShowUIView("Pre_Light");
        UIMgr.Instance.ShowUIView("Settings_Light");
        UIMgr.Instance.ShowUIView("Pre_Dark");
        UIMgr.Instance.ShowUIView("Settings_Dark");
        UIMgr.Instance.ShowUIView("SignUp");
        UIMgr.Instance.ShowUIView("SignIn");
        UIMgr.Instance.SetUIActive("Pre_Light", false);
        UIMgr.Instance.SetUIActive("Settings_Light", false);
        UIMgr.Instance.SetUIActive("Pre_Dark", false);
        UIMgr.Instance.SetUIActive("Settings_Dark", false);
        UIMgr.Instance.SetUIActive("SignUp", false);
        UIMgr.Instance.SetUIActive("SignIn", false);
        // this.add_button_listener("Button_Play", this.onStartClick);
        // MapMgr.Instance.LoadRes();
    }

    /// <summary>
    /// 监听按下开始按钮
    /// </summary>
    private void onStartClick() {
        // 目前为了调试，先不搞登录
        // UIMgr.Instance.SetUIActive("SignIn", false);
        // UIMgr.Instance.SetUIActive("Pre_" + GlobalSettings.Theme, true);
        // UIMgr.Instance.SetUIActive("Start", false);
        // if (GlobalSettings.Theme == "Light")
        // {
        //     Pre_Light_UICtrl.isShowCharacter = true;
        // }
        // else
        // {
        //     Pre_Dark_UICtrl.isShowCharacter = true;
        // }
    }
}
