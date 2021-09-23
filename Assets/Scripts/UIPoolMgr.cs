using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/// <summary>
/// ui内存池，对反复创建销毁的UI放入池中
/// </summary>
public class UIPoolMgr : MonoBehaviour
{
    static public UIPoolMgr self = null;
    private Dictionary<enUIType, List<GameObject>> m_mapPool = new Dictionary<enUIType, List<GameObject>>();//内存池,enUIType 代表类型，list为那种类型保存的obj
    public GameObject m_typeHero;
    public GameObject m_unitUI;//英雄的头上血条

	// Use this for initialization
	void Start ()
	{
        self = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public GameObject getFromPool(enUIType sType)
    {
        GameObject obj = null;
        if (m_mapPool.ContainsKey(sType) && m_mapPool[sType].Count > 0)
        {
            obj = m_mapPool[sType][0];
            obj.SetActive(true);
            m_mapPool[sType].RemoveAt(0);
        }
        else
        {
            switch (sType)
            {
                case enUIType.Hero:
                    obj = Instantiate(m_typeHero) as GameObject;
                    obj.SetActive(true);
                    break;
                case enUIType.Soldier:
                    obj = Instantiate(m_typeHero) as GameObject;
                    obj.SetActive(true);
                    break;
            }
        }
        return obj;
    }
}
