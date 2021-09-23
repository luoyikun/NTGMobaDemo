using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum enSkillHintType
{
    Direction,//方向性
    PointRange,//小圆，指向范围
    Fan,//扇形
    Select,//可以选择目标
    NoTar,//无目标
}
public class SkillHintCtrl : MonoBehaviour
{
    public UnitCtrl m_unit;
    public GameObject m_bg;
    public GameObject m_pointRange;
    public GameObject m_direction;
    public GameObject m_select;
    public GameObject m_fan;
    GameObject m_tarHint;
    public Color m_colNormal;
    public Color m_colCancel;
    private float m_selectDegree = 60.0f;//选择型技能只有一个固定角度
    public Vector3 m_pos;//指示器的世界坐标
    public Vector3 m_dir;//指示器的方向
    public void init(UnitCtrl unit)
    {
        m_unit = unit;
    }

    /// <summary>
    /// 指示器位置更新
    /// </summary>
    /// <param name="skill">摇杆上绑定的技能信息</param>
    /// <param name="dir">摇杆方向向量</param>
    /// <param name="obj">激活的类型指示器</param>
    public void hintUpdate(SkillCtrl skill, Vector2 dir, GameObject obj,bool isTar)
    {
        float bgRange = skill.m_disRange;//施法距离
        Vector3 scale = m_bg.transform.localScale;
        scale.x = bgRange * 0.1f * 2.0f;
        scale.z = scale.x;
        m_bg.transform.localScale = scale;

        float smallRange = skill.m_smallRange;//影响范围
        Vector3 scaleSmall = Vector3.zero;
        if (skill.m_hintType == enSkillHintType.PointRange)
        {
            scaleSmall.x = 0.1f * smallRange * 2.0f;
            scaleSmall.z = scaleSmall.x;
        }
        else if (skill.m_hintType == enSkillHintType.Fan || skill.m_hintType == enSkillHintType.Select)
        {
            //m_fan.transform.localScale = new Vector3(5.0f, 1.0f, 1.0f * bgRange);
            scaleSmall.x = bgRange;
            scaleSmall.z = bgRange;
            float degree = skill.m_hintType == enSkillHintType.Fan ? skill.m_degree : m_selectDegree;
            obj.GetComponent<MeshFan>().angleDegree = degree;
        }
        else 
        {
            scaleSmall.x = 0.1f * smallRange;
            scaleSmall.z = 1.0f;
        }
        scaleSmall.y = 1.0f;
        obj.transform.localScale = scaleSmall;

        if (skill.m_hintType == enSkillHintType.PointRange)
        {
            if (isTar)
                obj.transform.localPosition = new Vector3(dir.x * bgRange, 0, dir.y * bgRange );
            else
                obj.transform.localPosition = new Vector3(0, 0, 0);//当我没有目标时，小圆是在我脚下为中心的
        }
        else
        {
            //if (isTar)
            {
                Vector3 posWorld = obj.transform.position;
                obj.transform.LookAt(new Vector3(posWorld.x + dir.x, posWorld.y, posWorld.z + dir.y));
            }
            //else
            //{
            //    Vector3 posWorld = obj.transform.position;
            //    obj.transform.LookAt(posWorld + MainMgr.self.m_myCtrl.m_model.transform.forward);
            //}
        }

        m_pos = obj.transform.position;
        m_dir = new Vector3(dir.x,0,dir.y);//obj.transform.forward;
        //m_dir = obj.transform.forward;
        //如果是选择提示器，还要进行选择敌人
        if (skill.m_hintType != enSkillHintType.PointRange)
        {
            UnitCtrl unit = tarSelect(skill,obj);
            if (unit != null)
            {
                MainMgr.self.m_myCtrl.m_tar = unit;
                switch (unit.m_type)
                {
                    case enUIType.Hero:
                        MainMgr.self.m_myCtrl.m_tar.m_tarPriority = enTarPriority.Hero;
                        break;
                    case enUIType.Tower:
                    case enUIType.Base:
                        MainMgr.self.m_myCtrl.m_tar.m_tarPriority = enTarPriority.Tower;
                        break;
                    default:
                        MainMgr.self.m_myCtrl.m_tar.m_tarPriority = enTarPriority.Other;
                        break;
                }
                UIMgr.self.footLockSet(true,unit);
            }
        }
        else if (skill.m_hintType == enSkillHintType.PointRange)
        {
            UnitCtrl unit = tarSelectByPointRange(skill, obj);
            if (unit != null)
            {
                MainMgr.self.m_myCtrl.m_tar = unit;
                switch (unit.m_type)
                {
                    case enUIType.Hero:
                        MainMgr.self.m_myCtrl.m_tar.m_tarPriority = enTarPriority.Hero;
                        break;
                    case enUIType.Tower:
                    case enUIType.Base:
                        MainMgr.self.m_myCtrl.m_tar.m_tarPriority = enTarPriority.Tower;
                        break;
                    default:
                        MainMgr.self.m_myCtrl.m_tar.m_tarPriority = enTarPriority.Other;
                        break;
                }
                UIMgr.self.footLockSet(true,unit);
            }
        }
    }

    public void hintSetColor(bool isNormal)
    {
        Color col;
        col = isNormal == true ? m_colNormal:m_colCancel;
        //col = isNormal == true ? Color.blue : Color.red;
        Renderer hintRenderer = m_tarHint.GetComponent<Renderer>();
        hintRenderer.material.SetColor("_Color",col);
        Renderer bgRenderer = m_bg.GetComponent<Renderer>();
        bgRenderer.material.SetColor("_Color",col);
    }

    public void hintInit(SkillCtrl skill, Vector2 dir,bool isTar = false)
    {
        switch (skill.m_hintType)
        {
            case enSkillHintType.PointRange:
                m_pointRange.SetActive(true);
                m_tarHint = m_pointRange;
                break;
            case enSkillHintType.Direction:
                m_direction.SetActive(true);
                m_tarHint = m_direction;
                break;
            case enSkillHintType.Fan:
                m_fan.SetActive(true);
                m_tarHint = m_fan;
                //fanUpdate(skill, dir, m_fan);
                break;
            case enSkillHintType.Select:
                m_select.SetActive(true);
                m_tarHint = m_select;
                break;
        }
        m_bg.SetActive(true);
        hintSetColor(true);
        hintUpdate(skill, dir, m_tarHint, isTar);
    }

    public void hintUpdate(SkillCtrl skill,Vector2 dir)
    {
        hintUpdate(skill, dir, m_tarHint,true);
    }

    public void hintHide()
    {
        m_bg.SetActive(false);
        m_tarHint.SetActive(false);
        m_tarHint.transform.localPosition = Vector3.zero;
        m_bg.transform.localPosition = Vector3.zero;
        m_bg.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        m_tarHint.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }

    /// <summary>
    /// 提示器锁定目标
    /// </summary>
    /// <param name="skill"></param>
    /// <param name="obj"></param>
    public UnitCtrl tarSelect(SkillCtrl skill, GameObject obj)
    {
        UnitCtrl unitSelect = null;
        float fRadius = 0.0f;
        fRadius = skill.m_disRange; //技能的半径
        Vector3 pos = transform.position;
        Collider[] bufCollider = Physics.OverlapSphere(pos, fRadius); //获取周围成员
        List<UnitCtrl> listUnit = new List<UnitCtrl>();
        foreach (var item in bufCollider)
        {
            UnitCtrl unit = item.GetComponent<UnitCtrl>();
            if (unit != null)
            {
                if (!MainMgr.self.isWe(unit.m_camp) && unit.isAlive && unit.m_isVisible && unit.m_isCanLock) //非我方，活着，可见,可锁定
                {
                    listUnit.Add(unit);
                }
            }
        }

        float minDegree = m_selectDegree/2.0f;
        //在大圆范围内的再进行筛选  1.满足选择范围夹角，2.离中心射线夹角最小的  3.离我最近的
        foreach (var unit in listUnit)
        {
            Vector3 unitVec = unit.transform.position - obj.transform.position;
            Vector3 selectVec = obj.transform.forward;
            float degree = Vector3.Angle(unitVec, selectVec);
            if (degree <= minDegree)
            {
                minDegree = degree;
                unitSelect = unit;
            }
        }

        if (skill.m_hintType == enSkillHintType.Select)//如果是选择提示器，需要在头上显示红柱子
        {   
            if (unitSelect != null)
            {
                UIMgr.self.headLockSet(true, unitSelect);
            }
            else
            {
                UIMgr.self.headLockSet(false, unitSelect);
            }
        }

        return unitSelect;
    }

    /// <summary>
    /// 指向范围，选择离圆心近的敌人
    /// </summary>
    /// <param name="skill"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public UnitCtrl tarSelectByPointRange(SkillCtrl skill, GameObject obj)
    {
        UnitCtrl unitSelect = null;
        float fRadius = 0.0f;
        fRadius = skill.m_smallRange; //技能的小圆半径
        Vector3 pos = obj.transform.position;//小圆的世界坐标
        Collider[] bufCollider = Physics.OverlapSphere(pos, fRadius); //获取周围成员
        List<UnitCtrl> listUnit = new List<UnitCtrl>();
        foreach (var item in bufCollider)
        {
            UnitCtrl unit = item.GetComponent<UnitCtrl>();
            if (unit != null)
            {
                if (!MainMgr.self.isWe(unit.m_camp) && unit.isAlive && unit.m_isVisible && unit.m_isCanLock) //非我方，活着，可见，可锁定
                {
                    listUnit.Add(unit);
                }
            }
        }

        //筛选离圆心最近的
        float minDis = fRadius;
        foreach (var unit in listUnit)
        {
            float dis = Vector3.Distance(unit.transform.position, obj.transform.position);
            if (dis < minDis)
            {
                minDis = dis;
                unitSelect = unit;
            }
        }
        return unitSelect;
    }
}
