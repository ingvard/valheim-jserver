using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000059 RID: 89
public class MessageHud : MonoBehaviour
{
	// Token: 0x0600058D RID: 1421 RVA: 0x0002F7D2 File Offset: 0x0002D9D2
	private void Awake()
	{
		MessageHud.m_instance = this;
	}

	// Token: 0x0600058E RID: 1422 RVA: 0x0002F7DA File Offset: 0x0002D9DA
	private void OnDestroy()
	{
		MessageHud.m_instance = null;
	}

	// Token: 0x1700000D RID: 13
	// (get) Token: 0x0600058F RID: 1423 RVA: 0x0002F7E2 File Offset: 0x0002D9E2
	public static MessageHud instance
	{
		get
		{
			return MessageHud.m_instance;
		}
	}

	// Token: 0x06000590 RID: 1424 RVA: 0x0002F7EC File Offset: 0x0002D9EC
	private void Start()
	{
		this.m_messageText.canvasRenderer.SetAlpha(0f);
		this.m_messageIcon.canvasRenderer.SetAlpha(0f);
		this.m_messageCenterText.canvasRenderer.SetAlpha(0f);
		for (int i = 0; i < this.m_maxUnlockMessages; i++)
		{
			this.m_unlockMessages.Add(null);
		}
		ZRoutedRpc.instance.Register<int, string>("ShowMessage", new Action<long, int, string>(this.RPC_ShowMessage));
	}

	// Token: 0x06000591 RID: 1425 RVA: 0x0002F870 File Offset: 0x0002DA70
	private void Update()
	{
		if (Hud.IsUserHidden())
		{
			this.HideAll();
			return;
		}
		this.UpdateUnlockMsg(Time.deltaTime);
		this.UpdateMessage(Time.deltaTime);
		this.UpdateBiomeFound(Time.deltaTime);
	}

	// Token: 0x06000592 RID: 1426 RVA: 0x0002F8A4 File Offset: 0x0002DAA4
	private void HideAll()
	{
		for (int i = 0; i < this.m_maxUnlockMessages; i++)
		{
			if (this.m_unlockMessages[i] != null)
			{
				UnityEngine.Object.Destroy(this.m_unlockMessages[i]);
				this.m_unlockMessages[i] = null;
			}
		}
		this.m_messageText.canvasRenderer.SetAlpha(0f);
		this.m_messageIcon.canvasRenderer.SetAlpha(0f);
		this.m_messageCenterText.canvasRenderer.SetAlpha(0f);
		if (this.m_biomeMsgInstance)
		{
			UnityEngine.Object.Destroy(this.m_biomeMsgInstance);
			this.m_biomeMsgInstance = null;
		}
	}

	// Token: 0x06000593 RID: 1427 RVA: 0x0002F952 File Offset: 0x0002DB52
	public void MessageAll(MessageHud.MessageType type, string text)
	{
		ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ShowMessage", new object[]
		{
			(int)type,
			text
		});
	}

	// Token: 0x06000594 RID: 1428 RVA: 0x0002F97B File Offset: 0x0002DB7B
	private void RPC_ShowMessage(long sender, int type, string text)
	{
		this.ShowMessage((MessageHud.MessageType)type, text, 0, null);
	}

	// Token: 0x06000595 RID: 1429 RVA: 0x0002F988 File Offset: 0x0002DB88
	public void ShowMessage(MessageHud.MessageType type, string text, int amount = 0, Sprite icon = null)
	{
		if (Hud.IsUserHidden())
		{
			return;
		}
		text = Localization.instance.Localize(text);
		if (type == MessageHud.MessageType.TopLeft)
		{
			MessageHud.MsgData msgData = new MessageHud.MsgData();
			msgData.m_icon = icon;
			msgData.m_text = text;
			msgData.m_amount = amount;
			this.m_msgQeue.Enqueue(msgData);
			this.AddLog(text);
			return;
		}
		if (type != MessageHud.MessageType.Center)
		{
			return;
		}
		this.m_messageCenterText.text = text;
		this.m_messageCenterText.canvasRenderer.SetAlpha(1f);
		this.m_messageCenterText.CrossFadeAlpha(0f, 4f, true);
	}

	// Token: 0x06000596 RID: 1430 RVA: 0x0002FA1C File Offset: 0x0002DC1C
	private void UpdateMessage(float dt)
	{
		this.m_msgQueueTimer += dt;
		if (this.m_msgQeue.Count > 0)
		{
			MessageHud.MsgData msgData = this.m_msgQeue.Peek();
			bool flag = this.m_msgQueueTimer < 4f && msgData.m_text == this.currentMsg.m_text && msgData.m_icon == this.currentMsg.m_icon;
			if (this.m_msgQueueTimer >= 1f || flag)
			{
				MessageHud.MsgData msgData2 = this.m_msgQeue.Dequeue();
				this.m_messageText.text = msgData2.m_text;
				if (flag)
				{
					msgData2.m_amount += this.currentMsg.m_amount;
				}
				if (msgData2.m_amount > 1)
				{
					Text messageText = this.m_messageText;
					messageText.text = messageText.text + " x" + msgData2.m_amount;
				}
				this.m_messageText.canvasRenderer.SetAlpha(1f);
				this.m_messageText.CrossFadeAlpha(0f, 4f, true);
				if (msgData2.m_icon != null)
				{
					this.m_messageIcon.sprite = msgData2.m_icon;
					this.m_messageIcon.canvasRenderer.SetAlpha(1f);
					this.m_messageIcon.CrossFadeAlpha(0f, 4f, true);
				}
				else
				{
					this.m_messageIcon.canvasRenderer.SetAlpha(0f);
				}
				this.currentMsg = msgData2;
				this.m_msgQueueTimer = 0f;
			}
		}
	}

	// Token: 0x06000597 RID: 1431 RVA: 0x0002FBB0 File Offset: 0x0002DDB0
	private void UpdateBiomeFound(float dt)
	{
		if (this.m_biomeMsgInstance != null && this.m_biomeMsgInstance.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).IsTag("done"))
		{
			UnityEngine.Object.Destroy(this.m_biomeMsgInstance);
			this.m_biomeMsgInstance = null;
		}
		if (this.m_biomeFoundQueue.Count > 0 && this.m_biomeMsgInstance == null && this.m_msgQeue.Count == 0 && this.m_msgQueueTimer > 2f)
		{
			MessageHud.BiomeMessage biomeMessage = this.m_biomeFoundQueue.Dequeue();
			this.m_biomeMsgInstance = UnityEngine.Object.Instantiate<GameObject>(this.m_biomeFoundPrefab, base.transform);
			Text component = Utils.FindChild(this.m_biomeMsgInstance.transform, "Title").GetComponent<Text>();
			string text = Localization.instance.Localize(biomeMessage.m_text);
			component.text = text;
			if (biomeMessage.m_playStinger && this.m_biomeFoundStinger)
			{
				UnityEngine.Object.Instantiate<GameObject>(this.m_biomeFoundStinger);
			}
		}
	}

	// Token: 0x06000598 RID: 1432 RVA: 0x0002FCB4 File Offset: 0x0002DEB4
	public void ShowBiomeFoundMsg(string text, bool playStinger)
	{
		MessageHud.BiomeMessage biomeMessage = new MessageHud.BiomeMessage();
		biomeMessage.m_text = text;
		biomeMessage.m_playStinger = playStinger;
		this.m_biomeFoundQueue.Enqueue(biomeMessage);
	}

	// Token: 0x06000599 RID: 1433 RVA: 0x0002FCE4 File Offset: 0x0002DEE4
	public void QueueUnlockMsg(Sprite icon, string topic, string description)
	{
		MessageHud.UnlockMsg unlockMsg = new MessageHud.UnlockMsg();
		unlockMsg.m_icon = icon;
		unlockMsg.m_topic = Localization.instance.Localize(topic);
		unlockMsg.m_description = Localization.instance.Localize(description);
		this.m_unlockMsgQueue.Enqueue(unlockMsg);
		this.AddLog(topic + ":" + description);
		ZLog.Log("Queue unlock msg:" + topic + ":" + description);
	}

	// Token: 0x0600059A RID: 1434 RVA: 0x0002FD54 File Offset: 0x0002DF54
	private int GetFreeUnlockMsgSlot()
	{
		for (int i = 0; i < this.m_unlockMessages.Count; i++)
		{
			if (this.m_unlockMessages[i] == null)
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x0600059B RID: 1435 RVA: 0x0002FD90 File Offset: 0x0002DF90
	private void UpdateUnlockMsg(float dt)
	{
		for (int i = 0; i < this.m_unlockMessages.Count; i++)
		{
			GameObject gameObject = this.m_unlockMessages[i];
			if (!(gameObject == null) && gameObject.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).IsTag("done"))
			{
				UnityEngine.Object.Destroy(gameObject);
				this.m_unlockMessages[i] = null;
				break;
			}
		}
		if (this.m_unlockMsgQueue.Count > 0)
		{
			int freeUnlockMsgSlot = this.GetFreeUnlockMsgSlot();
			if (freeUnlockMsgSlot != -1)
			{
				Transform transform = base.transform;
				GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this.m_unlockMsgPrefab, transform);
				this.m_unlockMessages[freeUnlockMsgSlot] = gameObject2;
				RectTransform rectTransform = gameObject2.transform as RectTransform;
				Vector3 v = rectTransform.anchoredPosition;
				v.y -= (float)(this.m_maxUnlockMsgSpace * freeUnlockMsgSlot);
				rectTransform.anchoredPosition = v;
				MessageHud.UnlockMsg unlockMsg = this.m_unlockMsgQueue.Dequeue();
				Image component = rectTransform.Find("UnlockMessage/icon_bkg/UnlockIcon").GetComponent<Image>();
				Text component2 = rectTransform.Find("UnlockMessage/UnlockTitle").GetComponent<Text>();
				Text component3 = rectTransform.Find("UnlockMessage/UnlockDescription").GetComponent<Text>();
				component.sprite = unlockMsg.m_icon;
				component2.text = unlockMsg.m_topic;
				component3.text = unlockMsg.m_description;
			}
		}
	}

	// Token: 0x0600059C RID: 1436 RVA: 0x0002FEDF File Offset: 0x0002E0DF
	private void AddLog(string logText)
	{
		this.m_messageLog.Add(logText);
		while (this.m_messageLog.Count > this.m_maxLogMessages)
		{
			this.m_messageLog.RemoveAt(0);
		}
	}

	// Token: 0x0600059D RID: 1437 RVA: 0x0002FF0E File Offset: 0x0002E10E
	public List<string> GetLog()
	{
		return this.m_messageLog;
	}

	// Token: 0x04000631 RID: 1585
	private MessageHud.MsgData currentMsg = new MessageHud.MsgData();

	// Token: 0x04000632 RID: 1586
	private static MessageHud m_instance;

	// Token: 0x04000633 RID: 1587
	public Text m_messageText;

	// Token: 0x04000634 RID: 1588
	public Image m_messageIcon;

	// Token: 0x04000635 RID: 1589
	public Text m_messageCenterText;

	// Token: 0x04000636 RID: 1590
	public GameObject m_unlockMsgPrefab;

	// Token: 0x04000637 RID: 1591
	public int m_maxUnlockMsgSpace = 110;

	// Token: 0x04000638 RID: 1592
	public int m_maxUnlockMessages = 4;

	// Token: 0x04000639 RID: 1593
	public int m_maxLogMessages = 50;

	// Token: 0x0400063A RID: 1594
	public GameObject m_biomeFoundPrefab;

	// Token: 0x0400063B RID: 1595
	public GameObject m_biomeFoundStinger;

	// Token: 0x0400063C RID: 1596
	private Queue<MessageHud.BiomeMessage> m_biomeFoundQueue = new Queue<MessageHud.BiomeMessage>();

	// Token: 0x0400063D RID: 1597
	private List<string> m_messageLog = new List<string>();

	// Token: 0x0400063E RID: 1598
	private List<GameObject> m_unlockMessages = new List<GameObject>();

	// Token: 0x0400063F RID: 1599
	private Queue<MessageHud.UnlockMsg> m_unlockMsgQueue = new Queue<MessageHud.UnlockMsg>();

	// Token: 0x04000640 RID: 1600
	private Queue<MessageHud.MsgData> m_msgQeue = new Queue<MessageHud.MsgData>();

	// Token: 0x04000641 RID: 1601
	private float m_msgQueueTimer = -1f;

	// Token: 0x04000642 RID: 1602
	private GameObject m_biomeMsgInstance;

	// Token: 0x0200014F RID: 335
	public enum MessageType
	{
		// Token: 0x04001102 RID: 4354
		TopLeft = 1,
		// Token: 0x04001103 RID: 4355
		Center
	}

	// Token: 0x02000150 RID: 336
	private class UnlockMsg
	{
		// Token: 0x04001104 RID: 4356
		public Sprite m_icon;

		// Token: 0x04001105 RID: 4357
		public string m_topic;

		// Token: 0x04001106 RID: 4358
		public string m_description;
	}

	// Token: 0x02000151 RID: 337
	private class MsgData
	{
		// Token: 0x04001107 RID: 4359
		public Sprite m_icon;

		// Token: 0x04001108 RID: 4360
		public string m_text;

		// Token: 0x04001109 RID: 4361
		public int m_amount;
	}

	// Token: 0x02000152 RID: 338
	private class BiomeMessage
	{
		// Token: 0x0400110A RID: 4362
		public string m_text;

		// Token: 0x0400110B RID: 4363
		public bool m_playStinger;
	}
}
