using System;
using UnityEngine;

// Token: 0x020000FE RID: 254
public class TeleportHome : MonoBehaviour
{
	// Token: 0x06000F70 RID: 3952 RVA: 0x0006D7EC File Offset: 0x0006B9EC
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
		Game.instance.RequestRespawn(0f);
	}
}
