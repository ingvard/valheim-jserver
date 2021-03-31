using System;
using UnityEngine;

// Token: 0x02000017 RID: 23
public class Tameable : MonoBehaviour, Interactable
{
	// Token: 0x06000287 RID: 647 RVA: 0x0001477C File Offset: 0x0001297C
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_character = base.GetComponent<Character>();
		this.m_monsterAI = base.GetComponent<MonsterAI>();
		MonsterAI monsterAI = this.m_monsterAI;
		monsterAI.m_onConsumedItem = (Action<ItemDrop>)Delegate.Combine(monsterAI.m_onConsumedItem, new Action<ItemDrop>(this.OnConsumedItem));
		if (this.m_nview.IsValid())
		{
			this.m_nview.Register<ZDOID>("Command", new Action<long, ZDOID>(this.RPC_Command));
			base.InvokeRepeating("TamingUpdate", 3f, 3f);
		}
	}

	// Token: 0x06000288 RID: 648 RVA: 0x00014814 File Offset: 0x00012A14
	public string GetHoverText()
	{
		if (!this.m_nview.IsValid())
		{
			return "";
		}
		string text = Localization.instance.Localize(this.m_character.m_name);
		if (this.m_character.IsTamed())
		{
			text += Localization.instance.Localize(" ( $hud_tame, " + this.GetStatusString() + " )");
			return text + Localization.instance.Localize("\n[<color=yellow><b>$KEY_Use</b></color>] $hud_pet");
		}
		int tameness = this.GetTameness();
		if (tameness <= 0)
		{
			text += Localization.instance.Localize(" ( $hud_wild, " + this.GetStatusString() + " )");
		}
		else
		{
			text += Localization.instance.Localize(string.Concat(new object[]
			{
				" ( $hud_tameness  ",
				tameness,
				"%, ",
				this.GetStatusString(),
				" )"
			}));
		}
		return text;
	}

	// Token: 0x06000289 RID: 649 RVA: 0x0001490F File Offset: 0x00012B0F
	private string GetStatusString()
	{
		if (this.m_monsterAI.IsAlerted())
		{
			return "$hud_tamefrightened";
		}
		if (this.IsHungry())
		{
			return "$hud_tamehungry";
		}
		if (this.m_character.IsTamed())
		{
			return "$hud_tamehappy";
		}
		return "$hud_tameinprogress";
	}

	// Token: 0x0600028A RID: 650 RVA: 0x0001494C File Offset: 0x00012B4C
	public bool Interact(Humanoid user, bool hold)
	{
		if (hold)
		{
			return false;
		}
		if (!this.m_nview.IsValid())
		{
			return false;
		}
		string hoverName = this.m_character.GetHoverName();
		if (!this.m_character.IsTamed())
		{
			return false;
		}
		if (Time.time - this.m_lastPetTime > 1f)
		{
			this.m_lastPetTime = Time.time;
			this.m_petEffect.Create(this.m_character.GetCenterPoint(), Quaternion.identity, null, 1f);
			if (this.m_commandable)
			{
				this.Command(user);
			}
			else
			{
				user.Message(MessageHud.MessageType.Center, hoverName + " $hud_tamelove", 0, null);
			}
			return true;
		}
		return false;
	}

	// Token: 0x0600028B RID: 651 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x0600028C RID: 652 RVA: 0x000149F0 File Offset: 0x00012BF0
	private void TamingUpdate()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_character.IsTamed())
		{
			return;
		}
		if (this.IsHungry())
		{
			return;
		}
		if (this.m_monsterAI.IsAlerted())
		{
			return;
		}
		this.m_monsterAI.SetDespawnInDay(false);
		this.m_monsterAI.SetEventCreature(false);
		this.DecreaseRemainingTime(3f);
		if (this.GetRemainingTime() <= 0f)
		{
			this.Tame();
			return;
		}
		this.m_sootheEffect.Create(this.m_character.GetCenterPoint(), Quaternion.identity, null, 1f);
	}

	// Token: 0x0600028D RID: 653 RVA: 0x00014A98 File Offset: 0x00012C98
	public void Tame()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_character.IsTamed())
		{
			return;
		}
		this.m_monsterAI.MakeTame();
		this.m_tamedEffect.Create(this.m_character.GetCenterPoint(), Quaternion.identity, null, 1f);
		Player closestPlayer = Player.GetClosestPlayer(base.transform.position, 30f);
		if (closestPlayer)
		{
			closestPlayer.Message(MessageHud.MessageType.Center, this.m_character.m_name + " $hud_tamedone", 0, null);
		}
	}

	// Token: 0x0600028E RID: 654 RVA: 0x00014B38 File Offset: 0x00012D38
	public static void TameAllInArea(Vector3 point, float radius)
	{
		foreach (Character character in Character.GetAllCharacters())
		{
			if (!character.IsPlayer())
			{
				Tameable component = character.GetComponent<Tameable>();
				if (component)
				{
					component.Tame();
				}
			}
		}
	}

	// Token: 0x0600028F RID: 655 RVA: 0x00014BA0 File Offset: 0x00012DA0
	private void Command(Humanoid user)
	{
		this.m_nview.InvokeRPC("Command", new object[]
		{
			user.GetZDOID()
		});
	}

	// Token: 0x06000290 RID: 656 RVA: 0x00014BC8 File Offset: 0x00012DC8
	private Player GetPlayer(ZDOID characterID)
	{
		GameObject gameObject = ZNetScene.instance.FindInstance(characterID);
		if (gameObject)
		{
			return gameObject.GetComponent<Player>();
		}
		return null;
	}

	// Token: 0x06000291 RID: 657 RVA: 0x00014BF4 File Offset: 0x00012DF4
	private void RPC_Command(long sender, ZDOID characterID)
	{
		Player player = this.GetPlayer(characterID);
		if (player == null)
		{
			return;
		}
		if (this.m_monsterAI.GetFollowTarget())
		{
			this.m_monsterAI.SetFollowTarget(null);
			this.m_monsterAI.SetPatrolPoint();
			player.Message(MessageHud.MessageType.Center, this.m_character.GetHoverName() + " $hud_tamestay", 0, null);
			return;
		}
		this.m_monsterAI.ResetPatrolPoint();
		this.m_monsterAI.SetFollowTarget(player.gameObject);
		player.Message(MessageHud.MessageType.Center, this.m_character.GetHoverName() + " $hud_tamefollow", 0, null);
	}

	// Token: 0x06000292 RID: 658 RVA: 0x00014C98 File Offset: 0x00012E98
	public bool IsHungry()
	{
		DateTime d = new DateTime(this.m_nview.GetZDO().GetLong("TameLastFeeding", 0L));
		return (ZNet.instance.GetTime() - d).TotalSeconds > (double)this.m_fedDuration;
	}

	// Token: 0x06000293 RID: 659 RVA: 0x00014CE4 File Offset: 0x00012EE4
	private void ResetFeedingTimer()
	{
		this.m_nview.GetZDO().Set("TameLastFeeding", ZNet.instance.GetTime().Ticks);
	}

	// Token: 0x06000294 RID: 660 RVA: 0x00014D18 File Offset: 0x00012F18
	private int GetTameness()
	{
		float remainingTime = this.GetRemainingTime();
		return (int)((1f - Mathf.Clamp01(remainingTime / this.m_tamingTime)) * 100f);
	}

	// Token: 0x06000295 RID: 661 RVA: 0x00014D46 File Offset: 0x00012F46
	private void OnConsumedItem(ItemDrop item)
	{
		if (this.IsHungry())
		{
			this.m_sootheEffect.Create(this.m_character.GetCenterPoint(), Quaternion.identity, null, 1f);
		}
		this.ResetFeedingTimer();
	}

	// Token: 0x06000296 RID: 662 RVA: 0x00014D78 File Offset: 0x00012F78
	private void DecreaseRemainingTime(float time)
	{
		float num = this.GetRemainingTime();
		num -= time;
		if (num < 0f)
		{
			num = 0f;
		}
		this.m_nview.GetZDO().Set("TameTimeLeft", num);
	}

	// Token: 0x06000297 RID: 663 RVA: 0x00014DB4 File Offset: 0x00012FB4
	private float GetRemainingTime()
	{
		return this.m_nview.GetZDO().GetFloat("TameTimeLeft", this.m_tamingTime);
	}

	// Token: 0x040001FF RID: 511
	private const float m_playerMaxDistance = 15f;

	// Token: 0x04000200 RID: 512
	private const float m_tameDeltaTime = 3f;

	// Token: 0x04000201 RID: 513
	public float m_fedDuration = 30f;

	// Token: 0x04000202 RID: 514
	public float m_tamingTime = 1800f;

	// Token: 0x04000203 RID: 515
	public EffectList m_tamedEffect = new EffectList();

	// Token: 0x04000204 RID: 516
	public EffectList m_sootheEffect = new EffectList();

	// Token: 0x04000205 RID: 517
	public EffectList m_petEffect = new EffectList();

	// Token: 0x04000206 RID: 518
	public bool m_commandable;

	// Token: 0x04000207 RID: 519
	private Character m_character;

	// Token: 0x04000208 RID: 520
	private MonsterAI m_monsterAI;

	// Token: 0x04000209 RID: 521
	private ZNetView m_nview;

	// Token: 0x0400020A RID: 522
	private float m_lastPetTime;
}
