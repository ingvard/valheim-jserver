using System;
using UnityEngine;

// Token: 0x02000016 RID: 22
public class Talker : MonoBehaviour
{
	// Token: 0x06000282 RID: 642 RVA: 0x00014552 File Offset: 0x00012752
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_character = base.GetComponent<Character>();
		this.m_nview.Register<int, string, string>("Say", new Action<long, int, string, string>(this.RPC_Say));
	}

	// Token: 0x06000283 RID: 643 RVA: 0x00014588 File Offset: 0x00012788
	public void Say(Talker.Type type, string text)
	{
		ZLog.Log(string.Concat(new object[]
		{
			"Saying ",
			type,
			"  ",
			text
		}));
		this.m_nview.InvokeRPC(ZNetView.Everybody, "Say", new object[]
		{
			(int)type,
			Game.instance.GetPlayerProfile().GetName(),
			text
		});
	}

	// Token: 0x06000284 RID: 644 RVA: 0x000145FC File Offset: 0x000127FC
	private void RPC_Say(long sender, int ctype, string user, string text)
	{
		if (Player.m_localPlayer == null)
		{
			return;
		}
		float num = 0f;
		switch (ctype)
		{
		case 0:
			num = this.m_visperDistance;
			break;
		case 1:
			num = this.m_normalDistance;
			break;
		case 2:
			num = this.m_shoutDistance;
			break;
		}
		if (Vector3.Distance(base.transform.position, Player.m_localPlayer.transform.position) < num && Chat.instance)
		{
			Vector3 headPoint = this.m_character.GetHeadPoint();
			Chat.instance.OnNewChatMessage(base.gameObject, sender, headPoint, (Talker.Type)ctype, user, text);
		}
	}

	// Token: 0x040001F6 RID: 502
	public float m_visperDistance = 4f;

	// Token: 0x040001F7 RID: 503
	public float m_normalDistance = 15f;

	// Token: 0x040001F8 RID: 504
	public float m_shoutDistance = 70f;

	// Token: 0x040001F9 RID: 505
	private ZNetView m_nview;

	// Token: 0x040001FA RID: 506
	private Character m_character;

	// Token: 0x0200012E RID: 302
	public enum Type
	{
		// Token: 0x04001021 RID: 4129
		Whisper,
		// Token: 0x04001022 RID: 4130
		Normal,
		// Token: 0x04001023 RID: 4131
		Shout,
		// Token: 0x04001024 RID: 4132
		Ping
	}
}
