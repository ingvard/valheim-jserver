using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Token: 0x020000B5 RID: 181
public class World
{
	// Token: 0x06000BF9 RID: 3065 RVA: 0x000553E0 File Offset: 0x000535E0
	public World()
	{
		this.m_worldSavePath = World.GetWorldSavePath();
	}

	// Token: 0x06000BFA RID: 3066 RVA: 0x00055414 File Offset: 0x00053614
	public World(string name, bool loadError, bool versionError)
	{
		this.m_name = name;
		this.m_loadError = loadError;
		this.m_versionError = versionError;
		this.m_worldSavePath = World.GetWorldSavePath();
	}

	// Token: 0x06000BFB RID: 3067 RVA: 0x00055468 File Offset: 0x00053668
	public World(string name, string seed)
	{
		this.m_name = name;
		this.m_seedName = seed;
		this.m_seed = ((this.m_seedName == "") ? 0 : this.m_seedName.GetStableHashCode());
		this.m_uid = (long)name.GetStableHashCode() + Utils.GenerateUID();
		this.m_worldGenVersion = global::Version.m_worldGenVersion;
		this.m_worldSavePath = World.GetWorldSavePath();
	}

	// Token: 0x06000BFC RID: 3068 RVA: 0x000554F9 File Offset: 0x000536F9
	private static string GetWorldSavePath()
	{
		return Utils.GetSaveDataPath() + "/worlds";
	}

	// Token: 0x06000BFD RID: 3069 RVA: 0x0005550C File Offset: 0x0005370C
	public static List<World> GetWorldList()
	{
		string[] array;
		try
		{
			array = Directory.GetFiles(World.GetWorldSavePath(), "*.fwl");
		}
		catch
		{
			array = new string[0];
		}
		List<World> list = new List<World>();
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			World world = World.LoadWorld(Path.GetFileNameWithoutExtension(array2[i]));
			if (world != null)
			{
				list.Add(world);
			}
		}
		return list;
	}

	// Token: 0x06000BFE RID: 3070 RVA: 0x00055578 File Offset: 0x00053778
	public static void RemoveWorld(string name)
	{
		try
		{
			string str = World.GetWorldSavePath() + "/" + name;
			File.Delete(str + ".fwl");
			File.Delete(str + ".db");
		}
		catch
		{
		}
	}

	// Token: 0x06000BFF RID: 3071 RVA: 0x000555CC File Offset: 0x000537CC
	public string GetDBPath()
	{
		return this.m_worldSavePath + "/" + this.m_name + ".db";
	}

	// Token: 0x06000C00 RID: 3072 RVA: 0x000555E9 File Offset: 0x000537E9
	public string GetMetaPath()
	{
		return this.m_worldSavePath + "/" + this.m_name + ".fwl";
	}

	// Token: 0x06000C01 RID: 3073 RVA: 0x00055606 File Offset: 0x00053806
	public static string GetMetaPath(string name)
	{
		return World.GetWorldSavePath() + "/" + name + ".fwl";
	}

	// Token: 0x06000C02 RID: 3074 RVA: 0x0005561D File Offset: 0x0005381D
	public static bool HaveWorld(string name)
	{
		return File.Exists(World.GetWorldSavePath() + "/" + name + ".fwl");
	}

	// Token: 0x06000C03 RID: 3075 RVA: 0x0005563E File Offset: 0x0005383E
	public static World GetMenuWorld()
	{
		return new World("menu", "")
		{
			m_menu = true
		};
	}

	// Token: 0x06000C04 RID: 3076 RVA: 0x00055656 File Offset: 0x00053856
	public static World GetEditorWorld()
	{
		return new World("editor", "");
	}

	// Token: 0x06000C05 RID: 3077 RVA: 0x00055668 File Offset: 0x00053868
	public static string GenerateSeed()
	{
		string text = "";
		for (int i = 0; i < 10; i++)
		{
			text += "abcdefghijklmnpqrstuvwxyzABCDEFGHIJKLMNPQRSTUVWXYZ023456789"[UnityEngine.Random.Range(0, "abcdefghijklmnpqrstuvwxyzABCDEFGHIJKLMNPQRSTUVWXYZ023456789".Length)].ToString();
		}
		return text;
	}

	// Token: 0x06000C06 RID: 3078 RVA: 0x000556B4 File Offset: 0x000538B4
	public static World GetCreateWorld(string name)
	{
		ZLog.Log("Get create world " + name);
		World world = World.LoadWorld(name);
		if (!world.m_loadError && !world.m_versionError)
		{
			return world;
		}
		ZLog.Log(" creating");
		world = new World(name, World.GenerateSeed());
		world.SaveWorldMetaData();
		return world;
	}

	// Token: 0x06000C07 RID: 3079 RVA: 0x00055708 File Offset: 0x00053908
	public static World GetDevWorld()
	{
		World world = World.LoadWorld("DevWorld");
		if (!world.m_loadError && !world.m_versionError)
		{
			return world;
		}
		world = new World("DevWorld", "");
		world.SaveWorldMetaData();
		return world;
	}

	// Token: 0x06000C08 RID: 3080 RVA: 0x0005574C File Offset: 0x0005394C
	public void SaveWorldMetaData()
	{
		ZPackage zpackage = new ZPackage();
		zpackage.Write(global::Version.m_worldVersion);
		zpackage.Write(this.m_name);
		zpackage.Write(this.m_seedName);
		zpackage.Write(this.m_seed);
		zpackage.Write(this.m_uid);
		zpackage.Write(this.m_worldGenVersion);
		Directory.CreateDirectory(this.m_worldSavePath);
		string metaPath = this.GetMetaPath();
		string text = metaPath + ".new";
		string text2 = metaPath + ".old";
		byte[] array = zpackage.GetArray();
		FileStream fileStream = File.Create(text);
		BinaryWriter binaryWriter = new BinaryWriter(fileStream);
		binaryWriter.Write(array.Length);
		binaryWriter.Write(array);
		binaryWriter.Flush();
		fileStream.Flush(true);
		fileStream.Close();
		fileStream.Dispose();
		if (File.Exists(metaPath))
		{
			if (File.Exists(text2))
			{
				File.Delete(text2);
			}
			File.Move(metaPath, text2);
		}
		File.Move(text, metaPath);
	}

	// Token: 0x06000C09 RID: 3081 RVA: 0x00055830 File Offset: 0x00053A30
	public static World LoadWorld(string name)
	{
		FileStream fileStream = null;
		try
		{
			fileStream = File.OpenRead(World.GetMetaPath(name));
		}
		catch
		{
			if (fileStream != null)
			{
				fileStream.Dispose();
			}
			ZLog.Log("  failed to load " + name);
			return new World(name, true, false);
		}
		World result;
		try
		{
			BinaryReader binaryReader = new BinaryReader(fileStream);
			int count = binaryReader.ReadInt32();
			ZPackage zpackage = new ZPackage(binaryReader.ReadBytes(count));
			int num = zpackage.ReadInt();
			if (!global::Version.IsWorldVersionCompatible(num))
			{
				ZLog.Log("incompatible world version " + num);
				result = new World(name, false, true);
			}
			else
			{
				World world = new World();
				world.m_name = zpackage.ReadString();
				world.m_seedName = zpackage.ReadString();
				world.m_seed = zpackage.ReadInt();
				world.m_uid = zpackage.ReadLong();
				if (num >= 26)
				{
					world.m_worldGenVersion = zpackage.ReadInt();
				}
				result = world;
			}
		}
		catch
		{
			ZLog.LogWarning("  error loading world " + name);
			result = new World(name, true, false);
		}
		finally
		{
			if (fileStream != null)
			{
				fileStream.Dispose();
			}
		}
		return result;
	}

	// Token: 0x04000B1E RID: 2846
	public string m_name = "";

	// Token: 0x04000B1F RID: 2847
	public string m_seedName = "";

	// Token: 0x04000B20 RID: 2848
	public int m_seed;

	// Token: 0x04000B21 RID: 2849
	public long m_uid;

	// Token: 0x04000B22 RID: 2850
	public int m_worldGenVersion;

	// Token: 0x04000B23 RID: 2851
	public bool m_menu;

	// Token: 0x04000B24 RID: 2852
	public bool m_loadError;

	// Token: 0x04000B25 RID: 2853
	public bool m_versionError;

	// Token: 0x04000B26 RID: 2854
	private string m_worldSavePath = "";
}
