using System;
using System.Timers;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
public class Joystick : MonoBehaviour
{
    public GameObject m_objJoyCollider;//触发摇杆区域
    public GameObject m_objThumb;//中心小圆
    public GameObject m_objBg;//背景
    public Canvas can;

    private float m_limitBg;
    private float m_limitThumb;

    public bool m_bMoving;//包含是否点击到，是否可见
    public Vector2 m_dir = Vector2.zero;//方向供外部调用摇杆的方向
    public Touch m_touch;
    public bool m_isBgFixed = false;//bg是否是固定位置
    public bool m_isCanMove = true;//把它当做普通按钮还是摇杆处理
    public bool m_isFirstShow = true;//创建后是否要显示出来
    public bool m_isSkill = false;//是否是技能摇杆
    public string m_id;
    public float m_colliderRadius;//触摸区域的半径

    public SkillCtrl m_skill;//如果是技能摇杆对应的技能
	// Use this for initialization
	void Start ()
	{
	    float thumbRadius = m_objThumb.GetComponent<RectTransform>().sizeDelta.x/2;
	    float bgRadius = m_objBg.GetComponent<RectTransform>().sizeDelta.x/2;
        m_colliderRadius = m_objJoyCollider.GetComponent<RectTransform>().sizeDelta.x / 2;
        m_limitBg = m_colliderRadius - thumbRadius - bgRadius;
	    m_limitThumb = bgRadius;
        EventTriggerListener.Get(m_objJoyCollider).onDown += onDownCollider;
        EventTriggerListener.Get(m_objJoyCollider).onUp += onUpCollider;
        EventTriggerListener.Get(m_objJoyCollider).onDrag += onDragCollider;
	    m_bMoving = false;
        visibleSet(m_isFirstShow);
	    if (m_isSkill == true && m_skill != null)
	    {
	        m_id = m_skill.m_id;
	    }
	}

    public void visibleSet(bool isVisible)
    {
        float alp = (isVisible == true) ? 0.5f : 0.0f;
        transform.GetComponent<CanvasGroup>().alpha = alp;
    }
    void onUpCollider(GameObject obj)
    {
        if (m_isCanMove)
        {
            StopCoroutine("yieldStickMove");//手指移开要清楚协程
        }       
        m_objBg.transform.localPosition = new Vector3(0,0,0);
        m_objThumb.transform.localPosition = new Vector3(0,0,0);
        m_bMoving = false;
        
        visibleSet(m_isFirstShow);
    }

    private void onDownCollider(GameObject obj)
    {
        Debug.Log("JoystickDown");
        if (m_isSkill == true && UIMgr.self.m_isSkillHinting == true)
        {
            return;
        }
        m_bMoving = true;
        if (m_isCanMove)
        {
            visibleSet(true);
            RectTransform rect = transform as RectTransform;
            Vector3 newPos;
            if (Application.platform == UnityEngine.RuntimePlatform.WindowsEditor)
            {
                //isTouchInCollider(Input.mousePosition);

                newPos = uiPosGet(Input.mousePosition, rect);
            }
            else
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (isTouchInCollider(Input.touches[i].position))
                    {
                        m_touch = Input.touches[i];
                    }
                }
                newPos = uiPosGet(m_touch.position, rect);
            }
            if (m_isBgFixed == false)//对于移动摇杆，背景不是固定位置的
            {
                float dis = Vector3.Distance(newPos, new Vector3(0, 0, 0)); //鼠标与中心距离
                if (dis <= m_limitBg)
                {
                    newPos = newPos*dis/m_limitBg;
                }
                else
                {
                    Vector3 normalPos = newPos.normalized;
                    newPos = normalPos*m_limitBg*1.5f;
                }
                m_objBg.transform.localPosition = newPos;
                StartCoroutine("yieldStickMove");
            }

            if (m_isSkill)//技能摇杆，按下位置为中心位置
            {
                m_objThumb.transform.localPosition = new Vector3(0, 0, 0);
            }
        }
    }

    void onDragCollider(GameObject obj)
    {
        //Debug.Log("onDragCollider");
        if (m_isSkill)
        {
            StartCoroutine("yieldStickMove");
        }
    }

    Vector3 uiPosGet(Vector3 pos,RectTransform rect)
    {
        Vector2 pos2D;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, pos, can.worldCamera, out pos2D);
        Vector3 newPos = new Vector3(pos2D.x, pos2D.y, 0);
        return newPos;
    }

    /// <summary>
    /// 判断当前touch pos是否落在Collider范围内
    /// </summary>
    /// <param name="pos">触摸坐标</param>
    /// <param name="rect">joy的RectTransform</param>
    /// <returns></returns>
    bool isTouchInCollider(Vector3 pos)
    {
        bool bRet = false;
        RectTransform rect = transform as RectTransform;
        Vector3 newPos = uiPosGet(pos, rect);
        if (newPos.x >= -m_colliderRadius && newPos.x <= m_colliderRadius && newPos.y >= -m_colliderRadius &&
            newPos.y <= m_colliderRadius)
        {
            bRet = true;
        }
        return bRet;
    }
    private IEnumerator yieldStickMove()
    {
        while (true)
        {
            RectTransform rect = m_objBg.GetComponent<RectTransform>();
            Vector3 realTouchPos = Vector3.zero;
            if (Application.platform == UnityEngine.RuntimePlatform.WindowsEditor)
            {
                realTouchPos = uiPosGet(Input.mousePosition, rect);
            }
            else
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (m_touch.fingerId == Input.touches[i].fingerId)
                    {
                        realTouchPos = uiPosGet(Input.touches[i].position, rect);
                    }
                }
            }
       
            float dis = Vector3.Distance(new Vector3(0,0,0), realTouchPos);
            if (dis <= m_limitThumb)
            {
                //Vector3 normalPos = realTouchPos.normalized;
                //realTouchPos = normalPos * dis/m_limitThumb;
                m_objThumb.transform.localPosition = realTouchPos;
            }
            else
            {
                Vector3 normalPos = realTouchPos.normalized;
                realTouchPos = normalPos * m_limitThumb;
                m_objThumb.transform.localPosition = realTouchPos;
            }


            //Vector3 posNor = m_objThumb.transform.localPosition.normalized;
            //m_dir.x = posNor.x;
            //m_dir.y = posNor.y;

            Vector3 posNor = m_objThumb.transform.localPosition;
            m_dir.x = posNor.x / m_limitThumb;
            m_dir.y = posNor.y / m_limitThumb;

            //Vector3 cubePos = m_cube.transform.position;
            //m_cube.transform.LookAt(new Vector3(cubePos.x + m_dir.x, cubePos.y, cubePos.z + m_dir.y));
            //m_cube.transform.Translate(Vector3.forward * Time.deltaTime * 70);

            yield return null;
        }
    }
}
