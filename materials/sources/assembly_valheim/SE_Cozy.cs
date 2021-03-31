using System;
using UnityEngine;

// Token: 0x02000024 RID: 36
public class SE_Cozy : SE_Stats
{
	// Token: 0x060003A2 RID: 930 RVA: 0x0001F085 File Offset: 0x0001D285
	public override void Setup(Character character)
	{
		base.Setup(character);
		this.m_character.Message(MessageHud.MessageType.Center, "$se_resting_start", 0, null);
	}

	// Token: 0x060003A3 RID: 931 RVA: 0x0001F0A1 File Offset: 0x0001D2A1
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
		if (this.m_time > this.m_delay)
		{
			this.m_character.GetSEMan().AddStatusEffect(this.m_statusEffect, true);
		}
	}

	// Token: 0x060003A4 RID: 932 RVA: 0x0001F0D0 File Offset: 0x0001D2D0
	public override string GetIconText()
	{
		Player player = this.m_character as Player;
		return Localization.instance.Localize("$se_rested_comfort:" + player.GetComfortLevel());
	}

	// Token: 0x04000387 RID: 903
	[Header("__SE_Cozy__")]
	public float m_delay = 10f;

	// Token: 0x04000388 RID: 904
	public string m_statusEffect = "";

	// Token: 0x04000389 RID: 905
	private int m_comfortLevel;

	// Token: 0x0400038A RID: 906
	private float m_updateTimer;
}
