using System;
using System.IO;

// Token: 0x02000078 RID: 120
public struct ZDOID : IEquatable<ZDOID>
{
	// Token: 0x060007A6 RID: 1958 RVA: 0x0003C652 File Offset: 0x0003A852
	public ZDOID(BinaryReader reader)
	{
		this.m_userID = reader.ReadInt64();
		this.m_id = reader.ReadUInt32();
		this.m_hash = 0;
	}

	// Token: 0x060007A7 RID: 1959 RVA: 0x0003C673 File Offset: 0x0003A873
	public ZDOID(long userID, uint id)
	{
		this.m_userID = userID;
		this.m_id = id;
		this.m_hash = 0;
	}

	// Token: 0x060007A8 RID: 1960 RVA: 0x0003C68A File Offset: 0x0003A88A
	public ZDOID(ZDOID other)
	{
		this.m_userID = other.m_userID;
		this.m_id = other.m_id;
		this.m_hash = other.m_hash;
	}

	// Token: 0x060007A9 RID: 1961 RVA: 0x0003C6B0 File Offset: 0x0003A8B0
	public override string ToString()
	{
		return this.m_userID.ToString() + ":" + this.m_id.ToString();
	}

	// Token: 0x060007AA RID: 1962 RVA: 0x0003C6D2 File Offset: 0x0003A8D2
	public static bool operator ==(ZDOID a, ZDOID b)
	{
		return a.m_userID == b.m_userID && a.m_id == b.m_id;
	}

	// Token: 0x060007AB RID: 1963 RVA: 0x0003C6F2 File Offset: 0x0003A8F2
	public static bool operator !=(ZDOID a, ZDOID b)
	{
		return a.m_userID != b.m_userID || a.m_id != b.m_id;
	}

	// Token: 0x060007AC RID: 1964 RVA: 0x0003C715 File Offset: 0x0003A915
	public bool Equals(ZDOID other)
	{
		return other.m_userID == this.m_userID && other.m_id == this.m_id;
	}

	// Token: 0x060007AD RID: 1965 RVA: 0x0003C738 File Offset: 0x0003A938
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

	// Token: 0x060007AE RID: 1966 RVA: 0x0003C779 File Offset: 0x0003A979
	public override int GetHashCode()
	{
		if (this.m_hash == 0)
		{
			this.m_hash = (this.m_userID.GetHashCode() ^ this.m_id.GetHashCode());
		}
		return this.m_hash;
	}

	// Token: 0x060007AF RID: 1967 RVA: 0x0003C7A6 File Offset: 0x0003A9A6
	public bool IsNone()
	{
		return this.m_userID == 0L && this.m_id == 0U;
	}

	// Token: 0x17000016 RID: 22
	// (get) Token: 0x060007B0 RID: 1968 RVA: 0x0003C7BB File Offset: 0x0003A9BB
	public long userID
	{
		get
		{
			return this.m_userID;
		}
	}

	// Token: 0x17000017 RID: 23
	// (get) Token: 0x060007B1 RID: 1969 RVA: 0x0003C7C3 File Offset: 0x0003A9C3
	public uint id
	{
		get
		{
			return this.m_id;
		}
	}

	// Token: 0x040007D6 RID: 2006
	public static ZDOID None = new ZDOID(0L, 0U);

	// Token: 0x040007D7 RID: 2007
	private long m_userID;

	// Token: 0x040007D8 RID: 2008
	private uint m_id;

	// Token: 0x040007D9 RID: 2009
	private int m_hash;
}
