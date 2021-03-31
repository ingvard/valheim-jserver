using System;
using UnityEngine;

// Token: 0x020000C9 RID: 201
public class EventZone : MonoBehaviour
{
	// Token: 0x06000D03 RID: 3331 RVA: 0x0005CEB8 File Offset: 0x0005B0B8
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

	// Token: 0x06000D04 RID: 3332 RVA: 0x0005CEEC File Offset: 0x0005B0EC
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

	// Token: 0x06000D05 RID: 3333 RVA: 0x0005CF2C File Offset: 0x0005B12C
	public static string GetEvent()
	{
		if (EventZone.m_triggered && EventZone.m_triggered.m_event.Length > 0)
		{
			return EventZone.m_triggered.m_event;
		}
		return null;
	}

	// Token: 0x04000BE0 RID: 3040
	public string m_event = "";

	// Token: 0x04000BE1 RID: 3041
	private static EventZone m_triggered;
}
