using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200004D RID: 77
public class ConnectPanel : MonoBehaviour
{
	// Token: 0x17000005 RID: 5
	// (get) Token: 0x060004C1 RID: 1217 RVA: 0x0002647D File Offset: 0x0002467D
	public static ConnectPanel instance
	{
		get
		{
			return ConnectPanel.m_instance;
		}
	}

	// Token: 0x060004C2 RID: 1218 RVA: 0x00026484 File Offset: 0x00024684
	private void Start()
	{
		ConnectPanel.m_instance = this;
		this.m_root.gameObject.SetActive(false);
		this.m_playerListBaseSize = this.m_playerList.rect.height;
	}

	// Token: 0x060004C3 RID: 1219 RVA: 0x000264C1 File Offset: 0x000246C1
	public static bool IsVisible()
	{
		return ConnectPanel.m_instance && ConnectPanel.m_instance.m_root.gameObject.activeSelf;
	}

	// Token: 0x060004C4 RID: 1220 RVA: 0x000264E8 File Offset: 0x000246E8
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F2))
		{
			this.m_root.gameObject.SetActive(!this.m_root.gameObject.activeSelf);
		}
		if (this.m_root.gameObject.activeInHierarchy)
		{
			if (!ZNet.instance.IsServer() && ZNet.GetConnectionStatus() == ZNet.ConnectionStatus.Connected)
			{
				this.m_serverField.gameObject.SetActive(true);
				this.m_serverField.text = ZNet.GetServerString();
			}
			else
			{
				this.m_serverField.gameObject.SetActive(false);
			}
			this.m_worldField.text = ZNet.instance.GetWorldName();
			this.UpdateFps();
			this.m_myPort.gameObject.SetActive(ZNet.instance.IsServer());
			this.m_myPort.text = ZNet.instance.GetHostPort().ToString();
			this.m_myUID.text = ZNet.instance.GetUID().ToString();
			if (ZDOMan.instance != null)
			{
				this.m_zdos.text = ZDOMan.instance.NrOfObjects().ToString();
				float num;
				float num2;
				ZDOMan.instance.GetAverageStats(out num, out num2);
				this.m_zdosSent.text = num.ToString("0.0");
				this.m_zdosRecv.text = num2.ToString("0.0");
				this.m_activePeers.text = ZNet.instance.GetNrOfPlayers().ToString();
			}
			this.m_zdosPool.text = string.Concat(new object[]
			{
				ZDOPool.GetPoolActive(),
				" / ",
				ZDOPool.GetPoolSize(),
				" / ",
				ZDOPool.GetPoolTotal()
			});
			if (ZNetScene.instance)
			{
				this.m_zdosInstances.text = ZNetScene.instance.NrOfInstances().ToString();
			}
			float num3;
			float num4;
			int num5;
			float num6;
			float num7;
			ZNet.instance.GetNetStats(out num3, out num4, out num5, out num6, out num7);
			this.m_dataSent.text = (num6 / 1024f).ToString("0.0") + "kb/s";
			this.m_dataRecv.text = (num7 / 1024f).ToString("0.0") + "kb/s";
			this.m_ping.text = num5.ToString("0") + "ms";
			this.m_quality.text = ((int)(num3 * 100f)).ToString() + "% / " + ((int)(num4 * 100f)).ToString() + "%";
			this.m_clientSendQueue.text = ZDOMan.instance.GetClientChangeQueue().ToString();
			this.m_nrOfConnections.text = ZNet.instance.GetPeerConnections().ToString();
			string text = "";
			foreach (ZNetPeer znetPeer in ZNet.instance.GetConnectedPeers())
			{
				if (znetPeer.IsReady())
				{
					text = string.Concat(new object[]
					{
						text,
						znetPeer.m_socket.GetEndPointString(),
						" UID: ",
						znetPeer.m_uid,
						"\n"
					});
				}
				else
				{
					text = text + znetPeer.m_socket.GetEndPointString() + " connecting \n";
				}
			}
			this.m_connections.text = text;
			List<ZNet.PlayerInfo> playerList = ZNet.instance.GetPlayerList();
			float num8 = 16f;
			if (playerList.Count != this.m_playerListElements.Count)
			{
				foreach (GameObject obj in this.m_playerListElements)
				{
					UnityEngine.Object.Destroy(obj);
				}
				this.m_playerListElements.Clear();
				for (int i = 0; i < playerList.Count; i++)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_playerElement, this.m_playerList);
					(gameObject.transform as RectTransform).anchoredPosition = new Vector2(0f, (float)i * -num8);
					this.m_playerListElements.Add(gameObject);
				}
				float num9 = (float)playerList.Count * num8;
				num9 = Mathf.Max(this.m_playerListBaseSize, num9);
				this.m_playerList.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num9);
				this.m_playerListScroll.value = 1f;
			}
			for (int j = 0; j < playerList.Count; j++)
			{
				ZNet.PlayerInfo playerInfo = playerList[j];
				Text component = this.m_playerListElements[j].transform.Find("name").GetComponent<Text>();
				Text component2 = this.m_playerListElements[j].transform.Find("hostname").GetComponent<Text>();
				Component component3 = this.m_playerListElements[j].transform.Find("KickButton").GetComponent<Button>();
				component.text = playerInfo.m_name;
				component2.text = playerInfo.m_host;
				component3.gameObject.SetActive(false);
			}
			this.m_connectButton.interactable = this.ValidHost();
		}
	}

	// Token: 0x060004C5 RID: 1221 RVA: 0x00026A74 File Offset: 0x00024C74
	private void UpdateFps()
	{
		this.m_frameTimer += Time.deltaTime;
		this.m_frameSamples++;
		if (this.m_frameTimer > 1f)
		{
			float num = this.m_frameTimer / (float)this.m_frameSamples;
			this.m_fps.text = (1f / num).ToString("0.0");
			this.m_frameTime.text = "( " + (num * 1000f).ToString("00.0") + "ms )";
			this.m_frameSamples = 0;
			this.m_frameTimer = 0f;
		}
	}

	// Token: 0x060004C6 RID: 1222 RVA: 0x00026B1C File Offset: 0x00024D1C
	private bool ValidHost()
	{
		int num = 0;
		try
		{
			num = int.Parse(this.m_hostPort.text);
		}
		catch
		{
			return false;
		}
		return !string.IsNullOrEmpty(this.m_hostName.text) && num != 0;
	}

	// Token: 0x040004EF RID: 1263
	private static ConnectPanel m_instance;

	// Token: 0x040004F0 RID: 1264
	public Transform m_root;

	// Token: 0x040004F1 RID: 1265
	public Text m_serverField;

	// Token: 0x040004F2 RID: 1266
	public Text m_worldField;

	// Token: 0x040004F3 RID: 1267
	public Text m_statusField;

	// Token: 0x040004F4 RID: 1268
	public Text m_connections;

	// Token: 0x040004F5 RID: 1269
	public RectTransform m_playerList;

	// Token: 0x040004F6 RID: 1270
	public Scrollbar m_playerListScroll;

	// Token: 0x040004F7 RID: 1271
	public GameObject m_playerElement;

	// Token: 0x040004F8 RID: 1272
	public InputField m_hostName;

	// Token: 0x040004F9 RID: 1273
	public InputField m_hostPort;

	// Token: 0x040004FA RID: 1274
	public Button m_connectButton;

	// Token: 0x040004FB RID: 1275
	public Text m_myPort;

	// Token: 0x040004FC RID: 1276
	public Text m_myUID;

	// Token: 0x040004FD RID: 1277
	public Text m_knownHosts;

	// Token: 0x040004FE RID: 1278
	public Text m_nrOfConnections;

	// Token: 0x040004FF RID: 1279
	public Text m_pendingConnections;

	// Token: 0x04000500 RID: 1280
	public Toggle m_autoConnect;

	// Token: 0x04000501 RID: 1281
	public Text m_zdos;

	// Token: 0x04000502 RID: 1282
	public Text m_zdosPool;

	// Token: 0x04000503 RID: 1283
	public Text m_zdosSent;

	// Token: 0x04000504 RID: 1284
	public Text m_zdosRecv;

	// Token: 0x04000505 RID: 1285
	public Text m_zdosInstances;

	// Token: 0x04000506 RID: 1286
	public Text m_activePeers;

	// Token: 0x04000507 RID: 1287
	public Text m_ntp;

	// Token: 0x04000508 RID: 1288
	public Text m_upnp;

	// Token: 0x04000509 RID: 1289
	public Text m_dataSent;

	// Token: 0x0400050A RID: 1290
	public Text m_dataRecv;

	// Token: 0x0400050B RID: 1291
	public Text m_clientSendQueue;

	// Token: 0x0400050C RID: 1292
	public Text m_fps;

	// Token: 0x0400050D RID: 1293
	public Text m_frameTime;

	// Token: 0x0400050E RID: 1294
	public Text m_ping;

	// Token: 0x0400050F RID: 1295
	public Text m_quality;

	// Token: 0x04000510 RID: 1296
	private float m_playerListBaseSize;

	// Token: 0x04000511 RID: 1297
	private List<GameObject> m_playerListElements = new List<GameObject>();

	// Token: 0x04000512 RID: 1298
	private int m_frameSamples;

	// Token: 0x04000513 RID: 1299
	private float m_frameTimer;
}
