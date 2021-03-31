using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000018 RID: 24
public class TombStone : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06000299 RID: 665 RVA: 0x00014E10 File Offset: 0x00013010
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_container = base.GetComponent<Container>();
		this.m_floating = base.GetComponent<Floating>();
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_body.maxDepenetrationVelocity = 1f;
		Container container = this.m_container;
		container.m_onTakeAllSuccess = (Action)Delegate.Combine(container.m_onTakeAllSuccess, new Action(this.OnTakeAllSuccess));
		if (this.m_nview.IsOwner() && this.m_nview.GetZDO().GetLong("timeOfDeath", 0L) == 0L)
		{
			this.m_nview.GetZDO().Set("timeOfDeath", ZNet.instance.GetTime().Ticks);
			this.m_nview.GetZDO().Set("SpawnPoint", base.transform.position);
		}
		base.InvokeRepeating("UpdateDespawn", TombStone.m_updateDt, TombStone.m_updateDt);
	}

	// Token: 0x0600029A RID: 666 RVA: 0x00014F08 File Offset: 0x00013108
	private void Start()
	{
		string @string = this.m_nview.GetZDO().GetString("ownerName", "");
		base.GetComponent<Container>().m_name = @string;
		this.m_worldText.text = @string;
	}

	// Token: 0x0600029B RID: 667 RVA: 0x00014F48 File Offset: 0x00013148
	public string GetHoverText()
	{
		if (!this.m_nview.IsValid())
		{
			return "";
		}
		string @string = this.m_nview.GetZDO().GetString("ownerName", "");
		string str = this.m_text + " " + @string;
		if (this.m_container.GetInventory().NrOfItems() == 0)
		{
			return "";
		}
		return Localization.instance.Localize(str + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_container_open");
	}

	// Token: 0x0600029C RID: 668 RVA: 0x0000AC8C File Offset: 0x00008E8C
	public string GetHoverName()
	{
		return "";
	}

	// Token: 0x0600029D RID: 669 RVA: 0x00014FC4 File Offset: 0x000131C4
	public bool Interact(Humanoid character, bool hold)
	{
		if (hold)
		{
			return false;
		}
		if (this.m_container.GetInventory().NrOfItems() == 0)
		{
			return false;
		}
		if (this.IsOwner())
		{
			Player player = character as Player;
			if (this.EasyFitInInventory(player))
			{
				ZLog.Log("Grave should fit in inventory, loot all");
				this.m_container.TakeAll(character);
				return true;
			}
		}
		return this.m_container.Interact(character, false);
	}

	// Token: 0x0600029E RID: 670 RVA: 0x00015028 File Offset: 0x00013228
	private void OnTakeAllSuccess()
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer)
		{
			localPlayer.m_pickupEffects.Create(localPlayer.transform.position, Quaternion.identity, null, 1f);
			localPlayer.Message(MessageHud.MessageType.Center, "$piece_tombstone_recovered", 0, null);
		}
	}

	// Token: 0x0600029F RID: 671 RVA: 0x00015074 File Offset: 0x00013274
	private bool EasyFitInInventory(Player player)
	{
		int emptySlots = player.GetInventory().GetEmptySlots();
		return this.m_container.GetInventory().NrOfItems() <= emptySlots && player.GetInventory().GetTotalWeight() + this.m_container.GetInventory().GetTotalWeight() <= player.GetMaxCarryWeight();
	}

	// Token: 0x060002A0 RID: 672 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x060002A1 RID: 673 RVA: 0x000150CC File Offset: 0x000132CC
	public void Setup(string ownerName, long ownerUID)
	{
		this.m_nview.GetZDO().Set("ownerName", ownerName);
		this.m_nview.GetZDO().Set("owner", ownerUID);
		if (this.m_body)
		{
			this.m_body.velocity = new Vector3(0f, this.m_spawnUpVel, 0f);
		}
	}

	// Token: 0x060002A2 RID: 674 RVA: 0x00015132 File Offset: 0x00013332
	public long GetOwner()
	{
		if (!this.m_nview.IsValid())
		{
			return 0L;
		}
		return this.m_nview.GetZDO().GetLong("owner", 0L);
	}

	// Token: 0x060002A3 RID: 675 RVA: 0x0001515C File Offset: 0x0001335C
	public bool IsOwner()
	{
		long owner = this.GetOwner();
		long playerID = Game.instance.GetPlayerProfile().GetPlayerID();
		return owner == playerID;
	}

	// Token: 0x060002A4 RID: 676 RVA: 0x00015184 File Offset: 0x00013384
	private void UpdateDespawn()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_floater != null)
		{
			this.UpdateFloater();
		}
		if (this.m_nview.IsOwner())
		{
			this.PositionCheck();
			if (!this.m_container.IsInUse() && this.m_container.GetInventory().NrOfItems() <= 0)
			{
				this.GiveBoost();
				this.m_removeEffect.Create(base.transform.position, base.transform.rotation, null, 1f);
				this.m_nview.Destroy();
			}
		}
	}

	// Token: 0x060002A5 RID: 677 RVA: 0x00015220 File Offset: 0x00013420
	private void GiveBoost()
	{
		if (this.m_lootStatusEffect == null)
		{
			return;
		}
		Player player = this.FindOwner();
		if (player)
		{
			player.GetSEMan().AddStatusEffect(this.m_lootStatusEffect.name, true);
		}
	}

	// Token: 0x060002A6 RID: 678 RVA: 0x00015264 File Offset: 0x00013464
	private Player FindOwner()
	{
		long owner = this.GetOwner();
		if (owner == 0L)
		{
			return null;
		}
		return Player.GetPlayer(owner);
	}

	// Token: 0x060002A7 RID: 679 RVA: 0x00015284 File Offset: 0x00013484
	private void PositionCheck()
	{
		Vector3 vec = this.m_nview.GetZDO().GetVec3("SpawnPoint", base.transform.position);
		if (Utils.DistanceXZ(vec, base.transform.position) > 4f)
		{
			ZLog.Log("Tombstone moved too far from spawn position, reseting position");
			base.transform.position = vec;
			this.m_body.position = vec;
			this.m_body.velocity = Vector3.zero;
		}
		float groundHeight = ZoneSystem.instance.GetGroundHeight(base.transform.position);
		if (base.transform.position.y < groundHeight - 1f)
		{
			Vector3 position = base.transform.position;
			position.y = groundHeight + 0.5f;
			base.transform.position = position;
			this.m_body.position = position;
			this.m_body.velocity = Vector3.zero;
		}
	}

	// Token: 0x060002A8 RID: 680 RVA: 0x00015370 File Offset: 0x00013570
	private void UpdateFloater()
	{
		if (this.m_nview.IsOwner())
		{
			bool flag = this.m_floating.BeenInWater();
			this.m_nview.GetZDO().Set("inWater", flag);
			this.m_floater.SetActive(flag);
			return;
		}
		bool @bool = this.m_nview.GetZDO().GetBool("inWater", false);
		this.m_floater.SetActive(@bool);
	}

	// Token: 0x0400020B RID: 523
	private static float m_updateDt = 2f;

	// Token: 0x0400020C RID: 524
	public string m_text = "$piece_tombstone";

	// Token: 0x0400020D RID: 525
	public GameObject m_floater;

	// Token: 0x0400020E RID: 526
	public Text m_worldText;

	// Token: 0x0400020F RID: 527
	public float m_spawnUpVel = 5f;

	// Token: 0x04000210 RID: 528
	public StatusEffect m_lootStatusEffect;

	// Token: 0x04000211 RID: 529
	public EffectList m_removeEffect = new EffectList();

	// Token: 0x04000212 RID: 530
	private Container m_container;

	// Token: 0x04000213 RID: 531
	private ZNetView m_nview;

	// Token: 0x04000214 RID: 532
	private Floating m_floating;

	// Token: 0x04000215 RID: 533
	private Rigidbody m_body;
}
