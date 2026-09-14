using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class GrappleSnake : AirBreatherCreature
{
	public GrappleSnakeAI AI;

	public Vector2 facingDir;

	public Vector2 smoothedFacingDir = Vector2.up;

	public List<GrappleSnakeHook> hooks = new List<GrappleSnakeHook>();

	public float totalMass;

	public float grappleSnakeHookStrength;

	public float grappleSnakeStablizerHookStrength;

	public float grappleSnakeGravityCounterStrengthUp;

	public float grappleSnakeGravityCounterStrengthDown;

	public GrappleSnake(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		float num = 1.1f;
		BodyChunk[] array = new GrappleSnakeBodyChunk[Random.Range(8, 16)];
		base.bodyChunks = array;
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			float num2 = 1f - (float)i / ((float)base.bodyChunks.Length + 20f);
			float rad = 25f * num2 * num2;
			float num3 = 1f - (float)i / (float)base.bodyChunks.Length;
			float num4 = num * num3;
			totalMass += num4;
			base.bodyChunks[i] = new GrappleSnakeBodyChunk(this, 0, new Vector2(0f, 0f), rad, num4, 1f - (float)i / (float)base.bodyChunks.Length);
		}
		bodyChunkConnections = new BodyChunkConnection[base.bodyChunks.Length - 1];
		float num5 = 0f;
		for (int j = 0; j < base.bodyChunks.Length - 1; j++)
		{
			float num6 = (base.bodyChunks[j].rad + base.bodyChunks[j + 1].rad) * 0.75f;
			num5 += num6;
			(base.bodyChunks[j + 1] as GrappleSnakeBodyChunk).idealDistanceFromHead = num5;
			bodyChunkConnections[j] = new BodyChunkConnection(base.bodyChunks[j], base.bodyChunks[j + 1], num6, BodyChunkConnection.Type.Normal, 1f, -1f);
		}
		int num7 = 12;
		for (int k = 0; k < num7; k++)
		{
			float t = (float)k / (float)num7;
			float num8 = Mathf.Lerp(-150f, 150f, t);
			num8 += Random.Range(-15f, 15f);
			(base.firstChunk as GrappleSnakeBodyChunk).AddHook(new GrappleSnakeHook(this, base.firstChunk as GrappleSnakeBodyChunk, 800f, num8));
		}
		int num9 = 12;
		for (int l = 0; l < num9; l++)
		{
			float num10 = ((Random.value > 0.5f) ? (-90) : 90);
			num10 += Random.Range(-15f, 15f);
			GrappleSnakeBodyChunk grappleSnakeBodyChunk = base.bodyChunks[Random.Range(1, base.bodyChunks.Length - 4)] as GrappleSnakeBodyChunk;
			grappleSnakeBodyChunk.AddHook(new StabilizerGrappleSnakeHook(this, grappleSnakeBodyChunk, 200f, num10));
		}
		base.airFriction = 0.99f;
		base.gravity = 0.6f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new GrappleSnakeGraphics(this);
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		Vector2 p = (Vector2)Futile.mousePosition + room.game.cameras[0].pos;
		facingDir = Custom.DirVec(base.firstChunk.pos, p);
		smoothedFacingDir = facingDir;
		foreach (GrappleSnakeHook hook in hooks)
		{
			hook.Update();
		}
		Act();
	}

	public void Act()
	{
		AI.Update();
	}
}
