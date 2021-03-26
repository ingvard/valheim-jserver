using System;
using UnityEngine;

// Token: 0x0200002D RID: 45
public class SE_Spawn : StatusEffect
{
	// Token: 0x060003C6 RID: 966 RVA: 0x0001FD54 File Offset: 0x0001DF54
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
		if (this.m_spawned)
		{
			return;
		}
		if (this.m_time > this.m_delay)
		{
			this.m_spawned = true;
			Vector3 position = this.m_character.transform.TransformVector(this.m_spawnOffset);
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_prefab, position, Quaternion.identity);
			Projectile component = gameObject.GetComponent<Projectile>();
			if (component)
			{
				component.Setup(this.m_character, Vector3.zero, -1f, null, null);
			}
			this.m_spawnEffect.Create(gameObject.transform.position, gameObject.transform.rotation, null, 1f);
		}
	}

	// Token: 0x040003B6 RID: 950
	[Header("__SE_Spawn__")]
	public float m_delay = 10f;

	// Token: 0x040003B7 RID: 951
	public GameObject m_prefab;

	// Token: 0x040003B8 RID: 952
	public Vector3 m_spawnOffset = new Vector3(0f, 0f, 0f);

	// Token: 0x040003B9 RID: 953
	public EffectList m_spawnEffect = new EffectList();

	// Token: 0x040003BA RID: 954
	private bool m_spawned;
}
