using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

// Token: 0x02000089 RID: 137
public class ZPackage
{
	// Token: 0x060008C3 RID: 2243 RVA: 0x000426C4 File Offset: 0x000408C4
	public ZPackage()
	{
		this.m_writer = new BinaryWriter(this.m_stream);
		this.m_reader = new BinaryReader(this.m_stream);
	}

	// Token: 0x060008C4 RID: 2244 RVA: 0x000426FC File Offset: 0x000408FC
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

	// Token: 0x060008C5 RID: 2245 RVA: 0x0004276C File Offset: 0x0004096C
	public ZPackage(byte[] data)
	{
		this.m_writer = new BinaryWriter(this.m_stream);
		this.m_reader = new BinaryReader(this.m_stream);
		this.m_stream.Write(data, 0, data.Length);
		this.m_stream.Position = 0L;
	}

	// Token: 0x060008C6 RID: 2246 RVA: 0x000427CC File Offset: 0x000409CC
	public ZPackage(byte[] data, int dataSize)
	{
		this.m_writer = new BinaryWriter(this.m_stream);
		this.m_reader = new BinaryReader(this.m_stream);
		this.m_stream.Write(data, 0, dataSize);
		this.m_stream.Position = 0L;
	}

	// Token: 0x060008C7 RID: 2247 RVA: 0x00042827 File Offset: 0x00040A27
	public void Load(byte[] data)
	{
		this.Clear();
		this.m_stream.Write(data, 0, data.Length);
		this.m_stream.Position = 0L;
	}

	// Token: 0x060008C8 RID: 2248 RVA: 0x0004284C File Offset: 0x00040A4C
	public void Write(ZPackage pkg)
	{
		byte[] array = pkg.GetArray();
		this.m_writer.Write(array.Length);
		this.m_writer.Write(array);
	}

	// Token: 0x060008C9 RID: 2249 RVA: 0x0004287A File Offset: 0x00040A7A
	public void Write(byte[] array)
	{
		this.m_writer.Write(array.Length);
		this.m_writer.Write(array);
	}

	// Token: 0x060008CA RID: 2250 RVA: 0x00042896 File Offset: 0x00040A96
	public void Write(byte data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x060008CB RID: 2251 RVA: 0x000428A4 File Offset: 0x00040AA4
	public void Write(sbyte data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x060008CC RID: 2252 RVA: 0x000428B2 File Offset: 0x00040AB2
	public void Write(char data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x060008CD RID: 2253 RVA: 0x000428C0 File Offset: 0x00040AC0
	public void Write(bool data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x060008CE RID: 2254 RVA: 0x000428CE File Offset: 0x00040ACE
	public void Write(int data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x060008CF RID: 2255 RVA: 0x000428DC File Offset: 0x00040ADC
	public void Write(uint data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x060008D0 RID: 2256 RVA: 0x000428EA File Offset: 0x00040AEA
	public void Write(ulong data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x060008D1 RID: 2257 RVA: 0x000428F8 File Offset: 0x00040AF8
	public void Write(long data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x060008D2 RID: 2258 RVA: 0x00042906 File Offset: 0x00040B06
	public void Write(float data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x060008D3 RID: 2259 RVA: 0x00042914 File Offset: 0x00040B14
	public void Write(double data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x060008D4 RID: 2260 RVA: 0x00042922 File Offset: 0x00040B22
	public void Write(string data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x060008D5 RID: 2261 RVA: 0x00042930 File Offset: 0x00040B30
	public void Write(ZDOID id)
	{
		this.m_writer.Write(id.userID);
		this.m_writer.Write(id.id);
	}

	// Token: 0x060008D6 RID: 2262 RVA: 0x00042956 File Offset: 0x00040B56
	public void Write(Vector3 v3)
	{
		this.m_writer.Write(v3.x);
		this.m_writer.Write(v3.y);
		this.m_writer.Write(v3.z);
	}

	// Token: 0x060008D7 RID: 2263 RVA: 0x0004298B File Offset: 0x00040B8B
	public void Write(Vector2i v2)
	{
		this.m_writer.Write(v2.x);
		this.m_writer.Write(v2.y);
	}

	// Token: 0x060008D8 RID: 2264 RVA: 0x000429B0 File Offset: 0x00040BB0
	public void Write(Quaternion q)
	{
		this.m_writer.Write(q.x);
		this.m_writer.Write(q.y);
		this.m_writer.Write(q.z);
		this.m_writer.Write(q.w);
	}

	// Token: 0x060008D9 RID: 2265 RVA: 0x00042A01 File Offset: 0x00040C01
	public ZDOID ReadZDOID()
	{
		return new ZDOID(this.m_reader.ReadInt64(), this.m_reader.ReadUInt32());
	}

	// Token: 0x060008DA RID: 2266 RVA: 0x00042A1E File Offset: 0x00040C1E
	public bool ReadBool()
	{
		return this.m_reader.ReadBoolean();
	}

	// Token: 0x060008DB RID: 2267 RVA: 0x00042A2B File Offset: 0x00040C2B
	public char ReadChar()
	{
		return this.m_reader.ReadChar();
	}

	// Token: 0x060008DC RID: 2268 RVA: 0x00042A38 File Offset: 0x00040C38
	public byte ReadByte()
	{
		return this.m_reader.ReadByte();
	}

	// Token: 0x060008DD RID: 2269 RVA: 0x00042A45 File Offset: 0x00040C45
	public sbyte ReadSByte()
	{
		return this.m_reader.ReadSByte();
	}

	// Token: 0x060008DE RID: 2270 RVA: 0x00042A52 File Offset: 0x00040C52
	public int ReadInt()
	{
		return this.m_reader.ReadInt32();
	}

	// Token: 0x060008DF RID: 2271 RVA: 0x00042A5F File Offset: 0x00040C5F
	public uint ReadUInt()
	{
		return this.m_reader.ReadUInt32();
	}

	// Token: 0x060008E0 RID: 2272 RVA: 0x00042A6C File Offset: 0x00040C6C
	public long ReadLong()
	{
		return this.m_reader.ReadInt64();
	}

	// Token: 0x060008E1 RID: 2273 RVA: 0x00042A79 File Offset: 0x00040C79
	public ulong ReadULong()
	{
		return this.m_reader.ReadUInt64();
	}

	// Token: 0x060008E2 RID: 2274 RVA: 0x00042A86 File Offset: 0x00040C86
	public float ReadSingle()
	{
		return this.m_reader.ReadSingle();
	}

	// Token: 0x060008E3 RID: 2275 RVA: 0x00042A93 File Offset: 0x00040C93
	public double ReadDouble()
	{
		return this.m_reader.ReadDouble();
	}

	// Token: 0x060008E4 RID: 2276 RVA: 0x00042AA0 File Offset: 0x00040CA0
	public string ReadString()
	{
		return this.m_reader.ReadString();
	}

	// Token: 0x060008E5 RID: 2277 RVA: 0x00042AB0 File Offset: 0x00040CB0
	public Vector3 ReadVector3()
	{
		return new Vector3
		{
			x = this.m_reader.ReadSingle(),
			y = this.m_reader.ReadSingle(),
			z = this.m_reader.ReadSingle()
		};
	}

	// Token: 0x060008E6 RID: 2278 RVA: 0x00042AFC File Offset: 0x00040CFC
	public Vector2i ReadVector2i()
	{
		return new Vector2i
		{
			x = this.m_reader.ReadInt32(),
			y = this.m_reader.ReadInt32()
		};
	}

	// Token: 0x060008E7 RID: 2279 RVA: 0x00042B38 File Offset: 0x00040D38
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

	// Token: 0x060008E8 RID: 2280 RVA: 0x00042B98 File Offset: 0x00040D98
	public ZPackage ReadPackage()
	{
		int count = this.m_reader.ReadInt32();
		return new ZPackage(this.m_reader.ReadBytes(count));
	}

	// Token: 0x060008E9 RID: 2281 RVA: 0x00042BC4 File Offset: 0x00040DC4
	public void ReadPackage(ref ZPackage pkg)
	{
		int count = this.m_reader.ReadInt32();
		byte[] array = this.m_reader.ReadBytes(count);
		pkg.Clear();
		pkg.m_stream.Write(array, 0, array.Length);
		pkg.m_stream.Position = 0L;
	}

	// Token: 0x060008EA RID: 2282 RVA: 0x00042C10 File Offset: 0x00040E10
	public byte[] ReadByteArray()
	{
		int count = this.m_reader.ReadInt32();
		return this.m_reader.ReadBytes(count);
	}

	// Token: 0x060008EB RID: 2283 RVA: 0x00042C35 File Offset: 0x00040E35
	public string GetBase64()
	{
		return Convert.ToBase64String(this.GetArray());
	}

	// Token: 0x060008EC RID: 2284 RVA: 0x00042C42 File Offset: 0x00040E42
	public byte[] GetArray()
	{
		this.m_writer.Flush();
		this.m_stream.Flush();
		return this.m_stream.ToArray();
	}

	// Token: 0x060008ED RID: 2285 RVA: 0x00042C65 File Offset: 0x00040E65
	public void SetPos(int pos)
	{
		this.m_stream.Position = (long)pos;
	}

	// Token: 0x060008EE RID: 2286 RVA: 0x00042C74 File Offset: 0x00040E74
	public int GetPos()
	{
		return (int)this.m_stream.Position;
	}

	// Token: 0x060008EF RID: 2287 RVA: 0x00042C82 File Offset: 0x00040E82
	public int Size()
	{
		this.m_writer.Flush();
		this.m_stream.Flush();
		return (int)this.m_stream.Length;
	}

	// Token: 0x060008F0 RID: 2288 RVA: 0x00042CA6 File Offset: 0x00040EA6
	public void Clear()
	{
		this.m_writer.Flush();
		this.m_stream.SetLength(0L);
		this.m_stream.Position = 0L;
	}

	// Token: 0x060008F1 RID: 2289 RVA: 0x00042CD0 File Offset: 0x00040ED0
	public byte[] GenerateHash()
	{
		byte[] array = this.GetArray();
		return SHA512.Create().ComputeHash(array);
	}

	// Token: 0x0400085B RID: 2139
	private MemoryStream m_stream = new MemoryStream();

	// Token: 0x0400085C RID: 2140
	private BinaryWriter m_writer;

	// Token: 0x0400085D RID: 2141
	private BinaryReader m_reader;
}
