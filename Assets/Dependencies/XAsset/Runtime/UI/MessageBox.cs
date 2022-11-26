//
// MessageBox.cs
//
// Author:
//       fjy <jiyuan.feng@live.com>
//
// Copyright (c) 2020 fjy
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using GlobalData;

public class MessageBox : IEnumerator
{
    public enum EventId
    {
        Ok,
        No
    }

    private static readonly GameObject LightMessageBox = Resources.Load<GameObject>("UI/MessageBox/MessageBox_Light_UI");
    private static readonly GameObject DarkMessageBox = Resources.Load<GameObject>("UI/MessageBox/MessageBox_Dark_UI");
    private static readonly List<MessageBox> _showed = new List<MessageBox>();
    private static readonly List<MessageBox> _hidden = new List<MessageBox>();
    private Text _content;
    private Text _textNo;
    private Text _textOk;

    private Text _title;

    private bool _visible = true;
    
    private MessageBox(string type)
    {
        switch (GlobalSettings.Theme)
        {
            case "Light":
                gameObject = Object.Instantiate(LightMessageBox);
                break;
            case "Dark":
                gameObject = Object.Instantiate(DarkMessageBox);
                break;
            default:
                throw new Exception("ThemeError");
        }

        gameObject.transform.SetParent(GameObject.Find("Canvas").transform, false);

        switch (type)
        {
            case "Update":
                gameObject.transform.Find("Popup_Update").gameObject.SetActive(true);
                var ok1 = GetComponent<Button>("Popup_Update/Popup/Button_Update");
                var no1 = GetComponent<Button>("Popup_Update/Popup/Button_Later");
                ok1.onClick.AddListener(OnClickOk);
                no1.onClick.AddListener(OnClickNo);
                break;
            case "ErrorNetwork":
                gameObject.transform.Find("Popup_ErrorNetwork").gameObject.SetActive(true);
                var ok2 = GetComponent<Button>("Popup_ErrorNetwork/Popup/Button_Retry");
                var no2 = GetComponent<Button>("Popup_ErrorNetwork/Popup/Button_Ok");
                ok2.onClick.AddListener(OnClickOk);
                no2.onClick.AddListener(OnClickNo);
                break;
            case "Error":
                gameObject.transform.Find("Popup_Error").gameObject.SetActive(true);
                var ok3 = GetComponent<Button>("Popup_ErrorNetwork/Popup/Button_Ok");
                ok3.onClick.AddListener(OnClickOk);
                break;
            default:
                throw new Exception("MessageBoxTypeError");
        }
        
        Init();
    }

    public bool isOk { get; private set; }

    private GameObject gameObject { get; set; }

    public Action<EventId> onComplete { get; set; }

    public static void Dispose()
    {
        foreach (var item in _hidden) item.Destroy();

        _hidden.Clear();

        foreach (var item in _showed) item.Destroy();

        _showed.Clear();
    }
    
    public static void CloseAll()
    {
        for (var index = 0; index < _showed.Count; index++)
        {
            var messageBox = _showed[index];
            messageBox.Hide();
            _hidden.Add(messageBox);
        }
        _showed.Clear();
    }
    
    /// <summary>
    /// 释放MessageBox
    /// </summary>
    /// <param name="type">目前支持类型：Update ErrorNetwork Error</param>
    /// <returns></returns>
    public static MessageBox Show(string type)
    {
        if (_hidden.Count > 0)
        {
            var mb = _hidden[0];
            mb.Init();
            mb.gameObject.SetActive(true);
            _hidden.RemoveAt(0);
            return mb;
        }

        return new MessageBox(type);
    }

    private void Destroy()
    {
        _title = null;
        _textOk = null;
        _textNo = null;
        _content = null;
        Object.DestroyImmediate(gameObject);
        gameObject = null;
    }

    private void Init()
    {
        _showed.Add(this);
        _visible = true;
        isOk = false;
    }

    private T GetComponent<T>(string path) where T : Component
    {
        var trans = gameObject.transform.Find(path);
        return trans.GetComponent<T>();
    }

    private void OnClickNo()
    {
        HandleEvent(EventId.No);
    }

    private void OnClickOk()
    {
        HandleEvent(EventId.Ok);
    }

    private void HandleEvent(EventId id)
    {
        switch (id)
        {
            case EventId.Ok:
                break;
            case EventId.No:
                break;
            default:
                throw new ArgumentOutOfRangeException("id", id, null);
        }

        Close();

        isOk = id == EventId.Ok;

        if (onComplete == null) return;
        onComplete(id);
        onComplete = null;
    }

    #region IEnumerator implementation

    public bool MoveNext()
    {
        return _visible;
    }

    public void Reset()
    {
    }
    
    public void Close()
    {
        Hide();
        _hidden.Add(this);
        _showed.Remove(this);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        _visible = false;
    }

    public object Current => null;

    #endregion
}