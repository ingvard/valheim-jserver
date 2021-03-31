using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Token: 0x020000A7 RID: 167
public class PlayerProfile
{
	// Token: 0x06000B5B RID: 2907 RVA: 0x00052020 File Offset: 0x00050220
	public PlayerProfile(string filename = null)
	{
		this.m_filename = filename;
		this.m_playerName = "Stranger";
		this.m_playerID = Utils.GenerateUID();
	}

	// Token: 0x06000B5C RID: 2908 RVA: 0x00052087 File Offset: 0x00050287
	public bool Load()
	{
		return this.m_filename != null && this.LoadPlayerFromDisk();
	}

	// Token: 0x06000B5D RID: 2909 RVA: 0x00052099 File Offset: 0x00050299
	public bool Save()
	{
		return this.m_filename != null && this.SavePlayerToDisk();
	}

	// Token: 0x06000B5E RID: 2910 RVA: 0x000520AC File Offset: 0x000502AC
	public bool HaveIncompatiblPlayerData()
	{
		if (this.m_filename == null)
		{
			return false;
		}
		ZPackage zpackage = this.LoadPlayerDataFromDisk();
		if (zpackage == null)
		{
			return false;
		}
		if (!global::Version.IsPlayerVersionCompatible(zpackage.ReadInt()))
		{
			ZLog.Log("Player data is not compatible, ignoring");
			return true;
		}
		return false;
	}

	// Token: 0x06000B5F RID: 2911 RVA: 0x000520EC File Offset: 0x000502EC
	public void SavePlayerData(Player player)
	{
		ZPackage zpackage = new ZPackage();
		player.Save(zpackage);
		this.m_playerData = zpackage.GetArray();
	}

	// Token: 0x06000B60 RID: 2912 RVA: 0x00052114 File Offset: 0x00050314
	public void LoadPlayerData(Player player)
	{
		player.SetPlayerID(this.m_playerID, this.m_playerName);
		if (this.m_playerData != null)
		{
			ZPackage pkg = new ZPackage(this.m_playerData);
			player.Load(pkg);
			return;
		}
		player.GiveDefaultItems();
	}

	// Token: 0x06000B61 RID: 2913 RVA: 0x00052155 File Offset: 0x00050355
	public void SaveLogoutPoint()
	{
		if (Player.m_localPlayer && !Player.m_localPlayer.IsDead() && !Player.m_localPlayer.InIntro())
		{
			this.SetLogoutPoint(Player.m_localPlayer.transform.position);
		}
	}

	// Token: 0x06000B62 RID: 2914 RVA: 0x00052190 File Offset: 0x00050390
	private bool SavePlayerToDisk()
	{
		Directory.CreateDirectory(Utils.GetSaveDataPath() + "/characters");
		string text = Utils.GetSaveDataPath() + "/characters/" + this.m_filename + ".fch";
		string text2 = Utils.GetSaveDataPath() + "/characters/" + this.m_filename + ".fch.old";
		string text3 = Utils.GetSaveDataPath() + "/characters/" + this.m_filename + ".fch.new";
		ZPackage zpackage = new ZPackage();
		zpackage.Write(global::Version.m_playerVersion);
		zpackage.Write(this.m_playerStats.m_kills);
		zpackage.Write(this.m_playerStats.m_deaths);
		zpackage.Write(this.m_playerStats.m_crafts);
		zpackage.Write(this.m_playerStats.m_builds);
		zpackage.Write(this.m_worldData.Count);
		foreach (KeyValuePair<long, PlayerProfile.WorldPlayerData> keyValuePair in this.m_worldData)
		{
			zpackage.Write(keyValuePair.Key);
			zpackage.Write(keyValuePair.Value.m_haveCustomSpawnPoint);
			zpackage.Write(keyValuePair.Value.m_spawnPoint);
			zpackage.Write(keyValuePair.Value.m_haveLogoutPoint);
			zpackage.Write(keyValuePair.Value.m_logoutPoint);
			zpackage.Write(keyValuePair.Value.m_haveDeathPoint);
			zpackage.Write(keyValuePair.Value.m_deathPoint);
			zpackage.Write(keyValuePair.Value.m_homePoint);
			zpackage.Write(keyValuePair.Value.m_mapData != null);
			if (keyValuePair.Value.m_mapData != null)
			{
				zpackage.Write(keyValuePair.Value.m_mapData);
			}
		}
		zpackage.Write(this.m_playerName);
		zpackage.Write(this.m_playerID);
		zpackage.Write(this.m_startSeed);
		if (this.m_playerData != null)
		{
			zpackage.Write(true);
			zpackage.Write(this.m_playerData);
		}
		else
		{
			zpackage.Write(false);
		}
		byte[] array = zpackage.GenerateHash();
		byte[] array2 = zpackage.GetArray();
		FileStream fileStream = File.Create(text3);
		BinaryWriter binaryWriter = new BinaryWriter(fileStream);
		binaryWriter.Write(array2.Length);
		binaryWriter.Write(array2);
		binaryWriter.Write(array.Length);
		binaryWriter.Write(array);
		binaryWriter.Flush();
		fileStream.Flush(true);
		fileStream.Close();
		fileStream.Dispose();
		if (File.Exists(text))
		{
			if (File.Exists(text2))
			{
				File.Delete(text2);
			}
			File.Move(text, text2);
		}
		File.Move(text3, text);
		return true;
	}

	// Token: 0x06000B63 RID: 2915 RVA: 0x00052434 File Offset: 0x00050634
	private bool LoadPlayerFromDisk()
	{
		try
		{
			ZPackage zpackage = this.LoadPlayerDataFromDisk();
			if (zpackage == null)
			{
				ZLog.LogWarning("No player data");
				return false;
			}
			int num = zpackage.ReadInt();
			if (!global::Version.IsPlayerVersionCompatible(num))
			{
				ZLog.Log("Player data is not compatible, ignoring");
				return false;
			}
			if (num >= 28)
			{
				this.m_playerStats.m_kills = zpackage.ReadInt();
				this.m_playerStats.m_deaths = zpackage.ReadInt();
				this.m_playerStats.m_crafts = zpackage.ReadInt();
				this.m_playerStats.m_builds = zpackage.ReadInt();
			}
			this.m_worldData.Clear();
			int num2 = zpackage.ReadInt();
			for (int i = 0; i < num2; i++)
			{
				long key = zpackage.ReadLong();
				PlayerProfile.WorldPlayerData worldPlayerData = new PlayerProfile.WorldPlayerData();
				worldPlayerData.m_haveCustomSpawnPoint = zpackage.ReadBool();
				worldPlayerData.m_spawnPoint = zpackage.ReadVector3();
				worldPlayerData.m_haveLogoutPoint = zpackage.ReadBool();
				worldPlayerData.m_logoutPoint = zpackage.ReadVector3();
				if (num >= 30)
				{
					worldPlayerData.m_haveDeathPoint = zpackage.ReadBool();
					worldPlayerData.m_deathPoint = zpackage.ReadVector3();
				}
				worldPlayerData.m_homePoint = zpackage.ReadVector3();
				if (num >= 29 && zpackage.ReadBool())
				{
					worldPlayerData.m_mapData = zpackage.ReadByteArray();
				}
				this.m_worldData.Add(key, worldPlayerData);
			}
			this.m_playerName = zpackage.ReadString();
			this.m_playerID = zpackage.ReadLong();
			this.m_startSeed = zpackage.ReadString();
			if (zpackage.ReadBool())
			{
				this.m_playerData = zpackage.ReadByteArray();
			}
			else
			{
				this.m_playerData = null;
			}
		}
		catch (Exception ex)
		{
			ZLog.LogWarning("Exception while loading player profile:" + this.m_filename + " , " + ex.ToString());
		}
		return true;
	}

	// Token: 0x06000B64 RID: 2916 RVA: 0x0005260C File Offset: 0x0005080C
	private ZPackage LoadPlayerDataFromDisk()
	{
		string text = Utils.GetSaveDataPath() + "/characters/" + this.m_filename + ".fch";
		FileStream fileStream;
		try
		{
			fileStream = File.OpenRead(text);
		}
		catch
		{
			ZLog.Log("  failed to load " + text);
			return null;
		}
		byte[] data;
		try
		{
			BinaryReader binaryReader = new BinaryReader(fileStream);
			int count = binaryReader.ReadInt32();
			data = binaryReader.ReadBytes(count);
			int count2 = binaryReader.ReadInt32();
			binaryReader.ReadBytes(count2);
		}
		catch
		{
			ZLog.LogError("  error loading player.dat");
			fileStream.Dispose();
			return null;
		}
		fileStream.Dispose();
		return new ZPackage(data);
	}

	// Token: 0x06000B65 RID: 2917 RVA: 0x000526BC File Offset: 0x000508BC
	public void SetLogoutPoint(Vector3 point)
	{
		this.GetWorldData(ZNet.instance.GetWorldUID()).m_haveLogoutPoint = true;
		this.GetWorldData(ZNet.instance.GetWorldUID()).m_logoutPoint = point;
	}

	// Token: 0x06000B66 RID: 2918 RVA: 0x000526EA File Offset: 0x000508EA
	public void SetDeathPoint(Vector3 point)
	{
		this.GetWorldData(ZNet.instance.GetWorldUID()).m_haveDeathPoint = true;
		this.GetWorldData(ZNet.instance.GetWorldUID()).m_deathPoint = point;
	}

	// Token: 0x06000B67 RID: 2919 RVA: 0x00052718 File Offset: 0x00050918
	public void SetMapData(byte[] data)
	{
		long worldUID = ZNet.instance.GetWorldUID();
		if (worldUID != 0L)
		{
			this.GetWorldData(worldUID).m_mapData = data;
		}
	}

	// Token: 0x06000B68 RID: 2920 RVA: 0x00052740 File Offset: 0x00050940
	public byte[] GetMapData()
	{
		return this.GetWorldData(ZNet.instance.GetWorldUID()).m_mapData;
	}

	// Token: 0x06000B69 RID: 2921 RVA: 0x00052757 File Offset: 0x00050957
	public void ClearLoguoutPoint()
	{
		this.GetWorldData(ZNet.instance.GetWorldUID()).m_haveLogoutPoint = false;
	}

	// Token: 0x06000B6A RID: 2922 RVA: 0x0005276F File Offset: 0x0005096F
	public bool HaveLogoutPoint()
	{
		return this.GetWorldData(ZNet.instance.GetWorldUID()).m_haveLogoutPoint;
	}

	// Token: 0x06000B6B RID: 2923 RVA: 0x00052786 File Offset: 0x00050986
	public Vector3 GetLogoutPoint()
	{
		return this.GetWorldData(ZNet.instance.GetWorldUID()).m_logoutPoint;
	}

	// Token: 0x06000B6C RID: 2924 RVA: 0x0005279D File Offset: 0x0005099D
	public bool HaveDeathPoint()
	{
		return this.GetWorldData(ZNet.instance.GetWorldUID()).m_haveDeathPoint;
	}

	// Token: 0x06000B6D RID: 2925 RVA: 0x000527B4 File Offset: 0x000509B4
	public Vector3 GetDeathPoint()
	{
		return this.GetWorldData(ZNet.instance.GetWorldUID()).m_deathPoint;
	}

	// Token: 0x06000B6E RID: 2926 RVA: 0x000527CB File Offset: 0x000509CB
	public void SetCustomSpawnPoint(Vector3 point)
	{
		this.GetWorldData(ZNet.instance.GetWorldUID()).m_haveCustomSpawnPoint = true;
		this.GetWorldData(ZNet.instance.GetWorldUID()).m_spawnPoint = point;
	}

	// Token: 0x06000B6F RID: 2927 RVA: 0x000527F9 File Offset: 0x000509F9
	public Vector3 GetCustomSpawnPoint()
	{
		return this.GetWorldData(ZNet.instance.GetWorldUID()).m_spawnPoint;
	}

	// Token: 0x06000B70 RID: 2928 RVA: 0x00052810 File Offset: 0x00050A10
	public bool HaveCustomSpawnPoint()
	{
		return this.GetWorldData(ZNet.instance.GetWorldUID()).m_haveCustomSpawnPoint;
	}

	// Token: 0x06000B71 RID: 2929 RVA: 0x00052827 File Offset: 0x00050A27
	public void ClearCustomSpawnPoint()
	{
		this.GetWorldData(ZNet.instance.GetWorldUID()).m_haveCustomSpawnPoint = false;
	}

	// Token: 0x06000B72 RID: 2930 RVA: 0x0005283F File Offset: 0x00050A3F
	public void SetHomePoint(Vector3 point)
	{
		this.GetWorldData(ZNet.instance.GetWorldUID()).m_homePoint = point;
	}

	// Token: 0x06000B73 RID: 2931 RVA: 0x00052857 File Offset: 0x00050A57
	public Vector3 GetHomePoint()
	{
		return this.GetWorldData(ZNet.instance.GetWorldUID()).m_homePoint;
	}

	// Token: 0x06000B74 RID: 2932 RVA: 0x0005286E File Offset: 0x00050A6E
	public void SetName(string name)
	{
		this.m_playerName = name;
	}

	// Token: 0x06000B75 RID: 2933 RVA: 0x00052877 File Offset: 0x00050A77
	public string GetName()
	{
		return this.m_playerName;
	}

	// Token: 0x06000B76 RID: 2934 RVA: 0x0005287F File Offset: 0x00050A7F
	public long GetPlayerID()
	{
		return this.m_playerID;
	}

	// Token: 0x06000B77 RID: 2935 RVA: 0x00052888 File Offset: 0x00050A88
	public static List<PlayerProfile> GetAllPlayerProfiles()
	{
		string[] array;
		try
		{
			array = Directory.GetFiles(Utils.GetSaveDataPath() + "/characters", "*.fch");
		}
		catch
		{
			array = new string[0];
		}
		List<PlayerProfile> list = new List<PlayerProfile>();
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(array2[i]);
			PlayerProfile playerProfile = new PlayerProfile(fileNameWithoutExtension);
			if (!playerProfile.Load())
			{
				ZLog.Log("Failed to load " + fileNameWithoutExtension);
			}
			else
			{
				list.Add(playerProfile);
			}
		}
		return list;
	}

	// Token: 0x06000B78 RID: 2936 RVA: 0x0005291C File Offset: 0x00050B1C
	public static void RemoveProfile(string name)
	{
		try
		{
			File.Delete(Utils.GetSaveDataPath() + "/characters/" + name + ".fch");
		}
		catch
		{
		}
	}

	// Token: 0x06000B79 RID: 2937 RVA: 0x00052958 File Offset: 0x00050B58
	public static bool HaveProfile(string name)
	{
		return File.Exists(Utils.GetSaveDataPath() + "/characters/" + name + ".fch");
	}

	// Token: 0x06000B7A RID: 2938 RVA: 0x00052974 File Offset: 0x00050B74
	public string GetFilename()
	{
		return this.m_filename;
	}

	// Token: 0x06000B7B RID: 2939 RVA: 0x0005297C File Offset: 0x00050B7C
	private PlayerProfile.WorldPlayerData GetWorldData(long worldUID)
	{
		PlayerProfile.WorldPlayerData worldPlayerData;
		if (this.m_worldData.TryGetValue(worldUID, out worldPlayerData))
		{
			return worldPlayerData;
		}
		worldPlayerData = new PlayerProfile.WorldPlayerData();
		this.m_worldData.Add(worldUID, worldPlayerData);
		return worldPlayerData;
	}

	// Token: 0x04000AB7 RID: 2743
	private string m_filename = "";

	// Token: 0x04000AB8 RID: 2744
	private string m_playerName = "";

	// Token: 0x04000AB9 RID: 2745
	private long m_playerID;

	// Token: 0x04000ABA RID: 2746
	private string m_startSeed = "";

	// Token: 0x04000ABB RID: 2747
	public static Vector3 m_originalSpawnPoint = new Vector3(-676f, 50f, 299f);

	// Token: 0x04000ABC RID: 2748
	private Dictionary<long, PlayerProfile.WorldPlayerData> m_worldData = new Dictionary<long, PlayerProfile.WorldPlayerData>();

	// Token: 0x04000ABD RID: 2749
	public PlayerProfile.PlayerStats m_playerStats = new PlayerProfile.PlayerStats();

	// Token: 0x04000ABE RID: 2750
	private byte[] m_playerData;

	// Token: 0x02000181 RID: 385
	private class WorldPlayerData
	{
		// Token: 0x040011CD RID: 4557
		public Vector3 m_spawnPoint = Vector3.zero;

		// Token: 0x040011CE RID: 4558
		public bool m_haveCustomSpawnPoint;

		// Token: 0x040011CF RID: 4559
		public Vector3 m_logoutPoint = Vector3.zero;

		// Token: 0x040011D0 RID: 4560
		public bool m_haveLogoutPoint;

		// Token: 0x040011D1 RID: 4561
		public Vector3 m_deathPoint = Vector3.zero;

		// Token: 0x040011D2 RID: 4562
		public bool m_haveDeathPoint;

		// Token: 0x040011D3 RID: 4563
		public Vector3 m_homePoint = Vector3.zero;

		// Token: 0x040011D4 RID: 4564
		public byte[] m_mapData;
	}

	// Token: 0x02000182 RID: 386
	public class PlayerStats
	{
		// Token: 0x040011D5 RID: 4565
		public int m_kills;

		// Token: 0x040011D6 RID: 4566
		public int m_deaths;

		// Token: 0x040011D7 RID: 4567
		public int m_crafts;

		// Token: 0x040011D8 RID: 4568
		public int m_builds;
	}
}
