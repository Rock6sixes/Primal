using RWCustom;
using UnityEngine;

namespace Watcher;

public class BigSandGrubNeck
{
	public class Segment
	{
		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		public float rad;

		public Rope rope;

		public Segment previous;

		public BigSandGrubNeck owner;

		public Segment(BigSandGrubNeck owner, Vector2 pos, float rad, Segment previous)
		{
			this.owner = owner;
			this.pos = pos;
			lastPos = pos;
			this.rad = rad;
			this.previous = previous;
		}

		public void Reset(Vector2 pos)
		{
			this.pos = pos;
			lastPos = pos;
			vel = default(Vector2);
			rope?.Reset(pos);
		}

		public void Update()
		{
			lastPos = pos;
			pos += vel;
			if (previous != null)
			{
				if (rope == null)
				{
					rope = new Rope(owner.owner.room, pos, previous.pos, 0.1f);
				}
				rope.Update(pos, previous.pos);
				float totalLength = rope.totalLength;
				float num = owner.length / (float)(owner.tChunks.Length - 1) * (1f - owner.retractFac);
				if (totalLength > num)
				{
					Vector2 normalized = (rope.AConnect - rope.A).normalized;
					Vector2 normalized2 = (rope.BConnect - rope.B).normalized;
					pos += normalized * (totalLength - num) / 2f;
					vel += normalized * (totalLength - num) / 2f;
					previous.pos += normalized2 * (totalLength - num) / 2f;
					previous.vel += normalized2 * (totalLength - num) / 2f;
				}
			}
			SharedPhysics.TerrainCollisionData cd2 = SharedPhysics.VerticalCollision(cd: new SharedPhysics.TerrainCollisionData(pos, lastPos, vel, rad, new IntVector2(0, 0), goThroughFloors: true), room: owner.owner.room);
			cd2 = SharedPhysics.SlopesVertically(owner.owner.room, cd2);
			cd2 = SharedPhysics.HorizontalCollision(owner.owner.room, cd2);
			pos = cd2.pos;
			vel = cd2.vel;
		}
	}

	public Segment[] tChunks;

	public float length;

	public float speed = 0.6f;

	public bool limp;

	public float retractFac;

	public SandGrub owner;

	public Vector2? grabDest;

	public Segment Tip => tChunks[tChunks.Length - 1];

	public Vector2 BasePos
	{
		get
		{
			if (owner.burrow != null)
			{
				return owner.burrow.pos;
			}
			return owner.bodyChunks[0].pos;
		}
	}

	public BigSandGrubNeck(SandGrub owner, int segments, float length, float rad)
	{
		this.owner = owner;
		tChunks = new Segment[segments];
		for (int i = 0; i < tChunks.Length; i++)
		{
			tChunks[i] = new Segment(this, default(Vector2), rad, (i == 0) ? null : tChunks[i - 1]);
		}
		this.length = length;
	}

	public void Update()
	{
		if (owner.burrow == null)
		{
			grabDest = null;
		}
		if (grabDest.HasValue)
		{
			Tip.vel += (grabDest.Value - Tip.pos).normalized * speed;
		}
		for (int i = 0; i < tChunks.Length; i++)
		{
			tChunks[i].Update();
			tChunks[i].vel.y -= owner.gravity * (limp ? 1f : 0.2f);
		}
		tChunks[0].pos = BasePos;
		tChunks[0].vel = default(Vector2);
	}

	public void MoveGrabDest(Vector2 pos)
	{
		grabDest = pos;
	}

	public void ClearGrabDest()
	{
		grabDest = null;
	}

	public void Reset(Vector2 pos)
	{
		Segment[] array = tChunks;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Reset(pos);
		}
		grabDest = null;
	}
}
