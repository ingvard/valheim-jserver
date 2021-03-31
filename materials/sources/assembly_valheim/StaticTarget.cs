using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000FA RID: 250
public class StaticTarget : MonoBehaviour
{
	// Token: 0x06000F4E RID: 3918 RVA: 0x000027E2 File Offset: 0x000009E2
	public virtual bool IsValidMonsterTarget()
	{
		return true;
	}

	// Token: 0x06000F4F RID: 3919 RVA: 0x0006D1D0 File Offset: 0x0006B3D0
	public Vector3 GetCenter()
	{
		if (!this.m_haveCenter)
		{
			List<Collider> allColliders = this.GetAllColliders();
			this.m_center = Vector3.zero;
			foreach (Collider collider in allColliders)
			{
				if (collider)
				{
					this.m_center += collider.bounds.center;
				}
			}
			this.m_center /= (float)this.m_colliders.Count;
		}
		return this.m_center;
	}

	// Token: 0x06000F50 RID: 3920 RVA: 0x0006D27C File Offset: 0x0006B47C
	public List<Collider> GetAllColliders()
	{
		if (this.m_colliders == null)
		{
			Collider[] componentsInChildren = base.GetComponentsInChildren<Collider>();
			this.m_colliders = new List<Collider>();
			this.m_colliders.Capacity = componentsInChildren.Length;
			foreach (Collider collider in componentsInChildren)
			{
				if (collider.enabled && collider.gameObject.activeInHierarchy && !collider.isTrigger)
				{
					this.m_colliders.Add(collider);
				}
			}
		}
		return this.m_colliders;
	}

	// Token: 0x06000F51 RID: 3921 RVA: 0x0006D2F4 File Offset: 0x0006B4F4
	public Vector3 FindClosestPoint(Vector3 point)
	{
		List<Collider> allColliders = this.GetAllColliders();
		if (allColliders.Count == 0)
		{
			return base.transform.position;
		}
		float num = 9999999f;
		Vector3 result = Vector3.zero;
		foreach (Collider collider in allColliders)
		{
			MeshCollider meshCollider = collider as MeshCollider;
			Vector3 vector = (meshCollider && !meshCollider.convex) ? collider.ClosestPointOnBounds(point) : collider.ClosestPoint(point);
			float num2 = Vector3.Distance(point, vector);
			if (num2 < num)
			{
				result = vector;
				num = num2;
			}
		}
		return result;
	}

	// Token: 0x04000E2E RID: 3630
	public bool m_primaryTarget;

	// Token: 0x04000E2F RID: 3631
	public bool m_randomTarget = true;

	// Token: 0x04000E30 RID: 3632
	private List<Collider> m_colliders;

	// Token: 0x04000E31 RID: 3633
	private Vector3 m_center;

	// Token: 0x04000E32 RID: 3634
	private bool m_haveCenter;
}
