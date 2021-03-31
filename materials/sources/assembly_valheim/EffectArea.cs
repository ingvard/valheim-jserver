using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000C7 RID: 199
public class EffectArea : MonoBehaviour
{
	// Token: 0x06000CF5 RID: 3317 RVA: 0x0005CBEF File Offset: 0x0005ADEF
	private void Awake()
	{
		if (EffectArea.m_characterMask == 0)
		{
			EffectArea.m_characterMask = LayerMask.GetMask(new string[]
			{
				"character_trigger"
			});
		}
		this.m_collider = base.GetComponent<Collider>();
		EffectArea.m_allAreas.Add(this);
	}

	// Token: 0x06000CF6 RID: 3318 RVA: 0x0005CC27 File Offset: 0x0005AE27
	private void OnDestroy()
	{
		EffectArea.m_allAreas.Remove(this);
	}

	// Token: 0x06000CF7 RID: 3319 RVA: 0x0005CC38 File Offset: 0x0005AE38
	private void OnTriggerStay(Collider collider)
	{
		if (ZNet.instance == null)
		{
			return;
		}
		Character component = collider.GetComponent<Character>();
		if (component && component.IsOwner())
		{
			if (!string.IsNullOrEmpty(this.m_statusEffect))
			{
				component.GetSEMan().AddStatusEffect(this.m_statusEffect, true);
			}
			if ((this.m_type & EffectArea.Type.Heat) != (EffectArea.Type)0)
			{
				component.OnNearFire(base.transform.position);
			}
		}
	}

	// Token: 0x06000CF8 RID: 3320 RVA: 0x0005CCA8 File Offset: 0x0005AEA8
	public float GetRadius()
	{
		SphereCollider sphereCollider = this.m_collider as SphereCollider;
		if (sphereCollider != null)
		{
			return sphereCollider.radius;
		}
		return this.m_collider.bounds.size.magnitude;
	}

	// Token: 0x06000CF9 RID: 3321 RVA: 0x0005CCEC File Offset: 0x0005AEEC
	public static EffectArea IsPointInsideArea(Vector3 p, EffectArea.Type type, float radius = 0f)
	{
		int num = Physics.OverlapSphereNonAlloc(p, radius, EffectArea.m_tempColliders, EffectArea.m_characterMask);
		for (int i = 0; i < num; i++)
		{
			EffectArea component = EffectArea.m_tempColliders[i].GetComponent<EffectArea>();
			if (component && (component.m_type & type) != (EffectArea.Type)0)
			{
				return component;
			}
		}
		return null;
	}

	// Token: 0x06000CFA RID: 3322 RVA: 0x0005CD3C File Offset: 0x0005AF3C
	public static int GetBaseValue(Vector3 p, float radius)
	{
		int num = 0;
		int num2 = Physics.OverlapSphereNonAlloc(p, radius, EffectArea.m_tempColliders, EffectArea.m_characterMask);
		for (int i = 0; i < num2; i++)
		{
			EffectArea component = EffectArea.m_tempColliders[i].GetComponent<EffectArea>();
			if (component && (component.m_type & EffectArea.Type.PlayerBase) != (EffectArea.Type)0)
			{
				num++;
			}
		}
		return num;
	}

	// Token: 0x06000CFB RID: 3323 RVA: 0x0005CD8D File Offset: 0x0005AF8D
	public static List<EffectArea> GetAllAreas()
	{
		return EffectArea.m_allAreas;
	}

	// Token: 0x04000BD7 RID: 3031
	[BitMask(typeof(EffectArea.Type))]
	public EffectArea.Type m_type = EffectArea.Type.None;

	// Token: 0x04000BD8 RID: 3032
	public string m_statusEffect = "";

	// Token: 0x04000BD9 RID: 3033
	private Collider m_collider;

	// Token: 0x04000BDA RID: 3034
	private static int m_characterMask = 0;

	// Token: 0x04000BDB RID: 3035
	private static List<EffectArea> m_allAreas = new List<EffectArea>();

	// Token: 0x04000BDC RID: 3036
	private static Collider[] m_tempColliders = new Collider[128];

	// Token: 0x02000195 RID: 405
	public enum Type
	{
		// Token: 0x04001285 RID: 4741
		Heat = 1,
		// Token: 0x04001286 RID: 4742
		Fire,
		// Token: 0x04001287 RID: 4743
		PlayerBase = 4,
		// Token: 0x04001288 RID: 4744
		Burning = 8,
		// Token: 0x04001289 RID: 4745
		Teleport = 16,
		// Token: 0x0400128A RID: 4746
		NoMonsters = 32,
		// Token: 0x0400128B RID: 4747
		None = 999
	}
}
