using System;
using UnityEngine;

// Token: 0x020000BA RID: 186
public class Beehive : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06000C6E RID: 3182 RVA: 0x00059018 File Offset: 0x00057218
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		if (this.m_nview.IsOwner() && this.m_nview.GetZDO().GetLong("lastTime", 0L) == 0L)
		{
			this.m_nview.GetZDO().Set("lastTime", ZNet.instance.GetTime().Ticks);
		}
		this.m_nview.Register("Extract", new Action<long>(this.RPC_Extract));
		base.InvokeRepeating("UpdateBees", 0f, 10f);
	}

	// Token: 0x06000C6F RID: 3183 RVA: 0x000590C0 File Offset: 0x000572C0
	public string GetHoverText()
	{
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, false, false))
		{
			return Localization.instance.Localize(this.m_name + "\n$piece_noaccess");
		}
		int honeyLevel = this.GetHoneyLevel();
		if (honeyLevel > 0)
		{
			return Localization.instance.Localize(string.Concat(new object[]
			{
				this.m_name,
				" ( ",
				this.m_honeyItem.m_itemData.m_shared.m_name,
				" x ",
				honeyLevel,
				" )\n[<color=yellow><b>$KEY_Use</b></color>] $piece_beehive_extract"
			}));
		}
		return Localization.instance.Localize(this.m_name + " ( $piece_container_empty )\n[<color=yellow><b>$KEY_Use</b></color>] $piece_beehive_check");
	}

	// Token: 0x06000C70 RID: 3184 RVA: 0x0005917E File Offset: 0x0005737E
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000C71 RID: 3185 RVA: 0x00059188 File Offset: 0x00057388
	public bool Interact(Humanoid character, bool repeat)
	{
		if (repeat)
		{
			return false;
		}
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, true, false))
		{
			return true;
		}
		if (this.GetHoneyLevel() > 0)
		{
			this.Extract();
		}
		else
		{
			if (!this.CheckBiome())
			{
				character.Message(MessageHud.MessageType.Center, "$piece_beehive_area", 0, null);
				return true;
			}
			if (!this.HaveFreeSpace())
			{
				character.Message(MessageHud.MessageType.Center, "$piece_beehive_freespace", 0, null);
				return true;
			}
			if (!EnvMan.instance.IsDaylight())
			{
				character.Message(MessageHud.MessageType.Center, "$piece_beehive_sleep", 0, null);
				return true;
			}
			character.Message(MessageHud.MessageType.Center, "$piece_beehive_happy", 0, null);
		}
		return true;
	}

	// Token: 0x06000C72 RID: 3186 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000C73 RID: 3187 RVA: 0x00059221 File Offset: 0x00057421
	private void Extract()
	{
		this.m_nview.InvokeRPC("Extract", Array.Empty<object>());
	}

	// Token: 0x06000C74 RID: 3188 RVA: 0x00059238 File Offset: 0x00057438
	private void RPC_Extract(long caller)
	{
		int honeyLevel = this.GetHoneyLevel();
		if (honeyLevel > 0)
		{
			this.m_spawnEffect.Create(this.m_spawnPoint.position, Quaternion.identity, null, 1f);
			for (int i = 0; i < honeyLevel; i++)
			{
				Vector2 vector = UnityEngine.Random.insideUnitCircle * 0.5f;
				Vector3 position = this.m_spawnPoint.position + new Vector3(vector.x, 0.25f * (float)i, vector.y);
				UnityEngine.Object.Instantiate<ItemDrop>(this.m_honeyItem, position, Quaternion.identity);
			}
			this.ResetLevel();
		}
	}

	// Token: 0x06000C75 RID: 3189 RVA: 0x000592D4 File Offset: 0x000574D4
	private float GetTimeSinceLastUpdate()
	{
		DateTime d = new DateTime(this.m_nview.GetZDO().GetLong("lastTime", ZNet.instance.GetTime().Ticks));
		DateTime time = ZNet.instance.GetTime();
		TimeSpan timeSpan = time - d;
		this.m_nview.GetZDO().Set("lastTime", time.Ticks);
		double num = timeSpan.TotalSeconds;
		if (num < 0.0)
		{
			num = 0.0;
		}
		return (float)num;
	}

	// Token: 0x06000C76 RID: 3190 RVA: 0x0005935F File Offset: 0x0005755F
	private void ResetLevel()
	{
		this.m_nview.GetZDO().Set("level", 0);
	}

	// Token: 0x06000C77 RID: 3191 RVA: 0x00059378 File Offset: 0x00057578
	private void IncreseLevel(int i)
	{
		int num = this.GetHoneyLevel();
		num += i;
		num = Mathf.Clamp(num, 0, this.m_maxHoney);
		this.m_nview.GetZDO().Set("level", num);
	}

	// Token: 0x06000C78 RID: 3192 RVA: 0x000593B4 File Offset: 0x000575B4
	private int GetHoneyLevel()
	{
		return this.m_nview.GetZDO().GetInt("level", 0);
	}

	// Token: 0x06000C79 RID: 3193 RVA: 0x000593CC File Offset: 0x000575CC
	private void UpdateBees()
	{
		bool flag = this.CheckBiome() && this.HaveFreeSpace();
		bool active = flag && EnvMan.instance.IsDaylight();
		this.m_beeEffect.SetActive(active);
		if (this.m_nview.IsOwner() && flag)
		{
			float timeSinceLastUpdate = this.GetTimeSinceLastUpdate();
			float num = this.m_nview.GetZDO().GetFloat("product", 0f);
			num += timeSinceLastUpdate;
			if (num > this.m_secPerUnit)
			{
				int i = (int)(num / this.m_secPerUnit);
				this.IncreseLevel(i);
				num = 0f;
			}
			this.m_nview.GetZDO().Set("product", num);
		}
	}

	// Token: 0x06000C7A RID: 3194 RVA: 0x00059478 File Offset: 0x00057678
	private bool HaveFreeSpace()
	{
		float num;
		bool flag;
		Cover.GetCoverForPoint(this.m_coverPoint.position, out num, out flag);
		return num < this.m_maxCover;
	}

	// Token: 0x06000C7B RID: 3195 RVA: 0x000594A2 File Offset: 0x000576A2
	private bool CheckBiome()
	{
		return (Heightmap.FindBiome(base.transform.position) & this.m_biome) > Heightmap.Biome.None;
	}

	// Token: 0x04000B57 RID: 2903
	public string m_name = "";

	// Token: 0x04000B58 RID: 2904
	public Transform m_coverPoint;

	// Token: 0x04000B59 RID: 2905
	public Transform m_spawnPoint;

	// Token: 0x04000B5A RID: 2906
	public GameObject m_beeEffect;

	// Token: 0x04000B5B RID: 2907
	public float m_maxCover = 0.25f;

	// Token: 0x04000B5C RID: 2908
	[BitMask(typeof(Heightmap.Biome))]
	public Heightmap.Biome m_biome;

	// Token: 0x04000B5D RID: 2909
	public float m_secPerUnit = 10f;

	// Token: 0x04000B5E RID: 2910
	public int m_maxHoney = 4;

	// Token: 0x04000B5F RID: 2911
	public ItemDrop m_honeyItem;

	// Token: 0x04000B60 RID: 2912
	public EffectList m_spawnEffect = new EffectList();

	// Token: 0x04000B61 RID: 2913
	private ZNetView m_nview;
}
