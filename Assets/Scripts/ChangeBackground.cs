using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeBackground : MonoBehaviour
{
    // 背景数组
    public GameObject[] backgrounds;
    
    // 切换背景函数
    public void changeBackgroundToRoom1()
    {
        // 切换背景
        backgrounds[0].SetActive(true);
        backgrounds[1].SetActive(false);
        backgrounds[2].SetActive(false);
    }
    
    public void changeBackgroundToRoom2()
    {
        // 切换背景
        backgrounds[0].SetActive(false);
        backgrounds[1].SetActive(true);
        backgrounds[2].SetActive(false);
    }
    
    public void changeBackgroundToRoom3()
    {
        // 切换背景
        backgrounds[0].SetActive(false);
        backgrounds[1].SetActive(false);
        backgrounds[2].SetActive(true);
    }
    
    public void OnRoom1Enter()
    {
        Invoke(nameof(changeBackgroundToRoom1), 0.6f);
    }
    
    public void OnRoom2Enter()
    {
        Invoke(nameof(changeBackgroundToRoom2), 0.6f);
    }
    
    public void OnRoom3Enter()
    {
        Invoke(nameof(changeBackgroundToRoom3), 0.6f);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
