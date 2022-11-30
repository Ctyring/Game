using System.Collections;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using UnityEngine;
using UnityEngine.UI;

public class ChangeHead : MonoBehaviour
{
    public Sprite[] heads;
    public Image currentHead;
    // Start is called before the first frame update
    void Start()
    {
        if (Global.playerId == 0)
        {
            currentHead.sprite = heads[0];
        }
        else if (Global.playerId == 1)
        {
            currentHead.sprite = heads[1];
        }
        else if (Global.playerId == 2)
        {
            currentHead.sprite = heads[2];
        }
        else if (Global.playerId == 3)
        {
            currentHead.sprite = heads[3];
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
