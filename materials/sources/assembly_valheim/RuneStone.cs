using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000F0 RID: 240
public class RuneStone : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06000EC6 RID: 3782 RVA: 0x0006982D File Offset: 0x00067A2D
	public string GetHoverText()
	{
		return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_rune_read");
	}

	// Token: 0x06000EC7 RID: 3783 RVA: 0x00069849 File Offset: 0x00067A49
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000EC8 RID: 3784 RVA: 0x00069854 File Offset: 0x00067A54
	public bool Interact(Humanoid character, bool hold)
	{
		if (hold)
		{
			return false;
		}
		Player player = character as Player;
		RuneStone.RandomRuneText randomText = this.GetRandomText();
		if (randomText != null)
		{
			if (randomText.m_label.Length > 0)
			{
				player.AddKnownText(randomText.m_label, randomText.m_text);
			}
			TextViewer.instance.ShowText(TextViewer.Style.Rune, randomText.m_topic, randomText.m_text, true);
		}
		else
		{
			if (this.m_label.Length > 0)
			{
				player.AddKnownText(this.m_label, this.m_text);
			}
			TextViewer.instance.ShowText(TextViewer.Style.Rune, this.m_topic, this.m_text, true);
		}
		return false;
	}

	// Token: 0x06000EC9 RID: 3785 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000ECA RID: 3786 RVA: 0x000698EC File Offset: 0x00067AEC
	private RuneStone.RandomRuneText GetRandomText()
	{
		if (this.m_randomTexts.Count == 0)
		{
			return null;
		}
		Vector3 position = base.transform.position;
		int seed = (int)position.x * (int)position.z;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(seed);
		RuneStone.RandomRuneText result = this.m_randomTexts[UnityEngine.Random.Range(0, this.m_randomTexts.Count)];
		UnityEngine.Random.state = state;
		return result;
	}

	// Token: 0x04000DB4 RID: 3508
	public string m_name = "Rune stone";

	// Token: 0x04000DB5 RID: 3509
	public string m_topic = "";

	// Token: 0x04000DB6 RID: 3510
	public string m_label = "";

	// Token: 0x04000DB7 RID: 3511
	public string m_text = "";

	// Token: 0x04000DB8 RID: 3512
	public List<RuneStone.RandomRuneText> m_randomTexts;

	// Token: 0x020001A9 RID: 425
	[Serializable]
	public class RandomRuneText
	{
		// Token: 0x04001301 RID: 4865
		public string m_topic = "";

		// Token: 0x04001302 RID: 4866
		public string m_label = "";

		// Token: 0x04001303 RID: 4867
		public string m_text = "";
	}
}
