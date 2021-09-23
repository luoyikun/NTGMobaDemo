using System.ComponentModel;
//using UnityEditor.VersionControl;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class UnitCtrl : MonoBehaviour
{
    //读表属性
    public float m_maxHp = 100;
    public float m_maxMp = 100;
    public float m_viewXLen = 20;//我在世界坐标上能看到的长度
    public float m_viewYLen = 20;
    public int m_tarRange = 5;

    //属性相关
    public float m_hp = 100;
    public float m_mp = 100;
    
    public bool m_isCanMove = true;//一出生就是可以移动,晕住，放技能不可移动
    public bool m_isMoving = false;//一出生不是在移动
    public int m_noMoveCnt = 0; //影响移动的因素
    public bool isCanMove
    {
        get { return (isAlive && m_noMoveCnt == 0); }
    }

    public Vector3 m_dir = Vector3.zero;
    public UnitViewCtrl m_viewCtrl;
    
    public enCamp m_camp;
    public enUIType m_type;
    private bool m_alive = true;
    public string m_id;//唯一id用于服务器通信
    public string m_resId;//资源id，通过其查找资源
    public bool m_isVisible = true;//可见
    public bool m_isTransparent = false;//进入草丛或者隐身技能
    public bool m_isCanLock = true;//是否能够被锁定，针对金身技能
    public List<Renderer> m_listRenderer = new List<Renderer>();//使用链表型的数据一定要初始化
    public bool m_isMe = false;//该单位是否是我自己
    public bool m_isInGrass = false;//是否在草地里
    public SkillHintCtrl m_skillHint;
    //public List<SkillCtrl> m_listSkill;
    public Dictionary<int, SkillCtrl> m_mapSkill = new Dictionary<int, SkillCtrl>();
    public GameObject m_model;// 人物模型
    public Quaternion m_rot;
    //public GameObject m_headLock;//头上锁定
    public UiCtrl m_ui;//跟单位相关的UI
    public bool m_isInCam = false;//是否在摄像机内
    public UnitCtrl m_tar;//我当前的锁定目标
    public enTarPriority m_tarPriority = enTarPriority.None;//目标与优先级要进行同时更改
    public Transform m_transHeadUi;
    public Animator m_aniCtrl;
    public UnityEngine.AI.NavMeshAgent m_navAgent;
    public float m_moveSpeed = 10.0f;
    public bool isAlive
    {
        set { m_alive = value; }
        get { return m_alive; }
    }
    // Use this for initialization
    void Start()
    {
        
    }

    public Vector3 mPos
    {
        get
        {
            Vector3 head = m_transHeadUi.position;
            //Vector3 head = transform.position;
            return head;
        }
    }
    /// <summary>
    /// 初始化
    /// </summary>
    public void init()
    {
        tabDataInit();
        m_rot = m_skillHint.transform.rotation;
        Respawn();
        rendererInit();
        skillInit();
        m_skillHint.init(this);
        m_ui.init(this);
        renderVisibleInit();
        //MainMgr.self.renderInit(m_aniModel.transform);
        //m_aniModel.SetBool("walk", false);
        m_aniCtrl.SetBool("idle", true);
        if (m_isMe)
        {
            StartCoroutine("yieldFindTar");
            StartCoroutine("yieldTarUpdate");
        }
    }

    public void tabDataInit()
    {
        m_tarRange = 5;
    }
    private void skillInit()
    {
        //暂时定死4个技能,后期英雄的技能数量都是定死的。
        //for (int i = 0; i < 4; i++)
        //{
        //    string sName = "Skill" + i.ToString();
        //    Transform trans = transform.FindChild(sName);
        //    SkillCtrl skill = trans.GetComponent<SkillCtrl>();
        //    m_mapSkill[skill.m_id] = skill;
        //}

        Transform skillParent = MainMgr.self.m_preSkillPart.Find(m_resId);
        if (skillParent!=null)
        {
            foreach (Transform trans in skillParent)
            {
                SkillCtrl skill = trans.GetComponent<SkillCtrl>();
                if (skill != null)
                {
                    m_mapSkill[skill.m_idx] = skill;
                }
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (m_isMoving == true)
        {
            m_aniCtrl.SetBool("walk", true);
            Vector3 cubePos = m_model.transform.position;//模型转向
            m_model.transform.LookAt(new Vector3(cubePos.x + m_dir.x, cubePos.y, cubePos.z + m_dir.z));
            //Vector3 newDir = m_model.transform.forward;
            //newDir.y = 0;
            //transform.Translate(newDir * Time.deltaTime * 10);//外部的总物体位移


        }
        else
        {
            m_aniCtrl.SetBool("walk", false);
            m_aniCtrl.SetBool("idle", true);
            m_navAgent.velocity = Vector3.zero;

        }
    }

    //移动摇杆控制
    public void moveSet(bool bCanMove, Vector3 dir)
    {
        if (isCanMove == false)
        {
            return;
        }
        m_dir = dir;
        if (bCanMove && dir.magnitude > 0)
        {
            //m_navAgent.ResetPath();
            m_navAgent.velocity = dir.normalized * m_moveSpeed;
            m_isMoving = true;
        }
        else if (bCanMove == false)
        {
            m_navAgent.velocity = Vector3.zero;
            m_isMoving = false;
        }
    }

    /// <summary>
    /// 重生
    /// </summary>
    public void Respawn()
    {
        m_viewCtrl.Respawn(this);//重生时设置视野
    }

    void renderVisibleInit()
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            UnitVisible visible = r.gameObject.AddComponent<UnitVisible>();//人物模型应该放在子节点的最前面
            visible.init(this);
            break;
        }
    }
    
    public void rendererInit()
    {
        foreach (var render in transform.GetComponentsInChildren<Renderer>())
        {
            m_listRenderer.Add(render);
        }

        foreach (Renderer r in m_listRenderer)
        {
            foreach (var m in r.materials)
            {
                if (m.shader.name == "Legacy Shaders/Bumped Specular")
                    m.shader = Shader.Find("Legacy Shaders/Bumped Specular");
            }
        }
    }

    /// <summary>
    /// 可见与不可见
    /// </summary>
    /// <param name="isVisible"></param>
    public void visibleSet(bool isVisible)
    {
        m_isVisible = isVisible;
        if (isVisible)
        {
            foreach (var renderer in m_listRenderer)
            {
                renderer.enabled = true;
            }
        }
        else
        {
            foreach (var renderer in m_listRenderer)
            {
                renderer.enabled = false;
            }

            //要隐藏头上UI
            //todo
        }
    }

    /// <summary>
    /// 半透明处理
    /// </summary>
    /// <param name="isTransparent"></param>
    public void transparentSet(bool isTransparent)
    {
        m_isTransparent = isTransparent;
        foreach (var renderer in m_listRenderer)
        {
            if (isTransparent == true) //透明
            {
                foreach (var mat in renderer.materials)
                {
                    if (mat.shader.name == "Custom/AlphaSelfIllum")
                    {
                        mat.shader = Shader.Find("Custom/RoleAlpha");
                        mat.SetInt("_Hidden", 1);
                    }
                }
                //todo  隐藏身上buff效果
            }
            else
            {
                foreach (var mat in renderer.materials)
                {
                    if (mat.shader.name == "Custom/RoleAlpha")
                    {
                        mat.shader = Shader.Find("Custom/AlphaSelfIllum");
                    }
                }

                //todo 显示buff
            }
        }
    }

    private IEnumerator yieldFindTar()
    {
        while (true)
        {
            if (m_tar == null && UIMgr.self.m_isSkillHinting == false)//当前没有目标，选择一个最近的，需要每时检测，因为索敌范围内能够出现新的优先级更高的目标
            {
                m_tar = m_viewCtrl.tarNearestGet(ref m_tarPriority);
                if (m_tar != null)
                {
                    UIMgr.self.footLockSet(true, m_tar);
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// 判断当前目标是否超过或者死亡了 1.目标消失范围  2.目标死亡  3.当前有优先级更高的要进行更改
    /// </summary>
    /// <returns></returns>
    private IEnumerator yieldTarUpdate()
    {
        while (true)
        {
            if (m_tar != null && UIMgr.self.m_isSkillHinting == false)
            {
                //是否有更高优先级的目标进入索敌范围
                enTarPriority tarPriority = enTarPriority.None;
                UnitCtrl tarTmp = m_viewCtrl.tarNearestGet(ref tarPriority);
                if (tarTmp != null)//当前有目标
                {
                    if (tarPriority > m_tarPriority)//新的目标的优先级大于老的
                    {
                        m_tar = tarTmp;
                        UIMgr.self.footLockSet(true, m_tar);
                    }
                }

                //目标超过索敌范围,目标死亡
                bool bLose = m_viewCtrl.isLoseTar();
                if (bLose || m_tar.isAlive == false)
                {
                    m_tar = null;
                    UIMgr.self.footLockSet(false);
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    public bool doSkill(int skillID,UnitCtrl tar,Vector3 pos ,Vector3 dir)
    {
        Debug.Log("doSkill");
        bool ret = false;//释放技能有没有成功
        m_aniCtrl.SetInteger("skill", skillID);
        m_aniCtrl.SetBool("walk", false);
        m_aniCtrl.SetTrigger("shoot");
        m_mapSkill[skillID].doSkill(this,pos,dir,tar);
        //isCanMove = false;//放技能时不可移动
        //如果是带有指向性的，都要转动模型位置

        Vector3 cubePos = m_model.transform.position;
        m_model.transform.LookAt(new Vector3(cubePos.x + dir.x,cubePos.y,cubePos.z + dir.z));
        //Vector3 newDir = m_model.transform.forward;
        //newDir.y = 0;
        //transform.Translate(newDir * Time.deltaTime * 10);


        return ret;
    }

    /// <summary>
    /// 判断锁定的目标是否在我当前的范围内
    /// </summary>
    /// <param name="skill"></param>
    /// <returns></returns>
    public bool isTarInSkillRange(SkillCtrl skill)
    {
        bool ret = false;
        if (m_tar != null)
        {
            Vector3 tarPos = m_tar.transform.position;
            Vector3 myPos = transform.position;
            float dis = Vector3.Distance(tarPos, myPos);
            if (dis <= skill.m_disRange)
            {
                return true;
            }
        }
        return ret;
    }

    /// <summary>
    /// 我受到伤害
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="skillBe"></param>
    public void getHurt(UnitCtrl owner,SkillBehaviour skillBe)
    {
        m_hp -= skillBe.m_demage;
        float value = m_hp/m_maxHp;
        m_ui.m_headBar.m_hp.value = value;
    }
}
