using System;
using System.Collections;
using UnityEngine;

// Token: 0x02000032 RID: 50
public class CamShaker : MonoBehaviour
{
	// Token: 0x06000400 RID: 1024 RVA: 0x00020D31 File Offset: 0x0001EF31
	private void Start()
	{
		if (this.m_continous)
		{
			base.StartCoroutine("TriggerContinous");
			return;
		}
		if (this.m_delay <= 0f)
		{
			this.Trigger();
			return;
		}
		base.Invoke("Trigger", this.m_delay);
	}

	// Token: 0x06000401 RID: 1025 RVA: 0x00020D6D File Offset: 0x0001EF6D
	private IEnumerator TriggerContinous()
	{
		float t = 0f;
		for (;;)
		{
			this.Trigger();
			t += Time.deltaTime;
			if (this.m_continousDuration > 0f && t > this.m_continousDuration)
			{
				break;
			}
			yield return null;
		}
		yield break;
		yield break;
	}

	// Token: 0x06000402 RID: 1026 RVA: 0x00020D7C File Offset: 0x0001EF7C
	private void Trigger()
	{
		if (GameCamera.instance)
		{
			if (this.m_localOnly)
			{
				ZNetView component = base.GetComponent<ZNetView>();
				if (component && !component.IsOwner())
				{
					return;
				}
			}
			GameCamera.instance.AddShake(base.transform.position, this.m_range, this.m_strength, this.m_continous);
		}
	}

	// Token: 0x040003F3 RID: 1011
	public float m_strength = 1f;

	// Token: 0x040003F4 RID: 1012
	public float m_range = 50f;

	// Token: 0x040003F5 RID: 1013
	public float m_delay;

	// Token: 0x040003F6 RID: 1014
	public bool m_continous;

	// Token: 0x040003F7 RID: 1015
	public float m_continousDuration;

	// Token: 0x040003F8 RID: 1016
	public bool m_localOnly;
}
