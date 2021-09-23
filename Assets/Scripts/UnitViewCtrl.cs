using System.Linq;
//using UnityEditor.VersionControl;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum enTarPriority
{
    None,
    Other,
    Tower,
    Hero,
}

public class UnitViewCtrl : MonoBehaviour
{
    public UnitCtrl m_unitCtrl;
    private List<UnitCtrl> m_listUnitInView = new List<UnitCtrl>();
    public int m_gridX;//我当前所在格子x
    public int m_gridY;
    public float m_gridSize;
    public int m_gridViewXLen;//转为真实的格子数
    public int m_gridViewYLen;
    public bool m_isShowInMiniMap = false;
    

    //当前离我最近的目标
    public UnitCtrl m_hero = null;
    public UnitCtrl m_other = null;
    public UnitCtrl m_tower = null;

    //离我最近的列表
    public List<UnitCtrl> m_listHero = new List<UnitCtrl>();
    public List<UnitCtrl> m_listTower = new List<UnitCtrl>();
    public List<UnitCtrl> m_listOther = new List<UnitCtrl>();
 
    public List<UnitCtrl> listUnitInView
    {
        get
        {
            m_listUnitInView.Clear();
            return m_listUnitInView;
        }
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    /// <summary>
    /// 单位重生时调用,重置视野
    /// </summary>
    /// <param name="unitCtrl">单位的控制器</param>
    public void Respawn(UnitCtrl unitCtrl)
    {
        m_unitCtrl = unitCtrl;

        m_gridSize = MainMgr.self.m_gridSize;//每一格所占的尺寸（世界坐标）
        m_gridViewXLen = (int) (m_unitCtrl.m_viewXLen/m_gridSize);//我x方向上能看到的格子数量
        m_gridViewYLen = (int) (m_unitCtrl.m_viewYLen/m_gridSize);

        m_gridX = (int)(m_unitCtrl.transform.position.x/m_gridSize);
        m_gridY = (int) (m_unitCtrl.transform.position.z/m_gridSize);

        if (MainMgr.self.isInRange(m_gridX) && MainMgr.self.isInRange(m_gridY))
        {(MainMgr.self.m_gridAllUnit[m_gridX,m_gridY]).Add(m_unitCtrl);}

        if (m_unitCtrl.m_camp == MainMgr.self.m_camp)//如果是同一阵营的,unit视野下的方块权值+1
        {
            for (int x = m_gridX - m_gridViewXLen; x <= m_gridX + m_gridViewXLen; x++)
            {
                if (MainMgr.self.isInRange(x))
                {
                    for (int y = m_gridY - m_gridViewYLen; y <= m_gridY + m_gridViewYLen; y++)
                    {
                        if (MainMgr.self.isInRange(y))
                        {
                            MainMgr.self.m_gridCampUnitCnt[x, y]++;//权值加1
                        }
                    }
                }
            }
            unitShowInMiniMap();
        }
        else
        {
            //updataGrid();
        }
        StartCoroutine("yieldUpdataGrid");
    }

    void updataGrid()
    {
        //当前所在格子
        int gridX = (int)(m_unitCtrl.transform.position.x / m_gridSize);
        int gridY = (int)(m_unitCtrl.transform.position.z / m_gridSize);

        //与上一帧所在格子做比较
        int offsetX = gridX - m_gridX;
        int offsetY = gridY - m_gridY;


        if (m_unitCtrl.m_camp == MainMgr.self.m_camp) //跟我是同一阵营的，更新地图小格子的权值
        {
            if (offsetX > 0) //向右走
            {
                int left = m_gridX - m_gridViewXLen;
                int right = m_gridX + m_gridViewXLen + 1;
                for (int x = 0; x < offsetX; x++)
                {
                    for (int y = m_gridY - m_gridViewYLen; y <= m_gridY + m_gridViewYLen; y++)
                    {
                        if (MainMgr.self.isInRange(y))
                        {
                            if (MainMgr.self.isInRange(left + x))
                                MainMgr.self.m_gridCampUnitCnt[left + x, y]--;
                            if (MainMgr.self.isInRange(right + x))
                                MainMgr.self.m_gridCampUnitCnt[right + x, y]++;
                        }
                    }
                }
            }
            else if (offsetX < 0) //向左走
            {
                int left = m_gridX - m_gridViewXLen - 1;
                int right = m_gridX + m_gridViewXLen;
                for (int x = 0; x > offsetX; x--)
                {
                    for (int y = m_gridY - m_gridViewYLen; y <= m_gridY + m_gridViewYLen; y++)
                    {
                        if (MainMgr.self.isInRange(y))
                        {
                            if (MainMgr.self.isInRange(right + x))
                                MainMgr.self.m_gridCampUnitCnt[right + x, y]--;
                            if (MainMgr.self.isInRange(left + x))
                                MainMgr.self.m_gridCampUnitCnt[left + x, y]++;
                        }
                    }
                }
            }

            if (offsetY > 0) //向上走
            {
                int top = m_gridY + m_gridViewYLen + 1;
                int bottom = m_gridY - m_gridViewYLen;
                for (int y = 0; y < offsetY; y++)
                {
                    for (int x = gridX - m_gridViewXLen; x <= gridX + m_gridViewXLen; x++)
                    {
                        if (MainMgr.self.isInRange(x))
                        {
                            if (MainMgr.self.isInRange(bottom + y))
                                MainMgr.self.m_gridCampUnitCnt[x, bottom + y]--;
                            if (MainMgr.self.isInRange(top + y))
                                MainMgr.self.m_gridCampUnitCnt[x, top + y]++;
                        }
                    }
                }
            }
            else if (offsetY < 0) //向下走
            {
                int top = m_gridY + m_gridViewYLen;
                int bottom = m_gridY - m_gridViewYLen - 1;
                for (int y = 0; y > offsetY; y--)
                {
                    for (int x = gridX - m_gridViewXLen; x <= gridX + m_gridViewXLen; x++)
                    {
                        if (MainMgr.self.isInRange(x))
                        {
                            if (MainMgr.self.isInRange(top + y))
                                MainMgr.self.m_gridCampUnitCnt[x, top + y]--;
                            if (MainMgr.self.isInRange(bottom + y))
                                MainMgr.self.m_gridCampUnitCnt[x, bottom + y]++;
                        }
                    }
                }
            }
        }

        //有偏差，更新m_gridAllUnit数据
        if (offsetX != 0 || offsetY != 0)
        {
            MainMgr.self.m_gridAllUnit[m_gridX, m_gridY].Remove(m_unitCtrl);
            m_gridX = gridX;
            m_gridY = gridY;
            try
            {
                MainMgr.self.m_gridAllUnit[m_gridX, m_gridY].Add(m_unitCtrl);
            }
            catch
            {
                //Debug.Log(m_gridX + "++++++++++++++++++" + m_gridY);
            }
        }

        if (m_unitCtrl.m_camp != MainMgr.self.m_camp)//如果单位和我是不同阵营的
        {
            if (MainMgr.self.isInRange(gridX) && MainMgr.self.isInRange(gridY))
            {
                if (m_unitCtrl.m_isInGrass == false)//只有敌方不在草丛中才会被正常视野探测到
                {
                    if (MainMgr.self.m_gridCampUnitCnt[gridX, gridY] > 0)
                    {
                        unitShowInMiniMap();
                    }
                    else
                    {
                        unitHideInMinimap();
                    }
                }
            }
        }
    }
    //重生后每帧调用更新视野中的格子
    private IEnumerator yieldUpdataGrid()
    {
        while (m_unitCtrl.isAlive)
        {
            updataGrid();
            yield return null;
        }
    }

    public void unitShowInMiniMap()
    {
        if (m_isShowInMiniMap == false)
        {
            if (MainMgr.self.m_gridUnitInView.Contains(m_unitCtrl) && m_unitCtrl.isAlive)//草丛里尸体不加入到小地图中
            {
                return;
            }
            MainMgr.self.m_gridUnitInView.Add(m_unitCtrl);
            UIMgr.self.m_miniMap.unitActive(m_unitCtrl.m_id, true);
            m_isShowInMiniMap = true;
        }
    }

    /// <summary>
    /// 只是隐藏icon，不销毁，在小地图中
    /// </summary>
    public void unitHideInMinimap()
    {
        if (m_isShowInMiniMap == true)
        {
            if (MainMgr.self.m_gridUnitInView.Contains(m_unitCtrl))
            {
                MainMgr.self.m_gridUnitInView.Remove(m_unitCtrl);
                UIMgr.self.m_miniMap.unitActive(m_unitCtrl.m_id, false);
            }
            m_isShowInMiniMap = false;
        }
    }

    public UnitCtrl tarNearestGet(ref enTarPriority tarPriority)
    {
        UnitCtrl unit = null;
        tarPriority = enTarPriority.None;
        tarListGet();
        switch (m_unitCtrl.m_type)
        {
            case enUIType.Hero://我是英雄
                unit = nearestGet(m_listHero);//优先选择英雄
                if (unit != null)
                {
                    tarPriority = enTarPriority.Hero;
                    return unit;
                }
                unit = nearestGet(m_listTower);//塔
                if (unit != null)
                {
                    tarPriority = enTarPriority.Tower;
                    return unit;
                }
                unit = nearestGet(m_listOther);//其他：这里出现问题，当我在野区，敌方英雄离开索敌范围，会攻击野怪？？
                if (unit != null)
                {
                    tarPriority = enTarPriority.Other;
                    return unit;            
                }
                break;
        }
        return unit;
    }

    public void tarListGet()
    {
        bool bRet = false;
        m_listHero.Clear();
        m_listTower.Clear();
        m_listOther.Clear();
        for (int i = m_gridX - m_unitCtrl.m_tarRange; i <= m_gridX + m_unitCtrl.m_tarRange; i++)
        {
            if (MainMgr.self.isInRange(i))
            {
                for (int j = m_gridY - m_unitCtrl.m_tarRange; j <= m_gridY + m_unitCtrl.m_tarRange; j++)
                {
                    if (MainMgr.self.isInRange(j))
                    {
                        //MainMgr.self.m_gridAllUnit[m_gridX, m_gridY].Remove(m_unitCtrl);
                        foreach (var item in MainMgr.self.m_gridAllUnit[i, j])
                        {
                            if (!MainMgr.self.isWe(item.m_camp) && item.isAlive && item.m_isVisible && item.m_isCanLock) //非我方，活着，可见,可锁定（非金身状态）
                            {
                                switch (item.m_type)
                                {
                                    case enUIType.Hero:
                                        if (!m_listHero.Contains(item))
                                            m_listHero.Add(item);
                                        break;
                                    case enUIType.Tower:
                                    case enUIType.Base:
                                        if (!m_listTower.Contains(item))
                                            m_listTower.Add(item);
                                        break;
                                    default:
                                        if (!m_listOther.Contains(item))
                                            m_listOther.Add(item);
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        //m_hero = nearestGet(m_listHero);
        //m_tower = nearestGet(m_listTower);
        //m_other = nearestGet(m_listOther);
    }

    /// <summary>
    /// 从表中选择最近的单位
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public UnitCtrl nearestGet(List<UnitCtrl> list)
    {
        UnitCtrl unit = null;
        int x = m_unitCtrl.m_tarRange;
        int y = m_unitCtrl.m_tarRange;
        foreach (var item in list)
        {
            int itemX = Mathf.Abs(item.m_viewCtrl.m_gridX - m_gridX);
            int itemY = Mathf.Abs(item.m_viewCtrl.m_gridY - m_gridY);
            if (itemX <= x && itemY <= y)
            {
                x = itemX;
                y = itemY;
                unit = item;
            }
        }
        return unit;
    }

    /// <summary>
    /// 判断是否丢失目标,采用格子法
    /// </summary>
    /// <returns></returns>
    public bool isLoseTar()
    {
        bool bLose = false;
        int xDiff = Mathf.Abs(m_gridX - m_unitCtrl.m_tar.m_viewCtrl.m_gridX);
        int yDiff = Mathf.Abs(m_gridY - m_unitCtrl.m_tar.m_viewCtrl.m_gridY);
        if (xDiff > m_unitCtrl.m_tarRange || yDiff > m_unitCtrl.m_tarRange)
        {
            return true;
        }
        return bLose;
    }
}
