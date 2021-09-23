using System.ComponentModel;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class SkillBehaviour : MonoBehaviour {
    public string m_id = "1";
    public float m_disRange = 5.0f;//技能释放距离，大圆
    public float m_smallRange = 1.0f;//技能影响范围
    public float m_degree = 0.0f;//对扇形技能的度数
    public enSkillHintType m_hintType = enSkillHintType.PointRange;
    public GameObject m_preSkill;
    public CapsuleCollider m_collider;

    public List<UnitCtrl> m_listHitUnit = new List<UnitCtrl>();
    private GameObject effectObject = null;
    public UnitCtrl m_owner;
    public UnitCtrl m_tar;
    public float m_demage = 1.0f;
    // Update is called once per frame
    public virtual void doSkill(UnitCtrl unitOwner,Vector3 pos, Vector3 dir,UnitCtrl unitTar)
    {
        transform.position = pos;
        m_owner = unitOwner;
        m_tar = unitTar;
        foreach (Transform obj in transform)
        {
            foreach (Transform trans in obj)
            {
                trans.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }
        }
        foreach (var fx in transform.GetComponentsInChildren<ParticleSystem>())
        {
            fx.Stop();
            fx.Play();
        }
        m_collider = transform.GetComponent<CapsuleCollider>();
        StopCoroutine("doFly");
        StartCoroutine("doFly");
    }

    private IEnumerator doFly()
    {
        var d = 0.0f;
        while (d < 1.0f)
        {
            m_collider.enabled = true;
            yield return new WaitForSeconds(0.2f);
            m_collider.enabled = false;

            yield return new WaitForSeconds(0.2f);
            d += 0.4f;
        }

        yield return new WaitForSeconds(2.0f);//等待多少秒后删除自身
        Destroy(gameObject);
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if ( m_owner == null)
            return;
        var otherUnit = other.GetComponent<UnitCtrl>();
        if (otherUnit != null)
        {
            if (otherUnit.isAlive && !MainMgr.self.isWe(otherUnit.m_camp)) //todo mask 增加免疫伤害，例如技能伤害对防御塔无效
            {
                //todo 叠加被动就一次


                otherUnit.getHurt(m_owner,this);
            }
        }
    }
}
