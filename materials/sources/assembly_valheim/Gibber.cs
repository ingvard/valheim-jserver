using System;
using UnityEngine;

// Token: 0x0200003A RID: 58
public class Gibber : MonoBehaviour
{
	// Token: 0x0600042A RID: 1066 RVA: 0x00021B5E File Offset: 0x0001FD5E
	private void Start()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (!this.m_done)
		{
			this.Explode(base.transform.position, Vector3.zero);
		}
	}

	// Token: 0x0600042B RID: 1067 RVA: 0x00021B8A File Offset: 0x0001FD8A
	public void Setup(Vector3 hitPoint, Vector3 hitDirection)
	{
		this.Explode(hitPoint, hitDirection);
	}

	// Token: 0x0600042C RID: 1068 RVA: 0x00021B94 File Offset: 0x0001FD94
	private void DestroyAll()
	{
		if (this.m_nview)
		{
			if (this.m_nview.GetZDO().m_owner == 0L)
			{
				this.m_nview.ClaimOwnership();
			}
			if (this.m_nview.IsOwner())
			{
				ZNetScene.instance.Destroy(base.gameObject);
				return;
			}
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x0600042D RID: 1069 RVA: 0x00021BF4 File Offset: 0x0001FDF4
	private void CreateBodies()
	{
		MeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			GameObject gameObject = componentsInChildren[i].gameObject;
			if (!gameObject.GetComponent<Rigidbody>())
			{
				gameObject.AddComponent<BoxCollider>();
				gameObject.AddComponent<Rigidbody>();
			}
		}
	}

	// Token: 0x0600042E RID: 1070 RVA: 0x00021C40 File Offset: 0x0001FE40
	private void Explode(Vector3 hitPoint, Vector3 hitDirection)
	{
		this.m_done = true;
		base.InvokeRepeating("DestroyAll", this.m_timeout, 1f);
		Vector3 position = base.transform.position;
		float t = ((double)hitDirection.magnitude > 0.01) ? this.m_impactDirectionMix : 0f;
		this.CreateBodies();
		foreach (Rigidbody rigidbody in base.gameObject.GetComponentsInChildren<Rigidbody>())
		{
			float d = UnityEngine.Random.Range(this.m_minVel, this.m_maxVel);
			Vector3 a = Vector3.Lerp(Vector3.Normalize(rigidbody.worldCenterOfMass - position), hitDirection, t);
			rigidbody.velocity = a * d;
			rigidbody.angularVelocity = new Vector3(UnityEngine.Random.Range(-this.m_maxRotVel, this.m_maxRotVel), UnityEngine.Random.Range(-this.m_maxRotVel, this.m_maxRotVel), UnityEngine.Random.Range(-this.m_maxRotVel, this.m_maxRotVel));
		}
		foreach (Gibber.GibbData gibbData in this.m_gibbs)
		{
			if (gibbData.m_object && gibbData.m_chanceToSpawn < 1f && UnityEngine.Random.value > gibbData.m_chanceToSpawn)
			{
				UnityEngine.Object.Destroy(gibbData.m_object);
			}
		}
		if ((double)hitDirection.magnitude > 0.01)
		{
			Quaternion rot = Quaternion.LookRotation(hitDirection);
			this.m_punchEffector.Create(hitPoint, rot, null, 1f);
		}
	}

	// Token: 0x0400042C RID: 1068
	public EffectList m_punchEffector = new EffectList();

	// Token: 0x0400042D RID: 1069
	public GameObject m_gibHitEffect;

	// Token: 0x0400042E RID: 1070
	public GameObject m_gibDestroyEffect;

	// Token: 0x0400042F RID: 1071
	public float m_gibHitDestroyChance;

	// Token: 0x04000430 RID: 1072
	public Gibber.GibbData[] m_gibbs = new Gibber.GibbData[0];

	// Token: 0x04000431 RID: 1073
	public float m_minVel = 10f;

	// Token: 0x04000432 RID: 1074
	public float m_maxVel = 20f;

	// Token: 0x04000433 RID: 1075
	public float m_maxRotVel = 20f;

	// Token: 0x04000434 RID: 1076
	public float m_impactDirectionMix = 0.5f;

	// Token: 0x04000435 RID: 1077
	public float m_timeout = 5f;

	// Token: 0x04000436 RID: 1078
	private bool m_done;

	// Token: 0x04000437 RID: 1079
	private ZNetView m_nview;

	// Token: 0x0200013A RID: 314
	[Serializable]
	public class GibbData
	{
		// Token: 0x0400105F RID: 4191
		public GameObject m_object;

		// Token: 0x04001060 RID: 4192
		public float m_chanceToSpawn = 1f;
	}
}
