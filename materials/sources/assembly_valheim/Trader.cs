﻿using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000109 RID: 265
public class Trader : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06000FB1 RID: 4017 RVA: 0x0006E9AD File Offset: 0x0006CBAD
	private void Start()
	{
		this.m_animator = base.GetComponentInChildren<Animator>();
		this.m_lookAt = base.GetComponentInChildren<LookAt>();
		base.InvokeRepeating("RandomTalk", this.m_randomTalkInterval, this.m_randomTalkInterval);
	}

	// Token: 0x06000FB2 RID: 4018 RVA: 0x0006E9E0 File Offset: 0x0006CBE0
	private void Update()
	{
		Player closestPlayer = Player.GetClosestPlayer(base.transform.position, this.m_standRange);
		if (closestPlayer)
		{
			this.m_animator.SetBool("Stand", true);
			this.m_lookAt.SetLoockAtTarget(closestPlayer.GetHeadPoint());
			float num = Vector3.Distance(closestPlayer.transform.position, base.transform.position);
			if (!this.m_didGreet && num < this.m_greetRange)
			{
				this.m_didGreet = true;
				this.Say(this.m_randomGreets, "Greet");
				this.m_randomGreetFX.Create(base.transform.position, Quaternion.identity, null, 1f);
			}
			if (this.m_didGreet && !this.m_didGoodbye && num > this.m_byeRange)
			{
				this.m_didGoodbye = true;
				this.Say(this.m_randomGoodbye, "Greet");
				this.m_randomGoodbyeFX.Create(base.transform.position, Quaternion.identity, null, 1f);
				return;
			}
		}
		else
		{
			this.m_animator.SetBool("Stand", false);
			this.m_lookAt.ResetTarget();
		}
	}

	// Token: 0x06000FB3 RID: 4019 RVA: 0x0006EB08 File Offset: 0x0006CD08
	private void RandomTalk()
	{
		if (this.m_animator.GetBool("Stand") && !StoreGui.IsVisible() && Player.IsPlayerInRange(base.transform.position, this.m_greetRange))
		{
			this.Say(this.m_randomTalk, "Talk");
			this.m_randomTalkFX.Create(base.transform.position, Quaternion.identity, null, 1f);
		}
	}

	// Token: 0x06000FB4 RID: 4020 RVA: 0x0006EB79 File Offset: 0x0006CD79
	public string GetHoverText()
	{
		return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $raven_interact");
	}

	// Token: 0x06000FB5 RID: 4021 RVA: 0x0006EB95 File Offset: 0x0006CD95
	public string GetHoverName()
	{
		return Localization.instance.Localize(this.m_name);
	}

	// Token: 0x06000FB6 RID: 4022 RVA: 0x0006EBA8 File Offset: 0x0006CDA8
	public bool Interact(Humanoid character, bool hold)
	{
		if (hold)
		{
			return false;
		}
		StoreGui.instance.Show(this);
		this.Say(this.m_randomStartTrade, "Talk");
		this.m_randomStartTradeFX.Create(base.transform.position, Quaternion.identity, null, 1f);
		return false;
	}

	// Token: 0x06000FB7 RID: 4023 RVA: 0x0006EBFC File Offset: 0x0006CDFC
	private void DiscoverItems(Player player)
	{
		foreach (Trader.TradeItem tradeItem in this.m_items)
		{
			player.AddKnownItem(tradeItem.m_prefab.m_itemData);
		}
	}

	// Token: 0x06000FB8 RID: 4024 RVA: 0x0006EC5C File Offset: 0x0006CE5C
	private void Say(List<string> texts, string trigger)
	{
		this.Say(texts[UnityEngine.Random.Range(0, texts.Count)], trigger);
	}

	// Token: 0x06000FB9 RID: 4025 RVA: 0x0006EC78 File Offset: 0x0006CE78
	private void Say(string text, string trigger)
	{
		Chat.instance.SetNpcText(base.gameObject, Vector3.up * 1.5f, 20f, this.m_hideDialogDelay, "", text, false);
		if (trigger.Length > 0)
		{
			this.m_animator.SetTrigger(trigger);
		}
	}

	// Token: 0x06000FBA RID: 4026 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000FBB RID: 4027 RVA: 0x0006ECCB File Offset: 0x0006CECB
	public void OnBought(Trader.TradeItem item)
	{
		this.Say(this.m_randomBuy, "Buy");
		this.m_randomBuyFX.Create(base.transform.position, Quaternion.identity, null, 1f);
	}

	// Token: 0x06000FBC RID: 4028 RVA: 0x0006ED00 File Offset: 0x0006CF00
	public void OnSold()
	{
		this.Say(this.m_randomSell, "Sell");
		this.m_randomSellFX.Create(base.transform.position, Quaternion.identity, null, 1f);
	}

	// Token: 0x04000E78 RID: 3704
	public string m_name = "Haldor";

	// Token: 0x04000E79 RID: 3705
	public float m_standRange = 15f;

	// Token: 0x04000E7A RID: 3706
	public float m_greetRange = 5f;

	// Token: 0x04000E7B RID: 3707
	public float m_byeRange = 5f;

	// Token: 0x04000E7C RID: 3708
	public List<Trader.TradeItem> m_items = new List<Trader.TradeItem>();

	// Token: 0x04000E7D RID: 3709
	[Header("Dialog")]
	public float m_hideDialogDelay = 5f;

	// Token: 0x04000E7E RID: 3710
	public float m_randomTalkInterval = 30f;

	// Token: 0x04000E7F RID: 3711
	public List<string> m_randomTalk = new List<string>();

	// Token: 0x04000E80 RID: 3712
	public List<string> m_randomGreets = new List<string>();

	// Token: 0x04000E81 RID: 3713
	public List<string> m_randomGoodbye = new List<string>();

	// Token: 0x04000E82 RID: 3714
	public List<string> m_randomStartTrade = new List<string>();

	// Token: 0x04000E83 RID: 3715
	public List<string> m_randomBuy = new List<string>();

	// Token: 0x04000E84 RID: 3716
	public List<string> m_randomSell = new List<string>();

	// Token: 0x04000E85 RID: 3717
	public EffectList m_randomTalkFX = new EffectList();

	// Token: 0x04000E86 RID: 3718
	public EffectList m_randomGreetFX = new EffectList();

	// Token: 0x04000E87 RID: 3719
	public EffectList m_randomGoodbyeFX = new EffectList();

	// Token: 0x04000E88 RID: 3720
	public EffectList m_randomStartTradeFX = new EffectList();

	// Token: 0x04000E89 RID: 3721
	public EffectList m_randomBuyFX = new EffectList();

	// Token: 0x04000E8A RID: 3722
	public EffectList m_randomSellFX = new EffectList();

	// Token: 0x04000E8B RID: 3723
	private bool m_didGreet;

	// Token: 0x04000E8C RID: 3724
	private bool m_didGoodbye;

	// Token: 0x04000E8D RID: 3725
	private Animator m_animator;

	// Token: 0x04000E8E RID: 3726
	private LookAt m_lookAt;

	// Token: 0x020001AE RID: 430
	[Serializable]
	public class TradeItem
	{
		// Token: 0x04001318 RID: 4888
		public ItemDrop m_prefab;

		// Token: 0x04001319 RID: 4889
		public int m_stack = 1;

		// Token: 0x0400131A RID: 4890
		public int m_price = 100;
	}
}
