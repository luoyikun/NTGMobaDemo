using UnityEngine;
using System.Collections;
using UnityEngine.UI;
//单位头上UIbar
public enum enHeadBarType
{
    Me,
    OurHero,
    EnemyHero,
    Soldier,
    Wildlife,//野怪，加长点

}
public class UiHeadBar : MonoBehaviour
{
    public enHeadBarType m_headBarType;
    public Slider m_hp;//血条
    public Slider m_mp;// 魔方值或怒气值
    public UnitCtrl m_unit;

    public void init(UnitCtrl unit)
    {
        m_unit = unit;
    }
    // Use this for initialization
	void Start ()
	{
        //m_hp = transform.Find("hp").GetComponent<Slider>();
        //if (isHero(m_headBarType))
        //    m_mp = transform.Find("mp").GetComponent<Slider>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    //判断是否是英雄
    bool isHero(enHeadBarType headBarType)
    {
        bool bRet = false;
        if (headBarType == enHeadBarType.Me || headBarType == enHeadBarType.OurHero ||
            headBarType == enHeadBarType.EnemyHero)
        {
            bRet = true;
        }
        return bRet;
    }

    public void hpSet(float hp)
    {
        float val = hp/m_unit.m_maxHp;
        m_hp.value = val;
    }
}
