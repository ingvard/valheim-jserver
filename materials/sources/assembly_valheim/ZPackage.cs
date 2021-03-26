using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

// Token: 0x02000089 RID: 137
public class ZPackage
{
	// Token: 0x060008C2 RID: 2242 RVA: 0x00042610 File Offset: 0x00040810
	public ZPackage()
	{
		this.m_writer = new BinaryWriter(this.m_stream);
		this.m_reader = new BinaryReader(this.m_stream);
	}

	// Token: 0x060008C3 RID: 2243 RVA: 0x00042648 File Offset: 0x00040848
	public ZPackage(string base64String)
	{
		this.m_writer = new BinaryWriter(this.m_stream);
		this.m_reader = new BinaryReader(this.m_stream);
		if (string.IsNullOrEmpty(base64String))
		{
			return;
		}
		byte[] array = Convert.FromBase64String(base64String);
		this.m_stream.Write(array, 0, array.Length);
		this.m_stream.Position = 0L;
	}

	// Token: 0x060008C4 RID: 2244 RVA: 0x000426B8 File Offset: 0x000408B8
	public ZPackage(byte[] data)
	{
		this.m_writer = new BinaryWriter(this.m_stream);
		this.m_reader = new BinaryReader(this.m_stream);
		this.m_stream.Write(data, 0, data.Length);
		this.m_stream.Position = 0L;
	}

	// Token: 0x060008C5 RID: 2245 RVA: 0x00042718 File Offset: 0x00040918
	public ZPackage(byte[] data, int dataSize)
	{
		this.m_writer = new BinaryWriter(this.m_stream);
		this.m_reader = new BinaryReader(this.m_stream);
		this.m_stream.Write(data, 0, dataSize);
		this.m_stream.Position = 0L;
	}

	// Token: 0x060008C6 RID: 2246 RVA: 0x00042773 File Offset: 0x00040973
	public void Load(byte[] data)
	{
		this.Clear();
		this.m_stream.Write(data, 0, data.Length);
		this.m_stream.Position = 0L;
	}

	// Token: 0x060008C7 RID: 2247 RVA: 0x00042798 File Offset: 0x00040998
	public void Write(ZPackage pkg)
	{
		byte[] array = pkg.GetArray();
		this.m_writer.Write(array.Length);
		this.m_writer.Write(array);
	}

	// Token: 0x060008C8 RID: 2248 RVA: 0x000427C6 File Offset: 0x000409C6
	public void Write(byte[] array)
	{
		this.m_writer.Write(array.Length);
		this.m_writer.Write(array);
	}

	// Token: 0x060008C9 RID: 2249 RVA: 0x000427E2 File Offset: 0x000409E2
	public void Write(byte data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x060008CA RID: 2250 RVA: 0x000427F0 File Offset: 0x000409F0
	public void Write(sbyte data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x060008CB RID: 2251 RVA: 0x000427FE File Offset: 0x000409FE
	public void Write(char data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x060008CC RID: 2252 RVA: 0x0004280C File Offset: 0x00040A0C
	public void Write(bool data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x060008CD RID: 2253 RVA: 0x0004281A File Offset: 0x00040A1A
	public void Write(int data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x060008CE RID: 2254 RVA: 0x00042828 File Offset: 0x00040A28
	public void Write(uint data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x060008CF RID: 2255 RVA: 0x00042836 File Offset: 0x00040A36
	public void Write(ulong data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x060008D0 RID: 2256 RVA: 0x00042844 File Offset: 0x00040A44
	public void Write(long data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x060008D1 RID: 2257 RVA: 0x00042852 File Offset: 0x00040A52
	public void Write(float data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x060008D2 RID: 2258 RVA: 0x00042860 File Offset: 0x00040A60
	public void Write(double data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x060008D3 RID: 2259 RVA: 0x0004286E File Offset: 0x00040A6E
	public void Write(string data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x060008D4 RID: 2260 RVA: 0x0004287C File Offset: 0x00040A7C
	public void Write(ZDOID id)
	{
		this.m_writer.Write(id.userID);
		this.m_writer.Write(id.id);
	}

	// Token: 0x060008D5 RID: 2261 RVA: 0x000428A2 File Offset: 0x00040AA2
	public void Write(Vector3 v3)
	{
		this.m_writer.Write(v3.x);
		this.m_writer.Write(v3.y);
		this.m_writer.Write(v3.z);
	}

	// Token: 0x060008D6 RID: 2262 RVA: 0x000428D7 File Offset: 0x00040AD7
	public void Write(Vector2i v2)
	{
		this.m_writer.Write(v2.x);
		this.m_writer.Write(v2.y);
	}

	// Token: 0x060008D7 RID: 2263 RVA: 0x000428FC File Offset: 0x00040AFC
	public void Write(Quaternion q)
	{
		this.m_writer.Write(q.x);
		this.m_writer.Write(q.y);
		this.m_writer.Write(q.z);
		this.m_writer.Write(q.w);
	}

	// Token: 0x060008D8 RID: 2264 RVA: 0x0004294D File Offset: 0x00040B4D
	public ZDOID ReadZDOID()
	{
		return new ZDOID(this.m_reader.ReadInt64(), this.m_reader.ReadUInt32());
	}

	// Token: 0x060008D9 RID: 2265 RVA: 0x0004296A File Offset: 0x00040B6A
	public bool ReadBool()
	{
		return this.m_reader.ReadBoolean();
	}

	// Token: 0x060008DA RID: 2266 RVA: 0x00042977 File Offset: 0x00040B77
	public char ReadChar()
	{
		return this.m_reader.ReadChar();
	}

	// Token: 0x060008DB RID: 2267 RVA: 0x00042984 File Offset: 0x00040B84
	public byte ReadByte()
	{
		return this.m_reader.ReadByte();
	}

	// Token: 0x060008DC RID: 2268 RVA: 0x00042991 File Offset: 0x00040B91
	public sbyte ReadSByte()
	{
		return this.m_reader.ReadSByte();
	}

	// Token: 0x060008DD RID: 2269 RVA: 0x0004299E File Offset: 0x00040B9E
	public int ReadInt()
	{
		return this.m_reader.ReadInt32();
	}

	// Token: 0x060008DE RID: 2270 RVA: 0x000429AB File Offset: 0x00040BAB
	public uint ReadUInt()
	{
		return this.m_reader.ReadUInt32();
	}

	// Token: 0x060008DF RID: 2271 RVA: 0x000429B8 File Offset: 0x00040BB8
	public long ReadLong()
	{
		return this.m_reader.ReadInt64();
	}

	// Token: 0x060008E0 RID: 2272 RVA: 0x000429C5 File Offset: 0x00040BC5
	public ulong ReadULong()
	{
		return this.m_reader.ReadUInt64();
	}

	// Token: 0x060008E1 RID: 2273 RVA: 0x000429D2 File Offset: 0x00040BD2
	public float ReadSingle()
	{
		return this.m_reader.ReadSingle();
	}

	// Token: 0x060008E2 RID: 2274 RVA: 0x000429DF File Offset: 0x00040BDF
	public double ReadDouble()
	{
		return this.m_reader.ReadDouble();
	}

	// Token: 0x060008E3 RID: 2275 RVA: 0x000429EC File Offset: 0x00040BEC
	public string ReadString()
	{
		return this.m_reader.ReadString();
	}

	// Token: 0x060008E4 RID: 2276 RVA: 0x000429FC File Offset: 0x00040BFC
	public Vector3 ReadVector3()
	{
		return new Vector3
		{
			x = this.m_reader.ReadSingle(),
			y = this.m_reader.ReadSingle(),
			z = this.m_reader.ReadSingle()
		};
	}

	// Token: 0x060008E5 RID: 2277 RVA: 0x00042A48 File Offset: 0x00040C48
	public Vector2i ReadVector2i()
	{
		return new Vector2i
		{
			x = this.m_reader.ReadInt32(),
			y = this.m_reader.ReadInt32()
		};
	}

	// Token: 0x060008E6 RID: 2278 RVA: 0x00042A84 File Offset: 0x00040C84
	public Quaternion ReadQuaternion()
	{
		return new Quaternion
		{
			x = this.m_reader.ReadSingle(),
			y = this.m_reader.ReadSingle(),
			z = this.m_reader.ReadSingle(),
			w = this.m_reader.ReadSingle()
		};
	}

	// Token: 0x060008E7 RID: 2279 RVA: 0x00042AE4 File Offset: 0x00040CE4
	public ZPackage ReadPackage()
	{
		int count = this.m_reader.ReadInt32();
		return new ZPackage(this.m_reader.ReadBytes(count));
	}

	// Token: 0x060008E8 RID: 2280 RVA: 0x00042B10 File Offset: 0x00040D10
	public void ReadPackage(ref ZPackage pkg)
	{
		int count = this.m_reader.ReadInt32();
		byte[] array = this.m_reader.ReadBytes(count);
		pkg.Clear();
		pkg.m_stream.Write(array, 0, array.Length);
		pkg.m_stream.Position = 0L;
	}

	// Token: 0x060008E9 RID: 2281 RVA: 0x00042B5C File Offset: 0x00040D5C
	public byte[] ReadByteArray()
	{
		int count = this.m_reader.ReadInt32();
		return this.m_reader.ReadBytes(count);
	}

	// Token: 0x060008EA RID: 2282 RVA: 0x00042B81 File Offset: 0x00040D81
	public string GetBase64()
	{
		return Convert.ToBase64String(this.GetArray());
	}

	// Token: 0x060008EB RID: 2283 RVA: 0x00042B8E File Offset: 0x00040D8E
	public byte[] GetArray()
	{
		this.m_writer.Flush();
		this.m_stream.Flush();
		return this.m_stream.ToArray();
	}

	// Token: 0x060008EC RID: 2284 RVA: 0x00042BB1 File Offset: 0x00040DB1
	public void SetPos(int pos)
	{
		this.m_stream.Position = (long)pos;
	}

	// Token: 0x060008ED RID: 2285 RVA: 0x00042BC0 File Offset: 0x00040DC0
	public int GetPos()
	{
		return (int)this.m_stream.Position;
	}

	// Token: 0x060008EE RID: 2286 RVA: 0x00042BCE File Offset: 0x00040DCE
	public int Size()
	{
		this.m_writer.Flush();
		this.m_stream.Flush();
		return (int)this.m_stream.Length;
	}

	// Token: 0x060008EF RID: 2287 RVA: 0x00042BF2 File Offset: 0x00040DF2
	public void Clear()
	{
		this.m_writer.Flush();
		this.m_stream.SetLength(0L);
		this.m_stream.Position = 0L;
	}

	// Token: 0x060008F0 RID: 2288 RVA: 0x00042C1C File Offset: 0x00040E1C
	public byte[] GenerateHash()
	{
		byte[] array = this.GetArray();
		return SHA512.Create().ComputeHash(array);
	}

	// Token: 0x04000857 RID: 2135
	private MemoryStream m_stream = new MemoryStream();

	// Token: 0x04000858 RID: 2136
	private BinaryWriter m_writer;

	// Token: 0x04000859 RID: 2137
	private BinaryReader m_reader;
}
