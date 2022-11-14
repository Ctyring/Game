using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeBackground : MonoBehaviour
{
    // 背景数组
    public GameObject[] backgrounds;
    
    public void OnRoom1Enter()
    {
        // 切换背景
        backgrounds[0].SetActive(true);
        backgrounds[1].SetActive(false);
    }
    
    public void OnRoom2Enter()
    {
        // 切换背景
        backgrounds[0].SetActive(false);
        backgrounds[1].SetActive(true);
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
