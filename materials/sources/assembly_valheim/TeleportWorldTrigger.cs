using System;
using UnityEngine;

// Token: 0x02000100 RID: 256
public class TeleportWorldTrigger : MonoBehaviour
{
	// Token: 0x06000F80 RID: 3968 RVA: 0x0006DC35 File Offset: 0x0006BE35
	private void Awake()
	{
		this.m_tp = base.GetComponentInParent<TeleportWorld>();
	}

	// Token: 0x06000F81 RID: 3969 RVA: 0x0006DC44 File Offset: 0x0006BE44
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

	// Token: 0x04000E47 RID: 3655
	private TeleportWorld m_tp;
}
