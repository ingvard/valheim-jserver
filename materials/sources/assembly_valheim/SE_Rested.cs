using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200002A RID: 42
public class SE_Rested : SE_Stats
{
	// Token: 0x060003B7 RID: 951 RVA: 0x0001FA7C File Offset: 0x0001DC7C
	public override void Setup(Character character)
	{
		base.Setup(character);
		this.UpdateTTL();
		Player player = this.m_character as Player;
		this.m_character.Message(MessageHud.MessageType.Center, "$se_rested_start ($se_rested_comfort:" + player.GetComfortLevel().ToString() + ")", 0, null);
	}

	// Token: 0x060003B8 RID: 952 RVA: 0x0001FACD File Offset: 0x0001DCCD
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
		this.m_timeSinceComfortUpdate -= dt;
	}

	// Token: 0x060003B9 RID: 953 RVA: 0x0001FAE4 File Offset: 0x0001DCE4
	public override void ResetTime()
	{
		this.UpdateTTL();
	}

	// Token: 0x060003BA RID: 954 RVA: 0x0001FAEC File Offset: 0x0001DCEC
	private void UpdateTTL()
	{
		Player player = this.m_character as Player;
		float num = this.m_baseTTL + (float)(player.GetComfortLevel() - 1) * this.m_TTLPerComfortLevel;
		float num2 = this.m_ttl - this.m_time;
		if (num > num2)
		{
			this.m_ttl = num;
			this.m_time = 0f;
		}
	}

	// Token: 0x060003BB RID: 955 RVA: 0x0001FB44 File Offset: 0x0001DD44
	private static int PieceComfortSort(Piece x, Piece y)
	{
		if (x.m_comfortGroup != y.m_comfortGroup)
		{
			return x.m_comfortGroup.CompareTo(y.m_comfortGroup);
		}
		if (x.m_comfort != y.m_comfort)
		{
			return y.m_comfort.CompareTo(x.m_comfort);
		}
		return y.m_name.CompareTo(x.m_name);
	}

	// Token: 0x060003BC RID: 956 RVA: 0x0001FBB0 File Offset: 0x0001DDB0
	public static int CalculateComfortLevel(Player player)
	{
		List<Piece> nearbyPieces = SE_Rested.GetNearbyPieces(player.transform.position);
		nearbyPieces.Sort(new Comparison<Piece>(SE_Rested.PieceComfortSort));
		int num = 1;
		if (player.InShelter())
		{
			num++;
			int i = 0;
			while (i < nearbyPieces.Count)
			{
				Piece piece = nearbyPieces[i];
				if (i <= 0)
				{
					goto IL_77;
				}
				Piece piece2 = nearbyPieces[i - 1];
				if ((piece.m_comfortGroup == Piece.ComfortGroup.None || piece.m_comfortGroup != piece2.m_comfortGroup) && !(piece.m_name == piece2.m_name))
				{
					goto IL_77;
				}
				IL_80:
				i++;
				continue;
				IL_77:
				num += piece.m_comfort;
				goto IL_80;
			}
		}
		return num;
	}

	// Token: 0x060003BD RID: 957 RVA: 0x0001FC4B File Offset: 0x0001DE4B
	private static List<Piece> GetNearbyPieces(Vector3 point)
	{
		SE_Rested.m_tempPieces.Clear();
		Piece.GetAllPiecesInRadius(point, 10f, SE_Rested.m_tempPieces);
		return SE_Rested.m_tempPieces;
	}

	// Token: 0x040003AE RID: 942
	private static List<Piece> m_tempPieces = new List<Piece>();

	// Token: 0x040003AF RID: 943
	[Header("__SE_Rested__")]
	public float m_baseTTL = 300f;

	// Token: 0x040003B0 RID: 944
	public float m_TTLPerComfortLevel = 60f;

	// Token: 0x040003B1 RID: 945
	private const float m_comfortRadius = 10f;

	// Token: 0x040003B2 RID: 946
	private float m_timeSinceComfortUpdate;
}
