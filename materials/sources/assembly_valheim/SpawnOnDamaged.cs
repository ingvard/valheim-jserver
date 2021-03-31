using System;
using UnityEngine;

// Token: 0x020000F9 RID: 249
public class SpawnOnDamaged : MonoBehaviour
{
	// Token: 0x06000F4B RID: 3915 RVA: 0x0006D134 File Offset: 0x0006B334
	private void Start()
	{
		WearNTear component = base.GetComponent<WearNTear>();
		if (component)
		{
			WearNTear wearNTear = component;
			wearNTear.m_onDamaged = (Action)Delegate.Combine(wearNTear.m_onDamaged, new Action(this.OnDamaged));
		}
		Destructible component2 = base.GetComponent<Destructible>();
		if (component2)
		{
			Destructible destructible = component2;
			destructible.m_onDamaged = (Action)Delegate.Combine(destructible.m_onDamaged, new Action(this.OnDamaged));
		}
	}

	// Token: 0x06000F4C RID: 3916 RVA: 0x0006D1A3 File Offset: 0x0006B3A3
	private void OnDamaged()
	{
		if (this.m_spawnOnDamage)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.m_spawnOnDamage, base.transform.position, Quaternion.identity);
		}
	}

	// Token: 0x04000E2D RID: 3629
	public GameObject m_spawnOnDamage;
}
