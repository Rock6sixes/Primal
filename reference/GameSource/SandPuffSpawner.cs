using RWCustom;
using UnityEngine;

public class SandPuffSpawner : UpdatableAndDeletable
{
	public class SandPuff : CosmeticSprite
	{
		public float life;

		public float lastLife;

		public float lifeTime;

		public float size;

		public Vector2 motion;

		public Color color;

		public bool foreground;

		public SandPuff(Vector2 pos, float size, float lifeTime = 0f, bool foreground = false)
		{
			base.pos = pos;
			lastPos = pos;
			this.size = size;
			this.foreground = foreground;
			lastLife = 1f;
			life = 1f;
			if (lifeTime == 0f)
			{
				this.lifeTime = Mathf.Lerp(40f, 120f, Random.value) * Mathf.Lerp(0.5f, 1.5f, size);
			}
			else
			{
				this.lifeTime = lifeTime;
			}
			motion = new Vector2(0f, -0.25f);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			pos.y += motion.x;
			pos.x += motion.y;
			lastLife = life;
			life -= 1f / lifeTime;
			if (lastLife < 0f)
			{
				Destroy();
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			motion = (room.snow ? new Vector2(0f, 0f) : motion);
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["Spores"];
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer(foreground ? "Foreground" : "Background"));
			base.InitiateSprites(sLeaser, rCam);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			if (rCam.terrainPalette != null)
			{
				bool flag = rCam.LitAtCoordinate(pos) ?? (rCam.currentPalette.darkness < 0.5f);
				sLeaser.sprites[0].color = (flag ? rCam.terrainPalette.LightDustColor : rCam.terrainPalette.DarkDustColor);
			}
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
			sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
			sLeaser.sprites[0].scale = 10f * Mathf.Pow(1f - Mathf.Lerp(lastLife, life, timeStacker), 0.35f) * Mathf.Lerp(0.5f, 1.5f, size);
			sLeaser.sprites[0].alpha = Mathf.Lerp(lastLife, life, timeStacker);
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		foreach (AbstractCreature player in room.game.Players)
		{
			if (player.realizedCreature == null || player.realizedCreature.room != room)
			{
				continue;
			}
			BodyChunk[] bodyChunks = player.realizedObject.bodyChunks;
			foreach (BodyChunk bodyChunk in bodyChunks)
			{
				if (bodyChunk.ContactPoint.y < 0 && bodyChunk.terrainCurveNormal != Vector2.zero && room.aimap != null && room.aimap.getAItile(room.GetTilePosition(bodyChunk.pos + new Vector2(0f, -20f))).acc == AItile.Accessibility.Sand)
				{
					if (bodyChunk.lastContactPoint.y >= 0 && bodyChunk.lastPos.y - bodyChunk.pos.y > 5f)
					{
						Vector2 pos = bodyChunk.pos + new Vector2(0f, 0f - bodyChunk.rad);
						float rad = Custom.LerpMap(bodyChunk.lastPos.y - bodyChunk.pos.y, 5f, 10f, 0.5f, 1f);
						SpawnPuff(pos, rad);
					}
					else if (Random.value < 0.1f && Mathf.Abs(bodyChunk.lastPos.x - bodyChunk.pos.x) > 3f)
					{
						Vector2 pos2 = bodyChunk.pos + new Vector2(0f, 0f - bodyChunk.rad);
						float rad2 = 0.25f * Random.value;
						SpawnPuff(pos2, rad2);
					}
				}
			}
		}
	}

	public void SpawnPuff(Vector2 pos, float rad)
	{
		room.AddObject(new SandPuff(pos, rad));
	}
}
