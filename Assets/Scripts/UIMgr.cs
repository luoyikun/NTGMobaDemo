using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
public class UIMgr : MonoBehaviour
{
    public Joystick m_playerJoy;//操控玩家手柄
    //public PlayerCtrl m_playerCtrl;//我自己
    public MiniMap m_miniMap;//小地图
    public GameObject m_miniMapObserver;//小地图观察者
    public float m_miniMapScale;//小地图的缩放比例
    static public UIMgr self;
    public List<Button> m_listSkillBtn;
    public List<Joystick> m_listSkillJoy;//我的技能摇杆
    public Vector2 m_skillHint = Vector2.zero;
    public bool m_isSkillHinting = false;//当前是否处在正在技能辅助阶段
    public GameObject m_btnSkillHintCancel;//技能取消按钮
    public GameObject m_headLock;//头上的锁定条是全局唯一的
    public GameObject m_prefab;//立体池
    public float m_screenX;
    public float m_screenY;
    public Transform m_panelRoot;
    public bool m_isCancelSkill = false;//是否取消了技能的释放
    public GameObject m_footLock;//脚下锁定圈
    public List<Joystick> m_listJoySkill; 
	// Use this for initialization
	void Start ()
	{
	    self = this;
	    float fMiniMapBgSize = 300.0f;//小地图的最小尺寸
	    float fTerrainSize = 150.0f;
        //if (fMiniMapBgSize <= fTerrainSize)
	    m_miniMapScale = fMiniMapBgSize/fTerrainSize;

        //技能取消按钮绑定事件
        EventTriggerListener.Get(m_btnSkillHintCancel).onEnter = onEnterBtnSkillHintCancel;
        EventTriggerListener.Get(m_btnSkillHintCancel).onExit = onExitBtnSkillHintCancel;

	    CanvasScaler scaler = m_panelRoot.GetComponent<CanvasScaler>();
        m_screenX = scaler.referenceResolution.x;
        m_screenY = scaler.referenceResolution.y;

        m_btnSkillHintCancel.SetActive(false);
	    
	}
	
    void onEnterBtnSkillHintCancel(GameObject obj)
    {
        if (m_isSkillHinting == true)
        {
            m_isCancelSkill = true;
            MainMgr.self.m_myCtrl.m_skillHint.hintSetColor(false);
        }
    }

    void joySkillInit()
    {
        //后面技能ID要统一 
        m_listJoySkill[0].m_skill = MainMgr.self.m_myCtrl.m_mapSkill[1];
        m_listJoySkill[1].m_skill = MainMgr.self.m_myCtrl.m_mapSkill[2];
        //m_listJoySkill[2].m_skill = MainMgr.self.m_myCtrl.m_mapSkill["2"];
        //m_listJoySkill[3].m_skill = MainMgr.self.m_myCtrl.m_mapSkill["3"];

    }
    void onExitBtnSkillHintCancel(GameObject obj)
    {
        if (m_isSkillHinting == true)
        {
            m_isCancelSkill = false;
            MainMgr.self.m_myCtrl.m_skillHint.hintSetColor(true);
        }
    }

	// Update is called once per frame
	void Update () {
	
	}

    public void init()
    {
        playerJoyInit();
        miniMapInit();
        joySkillInit();//初始化的调用顺序要统一
        //skillPartInit();
        UnitUiMgr.self.init();
    }

    public void playerJoyInit()
    {
        StartCoroutine("yieldPlayerJoyMove");
    }

    /// <summary>
    ///  拿到所有单位创建icon
    /// </summary>
    public void miniMapInit()
    {
        
        //m_miniMap.unitCreate(1,enCamp.Red, enUIType.Hero);
        foreach (var unit in MainMgr.self.m_listUnitMiniMap)
        {
            m_miniMap.unitCreate(unit.m_id, unit.m_camp, unit.m_type);
        }
        StartCoroutine("yieldMiniMap");
    }

    private IEnumerator yieldPlayerJoyMove()
    {
        while (true)
        {
            if (MainMgr.self.m_isKeyCtrl == false)//如果是移动平台，使用移动摇杆
            {
                Vector3 dir = new Vector3(m_playerJoy.m_dir.x, 0, m_playerJoy.m_dir.y);
                MainMgr.self.m_myCtrl.moveSet(m_playerJoy.m_bMoving, dir);
            }
            yield return null;
        }
    }

    /// <summary>
    /// 更新我方探测到的单位icon
    /// </summary>
    /// <returns></returns>
    private IEnumerator yieldMiniMap()
    {
        while (true)
        {
            foreach (var unit in MainMgr.self.m_gridUnitInView)
            {
                Vector3 posPlayer = m_miniMapObserver.transform.InverseTransformPoint(unit.transform.position);
                posPlayer *= m_miniMapScale;
                m_miniMap.unitUpdata(unit.m_id, posPlayer);
            }
            //Vector3 posPlayer = m_miniMapObserver.transform.InverseTransformPoint(MainMgr.self.m_myCtrl.transform.position);
            //Debug.Log(posPlayer);
            //posPlayer *= m_miniMapScale;
            ////posPlayer.y = 0;
            //m_miniMap.unitUpdata(1,posPlayer);
            yield return null;
        }
    }

    /// <summary>
    /// 技能摇杆按下：1.激活取消按钮 2.初始化技能方向 3.摇杆在中间
    /// </summary>
    /// <param name="joy"></param>
    public void onDownSkillJoy(Joystick joy)
    {
        m_isCancelSkill = false;//当前没有取消技能
        m_btnSkillHintCancel.SetActive(true);
        m_isSkillHinting = true;
        MainMgr.self.m_myCtrl.m_skillHint.gameObject.SetActive(true);//使能技能提示器

        if (MainMgr.self.m_myCtrl.m_tar != null && MainMgr.self.m_myCtrl.isTarInSkillRange(joy.m_skill)) //当前存在锁定目标且目标在施法范围内，指示器朝向他
        {
            if (joy.m_skill.m_hintType != enSkillHintType.PointRange)//非小圆
            {
                Vector3 tarPos = MainMgr.self.m_myCtrl.m_tar.transform.position;
                Vector3 myPos = MainMgr.self.m_myCtrl.transform.position;
                Vector3 dir = (tarPos - myPos).normalized;
                Vector2 dir2 = Vector2.zero;
                dir2.x = dir.x;
                dir2.y = dir.z;
                MainMgr.self.m_myCtrl.m_skillHint.hintInit(joy.m_skill, dir2, true); //当前有目标
            }
            else
            {
                Vector3 tarPos = MainMgr.self.m_myCtrl.m_tar.transform.position;
                Vector3 myPos = MainMgr.self.m_myCtrl.transform.position;
                Vector3 dir = (tarPos - myPos) / joy.m_skill.m_disRange;//小圆还要确定指示器的位置

                Vector2 dir2 = Vector2.zero;
                dir2.x = Mathf.Clamp(dir.x,-1.0f,1.0f);
                dir2.y = Mathf.Clamp(dir.z, -1.0f, 1.0f);
                MainMgr.self.m_myCtrl.m_skillHint.hintInit(joy.m_skill, dir2, true); //当前有目标
            }
        }
        else//我当前没有锁定目标
        {
            Vector3 myDir3 = MainMgr.self.m_myCtrl.m_model.transform.forward;
            Vector2 myDir = Vector3.zero;
            myDir.x = myDir3.x;
            myDir.y = myDir3.z;
            MainMgr.self.m_myCtrl.m_skillHint.hintInit(joy.m_skill, myDir,false);//当前没有目标
        }
    }

    public void onDragSkillJoy(Joystick joy)
    {
        MainMgr.self.m_myCtrl.m_skillHint.hintUpdate(joy.m_skill, joy.m_dir);
    }

    public void onUpSkillJoy(Joystick joy)
    {
        m_btnSkillHintCancel.SetActive(false);
        Vector3 pos = MainMgr.self.m_myCtrl.m_skillHint.m_pos;
        Vector3 dir = MainMgr.self.m_myCtrl.m_skillHint.m_dir;
        MainMgr.self.m_myCtrl.m_skillHint.gameObject.SetActive(false);
        MainMgr.self.m_myCtrl.m_skillHint.hintHide();
        m_isSkillHinting = false;
        headLockSet(false, null);

        //释放技能的同时，如果地方英雄在我的技能范围内，强制把我的目标转向技能选中的目标
        if (m_isCancelSkill == false)
        {
            MainMgr.self.m_myCtrl.doSkill(joy.m_skill.m_idx,null, pos, dir);
        }
    }

    /// <summary>
    /// 头上的红柱子
    /// </summary>
    /// <param name="active"></param>
    /// <param name="unit"></param>
    public void headLockSet(bool active,UnitCtrl unit)
    {
        m_headLock.SetActive(active);
        if (active == true)
        {
            m_headLock.transform.SetParent(unit.transform);
            m_headLock.transform.localPosition = new Vector3(0,10,0);
            Renderer hintRenderer = m_headLock.GetComponent<Renderer>();
            hintRenderer.material.SetColor("_Color", Color.red);
        }
        else
        {
            m_headLock.transform.SetParent(m_prefab.transform);
            m_headLock.transform.localPosition = new Vector3(0, 0, 0);
        }
    }

    /// <summary>
    /// 脚下锁定红圈
    /// </summary>
    /// <param name="active"></param>
    /// <param name="unit"></param>
    public void footLockSet(bool active, UnitCtrl unit = null)
    {
        m_footLock.SetActive(active);
        if (active == true)
        {
            m_footLock.transform.SetParent(unit.transform);
            m_footLock.transform.localPosition = new Vector3(0, 0.01f, 0);
        }
        else
        {
            m_footLock.transform.SetParent(m_prefab.transform);
            m_footLock.transform.localPosition = new Vector3(0, 0, 0);
        }
    }
}
