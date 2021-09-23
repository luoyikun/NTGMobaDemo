using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

//单位UI相关的管理器
public class UnitUiMgr : MonoBehaviour
{
    public static UnitUiMgr self;
    public Dictionary<UnitCtrl, GameObject> m_mapUnitUI = new Dictionary<UnitCtrl, GameObject>();
    public Transform m_heroParent;

    public float ScreenX;
    public float ScreenY;

    void Awake()
    {
        
    }
	// Use this for initialization
	void Start ()
	{
	    self = this;

       
	}

    //所有的初始化都要在mainmgr中调用
    public void init()
    {
        var rt = UIMgr.self.m_panelRoot.GetComponent<RectTransform>();
        ScreenX = rt.sizeDelta.x;
        ScreenY = rt.sizeDelta.y;
    }
	// Update is called once per frame
	void Update () {
        
	}

    void LateUpdate()
    {
        foreach (var m in m_mapUnitUI)
        {
            var viewPoint = MainMgr.self.m_camMyView.WorldToViewportPoint(m.Key.mPos);
            m.Value.transform.localPosition = new Vector3((viewPoint.x - 0.5f) * ScreenX, (viewPoint.y - 0.5f) * ScreenY, 0);
        }
    }

}
