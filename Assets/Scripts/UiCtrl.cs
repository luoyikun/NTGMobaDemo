using UnityEngine;
using System.Collections;

//控制跟单元相关的ui，血条，伤害飘字,回城管理
public class UiCtrl : MonoBehaviour
{
    public UnitCtrl m_unit;//
    public UiHeadBar m_headBar;
    public void init(UnitCtrl unit)
    {
        m_unit = unit;
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
