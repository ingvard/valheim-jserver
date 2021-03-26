using System;
using UnityEngine;

// Token: 0x020000EF RID: 239
public class RopeAttachment : MonoBehaviour, Interactable, Hoverable
{
	// Token: 0x06000EBF RID: 3775 RVA: 0x000696E0 File Offset: 0x000678E0
	private void Awake()
	{
		this.m_boatBody = base.GetComponentInParent<Rigidbody>();
	}

	// Token: 0x06000EC0 RID: 3776 RVA: 0x000696EE File Offset: 0x000678EE
	public bool Interact(Humanoid character, bool hold)
	{
		if (hold)
		{
			return false;
		}
		if (this.m_puller)
		{
			this.m_puller = null;
			ZLog.Log("Detached rope");
		}
		else
		{
			this.m_puller = character;
			ZLog.Log("Attached rope");
		}
		return true;
	}

	// Token: 0x06000EC1 RID: 3777 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000EC2 RID: 3778 RVA: 0x00069727 File Offset: 0x00067927
	public string GetHoverText()
	{
		return this.m_hoverText;
	}

	// Token: 0x06000EC3 RID: 3779 RVA: 0x0006972F File Offset: 0x0006792F
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000EC4 RID: 3780 RVA: 0x00069738 File Offset: 0x00067938
	private void FixedUpdate()
	{
		if (this.m_puller && Vector3.Distance(this.m_puller.transform.position, base.transform.position) > this.m_pullDistance)
		{
			Vector3 position = ((this.m_puller.transform.position - base.transform.position).normalized * this.m_maxPullVel - this.m_boatBody.GetPointVelocity(base.transform.position)) * this.m_pullForce;
			this.m_boatBody.AddForceAtPosition(base.transform.position, position);
		}
	}

	// Token: 0x04000DAD RID: 3501
	public string m_name = "Rope";

	// Token: 0x04000DAE RID: 3502
	public string m_hoverText = "Pull";

	// Token: 0x04000DAF RID: 3503
	public float m_pullDistance = 5f;

	// Token: 0x04000DB0 RID: 3504
	public float m_pullForce = 1f;

	// Token: 0x04000DB1 RID: 3505
	public float m_maxPullVel = 1f;

	// Token: 0x04000DB2 RID: 3506
	private Rigidbody m_boatBody;

	// Token: 0x04000DB3 RID: 3507
	private Character m_puller;
}
