using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Menu;

public class RippleDial : RectangularMenuObject
{
	public TriangleMesh[] segments;

	public int collectedEggs;

	public int totalEggs;

	public Spiral spiral;

	public float segmentLength;

	public float depth;

	public float alpha = 1f;

	public const float radius = 180f;

	public const float segmentSpacing = 0f;

	public const float segmentThickness = 10f;

	public const float extraRadius = 0f;

	public const int loops = 5;

	public const int capTris = 8;

	public const int lineCols = 8;

	public RippleDial(Menu menu, MenuObject owner, Vector2 center, float depth, int collectedEggs, int totalEggs)
		: base(menu, owner, center + new Vector2(180f, 180f), new Vector2(180f, 180f) * 2f)
	{
		this.depth = depth;
		this.collectedEggs = collectedEggs;
		this.totalEggs = totalEggs;
		spiral.innerRadius = 35f;
		spiral.ringSpacing = (180f - spiral.innerRadius) / 5f;
		segmentLength = spiral.AngleToLength(31.915928f) / (float)totalEggs - 0f;
		TriangleMesh.Triangle[] tris = MakeSegmentTris();
		segments = new TriangleMesh[totalEggs];
		for (int i = 0; i < totalEggs; i++)
		{
			TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, customColor: true)
			{
				shader = menu.manager.rainWorld.Shaders["RippleDialSegment"]
			};
			segments[i] = triangleMesh;
			Container.AddChild(triangleMesh);
			MoveSegmentMesh(triangleMesh, i);
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		Vector2 position = Vector2.Lerp(lastPos, pos, timeStacker) - (owner as MenuScene).CamPos(timeStacker) * 80f / depth;
		float rotation = -90f;
		TriangleMesh[] array = segments;
		foreach (TriangleMesh triangleMesh in array)
		{
			triangleMesh.SetPosition(position);
			triangleMesh.rotation = rotation;
			triangleMesh.alpha = alpha;
			for (int j = 0; j < triangleMesh.verticeColors.Length; j++)
			{
				triangleMesh.verticeColors[j].a = alpha;
			}
		}
	}

	public TriangleMesh.Triangle[] MakeSegmentTris()
	{
		List<TriangleMesh.Triangle> list = new List<TriangleMesh.Triangle>();
		for (int i = 0; i < 8; i++)
		{
			int num = i * 3;
			list.Add(new TriangleMesh.Triangle(num, num + 3, num + 1));
			list.Add(new TriangleMesh.Triangle(num + 3, num + 4, num + 1));
			list.Add(new TriangleMesh.Triangle(num + 1, num + 4, num + 2));
			list.Add(new TriangleMesh.Triangle(num + 4, num + 5, num + 2));
		}
		int num2 = 27;
		for (int j = 0; j < 8; j++)
		{
			int b = ((j == 0) ? 2 : (num2 + j - 1));
			int c = ((j != 7) ? (num2 + j) : 0);
			list.Add(new TriangleMesh.Triangle(1, b, c));
		}
		num2 += 7;
		for (int k = 0; k < 8; k++)
		{
			int b2 = ((k == 0) ? 24 : (num2 + k - 1));
			int c2 = ((k == 7) ? 26 : (num2 + k));
			list.Add(new TriangleMesh.Triangle(25, b2, c2));
		}
		return list.ToArray();
	}

	public void MoveSegmentMesh(TriangleMesh mesh, int egg)
	{
		float num = (segmentLength + 0f) * (float)egg + 10f;
		float num2 = num + segmentLength - 20f;
		int v = 0;
		float lit = ((egg < collectedEggs) ? 1f : 0f);
		for (int i = 0; i <= 8; i++)
		{
			float num3 = (float)i / 8f;
			float x = Mathf.Lerp(num, num2, num3);
			Vector2 vector = spiral.LengthToPos(x);
			Vector2 vector2 = Custom.PerpendicularVector(spiral.LengthToDir(x));
			PushVert(vector + vector2 * 10f, num3, inside: false);
			PushVert(vector, num3, inside: true);
			PushVert(vector - vector2 * 10f, num3, inside: false);
		}
		for (int j = 0; j < 2; j++)
		{
			float x2 = ((j == 0) ? num : num2);
			Vector2 vector3 = spiral.LengthToPos(x2);
			Vector2 vector4 = spiral.LengthToDir(x2) * 10f;
			if (j == 0)
			{
				vector4 = -vector4;
			}
			Vector2 vector5 = Vector2.Perpendicular(vector4);
			for (int k = 0; k < 7; k++)
			{
				float f = (float)(k + 1) / 8f * (float)Math.PI;
				PushVert(vector3 + vector5 * Mathf.Cos(f) + vector4 * Mathf.Sin(f), j, inside: false);
			}
		}
		void PushVert(Vector2 pos, float along, bool inside)
		{
			mesh.MoveVertice(v, pos);
			mesh.verticeColors[v] = new Color(pos.x / 180f, pos.y / 180f, lit, 0.18f);
			mesh.UVvertices[v] = new Vector2(along, inside ? 0f : 1f);
			v++;
		}
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		TriangleMesh[] array = segments;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].RemoveFromContainer();
		}
	}
}
