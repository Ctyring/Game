using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 实现普通的单例模式
// where 限制模板的类型, new()指的是这个类型必须要能被实例化
public abstract class Singleton<T> where T : new() {
    private static T _instance;
    private static object mutex = new object();
    public static T Instance {
        get {
            if (_instance == null) {
                lock (mutex) { // 保证我们的单例，是线程安全的;
                    if (_instance == null) {
                        _instance = new T();
                    }
                }
            }
            return _instance;
        }
    }
}

// Monobeavior: 声音, 网络
// Unity单例

public class UnitySingleton<T> : MonoBehaviour
where T : Component {
    private static T _instance = null;
    public static T Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType(typeof(T)) as T;
                if (_instance == null) {
                    GameObject obj = new GameObject();
                    _instance = (T)obj.AddComponent(typeof(T));
                    obj.hideFlags = HideFlags.DontSave;
                    // obj.hideFlags = HideFlags.HideAndDontSave;
                    obj.name = typeof(T).Name;
                }
            }
            return _instance;
        }
    }

    public virtual void Awake() {
        DontDestroyOnLoad(this.gameObject);
        if (_instance == null) {
            _instance = this as T;
        }
        else {
            GameObject.Destroy(this.gameObject);
        }
    }
}





