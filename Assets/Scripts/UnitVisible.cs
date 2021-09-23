using UnityEngine;
using System.Collections;

public class UnitVisible : MonoBehaviour
{
    public UnitCtrl m_unit;

    public void init(UnitCtrl unit)
    {
        m_unit = unit;
    }
	
    public void OnBecameInvisible()
    {
        Debug.Log("Lose");

        m_unit.m_isInCam = false;// 不在摄像机视野内

        if (UnitUiMgr.self.m_mapUnitUI.ContainsKey(m_unit))
        {
            if (UnitUiMgr.self.m_mapUnitUI[m_unit] != null)
                UnitUiMgr.self.m_mapUnitUI[m_unit].SetActive(false);
        }
    }

    public void OnBecameVisible()
    {
        Debug.Log("Visible");
        m_unit.m_isInCam = false;// 不在摄像机视野内

        if (UnitUiMgr.self.m_mapUnitUI.ContainsKey(m_unit))
        {
            if (UnitUiMgr.self.m_mapUnitUI[m_unit] != null)
                UnitUiMgr.self.m_mapUnitUI[m_unit].SetActive(true);
        }
    }
}
