using System;
using UnityEngine;

// Token: 0x020000C8 RID: 200
public class EnvZone : MonoBehaviour
{
	// Token: 0x06000CFD RID: 3325 RVA: 0x0005CC4C File Offset: 0x0005AE4C
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
		if (this.m_force)
		{
			EnvMan.instance.SetForceEnvironment(this.m_environment);
		}
		EnvZone.m_triggered = this;
	}

	// Token: 0x06000CFE RID: 3326 RVA: 0x0005CC98 File Offset: 0x0005AE98
	private void OnTriggerExit(Collider collider)
	{
		if (EnvZone.m_triggered != this)
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
		if (this.m_force)
		{
			EnvMan.instance.SetForceEnvironment("");
		}
		EnvZone.m_triggered = null;
	}

	// Token: 0x06000CFF RID: 3327 RVA: 0x0005CCEF File Offset: 0x0005AEEF
	public static string GetEnvironment()
	{
		if (EnvZone.m_triggered && !EnvZone.m_triggered.m_force)
		{
			return EnvZone.m_triggered.m_environment;
		}
		return null;
	}

	// Token: 0x04000BD7 RID: 3031
	public string m_environment = "";

	// Token: 0x04000BD8 RID: 3032
	public bool m_force = true;

	// Token: 0x04000BD9 RID: 3033
	private static EnvZone m_triggered;
}
