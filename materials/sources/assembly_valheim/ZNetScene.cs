using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Token: 0x02000080 RID: 128
public class ZNetScene : MonoBehaviour
{
	// Token: 0x1700001A RID: 26
	// (get) Token: 0x06000876 RID: 2166 RVA: 0x000411FE File Offset: 0x0003F3FE
	public static ZNetScene instance
	{
		get
		{
			return ZNetScene.m_instance;
		}
	}

	// Token: 0x06000877 RID: 2167 RVA: 0x00041208 File Offset: 0x0003F408
	private void Awake()
	{
		ZNetScene.m_instance = this;
		foreach (GameObject gameObject in this.m_prefabs)
		{
			this.m_namedPrefabs.Add(gameObject.name.GetStableHashCode(), gameObject);
		}
		foreach (GameObject gameObject2 in this.m_nonNetViewPrefabs)
		{
			this.m_namedPrefabs.Add(gameObject2.name.GetStableHashCode(), gameObject2);
		}
		ZDOMan instance = ZDOMan.instance;
		instance.m_onZDODestroyed = (Action<ZDO>)Delegate.Combine(instance.m_onZDODestroyed, new Action<ZDO>(this.OnZDODestroyed));
		this.m_netSceneRoot = new GameObject("_NetSceneRoot");
		ZRoutedRpc.instance.Register<Vector3, Quaternion, int>("SpawnObject", new Action<long, Vector3, Quaternion, int>(this.RPC_SpawnObject));
	}

	// Token: 0x06000878 RID: 2168 RVA: 0x00041314 File Offset: 0x0003F514
	private void OnDestroy()
	{
		ZLog.Log("Net scene destroyed");
		if (ZNetScene.m_instance == this)
		{
			ZNetScene.m_instance = null;
		}
	}

	// Token: 0x06000879 RID: 2169 RVA: 0x00041334 File Offset: 0x0003F534
	public void Shutdown()
	{
		foreach (KeyValuePair<ZDO, ZNetView> keyValuePair in this.m_instances)
		{
			if (keyValuePair.Value)
			{
				keyValuePair.Value.ResetZDO();
				UnityEngine.Object.Destroy(keyValuePair.Value.gameObject);
			}
		}
		this.m_instances.Clear();
		base.enabled = false;
	}

	// Token: 0x0600087A RID: 2170 RVA: 0x000413C0 File Offset: 0x0003F5C0
	public void AddInstance(ZDO zdo, ZNetView nview)
	{
		this.m_instances[zdo] = nview;
		if (nview.transform.parent == null)
		{
			nview.transform.SetParent(this.m_netSceneRoot.transform);
		}
	}

	// Token: 0x0600087B RID: 2171 RVA: 0x000413F8 File Offset: 0x0003F5F8
	private bool IsPrefabZDOValid(ZDO zdo)
	{
		int prefab = zdo.GetPrefab();
		return prefab != 0 && !(this.GetPrefab(prefab) == null);
	}

	// Token: 0x0600087C RID: 2172 RVA: 0x00041424 File Offset: 0x0003F624
	private GameObject CreateObject(ZDO zdo)
	{
		int prefab = zdo.GetPrefab();
		if (prefab == 0)
		{
			return null;
		}
		GameObject prefab2 = this.GetPrefab(prefab);
		if (prefab2 == null)
		{
			return null;
		}
		Vector3 position = zdo.GetPosition();
		Quaternion rotation = zdo.GetRotation();
		ZNetView.m_useInitZDO = true;
		ZNetView.m_initZDO = zdo;
		GameObject result = UnityEngine.Object.Instantiate<GameObject>(prefab2, position, rotation);
		if (ZNetView.m_initZDO != null)
		{
			ZLog.LogWarning(string.Concat(new object[]
			{
				"ZDO ",
				zdo.m_uid,
				" not used when creating object ",
				prefab2.name
			}));
			ZNetView.m_initZDO = null;
		}
		ZNetView.m_useInitZDO = false;
		return result;
	}

	// Token: 0x0600087D RID: 2173 RVA: 0x000414C0 File Offset: 0x0003F6C0
	public void Destroy(GameObject go)
	{
		ZNetView component = go.GetComponent<ZNetView>();
		if (component && component.GetZDO() != null)
		{
			ZDO zdo = component.GetZDO();
			component.ResetZDO();
			this.m_instances.Remove(zdo);
			if (zdo.IsOwner())
			{
				ZDOMan.instance.DestroyZDO(zdo);
			}
		}
		UnityEngine.Object.Destroy(go);
	}

	// Token: 0x0600087E RID: 2174 RVA: 0x00041518 File Offset: 0x0003F718
	public GameObject GetPrefab(int hash)
	{
		GameObject result;
		if (this.m_namedPrefabs.TryGetValue(hash, out result))
		{
			return result;
		}
		return null;
	}

	// Token: 0x0600087F RID: 2175 RVA: 0x00041538 File Offset: 0x0003F738
	public GameObject GetPrefab(string name)
	{
		int stableHashCode = name.GetStableHashCode();
		return this.GetPrefab(stableHashCode);
	}

	// Token: 0x06000880 RID: 2176 RVA: 0x00041553 File Offset: 0x0003F753
	public int GetPrefabHash(GameObject go)
	{
		return go.name.GetStableHashCode();
	}

	// Token: 0x06000881 RID: 2177 RVA: 0x00041560 File Offset: 0x0003F760
	public bool IsAreaReady(Vector3 point)
	{
		Vector2i zone = ZoneSystem.instance.GetZone(point);
		if (!ZoneSystem.instance.IsZoneLoaded(zone))
		{
			return false;
		}
		this.m_tempCurrentObjects.Clear();
		ZDOMan.instance.FindSectorObjects(zone, 1, 0, this.m_tempCurrentObjects, null);
		foreach (ZDO zdo in this.m_tempCurrentObjects)
		{
			if (this.IsPrefabZDOValid(zdo) && !this.FindInstance(zdo))
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06000882 RID: 2178 RVA: 0x00041604 File Offset: 0x0003F804
	private bool InLoadingScreen()
	{
		return Player.m_localPlayer == null || Player.m_localPlayer.IsTeleporting();
	}

	// Token: 0x06000883 RID: 2179 RVA: 0x00041624 File Offset: 0x0003F824
	private void CreateObjects(List<ZDO> currentNearObjects, List<ZDO> currentDistantObjects)
	{
		int maxCreatedPerFrame = 10;
		if (this.InLoadingScreen())
		{
			maxCreatedPerFrame = 100;
		}
		int frameCount = Time.frameCount;
		foreach (ZDO zdo in this.m_instances.Keys)
		{
			zdo.m_tempCreateEarmark = frameCount;
		}
		int num = 0;
		this.CreateObjectsSorted(currentNearObjects, maxCreatedPerFrame, ref num);
		this.CreateDistantObjects(currentDistantObjects, maxCreatedPerFrame, ref num);
	}

	// Token: 0x06000884 RID: 2180 RVA: 0x000416A4 File Offset: 0x0003F8A4
	private void CreateObjectsSorted(List<ZDO> currentNearObjects, int maxCreatedPerFrame, ref int created)
	{
		this.m_tempCurrentObjects2.Clear();
		int frameCount = Time.frameCount;
		foreach (ZDO zdo in currentNearObjects)
		{
			if (zdo.m_tempCreateEarmark != frameCount && (zdo.m_distant || ZoneSystem.instance.IsZoneLoaded(zdo.GetSector())))
			{
				this.m_tempCurrentObjects2.Add(zdo);
			}
		}
		foreach (ZDO zdo2 in from item in this.m_tempCurrentObjects2
		orderby item.m_type descending
		select item)
		{
			if (this.CreateObject(zdo2) != null)
			{
				created++;
				if (created > maxCreatedPerFrame)
				{
					break;
				}
			}
			else if (ZNet.instance.IsServer())
			{
				zdo2.SetOwner(ZDOMan.instance.GetMyID());
				ZLog.Log("Destroyed invalid predab ZDO:" + zdo2.m_uid);
				ZDOMan.instance.DestroyZDO(zdo2);
			}
		}
	}

	// Token: 0x06000885 RID: 2181 RVA: 0x000417E8 File Offset: 0x0003F9E8
	private void CreateDistantObjects(List<ZDO> objects, int maxCreatedPerFrame, ref int created)
	{
		if (created > maxCreatedPerFrame)
		{
			return;
		}
		int frameCount = Time.frameCount;
		foreach (ZDO zdo in objects)
		{
			if (zdo.m_tempCreateEarmark != frameCount)
			{
				if (this.CreateObject(zdo) != null)
				{
					created++;
					if (created > maxCreatedPerFrame)
					{
						break;
					}
				}
				else if (ZNet.instance.IsServer())
				{
					zdo.SetOwner(ZDOMan.instance.GetMyID());
					ZLog.Log(string.Concat(new object[]
					{
						"Destroyed invalid predab ZDO:",
						zdo.m_uid,
						"  prefab hash:",
						zdo.GetPrefab()
					}));
					ZDOMan.instance.DestroyZDO(zdo);
				}
			}
		}
	}

	// Token: 0x06000886 RID: 2182 RVA: 0x000418CC File Offset: 0x0003FACC
	private void OnZDODestroyed(ZDO zdo)
	{
		ZNetView znetView;
		if (this.m_instances.TryGetValue(zdo, out znetView))
		{
			znetView.ResetZDO();
			UnityEngine.Object.Destroy(znetView.gameObject);
			this.m_instances.Remove(zdo);
		}
	}

	// Token: 0x06000887 RID: 2183 RVA: 0x00041908 File Offset: 0x0003FB08
	private void RemoveObjects(List<ZDO> currentNearObjects, List<ZDO> currentDistantObjects)
	{
		int frameCount = Time.frameCount;
		foreach (ZDO zdo in currentNearObjects)
		{
			zdo.m_tempRemoveEarmark = frameCount;
		}
		foreach (ZDO zdo2 in currentDistantObjects)
		{
			zdo2.m_tempRemoveEarmark = frameCount;
		}
		this.m_tempRemoved.Clear();
		foreach (ZNetView znetView in this.m_instances.Values)
		{
			if (znetView.GetZDO().m_tempRemoveEarmark != frameCount)
			{
				this.m_tempRemoved.Add(znetView);
			}
		}
		for (int i = 0; i < this.m_tempRemoved.Count; i++)
		{
			ZNetView znetView2 = this.m_tempRemoved[i];
			ZDO zdo3 = znetView2.GetZDO();
			znetView2.ResetZDO();
			UnityEngine.Object.Destroy(znetView2.gameObject);
			if (!zdo3.m_persistent && zdo3.IsOwner())
			{
				ZDOMan.instance.DestroyZDO(zdo3);
			}
			this.m_instances.Remove(zdo3);
		}
	}

	// Token: 0x06000888 RID: 2184 RVA: 0x00041A68 File Offset: 0x0003FC68
	public ZNetView FindInstance(ZDO zdo)
	{
		ZNetView result;
		if (this.m_instances.TryGetValue(zdo, out result))
		{
			return result;
		}
		return null;
	}

	// Token: 0x06000889 RID: 2185 RVA: 0x00041A88 File Offset: 0x0003FC88
	public bool HaveInstance(ZDO zdo)
	{
		return this.m_instances.ContainsKey(zdo);
	}

	// Token: 0x0600088A RID: 2186 RVA: 0x00041A98 File Offset: 0x0003FC98
	public GameObject FindInstance(ZDOID id)
	{
		ZDO zdo = ZDOMan.instance.GetZDO(id);
		if (zdo != null)
		{
			ZNetView znetView = this.FindInstance(zdo);
			if (znetView)
			{
				return znetView.gameObject;
			}
		}
		return null;
	}

	// Token: 0x0600088B RID: 2187 RVA: 0x00041ACC File Offset: 0x0003FCCC
	private void Update()
	{
		float deltaTime = Time.deltaTime;
		this.m_createDestroyTimer += deltaTime;
		if (this.m_createDestroyTimer >= 0.033333335f)
		{
			this.m_createDestroyTimer = 0f;
			this.CreateDestroyObjects();
		}
	}

	// Token: 0x0600088C RID: 2188 RVA: 0x00041B0C File Offset: 0x0003FD0C
	private void CreateDestroyObjects()
	{
		Vector2i zone = ZoneSystem.instance.GetZone(ZNet.instance.GetReferencePosition());
		this.m_tempCurrentObjects.Clear();
		this.m_tempCurrentDistantObjects.Clear();
		ZDOMan.instance.FindSectorObjects(zone, ZoneSystem.instance.m_activeArea, ZoneSystem.instance.m_activeDistantArea, this.m_tempCurrentObjects, this.m_tempCurrentDistantObjects);
		this.CreateObjects(this.m_tempCurrentObjects, this.m_tempCurrentDistantObjects);
		this.RemoveObjects(this.m_tempCurrentObjects, this.m_tempCurrentDistantObjects);
	}

	// Token: 0x0600088D RID: 2189 RVA: 0x00041B94 File Offset: 0x0003FD94
	public bool InActiveArea(Vector2i zone, Vector3 refPoint)
	{
		Vector2i zone2 = ZoneSystem.instance.GetZone(refPoint);
		return this.InActiveArea(zone, zone2);
	}

	// Token: 0x0600088E RID: 2190 RVA: 0x00041BB8 File Offset: 0x0003FDB8
	public bool InActiveArea(Vector2i zone, Vector2i refCenterZone)
	{
		int num = ZoneSystem.instance.m_activeArea - 1;
		return zone.x >= refCenterZone.x - num && zone.x <= refCenterZone.x + num && zone.y <= refCenterZone.y + num && zone.y >= refCenterZone.y - num;
	}

	// Token: 0x0600088F RID: 2191 RVA: 0x00041C17 File Offset: 0x0003FE17
	public bool OutsideActiveArea(Vector3 point)
	{
		return this.OutsideActiveArea(point, ZNet.instance.GetReferencePosition());
	}

	// Token: 0x06000890 RID: 2192 RVA: 0x00041C2C File Offset: 0x0003FE2C
	public bool OutsideActiveArea(Vector3 point, Vector3 refPoint)
	{
		Vector2i zone = ZoneSystem.instance.GetZone(refPoint);
		Vector2i zone2 = ZoneSystem.instance.GetZone(point);
		return zone2.x <= zone.x - ZoneSystem.instance.m_activeArea || zone2.x >= zone.x + ZoneSystem.instance.m_activeArea || zone2.y >= zone.y + ZoneSystem.instance.m_activeArea || zone2.y <= zone.y - ZoneSystem.instance.m_activeArea;
	}

	// Token: 0x06000891 RID: 2193 RVA: 0x00041CBC File Offset: 0x0003FEBC
	public bool HaveInstanceInSector(Vector2i sector)
	{
		foreach (KeyValuePair<ZDO, ZNetView> keyValuePair in this.m_instances)
		{
			if (keyValuePair.Value && !keyValuePair.Value.m_distant && ZoneSystem.instance.GetZone(keyValuePair.Value.transform.position) == sector)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000892 RID: 2194 RVA: 0x00041D50 File Offset: 0x0003FF50
	public int NrOfInstances()
	{
		return this.m_instances.Count;
	}

	// Token: 0x06000893 RID: 2195 RVA: 0x00041D60 File Offset: 0x0003FF60
	public void SpawnObject(Vector3 pos, Quaternion rot, GameObject prefab)
	{
		int prefabHash = this.GetPrefabHash(prefab);
		ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "SpawnObject", new object[]
		{
			pos,
			rot,
			prefabHash
		});
	}

	// Token: 0x06000894 RID: 2196 RVA: 0x00041DAC File Offset: 0x0003FFAC
	private void RPC_SpawnObject(long spawner, Vector3 pos, Quaternion rot, int prefabHash)
	{
		GameObject prefab = this.GetPrefab(prefabHash);
		if (prefab == null)
		{
			ZLog.Log("Missing prefab " + prefabHash);
			return;
		}
		UnityEngine.Object.Instantiate<GameObject>(prefab, pos, rot);
	}

	// Token: 0x04000834 RID: 2100
	private static ZNetScene m_instance;

	// Token: 0x04000835 RID: 2101
	private const int m_maxCreatedPerFrame = 10;

	// Token: 0x04000836 RID: 2102
	private const int m_maxDestroyedPerFrame = 20;

	// Token: 0x04000837 RID: 2103
	private const float m_createDestroyFps = 30f;

	// Token: 0x04000838 RID: 2104
	public List<GameObject> m_prefabs = new List<GameObject>();

	// Token: 0x04000839 RID: 2105
	public List<GameObject> m_nonNetViewPrefabs = new List<GameObject>();

	// Token: 0x0400083A RID: 2106
	private Dictionary<int, GameObject> m_namedPrefabs = new Dictionary<int, GameObject>();

	// Token: 0x0400083B RID: 2107
	private Dictionary<ZDO, ZNetView> m_instances = new Dictionary<ZDO, ZNetView>(new ZDOComparer());

	// Token: 0x0400083C RID: 2108
	private GameObject m_netSceneRoot;

	// Token: 0x0400083D RID: 2109
	private List<ZDO> m_tempCurrentObjects = new List<ZDO>();

	// Token: 0x0400083E RID: 2110
	private List<ZDO> m_tempCurrentObjects2 = new List<ZDO>();

	// Token: 0x0400083F RID: 2111
	private List<ZDO> m_tempCurrentDistantObjects = new List<ZDO>();

	// Token: 0x04000840 RID: 2112
	private List<ZNetView> m_tempRemoved = new List<ZNetView>();

	// Token: 0x04000841 RID: 2113
	private HashSet<ZDO> m_tempActiveZDOs = new HashSet<ZDO>(new ZDOComparer());

	// Token: 0x04000842 RID: 2114
	private float m_createDestroyTimer;
}
