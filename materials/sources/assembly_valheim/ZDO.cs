using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000077 RID: 119
public class ZDO : IEquatable<ZDO>
{
	// Token: 0x0600075A RID: 1882 RVA: 0x0003AC3F File Offset: 0x00038E3F
	public void Initialize(ZDOMan man, ZDOID id, Vector3 position)
	{
		this.m_zdoMan = man;
		this.m_uid = id;
		this.m_position = position;
		this.m_sector = ZoneSystem.instance.GetZone(this.m_position);
		this.m_zdoMan.AddToSector(this, this.m_sector);
	}

	// Token: 0x0600075B RID: 1883 RVA: 0x0003AC7E File Offset: 0x00038E7E
	public void Initialize(ZDOMan man)
	{
		this.m_zdoMan = man;
	}

	// Token: 0x0600075D RID: 1885 RVA: 0x0003ACBE File Offset: 0x00038EBE
	public bool IsValid()
	{
		return this.m_zdoMan != null;
	}

	// Token: 0x0600075E RID: 1886 RVA: 0x0003ACCC File Offset: 0x00038ECC
	public void Reset()
	{
		this.m_uid = ZDOID.None;
		this.m_persistent = false;
		this.m_owner = 0L;
		this.m_timeCreated = 0L;
		this.m_ownerRevision = 0U;
		this.m_dataRevision = 0U;
		this.m_pgwVersion = 0;
		this.m_distant = false;
		this.m_tempSortValue = 0f;
		this.m_tempHaveRevision = false;
		this.m_prefab = 0;
		this.m_sector = Vector2i.zero;
		this.m_position = Vector3.zero;
		this.m_rotation = Quaternion.identity;
		this.ReleaseFloats();
		this.ReleaseVec3();
		this.ReleaseQuats();
		this.ReleaseInts();
		this.ReleaseLongs();
		this.ReleaseStrings();
		this.m_zdoMan = null;
	}

	// Token: 0x0600075F RID: 1887 RVA: 0x0003AD7C File Offset: 0x00038F7C
	public ZDO Clone()
	{
		ZDO zdo = base.MemberwiseClone() as ZDO;
		zdo.m_floats = null;
		zdo.m_vec3 = null;
		zdo.m_quats = null;
		zdo.m_ints = null;
		zdo.m_longs = null;
		zdo.m_strings = null;
		if (this.m_floats != null && this.m_floats.Count > 0)
		{
			zdo.InitFloats();
			zdo.m_floats.Copy(this.m_floats);
		}
		if (this.m_vec3 != null && this.m_vec3.Count > 0)
		{
			zdo.InitVec3();
			zdo.m_vec3.Copy(this.m_vec3);
		}
		if (this.m_quats != null && this.m_quats.Count > 0)
		{
			zdo.InitQuats();
			zdo.m_quats.Copy(this.m_quats);
		}
		if (this.m_ints != null && this.m_ints.Count > 0)
		{
			zdo.InitInts();
			zdo.m_ints.Copy(this.m_ints);
		}
		if (this.m_longs != null && this.m_longs.Count > 0)
		{
			zdo.InitLongs();
			zdo.m_longs.Copy(this.m_longs);
		}
		if (this.m_strings != null && this.m_strings.Count > 0)
		{
			zdo.InitStrings();
			zdo.m_strings.Copy(this.m_strings);
		}
		return zdo;
	}

	// Token: 0x06000760 RID: 1888 RVA: 0x0003AECE File Offset: 0x000390CE
	public bool Equals(ZDO other)
	{
		return this == other;
	}

	// Token: 0x06000761 RID: 1889 RVA: 0x0003AED4 File Offset: 0x000390D4
	public void Set(KeyValuePair<int, int> hashPair, ZDOID id)
	{
		this.Set(hashPair.Key, id.userID);
		this.Set(hashPair.Value, (long)((ulong)id.id));
	}

	// Token: 0x06000762 RID: 1890 RVA: 0x0003AEFF File Offset: 0x000390FF
	public static KeyValuePair<int, int> GetHashZDOID(string name)
	{
		return new KeyValuePair<int, int>((name + "_u").GetStableHashCode(), (name + "_i").GetStableHashCode());
	}

	// Token: 0x06000763 RID: 1891 RVA: 0x0003AF26 File Offset: 0x00039126
	public void Set(string name, ZDOID id)
	{
		this.Set(ZDO.GetHashZDOID(name), id);
	}

	// Token: 0x06000764 RID: 1892 RVA: 0x0003AF38 File Offset: 0x00039138
	public ZDOID GetZDOID(KeyValuePair<int, int> hashPair)
	{
		long @long = this.GetLong(hashPair.Key, 0L);
		uint num = (uint)this.GetLong(hashPair.Value, 0L);
		if (@long == 0L || num == 0U)
		{
			return ZDOID.None;
		}
		return new ZDOID(@long, num);
	}

	// Token: 0x06000765 RID: 1893 RVA: 0x0003AF79 File Offset: 0x00039179
	public ZDOID GetZDOID(string name)
	{
		return this.GetZDOID(ZDO.GetHashZDOID(name));
	}

	// Token: 0x06000766 RID: 1894 RVA: 0x0003AF88 File Offset: 0x00039188
	public void Set(string name, float value)
	{
		int stableHashCode = name.GetStableHashCode();
		this.Set(stableHashCode, value);
	}

	// Token: 0x06000767 RID: 1895 RVA: 0x0003AFA4 File Offset: 0x000391A4
	public void Set(int hash, float value)
	{
		this.InitFloats();
		float num;
		if (this.m_floats.TryGetValue(hash, out num) && num == value)
		{
			return;
		}
		this.m_floats[hash] = value;
		this.IncreseDataRevision();
	}

	// Token: 0x06000768 RID: 1896 RVA: 0x0003AFE0 File Offset: 0x000391E0
	public void Set(string name, Vector3 value)
	{
		int stableHashCode = name.GetStableHashCode();
		this.Set(stableHashCode, value);
	}

	// Token: 0x06000769 RID: 1897 RVA: 0x0003AFFC File Offset: 0x000391FC
	public void Set(int hash, Vector3 value)
	{
		this.InitVec3();
		Vector3 lhs;
		if (this.m_vec3.TryGetValue(hash, out lhs) && lhs == value)
		{
			return;
		}
		this.m_vec3[hash] = value;
		this.IncreseDataRevision();
	}

	// Token: 0x0600076A RID: 1898 RVA: 0x0003B03C File Offset: 0x0003923C
	public void Set(string name, Quaternion value)
	{
		int stableHashCode = name.GetStableHashCode();
		this.Set(stableHashCode, value);
	}

	// Token: 0x0600076B RID: 1899 RVA: 0x0003B058 File Offset: 0x00039258
	public void Set(int hash, Quaternion value)
	{
		this.InitQuats();
		Quaternion lhs;
		if (this.m_quats.TryGetValue(hash, out lhs) && lhs == value)
		{
			return;
		}
		this.m_quats[hash] = value;
		this.IncreseDataRevision();
	}

	// Token: 0x0600076C RID: 1900 RVA: 0x0003B098 File Offset: 0x00039298
	public void Set(string name, int value)
	{
		this.Set(name.GetStableHashCode(), value);
	}

	// Token: 0x0600076D RID: 1901 RVA: 0x0003B0A8 File Offset: 0x000392A8
	public void Set(int hash, int value)
	{
		this.InitInts();
		int num;
		if (this.m_ints.TryGetValue(hash, out num) && num == value)
		{
			return;
		}
		this.m_ints[hash] = value;
		this.IncreseDataRevision();
	}

	// Token: 0x0600076E RID: 1902 RVA: 0x0003B0E3 File Offset: 0x000392E3
	public void Set(string name, bool value)
	{
		this.Set(name, value ? 1 : 0);
	}

	// Token: 0x0600076F RID: 1903 RVA: 0x0003B0F3 File Offset: 0x000392F3
	public void Set(int hash, bool value)
	{
		this.Set(hash, value ? 1 : 0);
	}

	// Token: 0x06000770 RID: 1904 RVA: 0x0003B103 File Offset: 0x00039303
	public void Set(string name, long value)
	{
		this.Set(name.GetStableHashCode(), value);
	}

	// Token: 0x06000771 RID: 1905 RVA: 0x0003B114 File Offset: 0x00039314
	public void Set(int hash, long value)
	{
		this.InitLongs();
		long num;
		if (this.m_longs.TryGetValue(hash, out num) && num == value)
		{
			return;
		}
		this.m_longs[hash] = value;
		this.IncreseDataRevision();
	}

	// Token: 0x06000772 RID: 1906 RVA: 0x0003B150 File Offset: 0x00039350
	public void Set(string name, byte[] bytes)
	{
		string value = Convert.ToBase64String(bytes);
		this.Set(name, value);
	}

	// Token: 0x06000773 RID: 1907 RVA: 0x0003B16C File Offset: 0x0003936C
	public byte[] GetByteArray(string name)
	{
		string @string = this.GetString(name, "");
		if (@string.Length > 0)
		{
			return Convert.FromBase64String(@string);
		}
		return null;
	}

	// Token: 0x06000774 RID: 1908 RVA: 0x0003B198 File Offset: 0x00039398
	public void Set(string name, string value)
	{
		this.InitStrings();
		int stableHashCode = name.GetStableHashCode();
		string a;
		if (this.m_strings.TryGetValue(stableHashCode, out a) && a == value)
		{
			return;
		}
		this.m_strings[stableHashCode] = value;
		this.IncreseDataRevision();
	}

	// Token: 0x06000775 RID: 1909 RVA: 0x0003B1DF File Offset: 0x000393DF
	public void SetPosition(Vector3 pos)
	{
		this.InternalSetPosition(pos);
	}

	// Token: 0x06000776 RID: 1910 RVA: 0x0003B1E8 File Offset: 0x000393E8
	public void InternalSetPosition(Vector3 pos)
	{
		if (this.m_position == pos)
		{
			return;
		}
		this.m_position = pos;
		this.SetSector(ZoneSystem.instance.GetZone(this.m_position));
		if (this.IsOwner())
		{
			this.IncreseDataRevision();
		}
	}

	// Token: 0x06000777 RID: 1911 RVA: 0x0003B224 File Offset: 0x00039424
	public void InvalidateSector()
	{
		this.SetSector(new Vector2i(-100000, -10000));
	}

	// Token: 0x06000778 RID: 1912 RVA: 0x0003B23C File Offset: 0x0003943C
	private void SetSector(Vector2i sector)
	{
		if (this.m_sector == sector)
		{
			return;
		}
		this.m_zdoMan.RemoveFromSector(this, this.m_sector);
		this.m_sector = sector;
		this.m_zdoMan.AddToSector(this, this.m_sector);
		if (ZNet.instance.IsServer())
		{
			this.m_zdoMan.ZDOSectorInvalidated(this);
		}
	}

	// Token: 0x06000779 RID: 1913 RVA: 0x0003B29B File Offset: 0x0003949B
	public Vector2i GetSector()
	{
		return this.m_sector;
	}

	// Token: 0x0600077A RID: 1914 RVA: 0x0003B2A3 File Offset: 0x000394A3
	public void SetRotation(Quaternion rot)
	{
		if (this.m_rotation == rot)
		{
			return;
		}
		this.m_rotation = rot;
		this.IncreseDataRevision();
	}

	// Token: 0x0600077B RID: 1915 RVA: 0x0003B2C1 File Offset: 0x000394C1
	public void SetType(ZDO.ObjectType type)
	{
		if (this.m_type == type)
		{
			return;
		}
		this.m_type = type;
		this.IncreseDataRevision();
	}

	// Token: 0x0600077C RID: 1916 RVA: 0x0003B2DA File Offset: 0x000394DA
	public void SetDistant(bool distant)
	{
		if (this.m_distant == distant)
		{
			return;
		}
		this.m_distant = distant;
		this.IncreseDataRevision();
	}

	// Token: 0x0600077D RID: 1917 RVA: 0x0003B2F3 File Offset: 0x000394F3
	public void SetPrefab(int prefab)
	{
		if (this.m_prefab == prefab)
		{
			return;
		}
		this.m_prefab = prefab;
		this.IncreseDataRevision();
	}

	// Token: 0x0600077E RID: 1918 RVA: 0x0003B30C File Offset: 0x0003950C
	public int GetPrefab()
	{
		return this.m_prefab;
	}

	// Token: 0x0600077F RID: 1919 RVA: 0x0003B314 File Offset: 0x00039514
	public Vector3 GetPosition()
	{
		return this.m_position;
	}

	// Token: 0x06000780 RID: 1920 RVA: 0x0003B31C File Offset: 0x0003951C
	public Quaternion GetRotation()
	{
		return this.m_rotation;
	}

	// Token: 0x06000781 RID: 1921 RVA: 0x0003B324 File Offset: 0x00039524
	private void IncreseDataRevision()
	{
		this.m_dataRevision += 1U;
		if (!ZNet.instance.IsServer())
		{
			ZDOMan.instance.ClientChanged(this.m_uid);
		}
	}

	// Token: 0x06000782 RID: 1922 RVA: 0x0003B350 File Offset: 0x00039550
	private void IncreseOwnerRevision()
	{
		this.m_ownerRevision += 1U;
		if (!ZNet.instance.IsServer())
		{
			ZDOMan.instance.ClientChanged(this.m_uid);
		}
	}

	// Token: 0x06000783 RID: 1923 RVA: 0x0003B37C File Offset: 0x0003957C
	public float GetFloat(string name, float defaultValue = 0f)
	{
		return this.GetFloat(name.GetStableHashCode(), defaultValue);
	}

	// Token: 0x06000784 RID: 1924 RVA: 0x0003B38C File Offset: 0x0003958C
	public float GetFloat(int hash, float defaultValue = 0f)
	{
		if (this.m_floats == null)
		{
			return defaultValue;
		}
		float result;
		if (this.m_floats.TryGetValue(hash, out result))
		{
			return result;
		}
		return defaultValue;
	}

	// Token: 0x06000785 RID: 1925 RVA: 0x0003B3B6 File Offset: 0x000395B6
	public Vector3 GetVec3(string name, Vector3 defaultValue)
	{
		return this.GetVec3(name.GetStableHashCode(), defaultValue);
	}

	// Token: 0x06000786 RID: 1926 RVA: 0x0003B3C8 File Offset: 0x000395C8
	public Vector3 GetVec3(int hash, Vector3 defaultValue)
	{
		if (this.m_vec3 == null)
		{
			return defaultValue;
		}
		Vector3 result;
		if (this.m_vec3.TryGetValue(hash, out result))
		{
			return result;
		}
		return defaultValue;
	}

	// Token: 0x06000787 RID: 1927 RVA: 0x0003B3F2 File Offset: 0x000395F2
	public Quaternion GetQuaternion(string name, Quaternion defaultValue)
	{
		return this.GetQuaternion(name.GetStableHashCode(), defaultValue);
	}

	// Token: 0x06000788 RID: 1928 RVA: 0x0003B404 File Offset: 0x00039604
	public Quaternion GetQuaternion(int hash, Quaternion defaultValue)
	{
		if (this.m_quats == null)
		{
			return defaultValue;
		}
		Quaternion result;
		if (this.m_quats.TryGetValue(hash, out result))
		{
			return result;
		}
		return defaultValue;
	}

	// Token: 0x06000789 RID: 1929 RVA: 0x0003B42E File Offset: 0x0003962E
	public int GetInt(string name, int defaultValue = 0)
	{
		return this.GetInt(name.GetStableHashCode(), defaultValue);
	}

	// Token: 0x0600078A RID: 1930 RVA: 0x0003B440 File Offset: 0x00039640
	public int GetInt(int hash, int defaultValue = 0)
	{
		if (this.m_ints == null)
		{
			return defaultValue;
		}
		int result;
		if (this.m_ints.TryGetValue(hash, out result))
		{
			return result;
		}
		return defaultValue;
	}

	// Token: 0x0600078B RID: 1931 RVA: 0x0003B46A File Offset: 0x0003966A
	public bool GetBool(string name, bool defaultValue = false)
	{
		return this.GetBool(name.GetStableHashCode(), defaultValue);
	}

	// Token: 0x0600078C RID: 1932 RVA: 0x0003B47C File Offset: 0x0003967C
	public bool GetBool(int hash, bool defaultValue = false)
	{
		if (this.m_ints == null)
		{
			return defaultValue;
		}
		int num;
		if (this.m_ints.TryGetValue(hash, out num))
		{
			return num != 0;
		}
		return defaultValue;
	}

	// Token: 0x0600078D RID: 1933 RVA: 0x0003B4A9 File Offset: 0x000396A9
	public long GetLong(string name, long defaultValue = 0L)
	{
		return this.GetLong(name.GetStableHashCode(), defaultValue);
	}

	// Token: 0x0600078E RID: 1934 RVA: 0x0003B4B8 File Offset: 0x000396B8
	public long GetLong(int hash, long defaultValue = 0L)
	{
		if (this.m_longs == null)
		{
			return defaultValue;
		}
		long result;
		if (this.m_longs.TryGetValue(hash, out result))
		{
			return result;
		}
		return defaultValue;
	}

	// Token: 0x0600078F RID: 1935 RVA: 0x0003B4E4 File Offset: 0x000396E4
	public string GetString(string name, string defaultValue = "")
	{
		if (this.m_strings == null)
		{
			return defaultValue;
		}
		string result;
		if (this.m_strings.TryGetValue(name.GetStableHashCode(), out result))
		{
			return result;
		}
		return defaultValue;
	}

	// Token: 0x06000790 RID: 1936 RVA: 0x0003B514 File Offset: 0x00039714
	public void Serialize(ZPackage pkg)
	{
		pkg.Write(this.m_persistent);
		pkg.Write(this.m_distant);
		pkg.Write(this.m_timeCreated);
		pkg.Write(this.m_pgwVersion);
		pkg.Write((sbyte)this.m_type);
		pkg.Write(this.m_prefab);
		pkg.Write(this.m_rotation);
		int num = 0;
		if (this.m_floats != null && this.m_floats.Count > 0)
		{
			num |= 1;
		}
		if (this.m_vec3 != null && this.m_vec3.Count > 0)
		{
			num |= 2;
		}
		if (this.m_quats != null && this.m_quats.Count > 0)
		{
			num |= 4;
		}
		if (this.m_ints != null && this.m_ints.Count > 0)
		{
			num |= 8;
		}
		if (this.m_strings != null && this.m_strings.Count > 0)
		{
			num |= 16;
		}
		if (this.m_longs != null && this.m_longs.Count > 0)
		{
			num |= 64;
		}
		pkg.Write(num);
		if (this.m_floats != null && this.m_floats.Count > 0)
		{
			pkg.Write((byte)this.m_floats.Count);
			foreach (KeyValuePair<int, float> keyValuePair in this.m_floats)
			{
				pkg.Write(keyValuePair.Key);
				pkg.Write(keyValuePair.Value);
			}
		}
		if (this.m_vec3 != null && this.m_vec3.Count > 0)
		{
			pkg.Write((byte)this.m_vec3.Count);
			foreach (KeyValuePair<int, Vector3> keyValuePair2 in this.m_vec3)
			{
				pkg.Write(keyValuePair2.Key);
				pkg.Write(keyValuePair2.Value);
			}
		}
		if (this.m_quats != null && this.m_quats.Count > 0)
		{
			pkg.Write((byte)this.m_quats.Count);
			foreach (KeyValuePair<int, Quaternion> keyValuePair3 in this.m_quats)
			{
				pkg.Write(keyValuePair3.Key);
				pkg.Write(keyValuePair3.Value);
			}
		}
		if (this.m_ints != null && this.m_ints.Count > 0)
		{
			pkg.Write((byte)this.m_ints.Count);
			foreach (KeyValuePair<int, int> keyValuePair4 in this.m_ints)
			{
				pkg.Write(keyValuePair4.Key);
				pkg.Write(keyValuePair4.Value);
			}
		}
		if (this.m_longs != null && this.m_longs.Count > 0)
		{
			pkg.Write((byte)this.m_longs.Count);
			foreach (KeyValuePair<int, long> keyValuePair5 in this.m_longs)
			{
				pkg.Write(keyValuePair5.Key);
				pkg.Write(keyValuePair5.Value);
			}
		}
		if (this.m_strings != null && this.m_strings.Count > 0)
		{
			pkg.Write((byte)this.m_strings.Count);
			foreach (KeyValuePair<int, string> keyValuePair6 in this.m_strings)
			{
				pkg.Write(keyValuePair6.Key);
				pkg.Write(keyValuePair6.Value);
			}
		}
	}

	// Token: 0x06000791 RID: 1937 RVA: 0x0003B918 File Offset: 0x00039B18
	public void Deserialize(ZPackage pkg)
	{
		this.m_persistent = pkg.ReadBool();
		this.m_distant = pkg.ReadBool();
		this.m_timeCreated = pkg.ReadLong();
		this.m_pgwVersion = pkg.ReadInt();
		this.m_type = (ZDO.ObjectType)pkg.ReadSByte();
		this.m_prefab = pkg.ReadInt();
		this.m_rotation = pkg.ReadQuaternion();
		int num = pkg.ReadInt();
		if ((num & 1) != 0)
		{
			this.InitFloats();
			int num2 = (int)pkg.ReadByte();
			for (int i = 0; i < num2; i++)
			{
				int key = pkg.ReadInt();
				this.m_floats[key] = pkg.ReadSingle();
			}
		}
		else
		{
			this.ReleaseFloats();
		}
		if ((num & 2) != 0)
		{
			this.InitVec3();
			int num3 = (int)pkg.ReadByte();
			for (int j = 0; j < num3; j++)
			{
				int key2 = pkg.ReadInt();
				this.m_vec3[key2] = pkg.ReadVector3();
			}
		}
		else
		{
			this.ReleaseVec3();
		}
		if ((num & 4) != 0)
		{
			this.InitQuats();
			int num4 = (int)pkg.ReadByte();
			for (int k = 0; k < num4; k++)
			{
				int key3 = pkg.ReadInt();
				this.m_quats[key3] = pkg.ReadQuaternion();
			}
		}
		else
		{
			this.ReleaseQuats();
		}
		if ((num & 8) != 0)
		{
			this.InitInts();
			int num5 = (int)pkg.ReadByte();
			for (int l = 0; l < num5; l++)
			{
				int key4 = pkg.ReadInt();
				this.m_ints[key4] = pkg.ReadInt();
			}
		}
		else
		{
			this.ReleaseInts();
		}
		if ((num & 64) != 0)
		{
			this.InitLongs();
			int num6 = (int)pkg.ReadByte();
			for (int m = 0; m < num6; m++)
			{
				int key5 = pkg.ReadInt();
				this.m_longs[key5] = pkg.ReadLong();
			}
		}
		else
		{
			this.ReleaseLongs();
		}
		if ((num & 16) != 0)
		{
			this.InitStrings();
			int num7 = (int)pkg.ReadByte();
			for (int n = 0; n < num7; n++)
			{
				int key6 = pkg.ReadInt();
				this.m_strings[key6] = pkg.ReadString();
			}
			return;
		}
		this.ReleaseStrings();
	}

	// Token: 0x06000792 RID: 1938 RVA: 0x0003BB24 File Offset: 0x00039D24
	public void Save(ZPackage pkg)
	{
		pkg.Write(this.m_ownerRevision);
		pkg.Write(this.m_dataRevision);
		pkg.Write(this.m_persistent);
		pkg.Write(this.m_owner);
		pkg.Write(this.m_timeCreated);
		pkg.Write(this.m_pgwVersion);
		pkg.Write((sbyte)this.m_type);
		pkg.Write(this.m_distant);
		pkg.Write(this.m_prefab);
		pkg.Write(this.m_sector);
		pkg.Write(this.m_position);
		pkg.Write(this.m_rotation);
		if (this.m_floats != null)
		{
			pkg.Write((char)this.m_floats.Count);
			using (Dictionary<int, float>.Enumerator enumerator = this.m_floats.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<int, float> keyValuePair = enumerator.Current;
					pkg.Write(keyValuePair.Key);
					pkg.Write(keyValuePair.Value);
				}
				goto IL_FB;
			}
		}
		pkg.Write('\0');
		IL_FB:
		if (this.m_vec3 != null)
		{
			pkg.Write((char)this.m_vec3.Count);
			using (Dictionary<int, Vector3>.Enumerator enumerator2 = this.m_vec3.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					KeyValuePair<int, Vector3> keyValuePair2 = enumerator2.Current;
					pkg.Write(keyValuePair2.Key);
					pkg.Write(keyValuePair2.Value);
				}
				goto IL_165;
			}
		}
		pkg.Write('\0');
		IL_165:
		if (this.m_quats != null)
		{
			pkg.Write((char)this.m_quats.Count);
			using (Dictionary<int, Quaternion>.Enumerator enumerator3 = this.m_quats.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					KeyValuePair<int, Quaternion> keyValuePair3 = enumerator3.Current;
					pkg.Write(keyValuePair3.Key);
					pkg.Write(keyValuePair3.Value);
				}
				goto IL_1D1;
			}
		}
		pkg.Write('\0');
		IL_1D1:
		if (this.m_ints != null)
		{
			pkg.Write((char)this.m_ints.Count);
			using (Dictionary<int, int>.Enumerator enumerator4 = this.m_ints.GetEnumerator())
			{
				while (enumerator4.MoveNext())
				{
					KeyValuePair<int, int> keyValuePair4 = enumerator4.Current;
					pkg.Write(keyValuePair4.Key);
					pkg.Write(keyValuePair4.Value);
				}
				goto IL_23D;
			}
		}
		pkg.Write('\0');
		IL_23D:
		if (this.m_longs != null)
		{
			pkg.Write((char)this.m_longs.Count);
			using (Dictionary<int, long>.Enumerator enumerator5 = this.m_longs.GetEnumerator())
			{
				while (enumerator5.MoveNext())
				{
					KeyValuePair<int, long> keyValuePair5 = enumerator5.Current;
					pkg.Write(keyValuePair5.Key);
					pkg.Write(keyValuePair5.Value);
				}
				goto IL_2A9;
			}
		}
		pkg.Write('\0');
		IL_2A9:
		if (this.m_strings != null)
		{
			pkg.Write((char)this.m_strings.Count);
			using (Dictionary<int, string>.Enumerator enumerator6 = this.m_strings.GetEnumerator())
			{
				while (enumerator6.MoveNext())
				{
					KeyValuePair<int, string> keyValuePair6 = enumerator6.Current;
					pkg.Write(keyValuePair6.Key);
					pkg.Write(keyValuePair6.Value);
				}
				return;
			}
		}
		pkg.Write('\0');
	}

	// Token: 0x06000793 RID: 1939 RVA: 0x0003BE94 File Offset: 0x0003A094
	public void Load(ZPackage pkg, int version)
	{
		this.m_ownerRevision = pkg.ReadUInt();
		this.m_dataRevision = pkg.ReadUInt();
		this.m_persistent = pkg.ReadBool();
		this.m_owner = pkg.ReadLong();
		this.m_timeCreated = pkg.ReadLong();
		this.m_pgwVersion = pkg.ReadInt();
		if (version >= 16 && version < 24)
		{
			pkg.ReadInt();
		}
		if (version >= 23)
		{
			this.m_type = (ZDO.ObjectType)pkg.ReadSByte();
		}
		if (version >= 22)
		{
			this.m_distant = pkg.ReadBool();
		}
		if (version < 13)
		{
			pkg.ReadChar();
			pkg.ReadChar();
		}
		if (version >= 17)
		{
			this.m_prefab = pkg.ReadInt();
		}
		this.m_sector = pkg.ReadVector2i();
		this.m_position = pkg.ReadVector3();
		this.m_rotation = pkg.ReadQuaternion();
		int num = (int)pkg.ReadChar();
		if (num > 0)
		{
			this.InitFloats();
			for (int i = 0; i < num; i++)
			{
				int key = pkg.ReadInt();
				this.m_floats[key] = pkg.ReadSingle();
			}
		}
		else
		{
			this.ReleaseFloats();
		}
		int num2 = (int)pkg.ReadChar();
		if (num2 > 0)
		{
			this.InitVec3();
			for (int j = 0; j < num2; j++)
			{
				int key2 = pkg.ReadInt();
				this.m_vec3[key2] = pkg.ReadVector3();
			}
		}
		else
		{
			this.ReleaseVec3();
		}
		int num3 = (int)pkg.ReadChar();
		if (num3 > 0)
		{
			this.InitQuats();
			for (int k = 0; k < num3; k++)
			{
				int key3 = pkg.ReadInt();
				this.m_quats[key3] = pkg.ReadQuaternion();
			}
		}
		else
		{
			this.ReleaseQuats();
		}
		int num4 = (int)pkg.ReadChar();
		if (num4 > 0)
		{
			this.InitInts();
			for (int l = 0; l < num4; l++)
			{
				int key4 = pkg.ReadInt();
				this.m_ints[key4] = pkg.ReadInt();
			}
		}
		else
		{
			this.ReleaseInts();
		}
		int num5 = (int)pkg.ReadChar();
		if (num5 > 0)
		{
			this.InitLongs();
			for (int m = 0; m < num5; m++)
			{
				int key5 = pkg.ReadInt();
				this.m_longs[key5] = pkg.ReadLong();
			}
		}
		else
		{
			this.ReleaseLongs();
		}
		int num6 = (int)pkg.ReadChar();
		if (num6 > 0)
		{
			this.InitStrings();
			for (int n = 0; n < num6; n++)
			{
				int key6 = pkg.ReadInt();
				this.m_strings[key6] = pkg.ReadString();
			}
		}
		else
		{
			this.ReleaseStrings();
		}
		if (version < 17)
		{
			this.m_prefab = this.GetInt("prefab", 0);
		}
	}

	// Token: 0x06000794 RID: 1940 RVA: 0x0003C119 File Offset: 0x0003A319
	public bool IsOwner()
	{
		return this.m_owner == this.m_zdoMan.GetMyID();
	}

	// Token: 0x06000795 RID: 1941 RVA: 0x0003C12E File Offset: 0x0003A32E
	public bool HasOwner()
	{
		return this.m_owner != 0L;
	}

	// Token: 0x06000796 RID: 1942 RVA: 0x0003C13C File Offset: 0x0003A33C
	public void Print()
	{
		ZLog.Log("UID:" + this.m_uid);
		ZLog.Log("Persistent:" + this.m_persistent.ToString());
		ZLog.Log("Owner:" + this.m_owner);
		ZLog.Log("Revision:" + this.m_ownerRevision);
		if (this.m_floats != null)
		{
			foreach (KeyValuePair<int, float> keyValuePair in this.m_floats)
			{
				ZLog.Log(string.Concat(new object[]
				{
					"F:",
					keyValuePair.Key,
					" = ",
					keyValuePair.Value
				}));
			}
		}
		if (this.m_vec3 != null)
		{
			foreach (KeyValuePair<int, Vector3> keyValuePair2 in this.m_vec3)
			{
				ZLog.Log(string.Concat(new object[]
				{
					"V:",
					keyValuePair2.Key,
					" = ",
					keyValuePair2.Value
				}));
			}
		}
		if (this.m_quats != null)
		{
			foreach (KeyValuePair<int, Quaternion> keyValuePair3 in this.m_quats)
			{
				ZLog.Log(string.Concat(new object[]
				{
					"Q:",
					keyValuePair3.Key,
					" = ",
					keyValuePair3.Value
				}));
			}
		}
		if (this.m_ints != null)
		{
			foreach (KeyValuePair<int, int> keyValuePair4 in this.m_ints)
			{
				ZLog.Log(string.Concat(new object[]
				{
					"I:",
					keyValuePair4.Key,
					" = ",
					keyValuePair4.Value
				}));
			}
		}
		if (this.m_longs != null)
		{
			foreach (KeyValuePair<int, long> keyValuePair5 in this.m_longs)
			{
				ZLog.Log(string.Concat(new object[]
				{
					"L:",
					keyValuePair5.Key,
					" = ",
					keyValuePair5.Value
				}));
			}
		}
		if (this.m_strings != null)
		{
			foreach (KeyValuePair<int, string> keyValuePair6 in this.m_strings)
			{
				ZLog.Log(string.Concat(new object[]
				{
					"S:",
					keyValuePair6.Key,
					" = ",
					keyValuePair6.Value
				}));
			}
		}
	}

	// Token: 0x06000797 RID: 1943 RVA: 0x0003C4C0 File Offset: 0x0003A6C0
	public void SetOwner(long uid)
	{
		if (this.m_owner == uid)
		{
			return;
		}
		this.m_owner = uid;
		this.IncreseOwnerRevision();
	}

	// Token: 0x06000798 RID: 1944 RVA: 0x0003C4D9 File Offset: 0x0003A6D9
	public void SetPGWVersion(int version)
	{
		this.m_pgwVersion = version;
	}

	// Token: 0x06000799 RID: 1945 RVA: 0x0003C4E2 File Offset: 0x0003A6E2
	public int GetPGWVersion()
	{
		return this.m_pgwVersion;
	}

	// Token: 0x0600079A RID: 1946 RVA: 0x0003C4EA File Offset: 0x0003A6EA
	private void InitFloats()
	{
		if (this.m_floats == null)
		{
			this.m_floats = Pool<Dictionary<int, float>>.Create();
			this.m_floats.Clear();
		}
	}

	// Token: 0x0600079B RID: 1947 RVA: 0x0003C50A File Offset: 0x0003A70A
	private void InitVec3()
	{
		if (this.m_vec3 == null)
		{
			this.m_vec3 = Pool<Dictionary<int, Vector3>>.Create();
			this.m_vec3.Clear();
		}
	}

	// Token: 0x0600079C RID: 1948 RVA: 0x0003C52A File Offset: 0x0003A72A
	private void InitQuats()
	{
		if (this.m_quats == null)
		{
			this.m_quats = Pool<Dictionary<int, Quaternion>>.Create();
			this.m_quats.Clear();
		}
	}

	// Token: 0x0600079D RID: 1949 RVA: 0x0003C54A File Offset: 0x0003A74A
	private void InitInts()
	{
		if (this.m_ints == null)
		{
			this.m_ints = Pool<Dictionary<int, int>>.Create();
			this.m_ints.Clear();
		}
	}

	// Token: 0x0600079E RID: 1950 RVA: 0x0003C56A File Offset: 0x0003A76A
	private void InitLongs()
	{
		if (this.m_longs == null)
		{
			this.m_longs = Pool<Dictionary<int, long>>.Create();
			this.m_longs.Clear();
		}
	}

	// Token: 0x0600079F RID: 1951 RVA: 0x0003C58A File Offset: 0x0003A78A
	private void InitStrings()
	{
		if (this.m_strings == null)
		{
			this.m_strings = Pool<Dictionary<int, string>>.Create();
			this.m_strings.Clear();
		}
	}

	// Token: 0x060007A0 RID: 1952 RVA: 0x0003C5AA File Offset: 0x0003A7AA
	private void ReleaseFloats()
	{
		if (this.m_floats != null)
		{
			Pool<Dictionary<int, float>>.Release(this.m_floats);
			this.m_floats = null;
		}
	}

	// Token: 0x060007A1 RID: 1953 RVA: 0x0003C5C6 File Offset: 0x0003A7C6
	private void ReleaseVec3()
	{
		if (this.m_vec3 != null)
		{
			Pool<Dictionary<int, Vector3>>.Release(this.m_vec3);
			this.m_vec3 = null;
		}
	}

	// Token: 0x060007A2 RID: 1954 RVA: 0x0003C5E2 File Offset: 0x0003A7E2
	private void ReleaseQuats()
	{
		if (this.m_quats != null)
		{
			Pool<Dictionary<int, Quaternion>>.Release(this.m_quats);
			this.m_quats = null;
		}
	}

	// Token: 0x060007A3 RID: 1955 RVA: 0x0003C5FE File Offset: 0x0003A7FE
	private void ReleaseInts()
	{
		if (this.m_ints != null)
		{
			Pool<Dictionary<int, int>>.Release(this.m_ints);
			this.m_ints = null;
		}
	}

	// Token: 0x060007A4 RID: 1956 RVA: 0x0003C61A File Offset: 0x0003A81A
	private void ReleaseLongs()
	{
		if (this.m_longs != null)
		{
			Pool<Dictionary<int, long>>.Release(this.m_longs);
			this.m_longs = null;
		}
	}

	// Token: 0x060007A5 RID: 1957 RVA: 0x0003C636 File Offset: 0x0003A836
	private void ReleaseStrings()
	{
		if (this.m_strings != null)
		{
			Pool<Dictionary<int, string>>.Release(this.m_strings);
			this.m_strings = null;
		}
	}

	// Token: 0x040007BE RID: 1982
	public ZDOID m_uid;

	// Token: 0x040007BF RID: 1983
	public bool m_persistent;

	// Token: 0x040007C0 RID: 1984
	public bool m_distant;

	// Token: 0x040007C1 RID: 1985
	public long m_owner;

	// Token: 0x040007C2 RID: 1986
	public long m_timeCreated;

	// Token: 0x040007C3 RID: 1987
	public uint m_ownerRevision;

	// Token: 0x040007C4 RID: 1988
	public uint m_dataRevision;

	// Token: 0x040007C5 RID: 1989
	public int m_pgwVersion;

	// Token: 0x040007C6 RID: 1990
	public ZDO.ObjectType m_type;

	// Token: 0x040007C7 RID: 1991
	public float m_tempSortValue;

	// Token: 0x040007C8 RID: 1992
	public bool m_tempHaveRevision;

	// Token: 0x040007C9 RID: 1993
	public int m_tempRemoveEarmark = -1;

	// Token: 0x040007CA RID: 1994
	public int m_tempCreateEarmark = -1;

	// Token: 0x040007CB RID: 1995
	private int m_prefab;

	// Token: 0x040007CC RID: 1996
	private Vector2i m_sector = Vector2i.zero;

	// Token: 0x040007CD RID: 1997
	private Vector3 m_position = Vector3.zero;

	// Token: 0x040007CE RID: 1998
	private Quaternion m_rotation = Quaternion.identity;

	// Token: 0x040007CF RID: 1999
	private Dictionary<int, float> m_floats;

	// Token: 0x040007D0 RID: 2000
	private Dictionary<int, Vector3> m_vec3;

	// Token: 0x040007D1 RID: 2001
	private Dictionary<int, Quaternion> m_quats;

	// Token: 0x040007D2 RID: 2002
	private Dictionary<int, int> m_ints;

	// Token: 0x040007D3 RID: 2003
	private Dictionary<int, long> m_longs;

	// Token: 0x040007D4 RID: 2004
	private Dictionary<int, string> m_strings;

	// Token: 0x040007D5 RID: 2005
	private ZDOMan m_zdoMan;

	// Token: 0x02000168 RID: 360
	public enum ObjectType
	{
		// Token: 0x04001164 RID: 4452
		Default,
		// Token: 0x04001165 RID: 4453
		Prioritized,
		// Token: 0x04001166 RID: 4454
		Solid
	}
}
