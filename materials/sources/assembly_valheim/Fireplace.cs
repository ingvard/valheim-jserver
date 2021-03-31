using System;
using UnityEngine;

// Token: 0x020000CB RID: 203
public class Fireplace : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06000D1D RID: 3357 RVA: 0x0005D7F0 File Offset: 0x0005B9F0
	public void Awake()
	{
		this.m_nview = base.gameObject.GetComponent<ZNetView>();
		this.m_piece = base.gameObject.GetComponent<Piece>();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		if (Fireplace.m_solidRayMask == 0)
		{
			Fireplace.m_solidRayMask = LayerMask.GetMask(new string[]
			{
				"Default",
				"static_solid",
				"Default_small",
				"piece",
				"terrain"
			});
		}
		if (this.m_nview.IsOwner() && this.m_nview.GetZDO().GetFloat("fuel", -1f) == -1f)
		{
			this.m_nview.GetZDO().Set("fuel", this.m_startFuel);
			if (this.m_startFuel > 0f)
			{
				this.m_fuelAddedEffects.Create(base.transform.position, base.transform.rotation, null, 1f);
			}
		}
		this.m_nview.Register("AddFuel", new Action<long>(this.RPC_AddFuel));
		base.InvokeRepeating("UpdateFireplace", 0f, 2f);
		base.InvokeRepeating("CheckEnv", 4f, 4f);
	}

	// Token: 0x06000D1E RID: 3358 RVA: 0x0005D930 File Offset: 0x0005BB30
	private void Start()
	{
		if (this.m_playerBaseObject && this.m_piece)
		{
			this.m_playerBaseObject.SetActive(this.m_piece.IsPlacedByPlayer());
		}
	}

	// Token: 0x06000D1F RID: 3359 RVA: 0x0005D964 File Offset: 0x0005BB64
	private double GetTimeSinceLastUpdate()
	{
		DateTime time = ZNet.instance.GetTime();
		DateTime d = new DateTime(this.m_nview.GetZDO().GetLong("lastTime", time.Ticks));
		TimeSpan timeSpan = time - d;
		this.m_nview.GetZDO().Set("lastTime", time.Ticks);
		double num = timeSpan.TotalSeconds;
		if (num < 0.0)
		{
			num = 0.0;
		}
		return num;
	}

	// Token: 0x06000D20 RID: 3360 RVA: 0x0005D9E4 File Offset: 0x0005BBE4
	private void UpdateFireplace()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_nview.IsOwner())
		{
			float num = this.m_nview.GetZDO().GetFloat("fuel", 0f);
			double timeSinceLastUpdate = this.GetTimeSinceLastUpdate();
			if (this.IsBurning())
			{
				float num2 = (float)(timeSinceLastUpdate / (double)this.m_secPerFuel);
				num -= num2;
				if (num <= 0f)
				{
					num = 0f;
				}
				this.m_nview.GetZDO().Set("fuel", num);
			}
		}
		this.UpdateState();
	}

	// Token: 0x06000D21 RID: 3361 RVA: 0x0005DA6F File Offset: 0x0005BC6F
	private void CheckEnv()
	{
		this.CheckUnderTerrain();
		if (this.m_enabledObjectLow != null && this.m_enabledObjectHigh != null)
		{
			this.CheckWet();
		}
	}

	// Token: 0x06000D22 RID: 3362 RVA: 0x0005DA9C File Offset: 0x0005BC9C
	private void CheckUnderTerrain()
	{
		this.m_blocked = false;
		float num;
		if (Heightmap.GetHeight(base.transform.position, out num) && num > base.transform.position.y + this.m_checkTerrainOffset)
		{
			this.m_blocked = true;
			return;
		}
		RaycastHit raycastHit;
		if (Physics.Raycast(base.transform.position + Vector3.up * this.m_coverCheckOffset, Vector3.up, out raycastHit, 0.5f, Fireplace.m_solidRayMask))
		{
			this.m_blocked = true;
			return;
		}
		if (this.m_smokeSpawner && this.m_smokeSpawner.IsBlocked())
		{
			this.m_blocked = true;
			return;
		}
	}

	// Token: 0x06000D23 RID: 3363 RVA: 0x0005DB48 File Offset: 0x0005BD48
	private void CheckWet()
	{
		float num;
		bool flag;
		Cover.GetCoverForPoint(base.transform.position + Vector3.up * this.m_coverCheckOffset, out num, out flag);
		this.m_wet = false;
		if (EnvMan.instance.GetWindIntensity() >= 0.8f && num < 0.7f)
		{
			this.m_wet = true;
		}
		if (EnvMan.instance.IsWet() && !flag)
		{
			this.m_wet = true;
		}
	}

	// Token: 0x06000D24 RID: 3364 RVA: 0x0005DBBC File Offset: 0x0005BDBC
	private void UpdateState()
	{
		if (this.IsBurning())
		{
			this.m_enabledObject.SetActive(true);
			if (this.m_enabledObjectHigh && this.m_enabledObjectLow)
			{
				this.m_enabledObjectHigh.SetActive(!this.m_wet);
				this.m_enabledObjectLow.SetActive(this.m_wet);
				return;
			}
		}
		else
		{
			this.m_enabledObject.SetActive(false);
			if (this.m_enabledObjectHigh && this.m_enabledObjectLow)
			{
				this.m_enabledObjectLow.SetActive(false);
				this.m_enabledObjectHigh.SetActive(false);
			}
		}
	}

	// Token: 0x06000D25 RID: 3365 RVA: 0x0005DC5C File Offset: 0x0005BE5C
	public string GetHoverText()
	{
		float @float = this.m_nview.GetZDO().GetFloat("fuel", 0f);
		return Localization.instance.Localize(string.Concat(new object[]
		{
			this.m_name,
			" ( $piece_fire_fuel ",
			Mathf.Ceil(@float),
			"/",
			(int)this.m_maxFuel,
			" )\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use ",
			this.m_fuelItem.m_itemData.m_shared.m_name,
			"\n[<color=yellow><b>1-8</b></color>] $piece_useitem"
		}));
	}

	// Token: 0x06000D26 RID: 3366 RVA: 0x0005DCF7 File Offset: 0x0005BEF7
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000D27 RID: 3367 RVA: 0x0005DD00 File Offset: 0x0005BF00
	public bool Interact(Humanoid user, bool hold)
	{
		if (hold)
		{
			return false;
		}
		if (!this.m_nview.HasOwner())
		{
			this.m_nview.ClaimOwnership();
		}
		Inventory inventory = user.GetInventory();
		if (inventory == null)
		{
			return true;
		}
		if (!inventory.HaveItem(this.m_fuelItem.m_itemData.m_shared.m_name))
		{
			user.Message(MessageHud.MessageType.Center, "$msg_outof " + this.m_fuelItem.m_itemData.m_shared.m_name, 0, null);
			return false;
		}
		if ((float)Mathf.CeilToInt(this.m_nview.GetZDO().GetFloat("fuel", 0f)) >= this.m_maxFuel)
		{
			user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_cantaddmore", new string[]
			{
				this.m_fuelItem.m_itemData.m_shared.m_name
			}), 0, null);
			return false;
		}
		user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_fireadding", new string[]
		{
			this.m_fuelItem.m_itemData.m_shared.m_name
		}), 0, null);
		inventory.RemoveItem(this.m_fuelItem.m_itemData.m_shared.m_name, 1);
		this.m_nview.InvokeRPC("AddFuel", Array.Empty<object>());
		return true;
	}

	// Token: 0x06000D28 RID: 3368 RVA: 0x0005DE4C File Offset: 0x0005C04C
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		if (item.m_shared.m_name == this.m_fuelItem.m_itemData.m_shared.m_name)
		{
			if ((float)Mathf.CeilToInt(this.m_nview.GetZDO().GetFloat("fuel", 0f)) >= this.m_maxFuel)
			{
				user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_cantaddmore", new string[]
				{
					item.m_shared.m_name
				}), 0, null);
				return true;
			}
			Inventory inventory = user.GetInventory();
			user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_fireadding", new string[]
			{
				item.m_shared.m_name
			}), 0, null);
			inventory.RemoveItem(item, 1);
			this.m_nview.InvokeRPC("AddFuel", Array.Empty<object>());
			return true;
		}
		else
		{
			if (!(this.m_fireworkItem != null) || !(item.m_shared.m_name == this.m_fireworkItem.m_itemData.m_shared.m_name))
			{
				return false;
			}
			if (!this.IsBurning())
			{
				user.Message(MessageHud.MessageType.Center, "$msg_firenotburning", 0, null);
				return true;
			}
			if (user.GetInventory().CountItems(this.m_fireworkItem.m_itemData.m_shared.m_name) < this.m_fireworkItems)
			{
				user.Message(MessageHud.MessageType.Center, "$msg_toofew " + this.m_fireworkItem.m_itemData.m_shared.m_name, 0, null);
				return true;
			}
			user.GetInventory().RemoveItem(item.m_shared.m_name, this.m_fireworkItems);
			user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_throwinfire", new string[]
			{
				item.m_shared.m_name
			}), 0, null);
			ZNetScene.instance.SpawnObject(base.transform.position, Quaternion.identity, this.m_fireworks);
			this.m_fuelAddedEffects.Create(base.transform.position, base.transform.rotation, null, 1f);
			return true;
		}
	}

	// Token: 0x06000D29 RID: 3369 RVA: 0x0005E064 File Offset: 0x0005C264
	private void RPC_AddFuel(long sender)
	{
		if (this.m_nview.IsOwner())
		{
			float num = this.m_nview.GetZDO().GetFloat("fuel", 0f);
			if ((float)Mathf.CeilToInt(num) >= this.m_maxFuel)
			{
				return;
			}
			num = Mathf.Clamp(num, 0f, this.m_maxFuel);
			num += 1f;
			num = Mathf.Clamp(num, 0f, this.m_maxFuel);
			this.m_nview.GetZDO().Set("fuel", num);
			this.m_fuelAddedEffects.Create(base.transform.position, base.transform.rotation, null, 1f);
			this.UpdateState();
		}
	}

	// Token: 0x06000D2A RID: 3370 RVA: 0x0005E11C File Offset: 0x0005C31C
	public bool CanBeRemoved()
	{
		return !this.IsBurning();
	}

	// Token: 0x06000D2B RID: 3371 RVA: 0x0005E128 File Offset: 0x0005C328
	public bool IsBurning()
	{
		if (this.m_blocked)
		{
			return false;
		}
		float waterLevel = WaterVolume.GetWaterLevel(this.m_enabledObject.transform.position, 1f);
		return this.m_enabledObject.transform.position.y >= waterLevel && this.m_nview.GetZDO().GetFloat("fuel", 0f) > 0f;
	}

	// Token: 0x06000D2C RID: 3372 RVA: 0x0005E198 File Offset: 0x0005C398
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(base.transform.position + Vector3.up * this.m_coverCheckOffset, 0.5f);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(base.transform.position + Vector3.up * this.m_checkTerrainOffset, new Vector3(1f, 0.01f, 1f));
	}

	// Token: 0x04000BF5 RID: 3061
	private ZNetView m_nview;

	// Token: 0x04000BF6 RID: 3062
	private Piece m_piece;

	// Token: 0x04000BF7 RID: 3063
	[Header("Fire")]
	public string m_name = "Fire";

	// Token: 0x04000BF8 RID: 3064
	public float m_startFuel = 3f;

	// Token: 0x04000BF9 RID: 3065
	public float m_maxFuel = 10f;

	// Token: 0x04000BFA RID: 3066
	public float m_secPerFuel = 3f;

	// Token: 0x04000BFB RID: 3067
	public float m_checkTerrainOffset = 0.2f;

	// Token: 0x04000BFC RID: 3068
	public float m_coverCheckOffset = 0.5f;

	// Token: 0x04000BFD RID: 3069
	private const float m_minimumOpenSpace = 0.5f;

	// Token: 0x04000BFE RID: 3070
	public GameObject m_enabledObject;

	// Token: 0x04000BFF RID: 3071
	public GameObject m_enabledObjectLow;

	// Token: 0x04000C00 RID: 3072
	public GameObject m_enabledObjectHigh;

	// Token: 0x04000C01 RID: 3073
	public GameObject m_playerBaseObject;

	// Token: 0x04000C02 RID: 3074
	public ItemDrop m_fuelItem;

	// Token: 0x04000C03 RID: 3075
	public SmokeSpawner m_smokeSpawner;

	// Token: 0x04000C04 RID: 3076
	public EffectList m_fuelAddedEffects = new EffectList();

	// Token: 0x04000C05 RID: 3077
	[Header("Fireworks")]
	public ItemDrop m_fireworkItem;

	// Token: 0x04000C06 RID: 3078
	public int m_fireworkItems = 2;

	// Token: 0x04000C07 RID: 3079
	public GameObject m_fireworks;

	// Token: 0x04000C08 RID: 3080
	private bool m_blocked;

	// Token: 0x04000C09 RID: 3081
	private bool m_wet;

	// Token: 0x04000C0A RID: 3082
	private static int m_solidRayMask;
}
