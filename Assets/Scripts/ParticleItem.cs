using UnityEngine;
using System.Collections;

/// <summary>
/// 粒子销毁
/// </summary>
public class ParticleItem : MonoBehaviour 
{
	public GameObject[] particleList;

    void Start()
    {
        var listRender = transform.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < listRender.Length; i++)
        {
            transform.GetComponentsInChildren<Renderer>()[i].material.shader =
                UnityEngine.Shader.Find(listRender[i].material.shader.name);
        }
    }
	void LateUpdate()
	{
		bool destoryStatus = true;
		foreach(GameObject particleObject in particleList)
		{
			if(particleObject != null)
			{
				if(particleObject.GetComponent<ParticleSystem>() == null || particleObject.GetComponent<ParticleSystem>().IsAlive()) destoryStatus = false;
			}
		}
		if(destoryStatus) Destroy(this.gameObject);
	}
}
