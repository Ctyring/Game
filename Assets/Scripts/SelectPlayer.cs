using System.Collections;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using UnityEngine;

public class SelectPlayer : MonoBehaviour
{
    public GameObject selected;
    private Vector3 targetPos;
    private int id;
    public void OnSelected(int id)
    {
        if (selected.activeSelf)
        {
            
        }
        else
        {
            selected.SetActive(true);
            selected.transform.localPosition = new Vector3(-384 + id * (256), -212, 0);
        }
        targetPos = new Vector3(-384 + id * (256), -212, 0);
        this.id = id;
    }
    
    public void OnEnsure()
    {
        Global.playerId = id;
        MMSceneLoadingManager.LoadScene("Demo", "LoadingScreen");
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        selected.transform.localPosition = Vector3.Lerp(selected.transform.localPosition, targetPos, 0.1f);
    }
}
