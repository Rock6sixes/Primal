using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class LocustShadows : CosmeticSprite
{
	public struct Cluster
	{
		public Vector2 lastPos;

		public Vector2 pos;

		public float seed;

		public float scale;

		public Cluster(Vector2 pos, float seed, float scale)
		{
			this.pos = pos;
			this.seed = seed;
			this.scale = scale;
			lastPos = pos;
		}
	}

	public float amount;

	public Vector2 dir;

	public List<Cluster> clusters = new List<Cluster>();

	public float spawnCounter;

	public float clustersPerTick;

	public float speed = 7f;

	public int timer;

	public LocustShadows()
	{
		dir = Custom.RotateAroundOrigo(Vector2.right, -20f);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		amount = room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.LocustSwarmConfig)?.GetAmount(3) ?? 0f;
		clustersPerTick = Mathf.Lerp(0f, 1f, Mathf.Pow(amount, 2f));
		timer++;
		FloatRect rect = new FloatRect(0f, 0f, 1400f, 800f).Grow(128f);
		if (clustersPerTick > 0f)
		{
			float num = 1f / clustersPerTick;
			spawnCounter += 1f;
			while (spawnCounter > num)
			{
				spawnCounter -= num;
				float num2 = spawnCounter * speed;
				Vector2 edgePoint = GetEdgePoint(rect, UnityEngine.Random.value);
				if (edgePoint != Vector2.zero)
				{
					clusters.Add(CreateCluster(edgePoint + dir * num2));
				}
			}
		}
		for (int num3 = clusters.Count - 1; num3 >= 0; num3--)
		{
			Cluster value = clusters[num3];
			value.lastPos = value.pos;
			value.pos += dir * speed;
			clusters[num3] = value;
		}
		FloatRect liveBounds = rect.Grow(50f);
		clusters.RemoveAll((Cluster cluster) => !liveBounds.Vector2Inside(cluster.pos));
	}

	public Cluster CreateCluster(Vector2 pos)
	{
		return new Cluster(pos, UnityEngine.Random.value, UnityEngine.Random.Range(0.8f, 1.2f));
	}

	public Vector2 GetEdgePoint(FloatRect rect, float t)
	{
		Vector2 vector = ((dir.y > 0f != dir.x > 0f) ? Vector2.Lerp(new Vector2(rect.left, rect.bottom), new Vector2(rect.right, rect.top), t) : Vector2.Lerp(new Vector2(rect.left, rect.top), new Vector2(rect.right, rect.bottom), t));
		Vector2 vector2 = vector - (rect.right - rect.left + rect.top - rect.bottom) * dir;
		FloatRect floatRect = Custom.RectCollision(vector, vector2, rect);
		return new Vector2(floatRect.left, floatRect.bottom);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		LocustSystem.LoadLocustClusterGraphic();
		sLeaser.sprites = new FSprite[2];
		sLeaser.sprites[0] = new FSprite("pixel")
		{
			scaleY = 800f,
			scaleX = 2f,
			anchorX = 0f,
			anchorY = 0f
		};
		sLeaser.sprites[1] = new FSprite("pixel")
		{
			scaleX = 1400f,
			scaleY = 2f,
			anchorX = 0f,
			anchorY = 0f
		};
		sLeaser.containers = new FContainer[1]
		{
			new FContainer()
		};
		AddToContainer(sLeaser, rCam, null);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Shadows");
		}
		newContatiner.AddChild(sLeaser.containers[0]);
		newContatiner.AddChild(sLeaser.sprites[0]);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		Vector2 vector = camPos - rCam.room.cameraPositions[rCam.currentCameraPosition];
		FContainer fContainer = sLeaser.containers[0];
		int num = 0;
		foreach (Cluster cluster in clusters)
		{
			if (fContainer.GetChildCount() <= num)
			{
				fContainer.AddChild(new FSprite("locust_cluster")
				{
					shader = rCam.game.rainWorld.Shaders["LocustClusterShadow"]
				});
			}
			float t = Mathf.PingPong(cluster.seed + ((float)timer + timeStacker) * Mathf.Lerp(0.7f, 1.2f, cluster.seed), 1f);
			t = (float)Math.PI / 180f * Mathf.Lerp(-45f, 45f, t);
			FSprite obj = (FSprite)fContainer.GetChildAt(num);
			obj.isVisible = true;
			obj.SetPosition(Vector2.Lerp(cluster.lastPos, cluster.pos, timeStacker) - vector);
			obj.scale = cluster.scale;
			obj.color = new Color(Mathf.Cos(t), Mathf.Sin(t), 0f, 1f);
			num++;
		}
		for (int i = num; i < fContainer.GetChildCount(); i++)
		{
			fContainer.GetChildAt(i).isVisible = false;
		}
		sLeaser.sprites[0].x = ((room.lightAngle.x > 0f) ? 0f : (rCam.sSize.x - 1f));
		sLeaser.sprites[0].color = Color.black;
		sLeaser.sprites[1].y = ((room.lightAngle.y < 0f) ? 0f : (rCam.sSize.y - 1f));
		sLeaser.sprites[1].color = Color.black;
	}
}
