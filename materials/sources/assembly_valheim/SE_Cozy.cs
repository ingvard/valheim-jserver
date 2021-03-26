using System;
using UnityEngine;

// Token: 0x02000024 RID: 36
public class SE_Cozy : SE_Stats
{
	// Token: 0x060003A1 RID: 929 RVA: 0x0001EFD1 File Offset: 0x0001D1D1
	public override void Setup(Character character)
	{
		base.Setup(character);
		this.m_character.Message(MessageHud.MessageType.Center, "$se_resting_start", 0, null);
	}

	// Token: 0x060003A2 RID: 930 RVA: 0x0001EFED File Offset: 0x0001D1ED
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
		if (this.m_time > this.m_delay)
		{
			this.m_character.GetSEMan().AddStatusEffect(this.m_statusEffect, true);
		}
	}

	// Token: 0x060003A3 RID: 931 RVA: 0x0001F01C File Offset: 0x0001D21C
	public override string GetIconText()
	{
		Player player = this.m_character as Player;
		return Localization.instance.Localize("$se_rested_comfort:" + player.GetComfortLevel());
	}

	// Token: 0x04000383 RID: 899
	[Header("__SE_Cozy__")]
	public float m_delay = 10f;

	// Token: 0x04000384 RID: 900
	public string m_statusEffect = "";

	// Token: 0x04000385 RID: 901
	private int m_comfortLevel;

	// Token: 0x04000386 RID: 902
	private float m_updateTimer;
}
