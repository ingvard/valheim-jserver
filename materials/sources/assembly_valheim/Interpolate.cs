using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000040 RID: 64
public class Interpolate
{
	// Token: 0x06000444 RID: 1092 RVA: 0x00022A6B File Offset: 0x00020C6B
	private static Vector3 Identity(Vector3 v)
	{
		return v;
	}

	// Token: 0x06000445 RID: 1093 RVA: 0x00022A6E File Offset: 0x00020C6E
	private static Vector3 TransformDotPosition(Transform t)
	{
		return t.position;
	}

	// Token: 0x06000446 RID: 1094 RVA: 0x00022A76 File Offset: 0x00020C76
	private static IEnumerable<float> NewTimer(float duration)
	{
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			yield return elapsedTime;
			elapsedTime += Time.deltaTime;
			if (elapsedTime >= duration)
			{
				yield return elapsedTime;
			}
		}
		yield break;
	}

	// Token: 0x06000447 RID: 1095 RVA: 0x00022A86 File Offset: 0x00020C86
	private static IEnumerable<float> NewCounter(int start, int end, int step)
	{
		for (int i = start; i <= end; i += step)
		{
			yield return (float)i;
		}
		yield break;
	}

	// Token: 0x06000448 RID: 1096 RVA: 0x00022AA4 File Offset: 0x00020CA4
	public static IEnumerator NewEase(Interpolate.Function ease, Vector3 start, Vector3 end, float duration)
	{
		IEnumerable<float> driver = Interpolate.NewTimer(duration);
		return Interpolate.NewEase(ease, start, end, duration, driver);
	}

	// Token: 0x06000449 RID: 1097 RVA: 0x00022AC4 File Offset: 0x00020CC4
	public static IEnumerator NewEase(Interpolate.Function ease, Vector3 start, Vector3 end, int slices)
	{
		IEnumerable<float> driver = Interpolate.NewCounter(0, slices + 1, 1);
		return Interpolate.NewEase(ease, start, end, (float)(slices + 1), driver);
	}

	// Token: 0x0600044A RID: 1098 RVA: 0x00022AE9 File Offset: 0x00020CE9
	private static IEnumerator NewEase(Interpolate.Function ease, Vector3 start, Vector3 end, float total, IEnumerable<float> driver)
	{
		Vector3 distance = end - start;
		foreach (float elapsedTime in driver)
		{
			yield return Interpolate.Ease(ease, start, distance, elapsedTime, total);
		}
		IEnumerator<float> enumerator = null;
		yield break;
		yield break;
	}

	// Token: 0x0600044B RID: 1099 RVA: 0x00022B18 File Offset: 0x00020D18
	private static Vector3 Ease(Interpolate.Function ease, Vector3 start, Vector3 distance, float elapsedTime, float duration)
	{
		start.x = ease(start.x, distance.x, elapsedTime, duration);
		start.y = ease(start.y, distance.y, elapsedTime, duration);
		start.z = ease(start.z, distance.z, elapsedTime, duration);
		return start;
	}

	// Token: 0x0600044C RID: 1100 RVA: 0x00022B7C File Offset: 0x00020D7C
	public static Interpolate.Function Ease(Interpolate.EaseType type)
	{
		Interpolate.Function result = null;
		switch (type)
		{
		case Interpolate.EaseType.Linear:
			result = new Interpolate.Function(Interpolate.Linear);
			break;
		case Interpolate.EaseType.EaseInQuad:
			result = new Interpolate.Function(Interpolate.EaseInQuad);
			break;
		case Interpolate.EaseType.EaseOutQuad:
			result = new Interpolate.Function(Interpolate.EaseOutQuad);
			break;
		case Interpolate.EaseType.EaseInOutQuad:
			result = new Interpolate.Function(Interpolate.EaseInOutQuad);
			break;
		case Interpolate.EaseType.EaseInCubic:
			result = new Interpolate.Function(Interpolate.EaseInCubic);
			break;
		case Interpolate.EaseType.EaseOutCubic:
			result = new Interpolate.Function(Interpolate.EaseOutCubic);
			break;
		case Interpolate.EaseType.EaseInOutCubic:
			result = new Interpolate.Function(Interpolate.EaseInOutCubic);
			break;
		case Interpolate.EaseType.EaseInQuart:
			result = new Interpolate.Function(Interpolate.EaseInQuart);
			break;
		case Interpolate.EaseType.EaseOutQuart:
			result = new Interpolate.Function(Interpolate.EaseOutQuart);
			break;
		case Interpolate.EaseType.EaseInOutQuart:
			result = new Interpolate.Function(Interpolate.EaseInOutQuart);
			break;
		case Interpolate.EaseType.EaseInQuint:
			result = new Interpolate.Function(Interpolate.EaseInQuint);
			break;
		case Interpolate.EaseType.EaseOutQuint:
			result = new Interpolate.Function(Interpolate.EaseOutQuint);
			break;
		case Interpolate.EaseType.EaseInOutQuint:
			result = new Interpolate.Function(Interpolate.EaseInOutQuint);
			break;
		case Interpolate.EaseType.EaseInSine:
			result = new Interpolate.Function(Interpolate.EaseInSine);
			break;
		case Interpolate.EaseType.EaseOutSine:
			result = new Interpolate.Function(Interpolate.EaseOutSine);
			break;
		case Interpolate.EaseType.EaseInOutSine:
			result = new Interpolate.Function(Interpolate.EaseInOutSine);
			break;
		case Interpolate.EaseType.EaseInExpo:
			result = new Interpolate.Function(Interpolate.EaseInExpo);
			break;
		case Interpolate.EaseType.EaseOutExpo:
			result = new Interpolate.Function(Interpolate.EaseOutExpo);
			break;
		case Interpolate.EaseType.EaseInOutExpo:
			result = new Interpolate.Function(Interpolate.EaseInOutExpo);
			break;
		case Interpolate.EaseType.EaseInCirc:
			result = new Interpolate.Function(Interpolate.EaseInCirc);
			break;
		case Interpolate.EaseType.EaseOutCirc:
			result = new Interpolate.Function(Interpolate.EaseOutCirc);
			break;
		case Interpolate.EaseType.EaseInOutCirc:
			result = new Interpolate.Function(Interpolate.EaseInOutCirc);
			break;
		}
		return result;
	}

	// Token: 0x0600044D RID: 1101 RVA: 0x00022D60 File Offset: 0x00020F60
	public static IEnumerable<Vector3> NewBezier(Interpolate.Function ease, Transform[] nodes, float duration)
	{
		IEnumerable<float> steps = Interpolate.NewTimer(duration);
		return Interpolate.NewBezier<Transform>(ease, nodes, new Interpolate.ToVector3<Transform>(Interpolate.TransformDotPosition), duration, steps);
	}

	// Token: 0x0600044E RID: 1102 RVA: 0x00022D8C File Offset: 0x00020F8C
	public static IEnumerable<Vector3> NewBezier(Interpolate.Function ease, Transform[] nodes, int slices)
	{
		IEnumerable<float> steps = Interpolate.NewCounter(0, slices + 1, 1);
		return Interpolate.NewBezier<Transform>(ease, nodes, new Interpolate.ToVector3<Transform>(Interpolate.TransformDotPosition), (float)(slices + 1), steps);
	}

	// Token: 0x0600044F RID: 1103 RVA: 0x00022DBC File Offset: 0x00020FBC
	public static IEnumerable<Vector3> NewBezier(Interpolate.Function ease, Vector3[] points, float duration)
	{
		IEnumerable<float> steps = Interpolate.NewTimer(duration);
		return Interpolate.NewBezier<Vector3>(ease, points, new Interpolate.ToVector3<Vector3>(Interpolate.Identity), duration, steps);
	}

	// Token: 0x06000450 RID: 1104 RVA: 0x00022DE8 File Offset: 0x00020FE8
	public static IEnumerable<Vector3> NewBezier(Interpolate.Function ease, Vector3[] points, int slices)
	{
		IEnumerable<float> steps = Interpolate.NewCounter(0, slices + 1, 1);
		return Interpolate.NewBezier<Vector3>(ease, points, new Interpolate.ToVector3<Vector3>(Interpolate.Identity), (float)(slices + 1), steps);
	}

	// Token: 0x06000451 RID: 1105 RVA: 0x00022E18 File Offset: 0x00021018
	private static IEnumerable<Vector3> NewBezier<T>(Interpolate.Function ease, IList nodes, Interpolate.ToVector3<T> toVector3, float maxStep, IEnumerable<float> steps)
	{
		if (nodes.Count >= 2)
		{
			Vector3[] points = new Vector3[nodes.Count];
			foreach (float elapsedTime in steps)
			{
				for (int i = 0; i < nodes.Count; i++)
				{
					points[i] = toVector3((T)((object)nodes[i]));
				}
				yield return Interpolate.Bezier(ease, points, elapsedTime, maxStep);
			}
			IEnumerator<float> enumerator = null;
			points = null;
		}
		yield break;
		yield break;
	}

	// Token: 0x06000452 RID: 1106 RVA: 0x00022E48 File Offset: 0x00021048
	private static Vector3 Bezier(Interpolate.Function ease, Vector3[] points, float elapsedTime, float duration)
	{
		for (int i = points.Length - 1; i > 0; i--)
		{
			for (int j = 0; j < i; j++)
			{
				points[j].x = ease(points[j].x, points[j + 1].x - points[j].x, elapsedTime, duration);
				points[j].y = ease(points[j].y, points[j + 1].y - points[j].y, elapsedTime, duration);
				points[j].z = ease(points[j].z, points[j + 1].z - points[j].z, elapsedTime, duration);
			}
		}
		return points[0];
	}

	// Token: 0x06000453 RID: 1107 RVA: 0x00022F35 File Offset: 0x00021135
	public static IEnumerable<Vector3> NewCatmullRom(Transform[] nodes, int slices, bool loop)
	{
		return Interpolate.NewCatmullRom<Transform>(nodes, new Interpolate.ToVector3<Transform>(Interpolate.TransformDotPosition), slices, loop);
	}

	// Token: 0x06000454 RID: 1108 RVA: 0x00022F4B File Offset: 0x0002114B
	public static IEnumerable<Vector3> NewCatmullRom(Vector3[] points, int slices, bool loop)
	{
		return Interpolate.NewCatmullRom<Vector3>(points, new Interpolate.ToVector3<Vector3>(Interpolate.Identity), slices, loop);
	}

	// Token: 0x06000455 RID: 1109 RVA: 0x00022F61 File Offset: 0x00021161
	private static IEnumerable<Vector3> NewCatmullRom<T>(IList nodes, Interpolate.ToVector3<T> toVector3, int slices, bool loop)
	{
		if (nodes.Count >= 2)
		{
			yield return toVector3((T)((object)nodes[0]));
			int last = nodes.Count - 1;
			int current = 0;
			while (loop || current < last)
			{
				if (loop && current > last)
				{
					current = 0;
				}
				int previous = (current == 0) ? (loop ? last : current) : (current - 1);
				int start = current;
				int end = (current == last) ? (loop ? 0 : current) : (current + 1);
				int next = (end == last) ? (loop ? 0 : end) : (end + 1);
				int stepCount = slices + 1;
				int num;
				for (int step = 1; step <= stepCount; step = num + 1)
				{
					yield return Interpolate.CatmullRom(toVector3((T)((object)nodes[previous])), toVector3((T)((object)nodes[start])), toVector3((T)((object)nodes[end])), toVector3((T)((object)nodes[next])), (float)step, (float)stepCount);
					num = step;
				}
				num = current;
				current = num + 1;
			}
		}
		yield break;
	}

	// Token: 0x06000456 RID: 1110 RVA: 0x00022F88 File Offset: 0x00021188
	private static Vector3 CatmullRom(Vector3 previous, Vector3 start, Vector3 end, Vector3 next, float elapsedTime, float duration)
	{
		float num = elapsedTime / duration;
		float num2 = num * num;
		float num3 = num2 * num;
		return previous * (-0.5f * num3 + num2 - 0.5f * num) + start * (1.5f * num3 + -2.5f * num2 + 1f) + end * (-1.5f * num3 + 2f * num2 + 0.5f * num) + next * (0.5f * num3 - 0.5f * num2);
	}

	// Token: 0x06000457 RID: 1111 RVA: 0x00023016 File Offset: 0x00021216
	private static float Linear(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		return distance * (elapsedTime / duration) + start;
	}

	// Token: 0x06000458 RID: 1112 RVA: 0x00023026 File Offset: 0x00021226
	private static float EaseInQuad(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		return distance * elapsedTime * elapsedTime + start;
	}

	// Token: 0x06000459 RID: 1113 RVA: 0x0002303F File Offset: 0x0002123F
	private static float EaseOutQuad(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		return -distance * elapsedTime * (elapsedTime - 2f) + start;
	}

	// Token: 0x0600045A RID: 1114 RVA: 0x00023060 File Offset: 0x00021260
	private static float EaseInOutQuad(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 2f : (elapsedTime / (duration / 2f)));
		if (elapsedTime < 1f)
		{
			return distance / 2f * elapsedTime * elapsedTime + start;
		}
		elapsedTime -= 1f;
		return -distance / 2f * (elapsedTime * (elapsedTime - 2f) - 1f) + start;
	}

	// Token: 0x0600045B RID: 1115 RVA: 0x000230BC File Offset: 0x000212BC
	private static float EaseInCubic(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		return distance * elapsedTime * elapsedTime * elapsedTime + start;
	}

	// Token: 0x0600045C RID: 1116 RVA: 0x000230D7 File Offset: 0x000212D7
	private static float EaseOutCubic(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		elapsedTime -= 1f;
		return distance * (elapsedTime * elapsedTime * elapsedTime + 1f) + start;
	}

	// Token: 0x0600045D RID: 1117 RVA: 0x00023104 File Offset: 0x00021304
	private static float EaseInOutCubic(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 2f : (elapsedTime / (duration / 2f)));
		if (elapsedTime < 1f)
		{
			return distance / 2f * elapsedTime * elapsedTime * elapsedTime + start;
		}
		elapsedTime -= 2f;
		return distance / 2f * (elapsedTime * elapsedTime * elapsedTime + 2f) + start;
	}

	// Token: 0x0600045E RID: 1118 RVA: 0x0002315D File Offset: 0x0002135D
	private static float EaseInQuart(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		return distance * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
	}

	// Token: 0x0600045F RID: 1119 RVA: 0x0002317A File Offset: 0x0002137A
	private static float EaseOutQuart(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		elapsedTime -= 1f;
		return -distance * (elapsedTime * elapsedTime * elapsedTime * elapsedTime - 1f) + start;
	}

	// Token: 0x06000460 RID: 1120 RVA: 0x000231A8 File Offset: 0x000213A8
	private static float EaseInOutQuart(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 2f : (elapsedTime / (duration / 2f)));
		if (elapsedTime < 1f)
		{
			return distance / 2f * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
		}
		elapsedTime -= 2f;
		return -distance / 2f * (elapsedTime * elapsedTime * elapsedTime * elapsedTime - 2f) + start;
	}

	// Token: 0x06000461 RID: 1121 RVA: 0x00023206 File Offset: 0x00021406
	private static float EaseInQuint(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		return distance * elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
	}

	// Token: 0x06000462 RID: 1122 RVA: 0x00023225 File Offset: 0x00021425
	private static float EaseOutQuint(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		elapsedTime -= 1f;
		return distance * (elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + 1f) + start;
	}

	// Token: 0x06000463 RID: 1123 RVA: 0x00023254 File Offset: 0x00021454
	private static float EaseInOutQuint(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 2f : (elapsedTime / (duration / 2f)));
		if (elapsedTime < 1f)
		{
			return distance / 2f * elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
		}
		elapsedTime -= 2f;
		return distance / 2f * (elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + 2f) + start;
	}

	// Token: 0x06000464 RID: 1124 RVA: 0x000232B5 File Offset: 0x000214B5
	private static float EaseInSine(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		return -distance * Mathf.Cos(elapsedTime / duration * 1.5707964f) + distance + start;
	}

	// Token: 0x06000465 RID: 1125 RVA: 0x000232D3 File Offset: 0x000214D3
	private static float EaseOutSine(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		return distance * Mathf.Sin(elapsedTime / duration * 1.5707964f) + start;
	}

	// Token: 0x06000466 RID: 1126 RVA: 0x000232EE File Offset: 0x000214EE
	private static float EaseInOutSine(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		return -distance / 2f * (Mathf.Cos(3.1415927f * elapsedTime / duration) - 1f) + start;
	}

	// Token: 0x06000467 RID: 1127 RVA: 0x00023316 File Offset: 0x00021516
	private static float EaseInExpo(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		return distance * Mathf.Pow(2f, 10f * (elapsedTime / duration - 1f)) + start;
	}

	// Token: 0x06000468 RID: 1128 RVA: 0x0002333C File Offset: 0x0002153C
	private static float EaseOutExpo(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		return distance * (-Mathf.Pow(2f, -10f * elapsedTime / duration) + 1f) + start;
	}

	// Token: 0x06000469 RID: 1129 RVA: 0x00023364 File Offset: 0x00021564
	private static float EaseInOutExpo(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 2f : (elapsedTime / (duration / 2f)));
		if (elapsedTime < 1f)
		{
			return distance / 2f * Mathf.Pow(2f, 10f * (elapsedTime - 1f)) + start;
		}
		elapsedTime -= 1f;
		return distance / 2f * (-Mathf.Pow(2f, -10f * elapsedTime) + 2f) + start;
	}

	// Token: 0x0600046A RID: 1130 RVA: 0x000233DC File Offset: 0x000215DC
	private static float EaseInCirc(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		return -distance * (Mathf.Sqrt(1f - elapsedTime * elapsedTime) - 1f) + start;
	}

	// Token: 0x0600046B RID: 1131 RVA: 0x00023407 File Offset: 0x00021607
	private static float EaseOutCirc(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		elapsedTime -= 1f;
		return distance * Mathf.Sqrt(1f - elapsedTime * elapsedTime) + start;
	}

	// Token: 0x0600046C RID: 1132 RVA: 0x00023434 File Offset: 0x00021634
	private static float EaseInOutCirc(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 2f : (elapsedTime / (duration / 2f)));
		if (elapsedTime < 1f)
		{
			return -distance / 2f * (Mathf.Sqrt(1f - elapsedTime * elapsedTime) - 1f) + start;
		}
		elapsedTime -= 2f;
		return distance / 2f * (Mathf.Sqrt(1f - elapsedTime * elapsedTime) + 1f) + start;
	}

	// Token: 0x0200013B RID: 315
	public enum EaseType
	{
		// Token: 0x0400105B RID: 4187
		Linear,
		// Token: 0x0400105C RID: 4188
		EaseInQuad,
		// Token: 0x0400105D RID: 4189
		EaseOutQuad,
		// Token: 0x0400105E RID: 4190
		EaseInOutQuad,
		// Token: 0x0400105F RID: 4191
		EaseInCubic,
		// Token: 0x04001060 RID: 4192
		EaseOutCubic,
		// Token: 0x04001061 RID: 4193
		EaseInOutCubic,
		// Token: 0x04001062 RID: 4194
		EaseInQuart,
		// Token: 0x04001063 RID: 4195
		EaseOutQuart,
		// Token: 0x04001064 RID: 4196
		EaseInOutQuart,
		// Token: 0x04001065 RID: 4197
		EaseInQuint,
		// Token: 0x04001066 RID: 4198
		EaseOutQuint,
		// Token: 0x04001067 RID: 4199
		EaseInOutQuint,
		// Token: 0x04001068 RID: 4200
		EaseInSine,
		// Token: 0x04001069 RID: 4201
		EaseOutSine,
		// Token: 0x0400106A RID: 4202
		EaseInOutSine,
		// Token: 0x0400106B RID: 4203
		EaseInExpo,
		// Token: 0x0400106C RID: 4204
		EaseOutExpo,
		// Token: 0x0400106D RID: 4205
		EaseInOutExpo,
		// Token: 0x0400106E RID: 4206
		EaseInCirc,
		// Token: 0x0400106F RID: 4207
		EaseOutCirc,
		// Token: 0x04001070 RID: 4208
		EaseInOutCirc
	}

	// Token: 0x0200013C RID: 316
	// (Invoke) Token: 0x060010D8 RID: 4312
	public delegate Vector3 ToVector3<T>(T v);

	// Token: 0x0200013D RID: 317
	// (Invoke) Token: 0x060010DC RID: 4316
	public delegate float Function(float a, float b, float c, float d);
}
