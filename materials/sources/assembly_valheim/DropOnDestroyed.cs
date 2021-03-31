using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000C6 RID: 198
public class DropOnDestroyed : MonoBehaviour
{
	// Token: 0x06000CF2 RID: 3314 RVA: 0x0005CA74 File Offset: 0x0005AC74
	private void Awake()
	{
		IDestructible component = base.GetComponent<IDestructible>();
		Destructible destructible = component as Destructible;
		if (destructible)
		{
			Destructible destructible2 = destructible;
			destructible2.m_onDestroyed = (Action)Delegate.Combine(destructible2.m_onDestroyed, new Action(this.OnDestroyed));
		}
		WearNTear wearNTear = component as WearNTear;
		if (wearNTear)
		{
			WearNTear wearNTear2 = wearNTear;
			wearNTear2.m_onDestroyed = (Action)Delegate.Combine(wearNTear2.m_onDestroyed, new Action(this.OnDestroyed));
		}
	}

	// Token: 0x06000CF3 RID: 3315 RVA: 0x0005CAE8 File Offset: 0x0005ACE8
	private void OnDestroyed()
	{
		float groundHeight = ZoneSystem.instance.GetGroundHeight(base.transform.position);
		Vector3 position = base.transform.position;
		if (position.y < groundHeight)
		{
			position.y = groundHeight + 0.1f;
		}
		List<GameObject> dropList = this.m_dropWhenDestroyed.GetDropList();
		for (int i = 0; i < dropList.Count; i++)
		{
			Vector2 vector = UnityEngine.Random.insideUnitCircle * 0.5f;
			Vector3 position2 = position + Vector3.up * this.m_spawnYOffset + new Vector3(vector.x, this.m_spawnYStep * (float)i, vector.y);
			Quaternion rotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
			UnityEngine.Object.Instantiate<GameObject>(dropList[i], position2, rotation);
		}
	}

	// Token: 0x04000BD4 RID: 3028
	[Header("Drops")]
	public DropTable m_dropWhenDestroyed = new DropTable();

	// Token: 0x04000BD5 RID: 3029
	public float m_spawnYOffset = 0.5f;

	// Token: 0x04000BD6 RID: 3030
	public float m_spawnYStep = 0.3f;
}
