using System;
using UnityEngine;

// Token: 0x020000FE RID: 254
public class TeleportHome : MonoBehaviour
{
	// Token: 0x06000F71 RID: 3953 RVA: 0x0006D974 File Offset: 0x0006BB74
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
