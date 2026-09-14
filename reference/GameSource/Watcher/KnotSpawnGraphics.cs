using System;
using UnityEngine;

namespace Watcher;

public class KnotSpawnGraphics : ComplexGraphicsModule
{
	public float radius = 20f;

	public float thickness = 2f;

	public static string[] karmaSpriteNames = new string[10] { "ripple1.0", "ripple1.5", "ripple2.0", "ripple2.5", "ripple3.0", "ripple3.5", "ripple4.0", "ripple4.5", "ripple5.0", "karma9-9" };

	public int circleSpriteCount;

	public (Vector2 pos, float radius, float maxAngle, bool filled)[] circles;

	public (Vector2 start, Vector2 end)[] lines;

	public KnotSpawn critter => base.owner as KnotSpawn;

	public KnotSpawnGraphics(KnotSpawn owner)
		: base(owner, internalContainers: false)
	{
		circles = new(Vector2, float, float, bool)[UnityEngine.Random.Range(1, 5)];
		for (int i = 0; i < circles.Length; i++)
		{
			float num = Mathf.Lerp(radius * 0.25f, radius, UnityEngine.Random.value);
			Vector2 item = Vector2.up * Mathf.Lerp(0f - (radius - num), radius - num, UnityEngine.Random.value);
			float item2 = ((UnityEngine.Random.value < 0.25f) ? Mathf.Lerp((float)Math.PI / 2f, (float)Math.PI, UnityEngine.Random.value) : ((float)Math.PI));
			bool flag = UnityEngine.Random.value < 0.5f;
			circles[i] = (pos: item, radius: num, maxAngle: item2, filled: flag);
			circleSpriteCount++;
			if (flag)
			{
				circleSpriteCount++;
			}
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites = new FSprite[2 + circleSpriteCount];
		TriangleMesh triangleMesh = TriangleMesh.MakeLongMeshAtlased(24, pointyTip: false, customColor: false);
		sLeaser.sprites[0] = triangleMesh;
		TriangleMesh triangleMesh2 = TriangleMesh.MakeLongMeshAtlased(24, pointyTip: false, customColor: false);
		for (int i = 0; i < triangleMesh2.vertices.Length; i += 2)
		{
			float f = (float)i / ((float)triangleMesh2.vertices.Length - 2f) * (float)Math.PI * 2f;
			triangleMesh2.vertices[i] = new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * (radius - thickness);
			triangleMesh2.vertices[i + 1] = new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * (radius + thickness);
		}
		sLeaser.sprites[1] = triangleMesh2;
		int num = 1;
		for (int j = 0; j < circles.Length; j++)
		{
			(Vector2, float, float, bool) tuple = circles[j];
			if (tuple.Item4)
			{
				FSprite fSprite = new FSprite("Circle20");
				fSprite.width = tuple.Item2 * 2f;
				fSprite.height = tuple.Item2 * 2f;
				fSprite.color = Color.black;
				num++;
				sLeaser.sprites[num] = fSprite;
			}
			TriangleMesh triangleMesh3 = TriangleMesh.MakeLongMeshAtlased(16, pointyTip: false, customColor: false);
			for (int k = 0; k < triangleMesh3.vertices.Length; k += 2)
			{
				float t = (float)k / ((float)triangleMesh3.vertices.Length - 2f);
				float f2 = Mathf.Lerp(0f - tuple.Item3, tuple.Item3, t) + (float)Math.PI / 2f * (float)((!(tuple.Item1.y > 0f)) ? 1 : (-1));
				triangleMesh3.vertices[k] = new Vector2(Mathf.Cos(f2), Mathf.Sin(f2)) * (tuple.Item2 - 1f) + tuple.Item1;
				triangleMesh3.vertices[k + 1] = new Vector2(Mathf.Cos(f2), Mathf.Sin(f2)) * (tuple.Item2 + 1f) + tuple.Item1;
			}
			num++;
			sLeaser.sprites[num] = triangleMesh3;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		float num = ((float)rCam.game.clock + timeStacker) * 0.5f;
		Vector2 vector = Vector2.Lerp(critter.bodyChunks[0].lastPos, critter.bodyChunks[0].pos, timeStacker) - camPos;
		Quaternion quaternion = Quaternion.AngleAxis(num + 90f, Vector3.up);
		TriangleMesh triangleMesh = sLeaser.sprites[0] as TriangleMesh;
		for (int i = 0; i < triangleMesh.vertices.Length; i += 2)
		{
			float f = (float)i / ((float)triangleMesh.vertices.Length - 2f) * (float)Math.PI * 2f;
			Vector3 vector2 = new Vector3(Mathf.Cos(f) * radius, Mathf.Sin(f) * radius, 0f - thickness);
			vector2 = quaternion * vector2;
			Vector3 vector3 = new Vector3(Mathf.Cos(f) * radius, Mathf.Sin(f) * radius, thickness);
			vector3 = quaternion * vector3;
			triangleMesh.MoveVertice(i, new Vector2(vector2.x, vector2.y));
			triangleMesh.MoveVertice(i + 1, new Vector2(vector3.x, vector3.y));
		}
		triangleMesh.SetPosition(vector);
		sLeaser.sprites[1].SetPosition(vector);
		sLeaser.sprites[1].scaleX = Mathf.Sin(num * ((float)Math.PI / 180f));
		int num2 = 1;
		for (int j = 0; j < circles.Length; j++)
		{
			(Vector2, float, float, bool) tuple = circles[j];
			if (tuple.Item4)
			{
				num2++;
				FSprite obj = sLeaser.sprites[num2];
				obj.SetPosition(vector + tuple.Item1);
				obj.scaleX = Mathf.Sin(num * ((float)Math.PI / 180f));
			}
			num2++;
			FSprite obj2 = sLeaser.sprites[num2];
			obj2.SetPosition(vector);
			obj2.scaleX = Mathf.Sin(num * ((float)Math.PI / 180f));
		}
	}
}
