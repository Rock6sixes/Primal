using RWCustom;
using UnityEngine;

namespace Watcher;

public class Prince : PhysicalObject
{
	public PrinceBehavior behavior;

	public PrinceStem stem;

	public SimpleSegment head;

	public float bloom;

	public float lastBloom;

	public float neckLength = 30f;

	public bool Consious => true;

	public Prince(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		Vector2 pos = abstractPhysicalObject.pos.Tile.ToVector2() * 20f + new Vector2(10f, 10f);
		if (Custom.rainWorld.progression.miscProgressionData.beaten_Watcher_SentientRot && !Region.IsDaemonRegion(abstractPhysicalObject.world.name))
		{
			pos = new Vector2(530f, 510f);
		}
		base.bodyChunks = new BodyChunk[5];
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			float num = Mathf.Lerp(18f, 32f, (float)i / (float)(base.bodyChunks.Length - 1));
			base.bodyChunks[i] = new BodyChunk(this, i, pos, num, num / 32f);
		}
		bodyChunkConnections = new BodyChunkConnection[base.bodyChunks.Length - 1];
		for (int j = 0; j < bodyChunkConnections.Length; j++)
		{
			bodyChunkConnections[j] = new BodyChunkConnection(base.bodyChunks[j], base.bodyChunks[j + 1], base.bodyChunks[j].rad + ((j == 0) ? 0f : 9f), BodyChunkConnection.Type.Normal, 1f, 0.5f);
		}
		base.airFriction = 0.95f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.17f;
		collisionLayer = 1;
		base.waterFriction = 0.92f;
		base.buoyancy = 0.95f;
		behavior = new PrinceBehavior(this);
		head.Reset(pos);
		if (abstractPhysicalObject.world.game.rainWorld.progression.miscProgressionData.beaten_Watcher_SentientRot && !Region.IsDaemonRegion(abstractPhysicalObject.world.name))
		{
			bloom = 1f;
			lastBloom = 1f;
		}
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			GraphicsModule graphicsModule2 = (base.graphicsModule = new PrinceGraphics(this, internalContainers: false));
		}
	}

	public override void HitByWeapon(Weapon weapon)
	{
		base.HitByWeapon(weapon);
		behavior.Pain();
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastBloom = bloom;
		behavior.Update();
		head.lastPos = head.pos;
		head.vel *= 0.7f;
		head.vel += behavior.GetToDir;
		head.vel += (head.pos - base.bodyChunks[0].pos).normalized * ((neckLength - Vector2.Distance(base.bodyChunks[0].pos, head.pos)) * 0.2f);
		head.pos += head.vel;
		foreach (AbstractCreature player2 in room.game.Players)
		{
			if (player2.realizedCreature is Player player && player.room == room && player.graphicsModule is PlayerGraphics playerGraphics && Custom.DistLess(player.firstChunk.pos, head.pos, 1500f))
			{
				playerGraphics.LookAtPoint(head.pos, 10000f);
			}
		}
	}
}
