using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class MiniMap : MonoBehaviour {
    //public GameObject
	// Use this for initialization
    public Dictionary<string, GameObject> m_mapObj = new Dictionary<string, GameObject>();

	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    //创建小地图中的元素
    public void unitCreate(int id,int type,int camp,string icon) 
    {
        
    }

    public void unitCreate(string id,enCamp camp,enUIType type)
    {
        //GameObject obj = Instantiate(m_typeHero) as GameObject;
        //obj.SetActive(true);
        //obj.transform.SetParent(transform);
        //var rect = obj.GetComponent<RectTransform>();
        //rect.localScale = new Vector3(1,1,1);
        //m_mapObj[id] = obj;

        GameObject obj = UIPoolMgr.self.getFromPool(type);
        obj.transform.SetParent(transform);
        var rect = obj.GetComponent<RectTransform>();
        rect.localScale = new Vector3(1,1,1);
        m_mapObj[id] = obj;
    }

    public void unitUpdata(string id, Vector3 pos)
    {
        Vector3 newPos = Vector3.zero;
        newPos.x = pos.x;
        newPos.y = pos.z;
        newPos.z = 0;
        m_mapObj[id].transform.localPosition = newPos;
    }

    public void unitActive(string id,bool bActive)
    {
        if (m_mapObj.ContainsKey(id))
        {m_mapObj[id].SetActive(bActive);}
    }
}
