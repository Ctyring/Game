using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using HotUpdateScripts.UiControl;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using JEngine.Core;
using TMPro;

public class UI_ctrl : MonoBehaviour
{
    public Dictionary<string, GameObject> view = new Dictionary<string, GameObject>();

    private void load_all_object(GameObject root, string path)
    {
        foreach (Transform tf in root.transform)
        {
            if (this.view.ContainsKey(path + tf.gameObject.name))
            {
                // Debugger.LogWarning("Warning object is exist:" + path + tf.gameObject.name + "!");
                continue;
            }
            this.view.Add(path + tf.gameObject.name, tf.gameObject);
            load_all_object(tf.gameObject, path + tf.gameObject.name + "/");
        }

    }

    public virtual void Awake()
    {
        this.load_all_object(this.gameObject, "");
    }


    public void add_button_listener(string viewName, UnityAction onclick)
    {
        Button bt = this.view[viewName].GetComponent<Button>();
        if (bt == null)
        {
            Debug.LogWarning("UI_manager add_button_listener: not Button Component!");
            return;
        }

        bt.onClick.AddListener(onclick);
    }

    /// <summary>
    /// 获取UI内的字符串
    /// </summary>
    /// <param name="viewName">UI节点名</param>
    /// <returns></returns>
    public string Get_TextMeshPro_Content(string viewName)
    {
        TMP_InputField text = this.view[viewName].GetComponent<TMP_InputField>();
        if (text == null)
        {
            Debug.LogWarning("UI_manager Get_TextMeshPro_Content: not TextMeshPro Component!");
            return "";
        }
        return text.text;
    }
}
public class UIMgr : HotUpdateSingleton<UIMgr>
{
    private Transform canvas = null;

    public override void Awake() {
        base.Awake();
        this.canvas = GameObject.Find("Canvas").transform;
    }

    /// <summary>
    /// 场景改变的时候要改变的内容
    /// </summary>
    public void ChangeScene()
    {
        this.canvas = GameObject.Find("Canvas").transform;
    }

    public void SetUIActive(string name, bool isActive)
    {
        string UIName = name + "_UI";
        this.canvas.Find(UIName).gameObject.SetActive(isActive);
    }

    public void ShowUIView(string name)
    {
        string path = "Assets/HotUpdateResources/Prefab/UI/" + name + "_UI.prefab";
        string scriptName = name + "_UICtrl";
        // GameObject ui_prefab = (GameObject)ResMgr.Instance.GetAssetCache<GameObject>(path);
        GameObject ui_prefab = (GameObject) JResource.LoadRes<GameObject>(path);
        GameObject ui_view = GameObject.Instantiate(ui_prefab);
        ui_view.name = ui_prefab.name;
        ui_view.transform.SetParent(this.canvas, false);
        ClassBind c = ui_view.AddComponent<ClassBind>();
        ClassData classData = new ClassData();
        classData.classNamespace = "HotUpdateScripts.UiControl";
        classData.className = scriptName;
        c.BindSelf();
        c.AddClass(classData);
        c.Active(classData);
    }
}
