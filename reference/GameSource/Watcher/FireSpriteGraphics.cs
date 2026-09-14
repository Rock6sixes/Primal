using RWCustom;
using UnityEngine;

namespace Watcher;

public class FireSpriteGraphics : GraphicsModule
{
	public class Wing
	{
		public Vector2 root;

		public Vector2 pos;

		public int index;

		public float randomOffset;

		public int segments;

		public Vector2 bodyPos;

		public FireSprite owner;

		public Room room;

		public float[] rotation { get; set; }

		public float[] lastRotation { get; set; }

		public Vector2[] points { get; set; }

		public Vector2[] segPos { get; set; }

		public Vector2[] lastSegPos { get; set; }

		public Vector2 wingScale { get; set; }

		public Vector2 lastWingScale { get; set; }

		public Wing(FireSprite ow, Room room, int index, int lengthSegments)
		{
			owner = ow;
			this.index = index;
			this.room = room;
			segments = lengthSegments;
			rotation = new float[segments * 2];
			lastRotation = new float[segments * 2];
			points = new Vector2[segments * 2];
			segPos = new Vector2[segments * 4];
			lastSegPos = new Vector2[segments * 4];
			for (int i = 0; i < lastSegPos.Length; i++)
			{
				lastSegPos[i] = ow.bodyChunks[0].pos;
				segPos[i] = ow.bodyChunks[0].pos;
			}
			randomOffset = Random.value;
		}

		public void UpdateSegmentPosition()
		{
			Vector2 vector = owner.mainBodyChunk.pos;
			for (int i = 0; i < points.Length; i++)
			{
				_ = (float)i / (float)segments;
				int num = i;
				Vector2 vector2 = Custom.DegToVec(rotation[i]);
				if (i > 0)
				{
					Vector2 b = points[num - 1] + vector2 * (wingScale.y * (24f / (float)segments));
					points[i] = Vector2.Lerp(points[i], b, 0.4f);
				}
				else
				{
					points[i] = vector;
				}
			}
			for (int j = 0; j < segments * 2; j++)
			{
				int num2 = j * 2;
				int num3 = num2 + 1;
				int num4 = Mathf.Max(j - 1, 0);
				int num5 = Mathf.Min(j + 1, points.Length - 1);
				lastSegPos[num2] = segPos[num2];
				lastSegPos[num3] = segPos[num3];
				Vector2 vector3 = Vector2.zero;
				Vector2 vector4 = Custom.PerpendicularVector((points[num5] - points[j]).normalized) * (wingScale.x * 10f);
				if (j != 0 && j != segments * 2 - 1)
				{
					Vector2 v = points[j] - points[num4];
					Vector2 v2 = points[num5] - points[j];
					vector4 = Vector2.Lerp(Custom.PerpendicularVector(v), Custom.PerpendicularVector(v2), 0.5f) * (wingScale.x * 10f);
					vector3 = (v2.normalized - v.normalized) * (vector4.magnitude * 1f);
				}
				segPos[num2] = points[j] + vector4 - vector3;
				segPos[num3] = points[j] - vector4 - vector3;
			}
			bodyPos = vector;
		}

		public void UpdateWingRotation(bool landed, float flyDirection, float lastFlyDirection, float speed)
		{
			float b = ((!landed) ? Mathf.LerpAngle((float)(120 * (index - 1)) + flyDirection, flyDirection, speed * 0.7f) : Custom.VecToDeg(pos - room.MiddleOfTile(room.GetTilePosition(pos) - Custom.eightDirections[index + 5])));
			for (int i = 0; i < rotation.Length; i++)
			{
				lastRotation[i] = rotation[i];
			}
			for (int num = rotation.Length - 1; num > 0; num--)
			{
				rotation[num] = rotation[num - 1];
			}
			rotation[0] = Mathf.LerpAngle(rotation[0], b, owner.ai.collectingEnergy ? 0.01f : 0.5f);
		}

		public void UpdateWingScale(bool landed, float speed)
		{
			float num = Mathf.Clamp01(Mathf.Abs(owner.graphicsData.lastFlyDirection - owner.graphicsData.flyDirection) / 20f);
			Vector2 b = ((!landed) ? new Vector2(2f + 3f * Mathf.Clamp01(1f - speed) * (1.01f - num), (1f + (2f + 2f * (float)(index % 2)) * speed) * (1.01f - num)) : (new Vector2(2f, 1f + 0.5f * (float)((index + 1) % 2)) * 3f));
			if (owner.divingToHelp)
			{
				b = Vector2.Lerp(wingScale, Vector2.one * 0.0001f, 0.8f);
			}
			lastWingScale = wingScale;
			if (owner.enteringShortCut.HasValue)
			{
				b = Vector2.zero;
			}
			wingScale = Vector2.Lerp(wingScale, b, speed * 0.2f) * owner.justSwitchedRooms.normalized;
		}

		public void Reset()
		{
			Vector2 vector = (bodyPos = owner.abstractCreature.Room.realizedRoom.MiddleOfTile(owner.abstractCreature.pos));
			for (int i = 0; i < points.Length; i++)
			{
				points[i] = vector;
			}
			for (int j = 0; j < segPos.Length; j++)
			{
				segPos[j] = vector;
				lastSegPos[j] = vector;
			}
			for (int k = 0; k < rotation.Length; k++)
			{
				rotation[k] = 0f;
				lastRotation[k] = 0f;
			}
		}
	}

	public class Data
	{
		public Wing[] wings;

		public FireSprite owner;

		public float speed;

		public float lastSpeed;

		public float flyDirection;

		public float lastFlyDirection;

		public float lastWingAnimation;

		public float wingAnimation;

		public int totalSprites;

		public int widthSegments = 4;

		public int lengthSegments = 8;

		public float wingsOpen = 1f;

		public int wingSpin;

		public bool clipped;

		public Vector2 Pos => owner.mainBodyChunk.pos;

		public Vector2 LastPos => owner.mainBodyChunk.lastPos;

		public Data(FireSprite owner)
		{
			this.owner = owner;
		}

		public void Update()
		{
			wingSpin = (wingSpin + 1) % 360;
			lastSpeed = speed;
			wingsOpen = Mathf.Lerp(wingsOpen, owner.wingOpenTimer.normalized, 0.3f);
			speed = Mathf.Lerp(lastSpeed, Mathf.Clamp01(Mathf.InverseLerp(0f, 14f, Vector2.Distance(Pos, LastPos))), 0.2f);
			lastFlyDirection = flyDirection;
			clipped = owner.IsFarFromCamera || !owner.room.BeingViewed;
			if (owner.ai.collectingEnergy)
			{
				flyDirection = Mathf.LerpAngle(flyDirection, 180f, 0.5f);
			}
			else
			{
				flyDirection = Mathf.LerpAngle(flyDirection, Custom.VecToDeg(LastPos - Pos), 3f);
			}
			for (int i = 0; i < wings.Length; i++)
			{
				wings[i].pos = Pos;
				wings[i].UpdateWingRotation(owner.ai.collectingEnergy, flyDirection, lastFlyDirection, speed);
				wings[i].UpdateWingScale(owner.ai.collectingEnergy, speed);
				wings[i].UpdateSegmentPosition();
			}
			lastWingAnimation = wingAnimation;
			wingAnimation += speed * 0.075f;
		}

		public void Reset()
		{
			if (!owner.ai.collectingEnergy)
			{
				for (int i = 0; i < wings.Length; i++)
				{
					wings[i].Reset();
				}
			}
		}
	}

	public bool refreshUV = true;

	public FireSprite ThisCreature => base.owner as FireSprite;

	public Room room => base.owner.room;

	public Data data => ThisCreature.graphicsData;

	public Wing[] wings => data.wings;

	public float speed
	{
		get
		{
			return data.speed;
		}
		set
		{
			data.speed = value;
		}
	}

	public float lastSpeed
	{
		get
		{
			return data.lastSpeed;
		}
		set
		{
			data.lastSpeed = value;
		}
	}

	public float flyDirection
	{
		get
		{
			return data.flyDirection;
		}
		set
		{
			data.flyDirection = value;
		}
	}

	public float lastFlyDirection
	{
		get
		{
			return data.lastFlyDirection;
		}
		set
		{
			data.lastFlyDirection = value;
		}
	}

	public float lastWingAnimation
	{
		get
		{
			return data.lastWingAnimation;
		}
		set
		{
			data.lastWingAnimation = value;
		}
	}

	public float wingAnimation
	{
		get
		{
			return data.wingAnimation;
		}
		set
		{
			data.wingAnimation = value;
		}
	}

	public int totalSprites
	{
		get
		{
			return data.totalSprites;
		}
		set
		{
			data.totalSprites = value;
		}
	}

	public int widthSegments
	{
		get
		{
			return data.widthSegments;
		}
		set
		{
			data.widthSegments = value;
		}
	}

	public int lengthSegments
	{
		get
		{
			return data.lengthSegments;
		}
		set
		{
			data.lengthSegments = value;
		}
	}

	public float wingsOpen
	{
		get
		{
			return data.wingsOpen;
		}
		set
		{
			data.wingsOpen = value;
		}
	}

	public int wingSpin
	{
		get
		{
			return data.wingSpin;
		}
		set
		{
			data.wingSpin = value;
		}
	}

	public FireSpriteGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		if (data == null)
		{
			ThisCreature.graphicsData = new Data(ThisCreature);
		}
		totalSprites = 1;
		if (wings == null)
		{
			data.wings = new Wing[3];
			for (int i = 0; i < wings.Length; i++)
			{
				wings[i] = new Wing(ow as FireSprite, room, i, lengthSegments);
			}
		}
		totalSprites += (lengthSegments - 3) * 3;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites = new FSprite[totalSprites];
		refreshUV = true;
		int num = 0;
		sLeaser.sprites[num] = new FSprite("Futile_White");
		sLeaser.sprites[num].shader = Custom.rainWorld.Shaders["FireSpriteBody"];
		sLeaser.sprites[num].color = BoxWormGraphics.BaseColor(room);
		sLeaser.sprites[num].alpha = 1f;
		num++;
		for (int i = 0; i < wings.Length; i++)
		{
			for (int j = 0; j < lengthSegments - 3; j++)
			{
				FSprite fSprite = TriangleMesh.MakeLongMesh(widthSegments, pointyTip: false, customColor: true);
				fSprite.shader = Custom.rainWorld.Shaders["FireSpriteWing"];
				sLeaser.sprites[num + i * (lengthSegments - 3) + j] = fSprite;
			}
		}
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Bloom"));
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (!ThisCreature.dead)
		{
			base.AddToContainer(sLeaser, rCam, newContatiner);
			for (int i = 1; i < totalSprites; i++)
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
			newContatiner.AddChild(sLeaser.sprites[0]);
		}
	}

	public void RefreshUV(int index, RoomCamera.SpriteLeaser sLeaser)
	{
		if (!refreshUV)
		{
			return;
		}
		for (int i = 0; i < wings.Length; i++)
		{
			for (int j = 0; j < lengthSegments - 3; j++)
			{
				int num = index + i * (lengthSegments - 3) + j;
				TriangleMesh triangleMesh = sLeaser.sprites[num] as TriangleMesh;
				Vector2 vector = Vector2.up * ((float)j / (float)(lengthSegments - 3));
				Vector2 vector2 = vector + Vector2.right;
				Vector2 vector3 = Vector2.up * ((float)(j + 1) / (float)(lengthSegments - 3));
				Vector2 vector4 = vector3 + Vector2.right;
				triangleMesh.UVvertices[0] = vector;
				triangleMesh.UVvertices[1] = vector3;
				triangleMesh.UVvertices[triangleMesh.vertices.Length - 2] = vector2;
				triangleMesh.UVvertices[triangleMesh.vertices.Length - 1] = vector4;
				for (int k = 1; k <= (widthSegments - 1) * 2; k++)
				{
					float t = (float)k / (float)(widthSegments * 2);
					triangleMesh.UVvertices[k * 2] = Vector2.Lerp(vector, vector2, t);
					triangleMesh.UVvertices[k * 2 + 1] = Vector2.Lerp(vector3, vector4, t);
				}
			}
		}
		refreshUV = false;
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (ThisCreature.dead)
		{
			sLeaser.RemoveAllSpritesFromContainer();
			return;
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].isVisible = !data.clipped;
		}
		if (data.clipped)
		{
			return;
		}
		culled = ThisCreature.ai.behavior == FireSpriteAI.Behavior.Helping;
		Vector2 position = Vector2.Lerp(ThisCreature.mainBodyChunk.lastPos, ThisCreature.mainBodyChunk.pos, timeStacker) - camPos;
		float rotation = Mathf.Lerp(lastFlyDirection, flyDirection, timeStacker);
		int num = 0;
		Vector2 vector = Custom.EncodeFloatRG(Custom.Mod(Mathf.Lerp(lastWingAnimation, wingAnimation, timeStacker), 1f));
		float b = Mathf.Clamp01(wingsOpen * 2f) * 0.2f + Mathf.Lerp(lastSpeed, speed, timeStacker) * 0.8f;
		sLeaser.sprites[num].SetPosition(position);
		sLeaser.sprites[num].rotation = rotation;
		sLeaser.sprites[num].scaleX = 2f - speed * 0.5f;
		sLeaser.sprites[num].scaleY = 2f + speed * 2f;
		sLeaser.sprites[num].color = new Color(vector.x, vector.y, b, 0f);
		num++;
		RefreshUV(num, sLeaser);
		for (int j = 0; j < wings.Length; j++)
		{
			_ = Vector2.Lerp(wings[j].lastWingScale, wings[j].wingScale, timeStacker) * 5f;
			for (int k = 0; k < lengthSegments - 3; k++)
			{
				int num2 = num + j * (lengthSegments - 3) + k;
				sLeaser.sprites[num2].color = new Color(vector.x, vector.y, b, wings[j].randomOffset);
				TriangleMesh triangleMesh = sLeaser.sprites[num2] as TriangleMesh;
				int num3 = k * 2;
				int num4 = num3 + 1;
				Vector2 vector2 = Vector2.Lerp(wings[j].lastSegPos[num3 + 2], wings[j].segPos[num3 + 2], timeStacker) - camPos;
				Vector2 vector3 = Vector2.Lerp(wings[j].lastSegPos[num4 + 2], wings[j].segPos[num4 + 2], timeStacker) - camPos;
				Vector2 vector4 = Vector2.Lerp(wings[j].lastSegPos[num3 + 4], wings[j].segPos[num3 + 4], timeStacker) - camPos;
				Vector2 vector5 = Vector2.Lerp(wings[j].lastSegPos[num4 + 4], wings[j].segPos[num4 + 4], timeStacker) - camPos;
				triangleMesh.MoveVertice(0, vector2);
				triangleMesh.MoveVertice(1, vector4);
				triangleMesh.MoveVertice(triangleMesh.vertices.Length - 2, vector3);
				triangleMesh.MoveVertice(triangleMesh.vertices.Length - 1, vector5);
				for (int l = 1; l <= (widthSegments - 1) * 2; l++)
				{
					float t = (float)l / (float)(widthSegments * 2);
					triangleMesh.MoveVertice(l * 2, Vector2.Lerp(vector2, vector3, t));
					triangleMesh.MoveVertice(l * 2 + 1, Vector2.Lerp(vector4, vector5, t));
				}
			}
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
	}

	public override void PushOutOf(Vector2 pos, float rad)
	{
		base.PushOutOf(pos, rad);
	}

	public override void Reset()
	{
		base.Reset();
		data.Reset();
	}

	public override void SuckedIntoShortCut(Vector2 shortCutPosition)
	{
		base.SuckedIntoShortCut(shortCutPosition);
	}
}
