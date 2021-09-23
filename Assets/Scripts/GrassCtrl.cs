using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class GrassCtrl : MonoBehaviour {
    List<UnitCtrl> m_listWeUnit = new List<UnitCtrl>();//这个草丛中我方单位
    List<UnitCtrl> m_listOtherUnit = new List<UnitCtrl>();//敌方单位
    //todo 当人回城，瞬间移动等要把草丛中该单位清空
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnTriggerEnter(Collider other)
    {
        UnitCtrl unit = other.GetComponentInParent<UnitCtrl>();
        unit.m_isInGrass = true;
        if (unit.m_camp == MainMgr.self.m_camp)
        {
            //加入我方阵营表
            m_listWeUnit.Add(unit);
            unit.transparentSet(true);

            foreach (var item in m_listOtherUnit)//探测草丛中活着的敌人
            {
                item.visibleSet(true);
                item.transparentSet(false);//即使是尸体也要更新状态
                item.m_viewCtrl.unitShowInMiniMap();//活着的敌人才能出现在小地图中
            }
        }
        else 
        {
            m_listOtherUnit.Add(unit);
            if (m_listWeUnit.Count > 0)//里面有友军，这个敌人只透明
            {
                unit.transparentSet(true);
            }
            else if (m_listWeUnit.Count == 0)//没有友军，消失在屏幕视野中
            {
                unit.visibleSet(false);
                unit.m_viewCtrl.unitHideInMinimap();
            }
        }
        
    }

    public void OnTriggerExit(Collider other)
    {
        UnitCtrl unit = other.GetComponentInParent<UnitCtrl>();
        unit.m_isInGrass = false;
        if (unit.m_camp == MainMgr.self.m_camp)
        {
            m_listWeUnit.Remove(unit);

            if (m_listWeUnit.Count == 0)//草丛里没有友军，草丛中的敌人不可见
            {
                foreach (var item in m_listOtherUnit)
                {
                    item.visibleSet(false);
                    item.m_viewCtrl.unitHideInMinimap();
                }
            }
        }
        else
        {
            m_listOtherUnit.Remove(unit);
        }
        unit.transparentSet(false);
        unit.visibleSet(true);
    }
}
