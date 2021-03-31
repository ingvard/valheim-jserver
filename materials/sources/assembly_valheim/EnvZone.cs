using System;
using UnityEngine;

// Token: 0x020000C8 RID: 200
public class EnvZone : MonoBehaviour
{
	// Token: 0x06000CFE RID: 3326 RVA: 0x0005CDD4 File Offset: 0x0005AFD4
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

	// Token: 0x06000CFF RID: 3327 RVA: 0x0005CE20 File Offset: 0x0005B020
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

	// Token: 0x06000D00 RID: 3328 RVA: 0x0005CE77 File Offset: 0x0005B077
	public static string GetEnvironment()
	{
		if (EnvZone.m_triggered && !EnvZone.m_triggered.m_force)
		{
			return EnvZone.m_triggered.m_environment;
		}
		return null;
	}

	// Token: 0x04000BDD RID: 3037
	public string m_environment = "";

	// Token: 0x04000BDE RID: 3038
	public bool m_force = true;

	// Token: 0x04000BDF RID: 3039
	private static EnvZone m_triggered;
}
