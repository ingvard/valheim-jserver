using System;
using UnityEngine;

// Token: 0x020000C9 RID: 201
public class EventZone : MonoBehaviour
{
	// Token: 0x06000D02 RID: 3330 RVA: 0x0005CD30 File Offset: 0x0005AF30
	private void OnTriggerStay(Collider collider)
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
		EventZone.m_triggered = this;
	}

	// Token: 0x06000D03 RID: 3331 RVA: 0x0005CD64 File Offset: 0x0005AF64
	private void OnTriggerExit(Collider collider)
	{
		if (EventZone.m_triggered != this)
		{
			return;
		}
		Player component = collider.GetComponent<Player>();
		if (component == null)
		{
			return;
		}
		if (Player.m_localPlayer != component)
		{
			return;
		}
		EventZone.m_triggered = null;
	}

	// Token: 0x06000D04 RID: 3332 RVA: 0x0005CDA4 File Offset: 0x0005AFA4
	public static string GetEvent()
	{
		if (EventZone.m_triggered && EventZone.m_triggered.m_event.Length > 0)
		{
			return EventZone.m_triggered.m_event;
		}
		return null;
	}

	// Token: 0x04000BDA RID: 3034
	public string m_event = "";

	// Token: 0x04000BDB RID: 3035
	private static EventZone m_triggered;
}
