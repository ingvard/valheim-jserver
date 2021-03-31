using System;
using UnityEngine;

// Token: 0x020000E4 RID: 228
public class Odin : MonoBehaviour
{
	// Token: 0x06000E34 RID: 3636 RVA: 0x0006574C File Offset: 0x0006394C
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
	}

	// Token: 0x06000E35 RID: 3637 RVA: 0x0006575C File Offset: 0x0006395C
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

	// Token: 0x04000CEE RID: 3310
	public float m_despawnCloseDistance = 20f;

	// Token: 0x04000CEF RID: 3311
	public float m_despawnFarDistance = 50f;

	// Token: 0x04000CF0 RID: 3312
	public EffectList m_despawn = new EffectList();

	// Token: 0x04000CF1 RID: 3313
	public float m_ttl = 300f;

	// Token: 0x04000CF2 RID: 3314
	private float m_time;

	// Token: 0x04000CF3 RID: 3315
	private ZNetView m_nview;
}
