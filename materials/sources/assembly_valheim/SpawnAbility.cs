using System;
using System.Collections;
using UnityEngine;

// Token: 0x0200001E RID: 30
public class SpawnAbility : MonoBehaviour, IProjectile
{
	// Token: 0x0600030D RID: 781 RVA: 0x0001A1C0 File Offset: 0x000183C0
	public void Setup(Character owner, Vector3 velocity, float hitNoise, HitData hitData, ItemDrop.ItemData item)
	{
		this.m_owner = owner;
		base.StartCoroutine("Spawn");
	}

	// Token: 0x0600030E RID: 782 RVA: 0x0000AC4C File Offset: 0x00008E4C
	public string GetTooltipString(int itemQuality)
	{
		return "";
	}

	// Token: 0x0600030F RID: 783 RVA: 0x0001A1D5 File Offset: 0x000183D5
	private IEnumerator Spawn()
	{
		int toSpawn = UnityEngine.Random.Range(this.m_minToSpawn, this.m_maxToSpawn);
		int num;
		for (int i = 0; i < toSpawn; i = num)
		{
			Vector3 vector;
			if (this.FindTarget(out vector))
			{
				Vector3 vector2 = this.m_spawnAtTarget ? vector : base.transform.position;
				Vector2 vector3 = UnityEngine.Random.insideUnitCircle * this.m_spawnRadius;
				Vector3 vector4 = vector2 + new Vector3(vector3.x, 0f, vector3.y);
				if (this.m_snapToTerrain)
				{
					float solidHeight = ZoneSystem.instance.GetSolidHeight(vector4);
					vector4.y = solidHeight;
				}
				vector4.y += this.m_spawnGroundOffset;
				if (Mathf.Abs(vector4.y - vector2.y) <= 100f)
				{
					GameObject gameObject = this.m_spawnPrefab[UnityEngine.Random.Range(0, this.m_spawnPrefab.Length)];
					if (this.m_maxSpawned <= 0 || SpawnSystem.GetNrOfInstances(gameObject) < this.m_maxSpawned)
					{
						GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, vector4, Quaternion.Euler(0f, UnityEngine.Random.value * 3.1415927f * 2f, 0f));
						Projectile component = gameObject2.GetComponent<Projectile>();
						if (component)
						{
							this.SetupProjectile(component, vector);
						}
						BaseAI component2 = gameObject2.GetComponent<BaseAI>();
						if (component2 != null && this.m_alertSpawnedCreature)
						{
							component2.Alert();
						}
						this.m_spawnEffects.Create(vector4, Quaternion.identity, null, 1f);
						if (this.m_spawnDelay > 0f)
						{
							yield return new WaitForSeconds(this.m_spawnDelay);
						}
					}
				}
			}
			num = i + 1;
		}
		UnityEngine.Object.Destroy(base.gameObject);
		yield break;
	}

	// Token: 0x06000310 RID: 784 RVA: 0x0001A1E4 File Offset: 0x000183E4
	private void SetupProjectile(Projectile projectile, Vector3 targetPoint)
	{
		Vector3 vector = (targetPoint - projectile.transform.position).normalized;
		Vector3 axis = Vector3.Cross(vector, Vector3.up);
		Quaternion rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(-this.m_projectileAccuracy, this.m_projectileAccuracy), Vector3.up);
		vector = Quaternion.AngleAxis(UnityEngine.Random.Range(-this.m_projectileAccuracy, this.m_projectileAccuracy), axis) * vector;
		vector = rotation * vector;
		projectile.Setup(this.m_owner, vector * this.m_projectileVelocity, -1f, null, null);
	}

	// Token: 0x06000311 RID: 785 RVA: 0x0001A278 File Offset: 0x00018478
	private bool FindTarget(out Vector3 point)
	{
		point = Vector3.zero;
		switch (this.m_targetType)
		{
		case SpawnAbility.TargetType.ClosestEnemy:
		{
			if (this.m_owner == null)
			{
				return false;
			}
			Character character = BaseAI.FindClosestEnemy(this.m_owner, base.transform.position, this.m_maxTargetRange);
			if (character != null)
			{
				point = character.transform.position;
				return true;
			}
			return false;
		}
		case SpawnAbility.TargetType.RandomEnemy:
		{
			if (this.m_owner == null)
			{
				return false;
			}
			Character character2 = BaseAI.FindRandomEnemy(this.m_owner, base.transform.position, this.m_maxTargetRange);
			if (character2 != null)
			{
				point = character2.transform.position;
				return true;
			}
			return false;
		}
		case SpawnAbility.TargetType.Caster:
			if (this.m_owner == null)
			{
				return false;
			}
			point = this.m_owner.transform.position;
			return true;
		case SpawnAbility.TargetType.Position:
			point = base.transform.position;
			return true;
		default:
			return false;
		}
	}

	// Token: 0x040002D7 RID: 727
	[Header("Spawn")]
	public GameObject[] m_spawnPrefab;

	// Token: 0x040002D8 RID: 728
	public bool m_alertSpawnedCreature = true;

	// Token: 0x040002D9 RID: 729
	public bool m_spawnAtTarget = true;

	// Token: 0x040002DA RID: 730
	public int m_minToSpawn = 1;

	// Token: 0x040002DB RID: 731
	public int m_maxToSpawn = 1;

	// Token: 0x040002DC RID: 732
	public int m_maxSpawned = 3;

	// Token: 0x040002DD RID: 733
	public float m_spawnRadius = 3f;

	// Token: 0x040002DE RID: 734
	public bool m_snapToTerrain = true;

	// Token: 0x040002DF RID: 735
	public float m_spawnGroundOffset;

	// Token: 0x040002E0 RID: 736
	public float m_spawnDelay;

	// Token: 0x040002E1 RID: 737
	public SpawnAbility.TargetType m_targetType;

	// Token: 0x040002E2 RID: 738
	public float m_maxTargetRange = 40f;

	// Token: 0x040002E3 RID: 739
	public EffectList m_spawnEffects = new EffectList();

	// Token: 0x040002E4 RID: 740
	[Header("Projectile")]
	public float m_projectileVelocity = 10f;

	// Token: 0x040002E5 RID: 741
	public float m_projectileAccuracy = 10f;

	// Token: 0x040002E6 RID: 742
	private Character m_owner;

	// Token: 0x02000134 RID: 308
	public enum TargetType
	{
		// Token: 0x0400103C RID: 4156
		ClosestEnemy,
		// Token: 0x0400103D RID: 4157
		RandomEnemy,
		// Token: 0x0400103E RID: 4158
		Caster,
		// Token: 0x0400103F RID: 4159
		Position
	}
}
