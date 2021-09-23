using UnityEngine;
using System.Collections;

//一些技能影响自身状态
public class SkillCtrl : MonoBehaviour
{
    public string m_id; //自身id，能够索引英雄
    public int m_idx;//技能对应的索引
    public float m_disRange = 5.0f;//技能释放距离，大圆
    public float m_smallRange = 1.0f;//技能影响范围
    public float m_degree = 0.0f;//对扇形技能的度数
    public enSkillHintType m_hintType = enSkillHintType.PointRange;
    public GameObject m_preSkill;
    public CapsuleCollider m_collider;
    public UnitCtrl m_unit;
    private float m_hardTime =2.0f;
	// Use this for initialization
	void Start ()
	{
	    if (transform.childCount > 0)
	    {
	        m_preSkill = transform.GetChild(0).gameObject;
	    }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public virtual void doSkill(UnitCtrl unitOwner,Vector3 pos,Vector3 dir,UnitCtrl unitTar)
    {
        m_unit = unitOwner;
        GameObject effectObject = (GameObject)Instantiate(m_preSkill);
		if(effectObject != null)
		{
            effectObject.SetActive(true);
		    //effectObject.transform.position = pos;
		    SkillBehaviour skillBe = effectObject.transform.GetComponent<SkillBehaviour>();
		    if (skillBe != null)
		    {
                skillBe.doSkill(unitOwner, pos, dir, unitTar);
		    }
		}

        //进入硬直时间跑秒
        StartCoroutine("yieldHardTime");
    }

    private IEnumerator yieldHardTime()
    {
        m_unit.m_noMoveCnt++;
        m_unit.m_navAgent.velocity = Vector3.zero;
        m_unit.m_isMoving = false;
        float hardTimeCnt = 0;
        while (hardTimeCnt < m_hardTime)
        {
            yield return null;
            hardTimeCnt += Time.deltaTime;
            
        }
        m_unit.m_noMoveCnt--;
    }
   
}
