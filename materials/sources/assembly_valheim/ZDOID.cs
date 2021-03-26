using System;
using System.IO;

// Token: 0x02000078 RID: 120
public struct ZDOID : IEquatable<ZDOID>
{
	// Token: 0x060007A5 RID: 1957 RVA: 0x0003C59E File Offset: 0x0003A79E
	public ZDOID(BinaryReader reader)
	{
		this.m_userID = reader.ReadInt64();
		this.m_id = reader.ReadUInt32();
		this.m_hash = 0;
	}

	// Token: 0x060007A6 RID: 1958 RVA: 0x0003C5BF File Offset: 0x0003A7BF
	public ZDOID(long userID, uint id)
	{
		this.m_userID = userID;
		this.m_id = id;
		this.m_hash = 0;
	}

	// Token: 0x060007A7 RID: 1959 RVA: 0x0003C5D6 File Offset: 0x0003A7D6
	public ZDOID(ZDOID other)
	{
		this.m_userID = other.m_userID;
		this.m_id = other.m_id;
		this.m_hash = other.m_hash;
	}

	// Token: 0x060007A8 RID: 1960 RVA: 0x0003C5FC File Offset: 0x0003A7FC
	public override string ToString()
	{
		return this.m_userID.ToString() + ":" + this.m_id.ToString();
	}

	// Token: 0x060007A9 RID: 1961 RVA: 0x0003C61E File Offset: 0x0003A81E
	public static bool operator ==(ZDOID a, ZDOID b)
	{
		return a.m_userID == b.m_userID && a.m_id == b.m_id;
	}

	// Token: 0x060007AA RID: 1962 RVA: 0x0003C63E File Offset: 0x0003A83E
	public static bool operator !=(ZDOID a, ZDOID b)
	{
		return a.m_userID != b.m_userID || a.m_id != b.m_id;
	}

	// Token: 0x060007AB RID: 1963 RVA: 0x0003C661 File Offset: 0x0003A861
	public bool Equals(ZDOID other)
	{
		return other.m_userID == this.m_userID && other.m_id == this.m_id;
	}

	// Token: 0x060007AC RID: 1964 RVA: 0x0003C684 File Offset: 0x0003A884
	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is ZDOID)
		{
			ZDOID zdoid = (ZDOID)obj;
			return zdoid.m_userID == this.m_userID && zdoid.m_id == this.m_id;
		}
		return false;
	}

	// Token: 0x060007AD RID: 1965 RVA: 0x0003C6C5 File Offset: 0x0003A8C5
	public override int GetHashCode()
	{
		if (this.m_hash == 0)
		{
			this.m_hash = (this.m_userID.GetHashCode() ^ this.m_id.GetHashCode());
		}
		return this.m_hash;
	}

	// Token: 0x060007AE RID: 1966 RVA: 0x0003C6F2 File Offset: 0x0003A8F2
	public bool IsNone()
	{
		return this.m_userID == 0L && this.m_id == 0U;
	}

	// Token: 0x17000016 RID: 22
	// (get) Token: 0x060007AF RID: 1967 RVA: 0x0003C707 File Offset: 0x0003A907
	public long userID
	{
		get
		{
			return this.m_userID;
		}
	}

	// Token: 0x17000017 RID: 23
	// (get) Token: 0x060007B0 RID: 1968 RVA: 0x0003C70F File Offset: 0x0003A90F
	public uint id
	{
		get
		{
			return this.m_id;
		}
	}

	// Token: 0x040007D2 RID: 2002
	public static ZDOID None = new ZDOID(0L, 0U);

	// Token: 0x040007D3 RID: 2003
	private long m_userID;

	// Token: 0x040007D4 RID: 2004
	private uint m_id;

	// Token: 0x040007D5 RID: 2005
	private int m_hash;
}
