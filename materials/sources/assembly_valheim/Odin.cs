using System;
using UnityEngine;

// Token: 0x020000E4 RID: 228
public class Odin : MonoBehaviour
{
	// Token: 0x06000E33 RID: 3635 RVA: 0x000655C4 File Offset: 0x000637C4
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
	}

	// Token: 0x06000E34 RID: 3636 RVA: 0x000655D4 File Offset: 0x000637D4
	private void Update()
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		Player closestPlayer = Player.GetClosestPlayer(base.transform.position, this.m_despawnFarDistance);
		if (closestPlayer == null)
		{
			this.m_despawn.Create(base.transform.position, base.transform.rotation, null, 1f);
			this.m_nview.Destroy();
			ZLog.Log("No player in range, despawning");
			return;
		}
		Vector3 forward = closestPlayer.transform.position - base.transform.position;
		forward.y = 0f;
		forward.Normalize();
		base.transform.rotation = Quaternion.LookRotation(forward);
		if (Vector3.Distance(closestPlayer.transform.position, base.transform.position) < this.m_despawnCloseDistance)
		{
			this.m_despawn.Create(base.transform.position, base.transform.rotation, null, 1f);
			this.m_nview.Destroy();
			ZLog.Log("Player go too close,despawning");
			return;
		}
		this.m_time += Time.deltaTime;
		if (this.m_time > this.m_ttl)
		{
			this.m_despawn.Create(base.transform.position, base.transform.rotation, null, 1f);
			this.m_nview.Destroy();
			ZLog.Log("timeout " + this.m_time + " , despawning");
			return;
		}
	}

	// Token: 0x04000CE8 RID: 3304
	public float m_despawnCloseDistance = 20f;

	// Token: 0x04000CE9 RID: 3305
	public float m_despawnFarDistance = 50f;

	// Token: 0x04000CEA RID: 3306
	public EffectList m_despawn = new EffectList();

	// Token: 0x04000CEB RID: 3307
	public float m_ttl = 300f;

	// Token: 0x04000CEC RID: 3308
	private float m_time;

	// Token: 0x04000CED RID: 3309
	private ZNetView m_nview;
}
