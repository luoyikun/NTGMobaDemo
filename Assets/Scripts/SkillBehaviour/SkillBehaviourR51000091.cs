using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SkillBehaviourR51000091 : SkillBehaviour
{
    private Vector3 startPos;
    public override void doSkill(UnitCtrl unitOwner,Vector3 pos, Vector3 dir,UnitCtrl unitTar)
    {
        startPos = pos;
        transform.position = pos;
        pos.y += 2;
        transform.position = pos;
        m_owner = unitOwner;
        m_tar = unitTar;

        Vector3 posWorld = transform.position;
        transform.LookAt(new Vector3(posWorld.x + dir.x, posWorld.y, posWorld.z + dir.z));

        foreach (var fx in transform.GetComponentsInChildren<ParticleSystem>())
        {
            fx.Stop();
            fx.Play();
        }
        StartCoroutine(doFly());
    }

    private IEnumerator doFly()
    {
        while ( (transform.position.x - startPos.x) * (transform.position.x - startPos.x) + (transform.position.z - startPos.z) * (transform.position.z - startPos.z) < 100)
        {
            transform.Translate(0, 0, 5.0f * Time.deltaTime);
            yield return null;
        }
        Destroy(gameObject);
    }
}
