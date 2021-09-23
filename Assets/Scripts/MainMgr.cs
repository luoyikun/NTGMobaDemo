using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

//这个是代表小地图上类型
public enum enUIType
{
    Hero,
    Soldier,
    Tower,
    Base,
    Wildlife,
    SmallDragon,
    BigDragon,
}

//阵营相关
public enum enCamp
{
    Red,
    Bule,
    Neutrality,
}

public class MainMgr : MonoBehaviour
{
    public static MainMgr self;
    public enCamp m_camp = enCamp.Red;

    public List<UnitCtrl>[,] m_gridAllUnit;// = new List<UnitCtrl>[150,150];//网格中所有unit
    public int[,] m_gridCampUnitCnt;//网格上有多少我方unit,权值
    public List<UnitCtrl> m_gridUnitInView;//网格中可见的unit
    public List<UnitCtrl> m_listUnitMiniMap = new List<UnitCtrl>();//需要创建minimap的unit

    public float m_gridSize = 1.0f;
    public float m_mapSize = 150.0f;

    public GameObject m_terrain;//地形
    public Transform m_prefabs;
    //public Transform m_uiPool;//ui池
    public PlayerCtrl m_myCtrl;
    public int m_gridLen;
    public Camera m_camMyView;
    public float m_camHeight;
    public Transform m_preSkillPart;
    public bool m_isKeyCtrl = false;
    public List<string> m_listResHero = new List<string>();//一共要创建的英雄的资源 
	// Use this for initialization
	void Start ()
	{
        //初始化顺序 1.技能 2.英雄 3.ui
	    self = this;
	    m_camHeight = m_camMyView.transform.position.y;
        viewDataInit();//地图视野格子生成
	    listResHeroInit();
	    resSkillInit();
	    //createHero("Player", enCamp.Red, "H000",new Vector3(90,0,93), true);
        createHero("EnemyA", enCamp.Bule, "H000", new Vector3(100, 0, 100), false);
        createHero("EnemyB", enCamp.Bule, "H000", new Vector3(50, 0, 50), false);
        createHero("EnemyC", enCamp.Bule, "H000", new Vector3(100, 0, 103), false);
        createHero("Friend", enCamp.Red, "H000", new Vector3(100, 0, 90), false);
        createHero("Soldier", enCamp.Bule, "H000", new Vector3(103, 0, 100), false, enUIType.Soldier);
	    

        createHeroFormAsset("Player", enCamp.Red, "H000", new Vector3(90, 0, 93), true);
	    //createSkillFormAsset("H000-0");
        UIMgr.self.init(); //先创建单位，再创建相关ui
	}

    void listResHeroInit()
    {
        m_listResHero.Add("H000");
    }

    //初始化需要的技能资源
    void resSkillInit()
    {
        for (int i = 0; i < m_listResHero.Count; i++)
        {
            GameObject objPart = new GameObject();
            objPart.name = m_listResHero[i];
            objPart.transform.SetParent(m_preSkillPart);
            for (int j = 1; j <= 2; j++)
            {
                string sName = m_listResHero[i] + "-" + j.ToString();
                createSkillFormAsset(sName,objPart.transform);
            }
        }
    }

	// Update is called once per frame
	void Update ()
	{
	    debugLineViewDraw();
	    Vector3 camPos = m_camMyView.transform.position;
	    camPos.x = m_myCtrl.transform.position.x;
        camPos.z = m_myCtrl.transform.position.z - m_camHeight;
	    m_camMyView.transform.position = camPos;

        //控制我自己移动
        if (Input.GetKey(KeyCode.W))
        {
            m_isKeyCtrl = true;
            MainMgr.self.m_myCtrl.moveSet(true, new Vector3(0, 0, 1));
	    }
        else if (Input.GetKey(KeyCode.A))
        {
            m_isKeyCtrl = true;
            MainMgr.self.m_myCtrl.moveSet(true, new Vector3(-1, 0, 0));
	    }
        else if (Input.GetKey(KeyCode.S))
        {
            m_isKeyCtrl = true;
            MainMgr.self.m_myCtrl.moveSet(true, new Vector3(0, 0, -1));
	    }
        else if (Input.GetKey(KeyCode.D))
        {
            m_isKeyCtrl = true;
            MainMgr.self.m_myCtrl.moveSet(true, new Vector3(1, 0, 0));
	    }
        else if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.A)|| Input.GetKeyUp(KeyCode.S)|| Input.GetKeyUp(KeyCode.D) )
        {
            MainMgr.self.m_myCtrl.moveSet(false, new Vector3(0,0,0));
            m_isKeyCtrl = false;
        }
	}

    private IEnumerator yieldPlayerJoyMove(Vector3 dir)
    {
        while (true)
        {
            MainMgr.self.m_myCtrl.moveSet(true, dir);
            yield return null;
        }
    }

    void viewDataInit()
    {
        m_gridLen = (int)(m_mapSize / m_gridSize);
        m_gridAllUnit = new List<UnitCtrl>[m_gridLen, m_gridLen];
        m_gridCampUnitCnt = new int[m_gridLen, m_gridLen];
        m_gridUnitInView = new List<UnitCtrl>();

        for (int i = 0; i < m_gridLen; i++)
        {
            for (int j = 0; j < m_gridLen; j++)
            {
                m_gridAllUnit[i,j] = new List<UnitCtrl>();
                m_gridCampUnitCnt[i, j] = 0;
            }
        }
    }

    public void createSkillFormAsset(string sRes,Transform objPart)
    {
        GameObject obj = ResCtrl.Instance.objCreateFromAsset(sRes);
        obj.transform.parent = objPart;
    }
    public PlayerCtrl createHeroFormAsset(string id, enCamp camp, string sRes, Vector3 pos, bool bPlayer = false, enUIType type = enUIType.Hero)
    {
        GameObject obj = ResCtrl.Instance.objCreateFromAsset(sRes);

        //obj.transform.parent = m_terrain.transform;
        obj.transform.localPosition = pos;//Vector3.zero;
        obj.name = id;
        obj.SetActive(true);
        PlayerCtrl playerCtrl = obj.GetComponent<PlayerCtrl>();
        playerCtrl.m_camp = camp;
        playerCtrl.m_type = type;
        playerCtrl.m_id = id;
        if (bPlayer == true)
        {
            m_myCtrl = playerCtrl;
            playerCtrl.m_isMe = true;
            playerCtrl.m_resId = "H000";
        }
        playerCtrl.init();

        //playerCtrl.visibleSet(false);
        //playerCtrl.transparentSet(true);

        //Destroy(trans);
        m_listUnitMiniMap.Add(playerCtrl);

        //创建单位后，再创建单位UI
        GameObject preUnitUI = UIPoolMgr.self.m_unitUI;
        //GameObject unitUi = Instantiate(preUnitUI,Vector3.zero,Quaternion.identity) as GameObject;
        GameObject unitUi = Instantiate(Resources.Load("Prefabs/UnitUI")) as GameObject;
        unitUi.SetActive(true);
        UiHeadBar headBar = unitUi.transform.GetComponentInChildren<UiHeadBar>();
        headBar.init(playerCtrl);
        playerCtrl.m_ui.m_headBar = headBar;
        UnitUiMgr.self.m_mapUnitUI[playerCtrl] = unitUi;
        unitUi.transform.SetParent(UnitUiMgr.self.m_heroParent);
        unitUi.transform.localPosition = Vector3.zero;
        unitUi.transform.localScale = new Vector3(1, 1, 1);
        //Destroy(trans.gameObject);
        return playerCtrl;
    }


    public PlayerCtrl createHero(string id,enCamp camp,string sRes,Vector3 pos,bool bPlayer = false,enUIType type = enUIType.Hero)
    {
        Transform trans = m_prefabs.Find("PreHeroPart/" + sRes);
        GameObject obj = Instantiate(trans.gameObject);
  
        obj.transform.parent = m_terrain.transform;
        obj.transform.localPosition = pos;//Vector3.zero;
        obj.name = id;
        obj.SetActive(true);
        PlayerCtrl playerCtrl = obj.GetComponent<PlayerCtrl>();
        playerCtrl.m_camp = camp;
        playerCtrl.m_type = type;
        playerCtrl.m_id = id;
        if (bPlayer == true)
        {
            m_myCtrl = playerCtrl;
            playerCtrl.m_isMe = true;
            playerCtrl.m_resId = "H000";
        }
        playerCtrl.init();
       
        //playerCtrl.visibleSet(false);
        //playerCtrl.transparentSet(true);
       
        //Destroy(trans);
        m_listUnitMiniMap.Add(playerCtrl);

        //创建单位后，再创建单位UI
        GameObject preUnitUI = UIPoolMgr.self.m_unitUI;
        //GameObject unitUi = Instantiate(preUnitUI,Vector3.zero,Quaternion.identity) as GameObject;
        GameObject unitUi = Instantiate(Resources.Load("Prefabs/UnitUI")) as GameObject;
        unitUi.SetActive(true);
        UiHeadBar headBar = unitUi.transform.GetComponentInChildren<UiHeadBar>();
        headBar.init(playerCtrl);
        playerCtrl.m_ui.m_headBar = headBar;
        UnitUiMgr.self.m_mapUnitUI[playerCtrl] = unitUi;
        unitUi.transform.SetParent(UnitUiMgr.self.m_heroParent);
        unitUi.transform.localPosition = Vector3.zero;
        unitUi.transform.localScale = new Vector3(1,1,1);
        //Destroy(trans.gameObject);
        return playerCtrl;
    }

    public bool isInRange(int x)
    {
        bool bIn = false;
        if (x >= 0 && x < m_gridLen)
        {
            bIn = true;
        }
        return bIn;
    }

    //绘制视野的辅助线
    public void debugLineViewDraw()
    {
        foreach (var unit in m_listUnitMiniMap)
        {
            if (unit.m_camp == m_camp)
            {
                Vector3 pos = unit.transform.position;

                Vector3 topLeft = new Vector3(pos.x - unit.m_viewXLen, 1, pos.z + unit.m_viewYLen);
                Vector3 topRight = new Vector3(pos.x + unit.m_viewXLen, 1, pos.z + unit.m_viewYLen);
                Vector3 bottomLeft = new Vector3(pos.x - unit.m_viewXLen, 1, pos.z - unit.m_viewYLen);
                Vector3 bottomRight = new Vector3(pos.x + unit.m_viewXLen, 1, pos.z - unit.m_viewYLen);
                Color col;
                if (unit.m_isMe == true)
                {
                    //绘制我的当前方向
                    col = Color.green;
                    Quaternion r = unit.m_model.transform.rotation;
                    Vector3 f0 = (unit.m_model.transform.position + (r * Vector3.forward) * 5.0f);
                    Debug.DrawLine(pos, f0, col);

                    //绘制我的索敌范围
                    Vector3 topTarLeft = new Vector3(pos.x - unit.m_tarRange, 1, pos.z + unit.m_tarRange);
                    Vector3 topTarRight = new Vector3(pos.x + unit.m_tarRange, 1, pos.z + unit.m_tarRange);
                    Vector3 bottomTarLeft = new Vector3(pos.x - unit.m_tarRange, 1, pos.z - unit.m_tarRange);
                    Vector3 bottomTarRight = new Vector3(pos.x + unit.m_tarRange, 1, pos.z - unit.m_tarRange);

                    col = Color.green;
                    Debug.DrawLine(topTarLeft, topTarRight, col);
                    Debug.DrawLine(topTarLeft, bottomTarLeft, col);
                    Debug.DrawLine(bottomTarLeft, bottomTarRight, col);
                    Debug.DrawLine(topTarRight, bottomTarRight, col);
                    col = Color.blue;
                }
                else
                {
                    col = Color.red;
                }

                //绘制视野范围
                Debug.DrawLine(topLeft, topRight, col);
                Debug.DrawLine(topLeft, bottomLeft, col);
                Debug.DrawLine(bottomLeft, bottomRight, col);
                Debug.DrawLine(topRight, bottomRight, col);


            }
        }
    }

    public bool isOppose(enCamp camp)
    {
        bool bEnemy = false;
        if (camp != m_camp && camp != enCamp.Neutrality)
        {
            bEnemy = true;
        }
        return bEnemy;
    }

    public bool isWe(enCamp camp)
    {
        bool bWe = false;
        if (camp == m_camp)
        {
            bWe = true;
        }
        return bWe;
    }

    public bool isNeutrality(enCamp camp)
    {
        bool b = false;
        if (camp == enCamp.Neutrality)
        {
            b = true;
        }
        return b;
    }

    public void renderInit(Transform trans)
    {
        var listRender = trans.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < listRender.Length; i++)
        {
            trans.GetComponentsInChildren<Renderer>()[i].material.shader =
                UnityEngine.Shader.Find(listRender[i].material.shader.name);
        }
    }
}
