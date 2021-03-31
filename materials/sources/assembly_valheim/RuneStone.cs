using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000F0 RID: 240
public class RuneStone : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06000EC7 RID: 3783 RVA: 0x000699B5 File Offset: 0x00067BB5
	public string GetHoverText()
	{
		return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_rune_read");
	}

	// Token: 0x06000EC8 RID: 3784 RVA: 0x000699D1 File Offset: 0x00067BD1
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000EC9 RID: 3785 RVA: 0x000699DC File Offset: 0x00067BDC
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

	// Token: 0x06000ECA RID: 3786 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000ECB RID: 3787 RVA: 0x00069A74 File Offset: 0x00067C74
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

	// Token: 0x04000DBA RID: 3514
	public string m_name = "Rune stone";

	// Token: 0x04000DBB RID: 3515
	public string m_topic = "";

	// Token: 0x04000DBC RID: 3516
	public string m_label = "";

	// Token: 0x04000DBD RID: 3517
	public string m_text = "";

	// Token: 0x04000DBE RID: 3518
	public List<RuneStone.RandomRuneText> m_randomTexts;

	// Token: 0x020001A9 RID: 425
	[Serializable]
	public class RandomRuneText
	{
		// Token: 0x04001308 RID: 4872
		public string m_topic = "";

		// Token: 0x04001309 RID: 4873
		public string m_label = "";

		// Token: 0x0400130A RID: 4874
		public string m_text = "";
	}
}
