using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000018 RID: 24
public class TombStone : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06000298 RID: 664 RVA: 0x00014D5C File Offset: 0x00012F5C
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

	// Token: 0x06000299 RID: 665 RVA: 0x00014E54 File Offset: 0x00013054
	private void Start()
	{
		string @string = this.m_nview.GetZDO().GetString("ownerName", "");
		base.GetComponent<Container>().m_name = @string;
		this.m_worldText.text = @string;
	}

	// Token: 0x0600029A RID: 666 RVA: 0x00014E94 File Offset: 0x00013094
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

	// Token: 0x0600029B RID: 667 RVA: 0x0000AC4C File Offset: 0x00008E4C
	public string GetHoverName()
	{
		return "";
	}

	// Token: 0x0600029C RID: 668 RVA: 0x00014F10 File Offset: 0x00013110
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

	// Token: 0x0600029D RID: 669 RVA: 0x00014F74 File Offset: 0x00013174
	private void OnTakeAllSuccess()
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer)
		{
			localPlayer.m_pickupEffects.Create(localPlayer.transform.position, Quaternion.identity, null, 1f);
			localPlayer.Message(MessageHud.MessageType.Center, "$piece_tombstone_recovered", 0, null);
		}
	}

	// Token: 0x0600029E RID: 670 RVA: 0x00014FC0 File Offset: 0x000131C0
	private bool EasyFitInInventory(Player player)
	{
		int emptySlots = player.GetInventory().GetEmptySlots();
		return this.m_container.GetInventory().NrOfItems() <= emptySlots && player.GetInventory().GetTotalWeight() + this.m_container.GetInventory().GetTotalWeight() <= player.GetMaxCarryWeight();
	}

	// Token: 0x0600029F RID: 671 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x060002A0 RID: 672 RVA: 0x00015018 File Offset: 0x00013218
	public void Setup(string ownerName, long ownerUID)
	{
		this.m_nview.GetZDO().Set("ownerName", ownerName);
		this.m_nview.GetZDO().Set("owner", ownerUID);
		if (this.m_body)
		{
			this.m_body.velocity = new Vector3(0f, this.m_spawnUpVel, 0f);
		}
	}

	// Token: 0x060002A1 RID: 673 RVA: 0x0001507E File Offset: 0x0001327E
	public long GetOwner()
	{
		if (!this.m_nview.IsValid())
		{
			return 0L;
		}
		return this.m_nview.GetZDO().GetLong("owner", 0L);
	}

	// Token: 0x060002A2 RID: 674 RVA: 0x000150A8 File Offset: 0x000132A8
	public bool IsOwner()
	{
		long owner = this.GetOwner();
		long playerID = Game.instance.GetPlayerProfile().GetPlayerID();
		return owner == playerID;
	}

	// Token: 0x060002A3 RID: 675 RVA: 0x000150D0 File Offset: 0x000132D0
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

	// Token: 0x060002A4 RID: 676 RVA: 0x0001516C File Offset: 0x0001336C
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

	// Token: 0x060002A5 RID: 677 RVA: 0x000151B0 File Offset: 0x000133B0
	private Player FindOwner()
	{
		long owner = this.GetOwner();
		if (owner == 0L)
		{
			return null;
		}
		return Player.GetPlayer(owner);
	}

	// Token: 0x060002A6 RID: 678 RVA: 0x000151D0 File Offset: 0x000133D0
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

	// Token: 0x060002A7 RID: 679 RVA: 0x000152BC File Offset: 0x000134BC
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

	// Token: 0x04000207 RID: 519
	private static float m_updateDt = 2f;

	// Token: 0x04000208 RID: 520
	public string m_text = "$piece_tombstone";

	// Token: 0x04000209 RID: 521
	public GameObject m_floater;

	// Token: 0x0400020A RID: 522
	public Text m_worldText;

	// Token: 0x0400020B RID: 523
	public float m_spawnUpVel = 5f;

	// Token: 0x0400020C RID: 524
	public StatusEffect m_lootStatusEffect;

	// Token: 0x0400020D RID: 525
	public EffectList m_removeEffect = new EffectList();

	// Token: 0x0400020E RID: 526
	private Container m_container;

	// Token: 0x0400020F RID: 527
	private ZNetView m_nview;

	// Token: 0x04000210 RID: 528
	private Floating m_floating;

	// Token: 0x04000211 RID: 529
	private Rigidbody m_body;
}
