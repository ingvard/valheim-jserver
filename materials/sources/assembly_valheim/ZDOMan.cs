using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

// Token: 0x02000079 RID: 121
public class ZDOMan
{
	// Token: 0x17000018 RID: 24
	// (get) Token: 0x060007B3 RID: 1971 RVA: 0x0003C7DA File Offset: 0x0003A9DA
	public static ZDOMan instance
	{
		get
		{
			return ZDOMan.m_instance;
		}
	}

	// Token: 0x060007B4 RID: 1972 RVA: 0x0003C7E4 File Offset: 0x0003A9E4
	public ZDOMan(int width)
	{
		ZDOMan.m_instance = this;
		this.m_myid = Utils.GenerateUID();
		ZRoutedRpc.instance.Register<ZPackage>("DestroyZDO", new Action<long, ZPackage>(this.RPC_DestroyZDO));
		ZRoutedRpc.instance.Register<ZDOID>("RequestZDO", new Action<long, ZDOID>(this.RPC_RequestZDO));
		this.m_width = width;
		this.m_halfWidth = this.m_width / 2;
		this.ResetSectorArray();
	}

	// Token: 0x060007B5 RID: 1973 RVA: 0x0003C8CE File Offset: 0x0003AACE
	private void ResetSectorArray()
	{
		this.m_objectsBySector = new List<ZDO>[this.m_width * this.m_width];
		this.m_objectsByOutsideSector.Clear();
	}

	// Token: 0x060007B6 RID: 1974 RVA: 0x0003C8F4 File Offset: 0x0003AAF4
	public void ShutDown()
	{
		if (!ZNet.instance.IsServer())
		{
			this.FlushClientObjects();
		}
		ZDOPool.Release(this.m_objectsByID);
		this.m_objectsByID.Clear();
		this.m_tempToSync.Clear();
		this.m_tempToSyncDistant.Clear();
		this.m_tempNearObjects.Clear();
		this.m_tempRemoveList.Clear();
		this.m_peers.Clear();
		this.ResetSectorArray();
		GC.Collect();
	}

	// Token: 0x060007B7 RID: 1975 RVA: 0x0003C96C File Offset: 0x0003AB6C
	public void PrepareSave()
	{
		this.m_saveData = new ZDOMan.SaveData();
		this.m_saveData.m_myid = this.m_myid;
		this.m_saveData.m_nextUid = this.m_nextUid;
		Stopwatch stopwatch = Stopwatch.StartNew();
		this.m_saveData.m_zdos = this.GetSaveClone();
		ZLog.Log("clone " + stopwatch.ElapsedMilliseconds);
		this.m_saveData.m_deadZDOs = new Dictionary<ZDOID, long>(this.m_deadZDOs);
	}

	// Token: 0x060007B8 RID: 1976 RVA: 0x0003C9F0 File Offset: 0x0003ABF0
	public void SaveAsync(BinaryWriter writer)
	{
		writer.Write(this.m_saveData.m_myid);
		writer.Write(this.m_saveData.m_nextUid);
		ZPackage zpackage = new ZPackage();
		writer.Write(this.m_saveData.m_zdos.Count);
		foreach (ZDO zdo in this.m_saveData.m_zdos)
		{
			writer.Write(zdo.m_uid.userID);
			writer.Write(zdo.m_uid.id);
			zpackage.Clear();
			zdo.Save(zpackage);
			byte[] array = zpackage.GetArray();
			writer.Write(array.Length);
			writer.Write(array);
		}
		writer.Write(this.m_saveData.m_deadZDOs.Count);
		foreach (KeyValuePair<ZDOID, long> keyValuePair in this.m_saveData.m_deadZDOs)
		{
			writer.Write(keyValuePair.Key.userID);
			writer.Write(keyValuePair.Key.id);
			writer.Write(keyValuePair.Value);
		}
		ZLog.Log("Saved " + this.m_saveData.m_zdos.Count + " zdos");
		this.m_saveData = null;
	}

	// Token: 0x060007B9 RID: 1977 RVA: 0x0003CB88 File Offset: 0x0003AD88
	public void Load(BinaryReader reader, int version)
	{
		reader.ReadInt64();
		uint num = reader.ReadUInt32();
		int num2 = reader.ReadInt32();
		ZDOPool.Release(this.m_objectsByID);
		this.m_objectsByID.Clear();
		this.ResetSectorArray();
		ZLog.Log(string.Concat(new object[]
		{
			"Loading ",
			num2,
			" zdos , my id ",
			this.m_myid,
			" data version:",
			version
		}));
		ZPackage zpackage = new ZPackage();
		for (int i = 0; i < num2; i++)
		{
			ZDO zdo = ZDOPool.Create(this);
			zdo.m_uid = new ZDOID(reader);
			int count = reader.ReadInt32();
			byte[] data = reader.ReadBytes(count);
			zpackage.Load(data);
			zdo.Load(zpackage, version);
			zdo.SetOwner(0L);
			this.m_objectsByID.Add(zdo.m_uid, zdo);
			this.AddToSector(zdo, zdo.GetSector());
			if (zdo.m_uid.userID == this.m_myid && zdo.m_uid.id >= num)
			{
				num = zdo.m_uid.id + 1U;
			}
		}
		this.m_deadZDOs.Clear();
		int num3 = reader.ReadInt32();
		for (int j = 0; j < num3; j++)
		{
			ZDOID key = new ZDOID(reader.ReadInt64(), reader.ReadUInt32());
			long value = reader.ReadInt64();
			this.m_deadZDOs.Add(key, value);
			if (key.userID == this.m_myid && key.id >= num)
			{
				num = key.id + 1U;
			}
		}
		this.CapDeadZDOList();
		ZLog.Log("Loaded " + this.m_deadZDOs.Count + " dead zdos");
		this.RemoveOldGeneratedZDOS();
		this.m_nextUid = num;
	}

	// Token: 0x060007BA RID: 1978 RVA: 0x0003CD68 File Offset: 0x0003AF68
	private void RemoveOldGeneratedZDOS()
	{
		List<ZDOID> list = new List<ZDOID>();
		foreach (KeyValuePair<ZDOID, ZDO> keyValuePair in this.m_objectsByID)
		{
			int pgwversion = keyValuePair.Value.GetPGWVersion();
			if (pgwversion != 0 && pgwversion != ZoneSystem.instance.m_pgwVersion)
			{
				list.Add(keyValuePair.Key);
				this.RemoveFromSector(keyValuePair.Value, keyValuePair.Value.GetSector());
				ZDOPool.Release(keyValuePair.Value);
			}
		}
		foreach (ZDOID key in list)
		{
			this.m_objectsByID.Remove(key);
		}
		ZLog.Log("Removed " + list.Count + " OLD generated ZDOS");
	}

	// Token: 0x060007BB RID: 1979 RVA: 0x0003CE70 File Offset: 0x0003B070
	private void CapDeadZDOList()
	{
		if (this.m_deadZDOs.Count > 100000)
		{
			List<KeyValuePair<ZDOID, long>> list = this.m_deadZDOs.ToList<KeyValuePair<ZDOID, long>>();
			list.Sort((KeyValuePair<ZDOID, long> a, KeyValuePair<ZDOID, long> b) => a.Value.CompareTo(b.Value));
			int num = list.Count - 100000;
			for (int i = 0; i < num; i++)
			{
				this.m_deadZDOs.Remove(list[i].Key);
			}
		}
	}

	// Token: 0x060007BC RID: 1980 RVA: 0x0003CEF4 File Offset: 0x0003B0F4
	public ZDO CreateNewZDO(Vector3 position)
	{
		long myid = this.m_myid;
		uint nextUid = this.m_nextUid;
		this.m_nextUid = nextUid + 1U;
		ZDOID zdoid = new ZDOID(myid, nextUid);
		while (this.GetZDO(zdoid) != null)
		{
			long myid2 = this.m_myid;
			nextUid = this.m_nextUid;
			this.m_nextUid = nextUid + 1U;
			zdoid = new ZDOID(myid2, nextUid);
		}
		return this.CreateNewZDO(zdoid, position);
	}

	// Token: 0x060007BD RID: 1981 RVA: 0x0003CF50 File Offset: 0x0003B150
	public ZDO CreateNewZDO(ZDOID uid, Vector3 position)
	{
		ZDO zdo = ZDOPool.Create(this, uid, position);
		zdo.m_owner = this.m_myid;
		zdo.m_timeCreated = ZNet.instance.GetTime().Ticks;
		this.m_objectsByID.Add(uid, zdo);
		return zdo;
	}

	// Token: 0x060007BE RID: 1982 RVA: 0x0003CF98 File Offset: 0x0003B198
	public void AddToSector(ZDO zdo, Vector2i sector)
	{
		int num = this.SectorToIndex(sector);
		if (num >= 0)
		{
			if (this.m_objectsBySector[num] != null)
			{
				this.m_objectsBySector[num].Add(zdo);
				return;
			}
			List<ZDO> list = new List<ZDO>();
			list.Add(zdo);
			this.m_objectsBySector[num] = list;
			return;
		}
		else
		{
			List<ZDO> list2;
			if (this.m_objectsByOutsideSector.TryGetValue(sector, out list2))
			{
				list2.Add(zdo);
				return;
			}
			list2 = new List<ZDO>();
			list2.Add(zdo);
			this.m_objectsByOutsideSector.Add(sector, list2);
			return;
		}
	}

	// Token: 0x060007BF RID: 1983 RVA: 0x0003D014 File Offset: 0x0003B214
	public void ZDOSectorInvalidated(ZDO zdo)
	{
		foreach (ZDOMan.ZDOPeer zdopeer in this.m_peers)
		{
			zdopeer.ZDOSectorInvalidated(zdo);
		}
	}

	// Token: 0x060007C0 RID: 1984 RVA: 0x0003D068 File Offset: 0x0003B268
	public void RemoveFromSector(ZDO zdo, Vector2i sector)
	{
		int num = this.SectorToIndex(sector);
		List<ZDO> list;
		if (num >= 0)
		{
			if (this.m_objectsBySector[num] != null)
			{
				this.m_objectsBySector[num].Remove(zdo);
				return;
			}
		}
		else if (this.m_objectsByOutsideSector.TryGetValue(sector, out list))
		{
			list.Remove(zdo);
		}
	}

	// Token: 0x060007C1 RID: 1985 RVA: 0x0003D0B4 File Offset: 0x0003B2B4
	public ZDO GetZDO(ZDOID id)
	{
		if (id == ZDOID.None)
		{
			return null;
		}
		ZDO result;
		if (this.m_objectsByID.TryGetValue(id, out result))
		{
			return result;
		}
		return null;
	}

	// Token: 0x060007C2 RID: 1986 RVA: 0x0003D0E4 File Offset: 0x0003B2E4
	public void AddPeer(ZNetPeer netPeer)
	{
		ZDOMan.ZDOPeer zdopeer = new ZDOMan.ZDOPeer();
		zdopeer.m_peer = netPeer;
		this.m_peers.Add(zdopeer);
		zdopeer.m_peer.m_rpc.Register<ZPackage>("ZDOData", new Action<ZRpc, ZPackage>(this.RPC_ZDOData));
	}

	// Token: 0x060007C3 RID: 1987 RVA: 0x0003D12C File Offset: 0x0003B32C
	public void RemovePeer(ZNetPeer netPeer)
	{
		ZDOMan.ZDOPeer zdopeer = this.FindPeer(netPeer);
		if (zdopeer != null)
		{
			this.m_peers.Remove(zdopeer);
			if (ZNet.instance.IsServer())
			{
				this.RemoveOrphanNonPersistentZDOS();
			}
		}
	}

	// Token: 0x060007C4 RID: 1988 RVA: 0x0003D164 File Offset: 0x0003B364
	private ZDOMan.ZDOPeer FindPeer(ZNetPeer netPeer)
	{
		foreach (ZDOMan.ZDOPeer zdopeer in this.m_peers)
		{
			if (zdopeer.m_peer == netPeer)
			{
				return zdopeer;
			}
		}
		return null;
	}

	// Token: 0x060007C5 RID: 1989 RVA: 0x0003D1C0 File Offset: 0x0003B3C0
	private ZDOMan.ZDOPeer FindPeer(ZRpc rpc)
	{
		foreach (ZDOMan.ZDOPeer zdopeer in this.m_peers)
		{
			if (zdopeer.m_peer.m_rpc == rpc)
			{
				return zdopeer;
			}
		}
		return null;
	}

	// Token: 0x060007C6 RID: 1990 RVA: 0x0003D224 File Offset: 0x0003B424
	public void Update(float dt)
	{
		if (ZNet.instance.IsServer())
		{
			this.ReleaseZDOS(dt);
		}
		this.SendZDOToPeers(dt);
		this.SendDestroyed();
		this.UpdateStats(dt);
	}

	// Token: 0x060007C7 RID: 1991 RVA: 0x0003D250 File Offset: 0x0003B450
	private void UpdateStats(float dt)
	{
		this.m_statTimer += dt;
		if (this.m_statTimer >= 1f)
		{
			this.m_statTimer = 0f;
			this.m_zdosSentLastSec = this.m_zdosSent;
			this.m_zdosRecvLastSec = this.m_zdosRecv;
			this.m_zdosRecv = 0;
			this.m_zdosSent = 0;
		}
	}

	// Token: 0x060007C8 RID: 1992 RVA: 0x0003D2AC File Offset: 0x0003B4AC
	private void SendZDOToPeers(float dt)
	{
		this.m_sendTimer += dt;
		if (this.m_sendTimer > 0.05f)
		{
			this.m_sendTimer = 0f;
			foreach (ZDOMan.ZDOPeer peer in this.m_peers)
			{
				this.SendZDOs(peer, false);
			}
		}
	}

	// Token: 0x060007C9 RID: 1993 RVA: 0x0003D328 File Offset: 0x0003B528
	private void FlushClientObjects()
	{
		foreach (ZDOMan.ZDOPeer peer in this.m_peers)
		{
			this.SendAllZDOs(peer);
		}
	}

	// Token: 0x060007CA RID: 1994 RVA: 0x0003D37C File Offset: 0x0003B57C
	private void ReleaseZDOS(float dt)
	{
		this.m_releaseZDOTimer += dt;
		if (this.m_releaseZDOTimer > 2f)
		{
			this.m_releaseZDOTimer = 0f;
			this.ReleaseNearbyZDOS(ZNet.instance.GetReferencePosition(), this.m_myid);
			foreach (ZDOMan.ZDOPeer zdopeer in this.m_peers)
			{
				this.ReleaseNearbyZDOS(zdopeer.m_peer.m_refPos, zdopeer.m_peer.m_uid);
			}
		}
	}

	// Token: 0x060007CB RID: 1995 RVA: 0x0003D420 File Offset: 0x0003B620
	private bool IsInPeerActiveArea(Vector2i sector, long uid)
	{
		if (uid == this.m_myid)
		{
			return ZNetScene.instance.InActiveArea(sector, ZNet.instance.GetReferencePosition());
		}
		ZNetPeer peer = ZNet.instance.GetPeer(uid);
		return peer != null && ZNetScene.instance.InActiveArea(sector, peer.GetRefPos());
	}

	// Token: 0x060007CC RID: 1996 RVA: 0x0003D470 File Offset: 0x0003B670
	private void ReleaseNearbyZDOS(Vector3 refPosition, long uid)
	{
		Vector2i zone = ZoneSystem.instance.GetZone(refPosition);
		this.m_tempNearObjects.Clear();
		this.FindSectorObjects(zone, ZoneSystem.instance.m_activeArea, 0, this.m_tempNearObjects, null);
		foreach (ZDO zdo in this.m_tempNearObjects)
		{
			if (zdo.m_persistent)
			{
				if (zdo.m_owner == uid)
				{
					if (!ZNetScene.instance.InActiveArea(zdo.GetSector(), zone))
					{
						zdo.SetOwner(0L);
					}
				}
				else if ((zdo.m_owner == 0L || !this.IsInPeerActiveArea(zdo.GetSector(), zdo.m_owner)) && ZNetScene.instance.InActiveArea(zdo.GetSector(), zone))
				{
					zdo.SetOwner(uid);
				}
			}
		}
	}

	// Token: 0x060007CD RID: 1997 RVA: 0x0003D550 File Offset: 0x0003B750
	public void DestroyZDO(ZDO zdo)
	{
		if (!zdo.IsOwner())
		{
			return;
		}
		this.m_destroySendList.Add(zdo.m_uid);
	}

	// Token: 0x060007CE RID: 1998 RVA: 0x0003D56C File Offset: 0x0003B76C
	private void SendDestroyed()
	{
		if (this.m_destroySendList.Count == 0)
		{
			return;
		}
		ZPackage zpackage = new ZPackage();
		zpackage.Write(this.m_destroySendList.Count);
		foreach (ZDOID id in this.m_destroySendList)
		{
			zpackage.Write(id);
		}
		this.m_destroySendList.Clear();
		ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "DestroyZDO", new object[]
		{
			zpackage
		});
	}

	// Token: 0x060007CF RID: 1999 RVA: 0x0003D610 File Offset: 0x0003B810
	private void RPC_DestroyZDO(long sender, ZPackage pkg)
	{
		int num = pkg.ReadInt();
		for (int i = 0; i < num; i++)
		{
			ZDOID uid = pkg.ReadZDOID();
			this.HandleDestroyedZDO(uid);
		}
	}

	// Token: 0x060007D0 RID: 2000 RVA: 0x0003D640 File Offset: 0x0003B840
	private void HandleDestroyedZDO(ZDOID uid)
	{
		if (uid.userID == this.m_myid && uid.id >= this.m_nextUid)
		{
			this.m_nextUid = uid.id + 1U;
		}
		ZDO zdo = this.GetZDO(uid);
		if (zdo == null)
		{
			return;
		}
		if (this.m_onZDODestroyed != null)
		{
			this.m_onZDODestroyed(zdo);
		}
		this.RemoveFromSector(zdo, zdo.GetSector());
		this.m_objectsByID.Remove(zdo.m_uid);
		ZDOPool.Release(zdo);
		foreach (ZDOMan.ZDOPeer zdopeer in this.m_peers)
		{
			zdopeer.m_zdos.Remove(uid);
		}
		if (ZNet.instance.IsServer())
		{
			long ticks = ZNet.instance.GetTime().Ticks;
			this.m_deadZDOs[uid] = ticks;
		}
	}

	// Token: 0x060007D1 RID: 2001 RVA: 0x0003D738 File Offset: 0x0003B938
	private void SendAllZDOs(ZDOMan.ZDOPeer peer)
	{
		while (this.SendZDOs(peer, true))
		{
		}
	}

	// Token: 0x060007D2 RID: 2002 RVA: 0x0003D744 File Offset: 0x0003B944
	private bool SendZDOs(ZDOMan.ZDOPeer peer, bool flush)
	{
		int sendQueueSize = peer.m_peer.m_socket.GetSendQueueSize();
		if (!flush && sendQueueSize > 10240)
		{
			return false;
		}
		int num = 10240 - sendQueueSize;
		if (num < 2048)
		{
			return false;
		}
		this.m_tempToSync.Clear();
		this.CreateSyncList(peer, this.m_tempToSync);
		if (this.m_tempToSync.Count == 0 && peer.m_invalidSector.Count == 0)
		{
			return false;
		}
		ZPackage zpackage = new ZPackage();
		bool flag = false;
		if (peer.m_invalidSector.Count > 0)
		{
			flag = true;
			zpackage.Write(peer.m_invalidSector.Count);
			foreach (ZDOID id in peer.m_invalidSector)
			{
				zpackage.Write(id);
			}
			peer.m_invalidSector.Clear();
		}
		else
		{
			zpackage.Write(0);
		}
		float time = Time.time;
		ZPackage zpackage2 = new ZPackage();
		bool flag2 = false;
		int num2 = 0;
		while (num2 < this.m_tempToSync.Count && zpackage.Size() <= num)
		{
			ZDO zdo = this.m_tempToSync[num2];
			peer.m_forceSend.Remove(zdo.m_uid);
			if (!ZNet.instance.IsServer())
			{
				this.m_clientChangeQueue.Remove(zdo.m_uid);
			}
			if (peer.ShouldSend(zdo))
			{
				zpackage.Write(zdo.m_uid);
				zpackage.Write(zdo.m_ownerRevision);
				zpackage.Write(zdo.m_dataRevision);
				zpackage.Write(zdo.m_owner);
				zpackage.Write(zdo.GetPosition());
				zpackage2.Clear();
				zdo.Serialize(zpackage2);
				zpackage.Write(zpackage2);
				peer.m_zdos[zdo.m_uid] = new ZDOMan.ZDOPeer.PeerZDOInfo(zdo.m_dataRevision, zdo.m_ownerRevision, time);
				flag2 = true;
				this.m_zdosSent++;
			}
			num2++;
		}
		zpackage.Write(ZDOID.None);
		if (flag2 || flag)
		{
			peer.m_peer.m_rpc.Invoke("ZDOData", new object[]
			{
				zpackage
			});
		}
		return flag2 || flag;
	}

	// Token: 0x060007D3 RID: 2003 RVA: 0x0003D98C File Offset: 0x0003BB8C
	private void RPC_ZDOData(ZRpc rpc, ZPackage pkg)
	{
		ZDOMan.ZDOPeer zdopeer = this.FindPeer(rpc);
		if (zdopeer == null)
		{
			ZLog.Log("ZDO data from unkown host, ignoring");
			return;
		}
		float time = Time.time;
		int num = 0;
		ZPackage pkg2 = new ZPackage();
		int num2 = pkg.ReadInt();
		for (int i = 0; i < num2; i++)
		{
			ZDOID id = pkg.ReadZDOID();
			ZDO zdo = this.GetZDO(id);
			if (zdo != null)
			{
				zdo.InvalidateSector();
			}
		}
		for (;;)
		{
			ZDOID zdoid = pkg.ReadZDOID();
			if (zdoid.IsNone())
			{
				break;
			}
			num++;
			uint num3 = pkg.ReadUInt();
			uint num4 = pkg.ReadUInt();
			long owner = pkg.ReadLong();
			Vector3 vector = pkg.ReadVector3();
			pkg.ReadPackage(ref pkg2);
			ZDO zdo2 = this.GetZDO(zdoid);
			bool flag = false;
			if (zdo2 != null)
			{
				if (num4 <= zdo2.m_dataRevision)
				{
					if (num3 > zdo2.m_ownerRevision)
					{
						zdo2.m_owner = owner;
						zdo2.m_ownerRevision = num3;
						zdopeer.m_zdos[zdoid] = new ZDOMan.ZDOPeer.PeerZDOInfo(num4, num3, time);
						continue;
					}
					continue;
				}
			}
			else
			{
				zdo2 = this.CreateNewZDO(zdoid, vector);
				flag = true;
			}
			zdo2.m_ownerRevision = num3;
			zdo2.m_dataRevision = num4;
			zdo2.m_owner = owner;
			zdo2.InternalSetPosition(vector);
			zdopeer.m_zdos[zdoid] = new ZDOMan.ZDOPeer.PeerZDOInfo(zdo2.m_dataRevision, zdo2.m_ownerRevision, time);
			zdo2.Deserialize(pkg2);
			if (ZNet.instance.IsServer() && flag && this.m_deadZDOs.ContainsKey(zdoid))
			{
				zdo2.SetOwner(this.m_myid);
				this.DestroyZDO(zdo2);
			}
		}
		this.m_zdosRecv += num;
	}

	// Token: 0x060007D4 RID: 2004 RVA: 0x0003DB34 File Offset: 0x0003BD34
	public void FindSectorObjects(Vector2i sector, int area, int distantArea, List<ZDO> sectorObjects, List<ZDO> distantSectorObjects = null)
	{
		this.FindObjects(sector, sectorObjects);
		for (int i = 1; i <= area; i++)
		{
			for (int j = sector.x - i; j <= sector.x + i; j++)
			{
				this.FindObjects(new Vector2i(j, sector.y - i), sectorObjects);
				this.FindObjects(new Vector2i(j, sector.y + i), sectorObjects);
			}
			for (int k = sector.y - i + 1; k <= sector.y + i - 1; k++)
			{
				this.FindObjects(new Vector2i(sector.x - i, k), sectorObjects);
				this.FindObjects(new Vector2i(sector.x + i, k), sectorObjects);
			}
		}
		List<ZDO> objects = (distantSectorObjects != null) ? distantSectorObjects : sectorObjects;
		for (int l = area + 1; l <= area + distantArea; l++)
		{
			for (int m = sector.x - l; m <= sector.x + l; m++)
			{
				this.FindDistantObjects(new Vector2i(m, sector.y - l), objects);
				this.FindDistantObjects(new Vector2i(m, sector.y + l), objects);
			}
			for (int n = sector.y - l + 1; n <= sector.y + l - 1; n++)
			{
				this.FindDistantObjects(new Vector2i(sector.x - l, n), objects);
				this.FindDistantObjects(new Vector2i(sector.x + l, n), objects);
			}
		}
	}

	// Token: 0x060007D5 RID: 2005 RVA: 0x0003DCB4 File Offset: 0x0003BEB4
	public void FindSectorObjects(Vector2i sector, int area, List<ZDO> sectorObjects)
	{
		for (int i = sector.y - area; i <= sector.y + area; i++)
		{
			for (int j = sector.x - area; j <= sector.x + area; j++)
			{
				Vector2i sector2 = new Vector2i(j, i);
				this.FindObjects(sector2, sectorObjects);
			}
		}
	}

	// Token: 0x060007D6 RID: 2006 RVA: 0x0003DD08 File Offset: 0x0003BF08
	private void CreateSyncList(ZDOMan.ZDOPeer peer, List<ZDO> toSync)
	{
		if (ZNet.instance.IsServer())
		{
			Vector3 refPos = peer.m_peer.GetRefPos();
			Vector2i zone = ZoneSystem.instance.GetZone(refPos);
			this.m_tempToSyncDistant.Clear();
			this.FindSectorObjects(zone, ZoneSystem.instance.m_activeArea, ZoneSystem.instance.m_activeDistantArea, toSync, this.m_tempToSyncDistant);
			this.ServerSortSendZDOS(toSync, refPos, peer);
			toSync.AddRange(this.m_tempToSyncDistant);
			this.AddForceSendZdos(peer, toSync);
			return;
		}
		this.m_tempRemoveList.Clear();
		foreach (ZDOID zdoid in this.m_clientChangeQueue)
		{
			ZDO zdo = this.GetZDO(zdoid);
			if (zdo != null)
			{
				toSync.Add(zdo);
			}
			else
			{
				this.m_tempRemoveList.Add(zdoid);
			}
		}
		foreach (ZDOID item in this.m_tempRemoveList)
		{
			this.m_clientChangeQueue.Remove(item);
		}
		this.ClientSortSendZDOS(toSync, peer);
		this.AddForceSendZdos(peer, toSync);
	}

	// Token: 0x060007D7 RID: 2007 RVA: 0x0003DE4C File Offset: 0x0003C04C
	private void AddForceSendZdos(ZDOMan.ZDOPeer peer, List<ZDO> syncList)
	{
		if (peer.m_forceSend.Count > 0)
		{
			this.m_tempRemoveList.Clear();
			foreach (ZDOID zdoid in peer.m_forceSend)
			{
				ZDO zdo = this.GetZDO(zdoid);
				if (zdo != null)
				{
					syncList.Insert(0, zdo);
				}
				else
				{
					this.m_tempRemoveList.Add(zdoid);
				}
			}
			foreach (ZDOID item in this.m_tempRemoveList)
			{
				peer.m_forceSend.Remove(item);
			}
		}
	}

	// Token: 0x060007D8 RID: 2008 RVA: 0x0003DF20 File Offset: 0x0003C120
	private static int ServerSendCompare(ZDO x, ZDO y)
	{
		bool flag = x.m_owner != ZDOMan.compareReceiver;
		bool flag2 = y.m_owner != ZDOMan.compareReceiver;
		if (flag && flag2)
		{
			if (x.m_type == y.m_type)
			{
				return x.m_tempSortValue.CompareTo(y.m_tempSortValue);
			}
			if (x.m_type == ZDO.ObjectType.Prioritized)
			{
				return -1;
			}
			if (y.m_type == ZDO.ObjectType.Prioritized)
			{
				return 1;
			}
			return x.m_tempSortValue.CompareTo(y.m_tempSortValue);
		}
		else
		{
			if (flag != flag2)
			{
				if (flag && x.m_type == ZDO.ObjectType.Prioritized)
				{
					return -1;
				}
				if (flag2 && y.m_type == ZDO.ObjectType.Prioritized)
				{
					return 1;
				}
			}
			if (x.m_type == y.m_type)
			{
				return x.m_tempSortValue.CompareTo(y.m_tempSortValue);
			}
			if (x.m_type == ZDO.ObjectType.Solid)
			{
				return -1;
			}
			if (y.m_type == ZDO.ObjectType.Solid)
			{
				return 1;
			}
			if (x.m_type == ZDO.ObjectType.Prioritized)
			{
				return -1;
			}
			if (y.m_type == ZDO.ObjectType.Prioritized)
			{
				return 1;
			}
			return x.m_tempSortValue.CompareTo(y.m_tempSortValue);
		}
	}

	// Token: 0x060007D9 RID: 2009 RVA: 0x0003E01C File Offset: 0x0003C21C
	private void ServerSortSendZDOS(List<ZDO> objects, Vector3 refPos, ZDOMan.ZDOPeer peer)
	{
		float time = Time.time;
		for (int i = 0; i < objects.Count; i++)
		{
			ZDO zdo = objects[i];
			Vector3 position = zdo.GetPosition();
			zdo.m_tempSortValue = Vector3.Distance(position, refPos);
			float num = 100f;
			ZDOMan.ZDOPeer.PeerZDOInfo peerZDOInfo;
			if (peer.m_zdos.TryGetValue(zdo.m_uid, out peerZDOInfo))
			{
				num = Mathf.Clamp(time - peerZDOInfo.m_syncTime, 0f, 100f);
				zdo.m_tempHaveRevision = true;
			}
			else
			{
				zdo.m_tempHaveRevision = false;
			}
			zdo.m_tempSortValue -= num * 1.5f;
		}
		ZDOMan.compareReceiver = peer.m_peer.m_uid;
		objects.Sort(new Comparison<ZDO>(ZDOMan.ServerSendCompare));
	}

	// Token: 0x060007DA RID: 2010 RVA: 0x0003E0E0 File Offset: 0x0003C2E0
	private static int ClientSendCompare(ZDO x, ZDO y)
	{
		if (x.m_type == y.m_type)
		{
			return x.m_tempSortValue.CompareTo(y.m_tempSortValue);
		}
		if (x.m_type == ZDO.ObjectType.Prioritized)
		{
			return -1;
		}
		if (y.m_type == ZDO.ObjectType.Prioritized)
		{
			return 1;
		}
		return x.m_tempSortValue.CompareTo(y.m_tempSortValue);
	}

	// Token: 0x060007DB RID: 2011 RVA: 0x0003E134 File Offset: 0x0003C334
	private void ClientSortSendZDOS(List<ZDO> objects, ZDOMan.ZDOPeer peer)
	{
		float time = Time.time;
		for (int i = 0; i < objects.Count; i++)
		{
			ZDO zdo = objects[i];
			zdo.m_tempSortValue = 0f;
			float num = 100f;
			ZDOMan.ZDOPeer.PeerZDOInfo peerZDOInfo;
			if (peer.m_zdos.TryGetValue(zdo.m_uid, out peerZDOInfo))
			{
				num = Mathf.Clamp(time - peerZDOInfo.m_syncTime, 0f, 100f);
			}
			zdo.m_tempSortValue -= num * 1.5f;
		}
		objects.Sort(new Comparison<ZDO>(ZDOMan.ClientSendCompare));
	}

	// Token: 0x060007DC RID: 2012 RVA: 0x0003E1C8 File Offset: 0x0003C3C8
	private void PrintZdoList(List<ZDO> zdos)
	{
		ZLog.Log("Sync list " + zdos.Count);
		foreach (ZDO zdo in zdos)
		{
			string text = "";
			int prefab = zdo.GetPrefab();
			if (prefab != 0)
			{
				GameObject prefab2 = ZNetScene.instance.GetPrefab(prefab);
				if (prefab2)
				{
					text = prefab2.name;
				}
			}
			ZLog.Log(string.Concat(new object[]
			{
				"  ",
				zdo.m_uid.ToString(),
				"  ",
				zdo.m_ownerRevision,
				" prefab:",
				text
			}));
		}
	}

	// Token: 0x060007DD RID: 2013 RVA: 0x0003E2AC File Offset: 0x0003C4AC
	private void AddDistantObjects(ZDOMan.ZDOPeer peer, int maxItems, List<ZDO> toSync)
	{
		if (peer.m_sendIndex >= this.m_objectsByID.Count)
		{
			peer.m_sendIndex = 0;
		}
		IEnumerable<KeyValuePair<ZDOID, ZDO>> enumerable = this.m_objectsByID.Skip(peer.m_sendIndex).Take(maxItems);
		peer.m_sendIndex += maxItems;
		foreach (KeyValuePair<ZDOID, ZDO> keyValuePair in enumerable)
		{
			toSync.Add(keyValuePair.Value);
		}
	}

	// Token: 0x060007DE RID: 2014 RVA: 0x0003E338 File Offset: 0x0003C538
	public long GetMyID()
	{
		return this.m_myid;
	}

	// Token: 0x060007DF RID: 2015 RVA: 0x0003E340 File Offset: 0x0003C540
	private int SectorToIndex(Vector2i s)
	{
		int num = s.x + this.m_halfWidth;
		int num2 = s.y + this.m_halfWidth;
		if (num < 0 || num2 < 0 || num >= this.m_width || num2 >= this.m_width)
		{
			return -1;
		}
		return num2 * this.m_width + num;
	}

	// Token: 0x060007E0 RID: 2016 RVA: 0x0003E390 File Offset: 0x0003C590
	private void FindObjects(Vector2i sector, List<ZDO> objects)
	{
		int num = this.SectorToIndex(sector);
		List<ZDO> collection;
		if (num >= 0)
		{
			if (this.m_objectsBySector[num] != null)
			{
				objects.AddRange(this.m_objectsBySector[num]);
				return;
			}
		}
		else if (this.m_objectsByOutsideSector.TryGetValue(sector, out collection))
		{
			objects.AddRange(collection);
		}
	}

	// Token: 0x060007E1 RID: 2017 RVA: 0x0003E3DC File Offset: 0x0003C5DC
	private void FindDistantObjects(Vector2i sector, List<ZDO> objects)
	{
		int num = this.SectorToIndex(sector);
		List<ZDO> list2;
		if (num >= 0)
		{
			List<ZDO> list = this.m_objectsBySector[num];
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					ZDO zdo = list[i];
					if (zdo.m_distant)
					{
						objects.Add(zdo);
					}
				}
				return;
			}
		}
		else if (this.m_objectsByOutsideSector.TryGetValue(sector, out list2))
		{
			for (int j = 0; j < list2.Count; j++)
			{
				ZDO zdo2 = list2[j];
				if (zdo2.m_distant)
				{
					objects.Add(zdo2);
				}
			}
		}
	}

	// Token: 0x060007E2 RID: 2018 RVA: 0x0003E46C File Offset: 0x0003C66C
	private void RemoveOrphanNonPersistentZDOS()
	{
		foreach (KeyValuePair<ZDOID, ZDO> keyValuePair in this.m_objectsByID)
		{
			ZDO value = keyValuePair.Value;
			if (!value.m_persistent && (value.m_owner == 0L || !this.IsPeerConnected(value.m_owner)))
			{
				ZLog.Log(string.Concat(new object[]
				{
					"Destroying abandoned non persistent zdo ",
					value.m_uid,
					" owner ",
					value.m_owner
				}));
				value.SetOwner(this.m_myid);
				this.DestroyZDO(value);
			}
		}
	}

	// Token: 0x060007E3 RID: 2019 RVA: 0x0003E530 File Offset: 0x0003C730
	private bool IsPeerConnected(long uid)
	{
		if (this.m_myid == uid)
		{
			return true;
		}
		using (List<ZDOMan.ZDOPeer>.Enumerator enumerator = this.m_peers.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_peer.m_uid == uid)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x060007E4 RID: 2020 RVA: 0x0003E59C File Offset: 0x0003C79C
	public void GetAllZDOsWithPrefab(string prefab, List<ZDO> zdos)
	{
		int stableHashCode = prefab.GetStableHashCode();
		foreach (ZDO zdo in this.m_objectsByID.Values)
		{
			if (zdo.GetPrefab() == stableHashCode)
			{
				zdos.Add(zdo);
			}
		}
	}

	// Token: 0x060007E5 RID: 2021 RVA: 0x0003E604 File Offset: 0x0003C804
	private static bool InvalidZDO(ZDO zdo)
	{
		return !zdo.IsValid();
	}

	// Token: 0x060007E6 RID: 2022 RVA: 0x0003E610 File Offset: 0x0003C810
	public bool GetAllZDOsWithPrefabIterative(string prefab, List<ZDO> zdos, ref int index)
	{
		int stableHashCode = prefab.GetStableHashCode();
		if (index >= this.m_objectsBySector.Length)
		{
			foreach (List<ZDO> list in this.m_objectsByOutsideSector.Values)
			{
				foreach (ZDO zdo in list)
				{
					if (zdo.GetPrefab() == stableHashCode)
					{
						zdos.Add(zdo);
					}
				}
			}
			zdos.RemoveAll(new Predicate<ZDO>(ZDOMan.InvalidZDO));
			return true;
		}
		int num = 0;
		while (index < this.m_objectsBySector.Length)
		{
			List<ZDO> list2 = this.m_objectsBySector[index];
			if (list2 != null)
			{
				foreach (ZDO zdo2 in list2)
				{
					if (zdo2.GetPrefab() == stableHashCode)
					{
						zdos.Add(zdo2);
					}
				}
				num++;
				if (num > 400)
				{
					break;
				}
			}
			index++;
		}
		return false;
	}

	// Token: 0x060007E7 RID: 2023 RVA: 0x0003E750 File Offset: 0x0003C950
	private List<ZDO> GetSaveClone()
	{
		List<ZDO> list = new List<ZDO>();
		for (int i = 0; i < this.m_objectsBySector.Length; i++)
		{
			if (this.m_objectsBySector[i] != null)
			{
				foreach (ZDO zdo in this.m_objectsBySector[i])
				{
					if (zdo.m_persistent)
					{
						list.Add(zdo.Clone());
					}
				}
			}
		}
		foreach (List<ZDO> list2 in this.m_objectsByOutsideSector.Values)
		{
			foreach (ZDO zdo2 in list2)
			{
				if (zdo2.m_persistent)
				{
					list.Add(zdo2.Clone());
				}
			}
		}
		return list;
	}

	// Token: 0x060007E8 RID: 2024 RVA: 0x0003E864 File Offset: 0x0003CA64
	public int NrOfObjects()
	{
		return this.m_objectsByID.Count;
	}

	// Token: 0x060007E9 RID: 2025 RVA: 0x0003E871 File Offset: 0x0003CA71
	public int GetSentZDOs()
	{
		return this.m_zdosSentLastSec;
	}

	// Token: 0x060007EA RID: 2026 RVA: 0x0003E879 File Offset: 0x0003CA79
	public int GetRecvZDOs()
	{
		return this.m_zdosRecvLastSec;
	}

	// Token: 0x060007EB RID: 2027 RVA: 0x0003E881 File Offset: 0x0003CA81
	public void GetAverageStats(out float sentZdos, out float recvZdos)
	{
		sentZdos = (float)this.m_zdosSentLastSec / 20f;
		recvZdos = (float)this.m_zdosRecvLastSec / 20f;
	}

	// Token: 0x060007EC RID: 2028 RVA: 0x0003E8A1 File Offset: 0x0003CAA1
	public int GetClientChangeQueue()
	{
		return this.m_clientChangeQueue.Count;
	}

	// Token: 0x060007ED RID: 2029 RVA: 0x0003E8AE File Offset: 0x0003CAAE
	public void RequestZDO(ZDOID id)
	{
		ZRoutedRpc.instance.InvokeRoutedRPC("RequestZDO", new object[]
		{
			id
		});
	}

	// Token: 0x060007EE RID: 2030 RVA: 0x0003E8D0 File Offset: 0x0003CAD0
	private void RPC_RequestZDO(long sender, ZDOID id)
	{
		ZDOMan.ZDOPeer peer = this.GetPeer(sender);
		if (peer != null)
		{
			peer.ForceSendZDO(id);
		}
	}

	// Token: 0x060007EF RID: 2031 RVA: 0x0003E8F0 File Offset: 0x0003CAF0
	private ZDOMan.ZDOPeer GetPeer(long uid)
	{
		foreach (ZDOMan.ZDOPeer zdopeer in this.m_peers)
		{
			if (zdopeer.m_peer.m_uid == uid)
			{
				return zdopeer;
			}
		}
		return null;
	}

	// Token: 0x060007F0 RID: 2032 RVA: 0x0003E954 File Offset: 0x0003CB54
	public void ForceSendZDO(ZDOID id)
	{
		foreach (ZDOMan.ZDOPeer zdopeer in this.m_peers)
		{
			zdopeer.ForceSendZDO(id);
		}
	}

	// Token: 0x060007F1 RID: 2033 RVA: 0x0003E9A8 File Offset: 0x0003CBA8
	public void ForceSendZDO(long peerID, ZDOID id)
	{
		if (ZNet.instance.IsServer())
		{
			ZDOMan.ZDOPeer peer = this.GetPeer(peerID);
			if (peer != null)
			{
				peer.ForceSendZDO(id);
				return;
			}
		}
		else
		{
			foreach (ZDOMan.ZDOPeer zdopeer in this.m_peers)
			{
				zdopeer.ForceSendZDO(id);
			}
		}
	}

	// Token: 0x060007F2 RID: 2034 RVA: 0x0003EA18 File Offset: 0x0003CC18
	public void ClientChanged(ZDOID id)
	{
		this.m_clientChangeQueue.Add(id);
	}

	// Token: 0x040007DA RID: 2010
	private static long compareReceiver;

	// Token: 0x040007DB RID: 2011
	public Action<ZDO> m_onZDODestroyed;

	// Token: 0x040007DC RID: 2012
	private Dictionary<ZDOID, ZDO> m_objectsByID = new Dictionary<ZDOID, ZDO>();

	// Token: 0x040007DD RID: 2013
	private List<ZDO>[] m_objectsBySector;

	// Token: 0x040007DE RID: 2014
	private Dictionary<Vector2i, List<ZDO>> m_objectsByOutsideSector = new Dictionary<Vector2i, List<ZDO>>();

	// Token: 0x040007DF RID: 2015
	private List<ZDOMan.ZDOPeer> m_peers = new List<ZDOMan.ZDOPeer>();

	// Token: 0x040007E0 RID: 2016
	private const int m_maxDeadZDOs = 100000;

	// Token: 0x040007E1 RID: 2017
	private Dictionary<ZDOID, long> m_deadZDOs = new Dictionary<ZDOID, long>();

	// Token: 0x040007E2 RID: 2018
	private List<ZDOID> m_destroySendList = new List<ZDOID>();

	// Token: 0x040007E3 RID: 2019
	private HashSet<ZDOID> m_clientChangeQueue = new HashSet<ZDOID>();

	// Token: 0x040007E4 RID: 2020
	private long m_myid;

	// Token: 0x040007E5 RID: 2021
	private uint m_nextUid = 1U;

	// Token: 0x040007E6 RID: 2022
	private int m_width;

	// Token: 0x040007E7 RID: 2023
	private int m_halfWidth;

	// Token: 0x040007E8 RID: 2024
	private float m_sendTimer;

	// Token: 0x040007E9 RID: 2025
	private const float m_sendFPS = 20f;

	// Token: 0x040007EA RID: 2026
	private float m_releaseZDOTimer;

	// Token: 0x040007EB RID: 2027
	private static ZDOMan m_instance;

	// Token: 0x040007EC RID: 2028
	private int m_zdosSent;

	// Token: 0x040007ED RID: 2029
	private int m_zdosRecv;

	// Token: 0x040007EE RID: 2030
	private int m_zdosSentLastSec;

	// Token: 0x040007EF RID: 2031
	private int m_zdosRecvLastSec;

	// Token: 0x040007F0 RID: 2032
	private float m_statTimer;

	// Token: 0x040007F1 RID: 2033
	private List<ZDO> m_tempToSync = new List<ZDO>();

	// Token: 0x040007F2 RID: 2034
	private List<ZDO> m_tempToSyncDistant = new List<ZDO>();

	// Token: 0x040007F3 RID: 2035
	private List<ZDO> m_tempNearObjects = new List<ZDO>();

	// Token: 0x040007F4 RID: 2036
	private List<ZDOID> m_tempRemoveList = new List<ZDOID>();

	// Token: 0x040007F5 RID: 2037
	private ZDOMan.SaveData m_saveData;

	// Token: 0x02000169 RID: 361
	private class ZDOPeer
	{
		// Token: 0x06001151 RID: 4433 RVA: 0x000785E8 File Offset: 0x000767E8
		public void ZDOSectorInvalidated(ZDO zdo)
		{
			if (zdo.m_owner == this.m_peer.m_uid)
			{
				return;
			}
			if (this.m_zdos.ContainsKey(zdo.m_uid) && !ZNetScene.instance.InActiveArea(zdo.GetSector(), this.m_peer.GetRefPos()))
			{
				this.m_invalidSector.Add(zdo.m_uid);
				this.m_zdos.Remove(zdo.m_uid);
			}
		}

		// Token: 0x06001152 RID: 4434 RVA: 0x0007865D File Offset: 0x0007685D
		public void ForceSendZDO(ZDOID id)
		{
			this.m_forceSend.Add(id);
		}

		// Token: 0x06001153 RID: 4435 RVA: 0x0007866C File Offset: 0x0007686C
		public bool ShouldSend(ZDO zdo)
		{
			ZDOMan.ZDOPeer.PeerZDOInfo peerZDOInfo;
			return !this.m_zdos.TryGetValue(zdo.m_uid, out peerZDOInfo) || (ulong)zdo.m_ownerRevision > (ulong)peerZDOInfo.m_ownerRevision || zdo.m_dataRevision > peerZDOInfo.m_dataRevision;
		}

		// Token: 0x04001167 RID: 4455
		public ZNetPeer m_peer;

		// Token: 0x04001168 RID: 4456
		public Dictionary<ZDOID, ZDOMan.ZDOPeer.PeerZDOInfo> m_zdos = new Dictionary<ZDOID, ZDOMan.ZDOPeer.PeerZDOInfo>();

		// Token: 0x04001169 RID: 4457
		public HashSet<ZDOID> m_forceSend = new HashSet<ZDOID>();

		// Token: 0x0400116A RID: 4458
		public HashSet<ZDOID> m_invalidSector = new HashSet<ZDOID>();

		// Token: 0x0400116B RID: 4459
		public int m_sendIndex;

		// Token: 0x020001C4 RID: 452
		public struct PeerZDOInfo
		{
			// Token: 0x060011E3 RID: 4579 RVA: 0x0007A87A File Offset: 0x00078A7A
			public PeerZDOInfo(uint dataRevision, uint ownerRevision, float syncTime)
			{
				this.m_dataRevision = dataRevision;
				this.m_ownerRevision = (long)((ulong)ownerRevision);
				this.m_syncTime = syncTime;
			}

			// Token: 0x040013C6 RID: 5062
			public uint m_dataRevision;

			// Token: 0x040013C7 RID: 5063
			public long m_ownerRevision;

			// Token: 0x040013C8 RID: 5064
			public float m_syncTime;
		}
	}

	// Token: 0x0200016A RID: 362
	private class SaveData
	{
		// Token: 0x0400116C RID: 4460
		public long m_myid;

		// Token: 0x0400116D RID: 4461
		public uint m_nextUid = 1U;

		// Token: 0x0400116E RID: 4462
		public List<ZDO> m_zdos;

		// Token: 0x0400116F RID: 4463
		public Dictionary<ZDOID, long> m_deadZDOs;
	}
}
