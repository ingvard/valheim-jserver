using System;
using UnityEngine;

// Token: 0x02000100 RID: 256
public class TeleportWorldTrigger : MonoBehaviour
{
	// Token: 0x06000F81 RID: 3969 RVA: 0x0006DDBD File Offset: 0x0006BFBD
	private void Awake()
	{
		this.m_tp = base.GetComponentInParent<TeleportWorld>();
	}

	// Token: 0x06000F82 RID: 3970 RVA: 0x0006DDCC File Offset: 0x0006BFCC
	private void OnTriggerEnter(Collider collider)
	{
		Player component = collider.GetComponent<Player>();
		if (component == null)
		{
			return;
		}
		if (Player.m_localPlayer != component)
		{
			return;
		}
		ZLog.Log("TRIGGER");
		this.m_tp.Teleport(component);
	}

	// Token: 0x04000E4D RID: 3661
	private TeleportWorld m_tp;
}
