using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000BF RID: 191
public class Container : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06000CA4 RID: 3236 RVA: 0x0005A820 File Offset: 0x00058A20
	private void Awake()
	{
		this.m_nview = (this.m_rootObjectOverride ? this.m_rootObjectOverride.GetComponent<ZNetView>() : base.GetComponent<ZNetView>());
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		this.m_inventory = new Inventory(this.m_name, this.m_bkg, this.m_width, this.m_height);
		Inventory inventory = this.m_inventory;
		inventory.m_onChanged = (Action)Delegate.Combine(inventory.m_onChanged, new Action(this.OnContainerChanged));
		this.m_piece = base.GetComponent<Piece>();
		if (this.m_nview)
		{
			this.m_nview.Register<long>("RequestOpen", new Action<long, long>(this.RPC_RequestOpen));
			this.m_nview.Register<bool>("OpenRespons", new Action<long, bool>(this.RPC_OpenRespons));
			this.m_nview.Register<long>("RequestTakeAll", new Action<long, long>(this.RPC_RequestTakeAll));
			this.m_nview.Register<bool>("TakeAllRespons", new Action<long, bool>(this.RPC_TakeAllRespons));
		}
		WearNTear wearNTear = this.m_rootObjectOverride ? this.m_rootObjectOverride.GetComponent<WearNTear>() : base.GetComponent<WearNTear>();
		if (wearNTear)
		{
			WearNTear wearNTear2 = wearNTear;
			wearNTear2.m_onDestroyed = (Action)Delegate.Combine(wearNTear2.m_onDestroyed, new Action(this.OnDestroyed));
		}
		Destructible destructible = this.m_rootObjectOverride ? this.m_rootObjectOverride.GetComponent<Destructible>() : base.GetComponent<Destructible>();
		if (destructible)
		{
			Destructible destructible2 = destructible;
			destructible2.m_onDestroyed = (Action)Delegate.Combine(destructible2.m_onDestroyed, new Action(this.OnDestroyed));
		}
		if (this.m_nview.IsOwner() && !this.m_nview.GetZDO().GetBool("addedDefaultItems", false))
		{
			this.AddDefaultItems();
			this.m_nview.GetZDO().Set("addedDefaultItems", true);
		}
		base.InvokeRepeating("CheckForChanges", 0f, 1f);
	}

	// Token: 0x06000CA5 RID: 3237 RVA: 0x0005AA20 File Offset: 0x00058C20
	private void AddDefaultItems()
	{
		foreach (ItemDrop.ItemData item in this.m_defaultItems.GetDropListItems())
		{
			this.m_inventory.AddItem(item);
		}
	}

	// Token: 0x06000CA6 RID: 3238 RVA: 0x0005AA80 File Offset: 0x00058C80
	private void DropAllItems(GameObject lootContainerPrefab)
	{
		while (this.m_inventory.NrOfItems() > 0)
		{
			Vector3 position = base.transform.position + UnityEngine.Random.insideUnitSphere * 1f;
			UnityEngine.Object.Instantiate<GameObject>(lootContainerPrefab, position, UnityEngine.Random.rotation).GetComponent<Container>().GetInventory().MoveAll(this.m_inventory);
		}
	}

	// Token: 0x06000CA7 RID: 3239 RVA: 0x0005AAE0 File Offset: 0x00058CE0
	private void DropAllItems()
	{
		List<ItemDrop.ItemData> allItems = this.m_inventory.GetAllItems();
		int num = 1;
		foreach (ItemDrop.ItemData item in allItems)
		{
			Vector3 position = base.transform.position + Vector3.up * 0.5f + UnityEngine.Random.insideUnitSphere * 0.3f;
			Quaternion rotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
			ItemDrop.DropItem(item, 0, position, rotation);
			num++;
		}
		this.m_inventory.RemoveAll();
		this.Save();
	}

	// Token: 0x06000CA8 RID: 3240 RVA: 0x0005ABA0 File Offset: 0x00058DA0
	private void OnDestroyed()
	{
		if (this.m_nview.IsOwner())
		{
			if (this.m_destroyedLootPrefab)
			{
				this.DropAllItems(this.m_destroyedLootPrefab);
				return;
			}
			this.DropAllItems();
		}
	}

	// Token: 0x06000CA9 RID: 3241 RVA: 0x0005ABD0 File Offset: 0x00058DD0
	private void CheckForChanges()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.Load();
		this.UpdateUseVisual();
		if (this.m_autoDestroyEmpty && this.m_nview.IsOwner() && !this.IsInUse() && this.m_inventory.NrOfItems() == 0)
		{
			this.m_nview.Destroy();
		}
	}

	// Token: 0x06000CAA RID: 3242 RVA: 0x0005AC2C File Offset: 0x00058E2C
	private void UpdateUseVisual()
	{
		bool flag;
		if (this.m_nview.IsOwner())
		{
			flag = this.m_inUse;
			this.m_nview.GetZDO().Set("InUse", this.m_inUse ? 1 : 0);
		}
		else
		{
			flag = (this.m_nview.GetZDO().GetInt("InUse", 0) == 1);
		}
		if (this.m_open)
		{
			this.m_open.SetActive(flag);
		}
		if (this.m_closed)
		{
			this.m_closed.SetActive(!flag);
		}
	}

	// Token: 0x06000CAB RID: 3243 RVA: 0x0005ACC0 File Offset: 0x00058EC0
	public string GetHoverText()
	{
		if (this.m_checkGuardStone && !PrivateArea.CheckAccess(base.transform.position, 0f, false, false))
		{
			return Localization.instance.Localize(this.m_name + "\n$piece_noaccess");
		}
		string text;
		if (this.m_inventory.NrOfItems() == 0)
		{
			text = this.m_name + " ( $piece_container_empty )";
		}
		else
		{
			text = this.m_name;
		}
		text += "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_container_open";
		return Localization.instance.Localize(text);
	}

	// Token: 0x06000CAC RID: 3244 RVA: 0x0005AD47 File Offset: 0x00058F47
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000CAD RID: 3245 RVA: 0x0005AD50 File Offset: 0x00058F50
	public bool Interact(Humanoid character, bool hold)
	{
		if (hold)
		{
			return false;
		}
		if (this.m_checkGuardStone && !PrivateArea.CheckAccess(base.transform.position, 0f, true, false))
		{
			return true;
		}
		long playerID = Game.instance.GetPlayerProfile().GetPlayerID();
		if (!this.CheckAccess(playerID))
		{
			character.Message(MessageHud.MessageType.Center, "$msg_cantopen", 0, null);
			return true;
		}
		this.m_nview.InvokeRPC("RequestOpen", new object[]
		{
			playerID
		});
		return true;
	}

	// Token: 0x06000CAE RID: 3246 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000CAF RID: 3247 RVA: 0x0005ADCE File Offset: 0x00058FCE
	public bool CanBeRemoved()
	{
		return this.m_privacy != Container.PrivacySetting.Private || this.GetInventory().NrOfItems() <= 0;
	}

	// Token: 0x06000CB0 RID: 3248 RVA: 0x0005ADEC File Offset: 0x00058FEC
	private bool CheckAccess(long playerID)
	{
		switch (this.m_privacy)
		{
		case Container.PrivacySetting.Private:
			return this.m_piece.GetCreator() == playerID;
		case Container.PrivacySetting.Group:
			return false;
		case Container.PrivacySetting.Public:
			return true;
		default:
			return false;
		}
	}

	// Token: 0x06000CB1 RID: 3249 RVA: 0x0005AE2B File Offset: 0x0005902B
	public bool IsOwner()
	{
		return this.m_nview.IsOwner();
	}

	// Token: 0x06000CB2 RID: 3250 RVA: 0x0005AE38 File Offset: 0x00059038
	public bool IsInUse()
	{
		return this.m_inUse;
	}

	// Token: 0x06000CB3 RID: 3251 RVA: 0x0005AE40 File Offset: 0x00059040
	public void SetInUse(bool inUse)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_inUse == inUse)
		{
			return;
		}
		this.m_inUse = inUse;
		this.UpdateUseVisual();
		if (inUse)
		{
			this.m_openEffects.Create(base.transform.position, base.transform.rotation, null, 1f);
			return;
		}
		this.m_closeEffects.Create(base.transform.position, base.transform.rotation, null, 1f);
	}

	// Token: 0x06000CB4 RID: 3252 RVA: 0x0005AEC6 File Offset: 0x000590C6
	public Inventory GetInventory()
	{
		return this.m_inventory;
	}

	// Token: 0x06000CB5 RID: 3253 RVA: 0x0005AED0 File Offset: 0x000590D0
	private void RPC_RequestOpen(long uid, long playerID)
	{
		ZLog.Log(string.Concat(new object[]
		{
			"Player ",
			uid,
			" wants to open ",
			base.gameObject.name,
			"   im: ",
			ZDOMan.instance.GetMyID()
		}));
		if (!this.m_nview.IsOwner())
		{
			ZLog.Log("  but im not the owner");
			return;
		}
		if ((this.IsInUse() || (this.m_wagon && this.m_wagon.InUse())) && uid != ZNet.instance.GetUID())
		{
			ZLog.Log("  in use");
			this.m_nview.InvokeRPC(uid, "OpenRespons", new object[]
			{
				false
			});
			return;
		}
		if (!this.CheckAccess(playerID))
		{
			ZLog.Log("  not yours");
			this.m_nview.InvokeRPC(uid, "OpenRespons", new object[]
			{
				false
			});
			return;
		}
		ZDOMan.instance.ForceSendZDO(uid, this.m_nview.GetZDO().m_uid);
		this.m_nview.GetZDO().SetOwner(uid);
		this.m_nview.InvokeRPC(uid, "OpenRespons", new object[]
		{
			true
		});
	}

	// Token: 0x06000CB6 RID: 3254 RVA: 0x0005B020 File Offset: 0x00059220
	private void RPC_OpenRespons(long uid, bool granted)
	{
		if (!Player.m_localPlayer)
		{
			return;
		}
		if (granted)
		{
			InventoryGui.instance.Show(this);
			return;
		}
		Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_inuse", 0, null);
	}

	// Token: 0x06000CB7 RID: 3255 RVA: 0x0005B050 File Offset: 0x00059250
	public bool TakeAll(Humanoid character)
	{
		if (this.m_checkGuardStone && !PrivateArea.CheckAccess(base.transform.position, 0f, true, false))
		{
			return false;
		}
		long playerID = Game.instance.GetPlayerProfile().GetPlayerID();
		if (!this.CheckAccess(playerID))
		{
			character.Message(MessageHud.MessageType.Center, "$msg_cantopen", 0, null);
			return false;
		}
		this.m_nview.InvokeRPC("RequestTakeAll", new object[]
		{
			playerID
		});
		return true;
	}

	// Token: 0x06000CB8 RID: 3256 RVA: 0x0005B0CC File Offset: 0x000592CC
	private void RPC_RequestTakeAll(long uid, long playerID)
	{
		ZLog.Log(string.Concat(new object[]
		{
			"Player ",
			uid,
			" wants to takeall from ",
			base.gameObject.name,
			"   im: ",
			ZDOMan.instance.GetMyID()
		}));
		if (!this.m_nview.IsOwner())
		{
			ZLog.Log("  but im not the owner");
			return;
		}
		if ((this.IsInUse() || (this.m_wagon && this.m_wagon.InUse())) && uid != ZNet.instance.GetUID())
		{
			ZLog.Log("  in use");
			this.m_nview.InvokeRPC(uid, "TakeAllRespons", new object[]
			{
				false
			});
			return;
		}
		if (!this.CheckAccess(playerID))
		{
			ZLog.Log("  not yours");
			this.m_nview.InvokeRPC(uid, "TakeAllRespons", new object[]
			{
				false
			});
			return;
		}
		if (Time.time - this.m_lastTakeAllTime < 2f)
		{
			return;
		}
		this.m_lastTakeAllTime = Time.time;
		this.m_nview.InvokeRPC(uid, "TakeAllRespons", new object[]
		{
			true
		});
	}

	// Token: 0x06000CB9 RID: 3257 RVA: 0x0005B210 File Offset: 0x00059410
	private void RPC_TakeAllRespons(long uid, bool granted)
	{
		if (!Player.m_localPlayer)
		{
			return;
		}
		if (granted)
		{
			this.m_nview.ClaimOwnership();
			ZDOMan.instance.ForceSendZDO(uid, this.m_nview.GetZDO().m_uid);
			Player.m_localPlayer.GetInventory().MoveAll(this.m_inventory);
			if (this.m_onTakeAllSuccess != null)
			{
				this.m_onTakeAllSuccess();
				return;
			}
		}
		else
		{
			Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_inuse", 0, null);
		}
	}

	// Token: 0x06000CBA RID: 3258 RVA: 0x0005B28E File Offset: 0x0005948E
	private void OnContainerChanged()
	{
		if (this.m_loading)
		{
			return;
		}
		if (!this.IsOwner())
		{
			return;
		}
		this.Save();
	}

	// Token: 0x06000CBB RID: 3259 RVA: 0x0005B2A8 File Offset: 0x000594A8
	private void Save()
	{
		ZPackage zpackage = new ZPackage();
		this.m_inventory.Save(zpackage);
		string @base = zpackage.GetBase64();
		this.m_nview.GetZDO().Set("items", @base);
		this.m_lastRevision = this.m_nview.GetZDO().m_dataRevision;
		this.m_lastDataString = @base;
	}

	// Token: 0x06000CBC RID: 3260 RVA: 0x0005B304 File Offset: 0x00059504
	private void Load()
	{
		if (this.m_nview.GetZDO().m_dataRevision == this.m_lastRevision)
		{
			return;
		}
		string @string = this.m_nview.GetZDO().GetString("items", "");
		if (string.IsNullOrEmpty(@string) || @string == this.m_lastDataString)
		{
			return;
		}
		ZPackage pkg = new ZPackage(@string);
		this.m_loading = true;
		this.m_inventory.Load(pkg);
		this.m_loading = false;
		this.m_lastRevision = this.m_nview.GetZDO().m_dataRevision;
		this.m_lastDataString = @string;
	}

	// Token: 0x04000B8E RID: 2958
	private float m_lastTakeAllTime;

	// Token: 0x04000B8F RID: 2959
	public Action m_onTakeAllSuccess;

	// Token: 0x04000B90 RID: 2960
	public string m_name = "Container";

	// Token: 0x04000B91 RID: 2961
	public Sprite m_bkg;

	// Token: 0x04000B92 RID: 2962
	public int m_width = 3;

	// Token: 0x04000B93 RID: 2963
	public int m_height = 2;

	// Token: 0x04000B94 RID: 2964
	public Container.PrivacySetting m_privacy = Container.PrivacySetting.Public;

	// Token: 0x04000B95 RID: 2965
	public bool m_checkGuardStone;

	// Token: 0x04000B96 RID: 2966
	public bool m_autoDestroyEmpty;

	// Token: 0x04000B97 RID: 2967
	public DropTable m_defaultItems = new DropTable();

	// Token: 0x04000B98 RID: 2968
	public GameObject m_open;

	// Token: 0x04000B99 RID: 2969
	public GameObject m_closed;

	// Token: 0x04000B9A RID: 2970
	public EffectList m_openEffects = new EffectList();

	// Token: 0x04000B9B RID: 2971
	public EffectList m_closeEffects = new EffectList();

	// Token: 0x04000B9C RID: 2972
	public ZNetView m_rootObjectOverride;

	// Token: 0x04000B9D RID: 2973
	public Vagon m_wagon;

	// Token: 0x04000B9E RID: 2974
	public GameObject m_destroyedLootPrefab;

	// Token: 0x04000B9F RID: 2975
	private Inventory m_inventory;

	// Token: 0x04000BA0 RID: 2976
	private ZNetView m_nview;

	// Token: 0x04000BA1 RID: 2977
	private Piece m_piece;

	// Token: 0x04000BA2 RID: 2978
	private bool m_inUse;

	// Token: 0x04000BA3 RID: 2979
	private bool m_loading;

	// Token: 0x04000BA4 RID: 2980
	private uint m_lastRevision;

	// Token: 0x04000BA5 RID: 2981
	private string m_lastDataString = "";

	// Token: 0x02000192 RID: 402
	public enum PrivacySetting
	{
		// Token: 0x0400127A RID: 4730
		Private,
		// Token: 0x0400127B RID: 4731
		Group,
		// Token: 0x0400127C RID: 4732
		Public
	}
}
