using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000C7 RID: 199
public class EffectArea : MonoBehaviour
{
	// Token: 0x06000CF4 RID: 3316 RVA: 0x0005CA67 File Offset: 0x0005AC67
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

	// Token: 0x06000CF5 RID: 3317 RVA: 0x0005CA9F File Offset: 0x0005AC9F
	private void OnDestroy()
	{
		EffectArea.m_allAreas.Remove(this);
	}

	// Token: 0x06000CF6 RID: 3318 RVA: 0x0005CAB0 File Offset: 0x0005ACB0
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

	// Token: 0x06000CF7 RID: 3319 RVA: 0x0005CB20 File Offset: 0x0005AD20
	public float GetRadius()
	{
		SphereCollider sphereCollider = this.m_collider as SphereCollider;
		if (sphereCollider != null)
		{
			return sphereCollider.radius;
		}
		return this.m_collider.bounds.size.magnitude;
	}

	// Token: 0x06000CF8 RID: 3320 RVA: 0x0005CB64 File Offset: 0x0005AD64
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

	// Token: 0x06000CF9 RID: 3321 RVA: 0x0005CBB4 File Offset: 0x0005ADB4
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

	// Token: 0x06000CFA RID: 3322 RVA: 0x0005CC05 File Offset: 0x0005AE05
	public static List<EffectArea> GetAllAreas()
	{
		return EffectArea.m_allAreas;
	}

	// Token: 0x04000BD1 RID: 3025
	[BitMask(typeof(EffectArea.Type))]
	public EffectArea.Type m_type = EffectArea.Type.None;

	// Token: 0x04000BD2 RID: 3026
	public string m_statusEffect = "";

	// Token: 0x04000BD3 RID: 3027
	private Collider m_collider;

	// Token: 0x04000BD4 RID: 3028
	private static int m_characterMask = 0;

	// Token: 0x04000BD5 RID: 3029
	private static List<EffectArea> m_allAreas = new List<EffectArea>();

	// Token: 0x04000BD6 RID: 3030
	private static Collider[] m_tempColliders = new Collider[128];

	// Token: 0x02000195 RID: 405
	public enum Type
	{
		// Token: 0x0400127E RID: 4734
		Heat = 1,
		// Token: 0x0400127F RID: 4735
		Fire,
		// Token: 0x04001280 RID: 4736
		PlayerBase = 4,
		// Token: 0x04001281 RID: 4737
		Burning = 8,
		// Token: 0x04001282 RID: 4738
		Teleport = 16,
		// Token: 0x04001283 RID: 4739
		NoMonsters = 32,
		// Token: 0x04001284 RID: 4740
		None = 999
	}
}
