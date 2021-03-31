using System;
using UnityEngine;

// Token: 0x020000EF RID: 239
public class RopeAttachment : MonoBehaviour, Interactable, Hoverable
{
	// Token: 0x06000EC0 RID: 3776 RVA: 0x00069868 File Offset: 0x00067A68
	private void Awake()
	{
		this.m_boatBody = base.GetComponentInParent<Rigidbody>();
	}

	// Token: 0x06000EC1 RID: 3777 RVA: 0x00069876 File Offset: 0x00067A76
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

	// Token: 0x06000EC2 RID: 3778 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000EC3 RID: 3779 RVA: 0x000698AF File Offset: 0x00067AAF
	public string GetHoverText()
	{
		return this.m_hoverText;
	}

	// Token: 0x06000EC4 RID: 3780 RVA: 0x000698B7 File Offset: 0x00067AB7
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000EC5 RID: 3781 RVA: 0x000698C0 File Offset: 0x00067AC0
	private void FixedUpdate()
	{
		if (this.m_puller && Vector3.Distance(this.m_puller.transform.position, base.transform.position) > this.m_pullDistance)
		{
			Vector3 position = ((this.m_puller.transform.position - base.transform.position).normalized * this.m_maxPullVel - this.m_boatBody.GetPointVelocity(base.transform.position)) * this.m_pullForce;
			this.m_boatBody.AddForceAtPosition(base.transform.position, position);
		}
	}

	// Token: 0x04000DB3 RID: 3507
	public string m_name = "Rope";

	// Token: 0x04000DB4 RID: 3508
	public string m_hoverText = "Pull";

	// Token: 0x04000DB5 RID: 3509
	public float m_pullDistance = 5f;

	// Token: 0x04000DB6 RID: 3510
	public float m_pullForce = 1f;

	// Token: 0x04000DB7 RID: 3511
	public float m_maxPullVel = 1f;

	// Token: 0x04000DB8 RID: 3512
	private Rigidbody m_boatBody;

	// Token: 0x04000DB9 RID: 3513
	private Character m_puller;
}
