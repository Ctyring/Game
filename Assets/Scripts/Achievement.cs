using System.Collections;
using System.Collections.Generic;
using Cinemachine.PostFX;
using MoreMountains.Tools;
using UnityEngine;

public class Achievement : MonoBehaviour
{
    // 解锁成就
    public void unlockAchievement(string name)
    {
        MMAchievementManager.UnlockAchievement(name);
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
