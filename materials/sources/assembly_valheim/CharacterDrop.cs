using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000004 RID: 4
[RequireComponent(typeof(Character))]
public class CharacterDrop : MonoBehaviour
{
	// Token: 0x060000DD RID: 221 RVA: 0x00006B84 File Offset: 0x00004D84
	private void Start()
	{
		this.m_character = base.GetComponent<Character>();
		if (this.m_character)
		{
			Character character = this.m_character;
			character.m_onDeath = (Action)Delegate.Combine(character.m_onDeath, new Action(this.OnDeath));
		}
	}

	// Token: 0x060000DE RID: 222 RVA: 0x00006BD1 File Offset: 0x00004DD1
	public void SetDropsEnabled(bool enabled)
	{
		this.m_dropsEnabled = enabled;
	}

	// Token: 0x060000DF RID: 223 RVA: 0x00006BDC File Offset: 0x00004DDC
	private void OnDeath()
	{
		if (!this.m_dropsEnabled)
		{
			return;
		}
		List<KeyValuePair<GameObject, int>> drops = this.GenerateDropList();
		Vector3 centerPos = this.m_character.GetCenterPoint() + base.transform.TransformVector(this.m_spawnOffset);
		CharacterDrop.DropItems(drops, centerPos, 0.5f);
	}

	// Token: 0x060000E0 RID: 224 RVA: 0x00006C28 File Offset: 0x00004E28
	public List<KeyValuePair<GameObject, int>> GenerateDropList()
	{
		List<KeyValuePair<GameObject, int>> list = new List<KeyValuePair<GameObject, int>>();
		int num = this.m_character ? Mathf.Max(1, (int)Mathf.Pow(2f, (float)(this.m_character.GetLevel() - 1))) : 1;
		foreach (CharacterDrop.Drop drop in this.m_drops)
		{
			if (!(drop.m_prefab == null))
			{
				float num2 = drop.m_chance;
				if (drop.m_levelMultiplier)
				{
					num2 *= (float)num;
				}
				if (UnityEngine.Random.value <= num2)
				{
					int num3 = UnityEngine.Random.Range(drop.m_amountMin, drop.m_amountMax);
					if (drop.m_levelMultiplier)
					{
						num3 *= num;
					}
					if (drop.m_onePerPlayer)
					{
						num3 = ZNet.instance.GetNrOfPlayers();
					}
					if (num3 > 0)
					{
						list.Add(new KeyValuePair<GameObject, int>(drop.m_prefab, num3));
					}
				}
			}
		}
		return list;
	}

	// Token: 0x060000E1 RID: 225 RVA: 0x00006D2C File Offset: 0x00004F2C
	public static void DropItems(List<KeyValuePair<GameObject, int>> drops, Vector3 centerPos, float dropArea)
	{
		foreach (KeyValuePair<GameObject, int> keyValuePair in drops)
		{
			for (int i = 0; i < keyValuePair.Value; i++)
			{
				Quaternion rotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
				Vector3 b = UnityEngine.Random.insideUnitSphere * dropArea;
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(keyValuePair.Key, centerPos + b, rotation);
				Rigidbody component = gameObject.GetComponent<Rigidbody>();
				if (component)
				{
					Vector3 insideUnitSphere = UnityEngine.Random.insideUnitSphere;
					if (insideUnitSphere.y < 0f)
					{
						insideUnitSphere.y = -insideUnitSphere.y;
					}
					component.AddForce(insideUnitSphere * 5f, ForceMode.VelocityChange);
				}
			}
		}
	}

	// Token: 0x040000B0 RID: 176
	public Vector3 m_spawnOffset = Vector3.zero;

	// Token: 0x040000B1 RID: 177
	public List<CharacterDrop.Drop> m_drops = new List<CharacterDrop.Drop>();

	// Token: 0x040000B2 RID: 178
	private const float m_dropArea = 0.5f;

	// Token: 0x040000B3 RID: 179
	private const float m_vel = 5f;

	// Token: 0x040000B4 RID: 180
	private bool m_dropsEnabled = true;

	// Token: 0x040000B5 RID: 181
	private Character m_character;

	// Token: 0x0200011E RID: 286
	[Serializable]
	public class Drop
	{
		// Token: 0x04000FBC RID: 4028
		public GameObject m_prefab;

		// Token: 0x04000FBD RID: 4029
		public int m_amountMin = 1;

		// Token: 0x04000FBE RID: 4030
		public int m_amountMax = 1;

		// Token: 0x04000FBF RID: 4031
		public float m_chance = 1f;

		// Token: 0x04000FC0 RID: 4032
		public bool m_onePerPlayer;

		// Token: 0x04000FC1 RID: 4033
		public bool m_levelMultiplier = true;
	}
}
