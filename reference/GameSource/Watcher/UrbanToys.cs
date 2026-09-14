using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class UrbanToys
{
	public class BallToy : PlayerCarryableItem, IDrawable
	{
		public float lastRotation;

		public float rotation;

		public float rotationVel;

		public StaticSoundLoop rollLoop;

		public float groundContact;

		public bool interactedWith;

		public BallToy(AbstractPhysicalObject abstractPhysicalObject)
			: base(abstractPhysicalObject)
		{
			base.bodyChunks = new BodyChunk[1];
			base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 26f, 0.2f);
			bodyChunkConnections = new BodyChunkConnection[0];
			base.airFriction = 0.999f;
			base.gravity = 0.9f;
			surfaceFriction = 0.8f;
			bounce = 0.9f;
			collisionLayer = 1;
			base.waterFriction = 0.95f;
			base.buoyancy = 1.1f;
		}

		public override void Update(bool eu)
		{
			lastRotation = rotation;
			base.Update(eu);
			rotationVel *= 0.98f;
			if (base.firstChunk.ContactPoint.y == -1)
			{
				rotationVel = (base.firstChunk.pos.x - base.firstChunk.lastPos.x) / ((float)Math.PI * 2f * base.firstChunk.rad * 5f);
			}
			rotation -= rotationVel;
			groundContact = Mathf.Lerp(groundContact, (base.firstChunk.ContactPoint.y == -1) ? 1 : 0, 0.9f);
			if (rollLoop != null)
			{
				rollLoop.Update();
				rollLoop.pos = base.firstChunk.pos;
				rollLoop.volume = Mathf.Clamp01(Mathf.Abs(base.firstChunk.vel.x) / 20f) * groundContact;
				rollLoop.pitch = 0.2f + Mathf.Clamp01(Mathf.Abs(base.firstChunk.vel.x) / 20f) * 0.8f;
			}
			else
			{
				rollLoop = new StaticSoundLoop(WatcherEnums.WatcherSoundID.BallToy_Roll_LOOP, default(Vector2), room, 1f, 1f);
			}
		}

		public override void PlaceInRoom(Room placeRoom)
		{
			base.PlaceInRoom(placeRoom);
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
		}

		public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
		{
			if (firstContact && speed > 1f)
			{
				float num = Mathf.Clamp01(speed / 8f);
				room.PlaySound(GetImpactSound(num), base.firstChunk.pos, num, 1f);
			}
			base.TerrainImpact(chunk, direction, speed, firstContact);
		}

		public SoundID GetImpactSound(float speed)
		{
			if (speed > 0.7f)
			{
				return WatcherEnums.WatcherSoundID.BallToy_BounceH;
			}
			if (speed > 0.3f)
			{
				return WatcherEnums.WatcherSoundID.BallToy_BounceM;
			}
			return WatcherEnums.WatcherSoundID.BallToy_BounceS;
		}

		public override void PickedUp(Creature upPicker)
		{
			interactedWith = true;
			base.PickedUp(upPicker);
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].color = Color.blue;
			sLeaser.sprites[0].scale = base.firstChunk.rad / 8f;
			sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["BallToy"];
			AddToContainer(sLeaser, rCam, null);
		}

		public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
		{
			base.Collide(otherObject, myChunk, otherChunk);
			if (otherObject is Player)
			{
				interactedWith = true;
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].SetPosition(Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker) - camPos);
			}
			Color color = sLeaser.sprites[0].color;
			color.a = Custom.Mod(Mathf.Lerp(lastRotation, rotation, timeStacker), 1f);
			sLeaser.sprites[0].color = color;
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Items");
			}
			newContatiner.AddChild(sLeaser.sprites[0]);
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			Color color = Color.Lerp(Color.Lerp(palette.blackColor, new Color(0.6f, 0.1f, 0.4f), 0.3f), palette.fogColor, 0.2f);
			sLeaser.sprites[0].color = color;
		}
	}

	public class SoftToy : PlayerCarryableItem, IDrawable
	{
		public float headRad = 15f;

		public int headCount = 5;

		public int neckSprites = 30;

		public Vector2[] headOffsets;

		public bool randomizeOffset;

		public float sitsOnGround;

		public bool interactedWith;

		public bool[] squeaked;

		public Counter soundCount = new Counter(10);

		public int neckIndex => 1 + headCount;

		public int eyeIndex => neckIndex + headCount * neckSprites;

		public float headTargetDistance => base.bodyChunks[0].rad + base.bodyChunks[1].rad + 20f;

		public SoftToy(AbstractPhysicalObject abstractPhysicalObject)
			: base(abstractPhysicalObject)
		{
			base.bodyChunks = new BodyChunk[1 + headCount];
			base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), headRad * 1.5f, 0.5f);
			for (int i = 1; i < base.bodyChunks.Length; i++)
			{
				base.bodyChunks[i] = new BodyChunk(this, i, new Vector2(0f, 0f), headRad, 0.01f);
			}
			bodyChunkConnections = new BodyChunkConnection[headCount];
			for (int j = 0; j < headCount; j++)
			{
				bodyChunkConnections[j] = new BodyChunkConnection(base.bodyChunks[1 + j], base.bodyChunks[0], headTargetDistance, BodyChunkConnection.Type.Pull, 0.9f, -1f);
			}
			base.airFriction = 0.999f;
			base.gravity = 0.9f;
			surfaceFriction = 0.3f;
			bounce = 0.3f;
			collisionLayer = 1;
			base.waterFriction = 0.95f;
			base.buoyancy = 1.1f;
			headOffsets = new Vector2[headCount];
			squeaked = new bool[1 + headCount];
			HeadOffset();
		}

		public void HeadOffset()
		{
			for (int i = 0; i < headCount; i++)
			{
				float num = Mathf.Ceil((float)i / 2f);
				if (i % 2 == 0)
				{
					num *= -1f;
				}
				headOffsets[i] = Vector2.ClampMagnitude(Custom.rotateVectorDeg(Vector2.up * headTargetDistance, num * headRad * 2f) + Custom.RNV() * (headTargetDistance * 0.25f), headTargetDistance);
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			RandomizeOnPickup();
			MoveHeads();
			if (grabbedBy.Count == 0)
			{
				soundCount.Tick();
			}
			sitsOnGround = Mathf.Lerp(sitsOnGround, (base.bodyChunks[0].ContactPoint.y == -1) ? 1 : 0, 0.1f);
			UpdateSqueak();
		}

		public void RandomizeOnPickup()
		{
			if (grabbedBy.Count > 0 && grabbedBy[0].grabbedChunk.index == 0)
			{
				randomizeOffset = true;
			}
			else if (randomizeOffset)
			{
				HeadOffset();
				randomizeOffset = false;
			}
		}

		public void MoveHeads()
		{
			for (int i = 0; i < base.bodyChunks.Length - 1; i++)
			{
				base.bodyChunks[i + 1].vel = Vector2.zero;
				if (grabbedBy.Count > 0 && grabbedBy[0].grabbedChunk == base.bodyChunks[i + 1])
				{
					headOffsets[i] = Vector2.ClampMagnitude(headOffsets[i] + (base.bodyChunks[i + 1].pos - base.bodyChunks[0].pos + headOffsets[i]) * 0.01f, headTargetDistance);
				}
				else
				{
					base.bodyChunks[i + 1].pos = Vector2.Lerp(base.bodyChunks[i + 1].pos, base.bodyChunks[0].pos + headOffsets[i], 0.1f);
				}
			}
		}

		public override void PlaceInRoom(Room placeRoom)
		{
			base.PlaceInRoom(placeRoom);
			BodyChunk[] array = base.bodyChunks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			}
		}

		public void HitSound(float speed)
		{
			if (soundCount.isFinished)
			{
				room.PlaySound(WatcherEnums.WatcherSoundID.SoftToy_Hit, base.firstChunk, loop: false, Mathf.Clamp01(speed / 9f), 1f);
				soundCount.Reset();
			}
		}

		public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
		{
			if (firstContact && speed > 3f)
			{
				HitSound(speed);
			}
			base.TerrainImpact(chunk, direction, speed, firstContact);
		}

		public void UpdateSqueak()
		{
			if (grabbedBy.Count <= 0)
			{
				for (int i = 0; i < squeaked.Length; i++)
				{
					squeaked[i] = false;
				}
				return;
			}
			for (int j = 0; j < grabbedBy.Count; j++)
			{
				int index = grabbedBy[j].grabbedChunk.index;
				if (!squeaked[index])
				{
					squeaked[index] = true;
					room.PlaySound(WatcherEnums.WatcherSoundID.SoftToy_Squeak, base.bodyChunks[index]);
					if (grabbedBy[j].grabber is Player)
					{
						interactedWith = true;
					}
				}
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1 + headCount * 2 + headCount * neckSprites];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].scale = base.firstChunk.rad / 8f * 1.3f;
			sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["SoftToyBody"];
			sLeaser.sprites[0].rotation = UnityEngine.Random.Range(0, 360);
			Color black = Color.black;
			for (int i = 1; i < neckIndex; i++)
			{
				sLeaser.sprites[i] = new FSprite("Futile_White");
				sLeaser.sprites[i].scale = base.bodyChunks[i].rad / 8f * 1.5f;
				sLeaser.sprites[i].shader = Custom.rainWorld.Shaders["SoftToyBody"];
				sLeaser.sprites[i].rotation = UnityEngine.Random.Range(0, 360);
				black.a = UnityEngine.Random.value;
				sLeaser.sprites[i].color = black;
			}
			for (int j = neckIndex; j < eyeIndex; j++)
			{
				sLeaser.sprites[j] = new FSprite("Futile_White");
				sLeaser.sprites[j].scale = base.bodyChunks[0].rad / 8f * 0.4f + UnityEngine.Random.value * 0.2f;
				sLeaser.sprites[j].shader = Custom.rainWorld.Shaders["SoftToyBody"];
				sLeaser.sprites[j].rotation = UnityEngine.Random.Range(0, 360);
				black.a = UnityEngine.Random.value;
				sLeaser.sprites[j].color = black;
			}
			for (int k = eyeIndex; k < sLeaser.sprites.Length; k++)
			{
				sLeaser.sprites[k] = new FSprite("Futile_White");
				sLeaser.sprites[k].scale = base.bodyChunks[0].rad / 8f * 0.6f;
				sLeaser.sprites[k].shader = Custom.rainWorld.Shaders["SoftToyEye"];
				sLeaser.sprites[k].rotation = UnityEngine.Random.Range(0, 360);
				black.a = UnityEngine.Random.value;
				sLeaser.sprites[k].color = black;
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.down * (sitsOnGround * base.bodyChunks[0].rad * 0.7f);
			for (int i = 0; i < neckIndex; i++)
			{
				sLeaser.sprites[i].SetPosition(Vector2.Lerp(base.bodyChunks[i].lastLastPos, base.bodyChunks[i].lastPos, timeStacker) - camPos);
			}
			sLeaser.sprites[0].scale = base.firstChunk.rad / 8f * (1.3f + sitsOnGround * 0.25f);
			sLeaser.sprites[0].SetPosition(sLeaser.sprites[0].GetPosition() + vector);
			for (int j = 1; j < neckIndex; j++)
			{
				int num = j + eyeIndex - 1;
				sLeaser.sprites[num].SetPosition(Vector2.Lerp(base.bodyChunks[j].lastLastPos, base.bodyChunks[j].lastPos, timeStacker) - camPos);
			}
			Vector2 vector2 = sLeaser.sprites[0].GetPosition() + camPos;
			for (int k = 0; k < headCount; k++)
			{
				Vector2 vector3 = Vector2.Lerp(base.bodyChunks[k + 1].lastLastPos, base.bodyChunks[k + 1].lastPos, timeStacker);
				Vector2 vector4 = vector2 + Custom.DirVec(vector2, vector3) * (base.bodyChunks[0].rad * 0.9f);
				Vector2 vector5 = Custom.PerpendicularVector(vector2 - vector3);
				for (int l = 0; l < neckSprites; l++)
				{
					int num2 = base.bodyChunks.Length + k * neckSprites + l;
					float num3 = 1f / (float)(neckSprites + 1) * (float)(l + 1);
					float num4 = Mathf.Sin(num3 * (float)Math.PI * 2f + 4.712389f) * 0.5f + 0.5f;
					sLeaser.sprites[num2].SetPosition(new Vector2(Mathf.Lerp(vector4.x, vector3.x, num3 * num3), Mathf.Lerp(vector4.y, vector3.y, num3)) + vector5 * (Mathf.Sin((float)k + num3 * (float)Math.PI * 5f) * num4 * 2f) - camPos);
				}
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Items");
			}
			for (int i = 0; i < headCount; i++)
			{
				for (int j = 0; j < neckSprites; j++)
				{
					newContatiner.AddChild(sLeaser.sprites[headCount + 1 + i * neckSprites + j]);
				}
				newContatiner.AddChild(sLeaser.sprites[i + 1]);
				newContatiner.AddChild(sLeaser.sprites[i + 1 + headCount + headCount * neckSprites]);
			}
			newContatiner.AddChild(sLeaser.sprites[0]);
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			Color b = new Color(0.85f, 0.306f, 0.669f);
			Color b2 = new Color(1f, 0.406f, 0.469f);
			Color b3 = new Color(1f, 0.6f, 1f);
			Color a = new Color(0.9f, 0.9f, 0.01f);
			Color b4 = new Color(1f, 0.8f, 0.5f);
			b = Color.Lerp(Color.Lerp(palette.blackColor, b, 0.75f), palette.fogColor, 0.2f);
			b2 = Color.Lerp(Color.Lerp(palette.blackColor, b2, 0.75f), palette.fogColor, 0.2f);
			b3 = Color.Lerp(Color.Lerp(palette.blackColor, b3, 0.75f), palette.fogColor, 0.2f);
			b.a = sLeaser.sprites[0].color.a;
			sLeaser.sprites[0].color = b;
			for (int i = 0; i < headCount; i++)
			{
				Color b5 = Color.Lerp(b, b2, sLeaser.sprites[i + 1].color.a);
				sLeaser.sprites[i + 1].color = b5;
				for (int j = 0; j < neckSprites; j++)
				{
					float num = 1f / (float)(neckSprites + 1) * (float)(j + 1);
					float num2 = Mathf.Sin(num * (float)Math.PI * 2f + 4.712389f) * 0.5f + 0.5f;
					Color color = Color.Lerp(Color.Lerp(b, b5, num), b3, (Mathf.Sin(j) * 0.5f + 0.5f) * num2);
					color.a = sLeaser.sprites[neckIndex + i * neckSprites + j].color.a;
					sLeaser.sprites[neckIndex + i * neckSprites + j].color = color;
				}
			}
			for (int k = eyeIndex; k < sLeaser.sprites.Length; k++)
			{
				Color color2 = Color.Lerp(a, b4, sLeaser.sprites[k - eyeIndex].color.a);
				color2.a = sLeaser.sprites[k].color.a;
				sLeaser.sprites[k].color = color2;
			}
		}
	}

	public class SpinToy : PlayerCarryableItem, IDrawable
	{
		public Counter spinTimer;

		public float spindownProgress;

		public float lastSprindownProgress;

		public float backForth;

		public float lastBackForth;

		public float leftRight;

		public float lastLeftRight;

		public float unstable;

		public float lastUnstable;

		public float lastSpin;

		public float spin;

		public float lastTopBottom;

		public float topBottom;

		public int spindownTime = 200;

		public int minSpinTime = 200;

		public int maxSpinTime = 200;

		public float randomOffset;

		public int nLen = 2;

		public int nWidth = 2;

		public string spriteA;

		public string spriteB;

		public bool spriteAChanged;

		public bool spriteBChanged;

		public float spinVolume;

		public StaticSoundLoop spinLoop;

		public bool interactedWith;

		public Vector2 pos;

		public Vector2 lastPos;

		public static string[] glyphs = new string[10] { "BigGlyph0", "BigGlyph1", "BigGlyph2", "BigGlyph3", "BigGlyph4", "BigGlyph5", "BigGlyph6", "BigGlyph7", "BigGlyph8", "BigGlyph9" };

		public SpinToy(AbstractPhysicalObject abstractPhysicalObject)
			: base(abstractPhysicalObject)
		{
			base.bodyChunks = new BodyChunk[1];
			base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 16f, 0.2f);
			bodyChunkConnections = new BodyChunkConnection[0];
			base.airFriction = 0.999f;
			base.gravity = 0.9f;
			collisionLayer = 1;
			base.waterFriction = 0.95f;
			base.buoyancy = 1.1f;
			spinTimer = new Counter(spindownTime);
			spinTimer.SetToMin();
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			lastSprindownProgress = spindownProgress;
			lastLeftRight = leftRight;
			lastBackForth = backForth;
			lastUnstable = unstable;
			lastTopBottom = topBottom;
			lastSpin = spin;
			lastPos = pos;
			if (grabbedBy.Count == 0)
			{
				spinTimer.Tick();
				spin += Mathf.InverseLerp(0f, 40f, (int)spinTimer) * 0.1f;
				if (!spinTimer.isFinished)
				{
					interactedWith = true;
				}
			}
			spindownProgress = Mathf.InverseLerp(spindownTime, 10f, (int)spinTimer);
			unstable = spindownProgress * spindownProgress;
			surfaceFriction = 0.45f - unstable * 0.25f;
			bounce = 0.5f - unstable * 0.3f;
			float num = 15f;
			float f = (spin * 0.02f + unstable + randomOffset) * num;
			leftRight = Mathf.Sin(f);
			topBottom = Mathf.Cos(f);
			pos = base.bodyChunks[0].pos;
			pos.y -= base.bodyChunks[0].rad + 2f;
			backForth = Mathf.Cos((spin * 0.02f + unstable + randomOffset) * num * 2f) * 0.5f + 0.5f;
			base.bodyChunks[0].vel.x += leftRight * 0.1f * (1f - unstable);
			spinVolume = Mathf.Lerp(spinVolume, ((int)spinTimer < spinTimer.max) ? (1f - spindownProgress) : 0f, 0.1f);
			if ((int)spinTimer == spinTimer.max - 1)
			{
				room.PlaySound(WatcherEnums.WatcherSoundID.SpinToy_Spin, base.firstChunk);
			}
		}

		public override void PlaceInRoom(Room placeRoom)
		{
			base.PlaceInRoom(placeRoom);
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
		}

		public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
		{
			if (firstContact && !spinTimer.isFinished && direction == Custom.fourDirections[1] && speed > 3f && grabbedBy.Count == 0)
			{
				room.PlaySound(WatcherEnums.WatcherSoundID.SpinToy_Impact, base.firstChunk.pos, Mathf.Clamp01(speed / 4f), 1f);
			}
			if (direction.x == -1 || direction.x == 1)
			{
				if (firstContact)
				{
					base.bodyChunks[chunk].vel.x = 0f - base.bodyChunks[chunk].vel.x;
				}
				base.bodyChunks[chunk].vel.x -= (float)direction.x * (1f - unstable) * 10f;
			}
			base.TerrainImpact(chunk, direction, speed, firstContact);
		}

		public override void PickedUp(Creature upPicker)
		{
			base.PickedUp(upPicker);
			if (spinTimer.isFinished)
			{
				ChargeSpin(UnityEngine.Random.Range(minSpinTime, maxSpinTime));
			}
		}

		public void ChargeSpin(int time)
		{
			spinTimer.SetMax(time + spindownTime);
			spinTimer.Reset();
			randomOffset = UnityEngine.Random.value;
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[7];
			for (int i = 0; i < 2; i++)
			{
				sLeaser.sprites[i] = new FSprite("LizardLowerTeeth3.2");
				sLeaser.sprites[i].anchorY = 1.07f;
				sLeaser.sprites[i].color = Color.blue;
				sLeaser.sprites[i].scale = 0.5f;
			}
			for (int j = 2; j < 4; j++)
			{
				sLeaser.sprites[j] = new FSprite("Cicada8body");
				sLeaser.sprites[j].color = Color.red;
				sLeaser.sprites[j].scale = 0.5f;
			}
			sLeaser.sprites[4] = new FSprite("Futile_White");
			sLeaser.sprites[4].anchorY = -0.25f;
			sLeaser.sprites[4].shader = Custom.rainWorld.Shaders["SpinToy"];
			TriangleMesh.Triangle[] tris = TriangleMesh.GridTriangles(nLen, nWidth);
			randomOffset = UnityEngine.Random.value;
			for (int k = 5; k < 7; k++)
			{
				sLeaser.sprites[k] = new TriangleMesh("Futile_White", tris, customColor: true, atlasedImage: true);
				sLeaser.sprites[k].shader = Custom.rainWorld.Shaders["SpinToyGlyph"];
				sLeaser.sprites[k].anchorY = -0.25f;
				sLeaser.sprites[k].element = GetRandomSprite(glyphs);
				UpdateUV(sLeaser.sprites[k]);
				UpdateMesh(sLeaser.sprites[k], 0f, 0f, 0f);
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			float num = 0.1f + Mathf.Lerp(lastUnstable, unstable, timeStacker) * 0.9f;
			float num2 = Mathf.Lerp(lastLeftRight, leftRight, timeStacker);
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].SetPosition(Vector2.Lerp(lastPos, pos, timeStacker) - camPos);
			}
			float num3 = Mathf.Lerp(lastBackForth, backForth, timeStacker);
			float num4 = Mathf.Lerp(lastTopBottom, topBottom, timeStacker);
			float num5 = Mathf.Lerp(0f, 0.16f, num3 * num) - 0.01f;
			float anchorY = Mathf.Lerp(0f, 0.59f, num3 * num) - 1.3f;
			float num6 = Custom.Mod(Mathf.Lerp(lastSpin, spin, timeStacker) + randomOffset, 1f);
			float num7 = Custom.Mod(Mathf.Lerp(lastSpin, spin, timeStacker) + 0.5f + randomOffset, 1f);
			bool flag = topBottom > 0f;
			float num8 = num2 * 50f * num;
			float num9 = num4 * num * 0.5f + 0.5f;
			sLeaser.sprites[0].isVisible = !flag;
			sLeaser.sprites[0].rotation = 180f + num8;
			sLeaser.sprites[1].isVisible = flag;
			sLeaser.sprites[1].rotation = 180f + num8;
			sLeaser.sprites[2].anchorY = anchorY;
			sLeaser.sprites[2].rotation = num8;
			sLeaser.sprites[2].isVisible = flag;
			sLeaser.sprites[3].anchorY = anchorY;
			sLeaser.sprites[3].rotation = num8;
			sLeaser.sprites[3].isVisible = !flag;
			sLeaser.sprites[4].scale = 4f;
			sLeaser.sprites[4].anchorY = num5;
			sLeaser.sprites[4].rotation = num8;
			sLeaser.sprites[4].color = new Color(num9, 0f, 0f, num6);
			if (num6 < 0.1f && !spriteAChanged)
			{
				sLeaser.sprites[5].element = GetRandomSprite(glyphs);
				spriteA = sLeaser.sprites[5].element.name;
				spriteAChanged = true;
			}
			else if (num6 > 0.1f)
			{
				spriteAChanged = false;
			}
			if (num7 < 0.1f && !spriteBChanged)
			{
				sLeaser.sprites[6].element = GetRandomSprite(glyphs);
				spriteB = sLeaser.sprites[6].element.name;
				spriteBChanged = true;
			}
			else if (num7 > 0.1f)
			{
				spriteBChanged = false;
			}
			sLeaser.sprites[5].scale = 4f;
			sLeaser.sprites[5].rotation = num8;
			UpdateMesh(sLeaser.sprites[5], num5 * 16f, num6, num9);
			sLeaser.sprites[6].scale = 4f;
			sLeaser.sprites[6].rotation = num8;
			UpdateMesh(sLeaser.sprites[6], num5 * 16f, num7, num9);
		}

		public FAtlasElement GetRandomSprite(string[] sprites)
		{
			FAtlasElement elementWithName;
			do
			{
				elementWithName = Futile.atlasManager.GetElementWithName(sprites[UnityEngine.Random.Range(0, sprites.Length)]);
			}
			while (!(elementWithName.name != spriteA) || !(elementWithName.name != spriteB));
			return elementWithName;
		}

		public void UpdateUV(FSprite m)
		{
			for (int i = 0; i <= nLen; i++)
			{
				TriangleMesh triangleMesh = m as TriangleMesh;
				int num = i * (nWidth + 1);
				Vector2 vector = Vector2.right * i / nLen;
				Vector2 vector2 = vector + Vector2.up;
				triangleMesh.UVvertices[num] = vector;
				triangleMesh.UVvertices[num + nWidth] = vector2;
				for (int j = 1; j < nWidth; j++)
				{
					triangleMesh.UVvertices[num + j] = Vector2.Lerp(vector, vector2, (float)j / (float)nWidth);
				}
			}
		}

		public void UpdateMesh(FSprite m, float offset, float rotation, float tilt)
		{
			for (int i = 0; i <= nLen; i++)
			{
				TriangleMesh triangleMesh = m as TriangleMesh;
				int num = i * (nWidth + 1);
				float t = (float)i / (float)nLen;
				Vector2 a = Vector2.Lerp(new Vector2(-8f, 0f - offset), new Vector2(8f, 0f - offset), t);
				Vector2 b = Vector2.Lerp(new Vector2(-8f, 16f - offset), new Vector2(8f, 16f - offset), t);
				triangleMesh.MoveVertice(num, a);
				triangleMesh.MoveVertice(num + nWidth, b);
				triangleMesh.verticeColors[num] = new Color(tilt, m.element.uvTopRight.x, m.element.uvTopRight.y, rotation);
				triangleMesh.verticeColors[num + nWidth] = new Color(tilt, m.element.uvTopRight.x, m.element.uvTopRight.y, rotation);
				for (int j = 1; j < nWidth; j++)
				{
					triangleMesh.MoveVertice(num + j, Vector2.Lerp(a, b, (float)j / (float)nWidth));
					triangleMesh.verticeColors[num + j] = new Color(tilt, m.element.uvTopRight.x, m.element.uvTopRight.y, rotation);
				}
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Items");
			}
			newContatiner.AddChild(sLeaser.sprites[0]);
			newContatiner.AddChild(sLeaser.sprites[2]);
			newContatiner.AddChild(sLeaser.sprites[4]);
			newContatiner.AddChild(sLeaser.sprites[5]);
			newContatiner.AddChild(sLeaser.sprites[6]);
			newContatiner.AddChild(sLeaser.sprites[1]);
			newContatiner.AddChild(sLeaser.sprites[3]);
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			Color color = Color.Lerp(Color.Lerp(palette.blackColor, new Color(0.6f, 0.1f, 0.4f), 0.1f), palette.fogColor, 0.2f);
			sLeaser.sprites[0].color = color;
			sLeaser.sprites[1].color = color;
			sLeaser.sprites[2].color = color;
			sLeaser.sprites[3].color = color;
		}
	}

	public class WeirdToy : PlayerCarryableItem, IDrawable
	{
		public int numberOfNails;

		public List<Vector2> nailOffsets;

		public Vector2[] nailVariations;

		public bool interactedWith;

		public bool wasPickedUp;

		public int spritesPerNail = 3;

		public float chunkDistMax => base.bodyChunks[0].rad * 1.5f;

		public float chunkDistMin => base.bodyChunks[0].rad * 0.6f;

		public int nailsIndex => 5;

		public WeirdToy(AbstractPhysicalObject abstractPhysicalObject)
			: base(abstractPhysicalObject)
		{
			base.bodyChunks = new BodyChunk[2];
			base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 14f, 0.3f);
			base.bodyChunks[1] = new BodyChunk(this, 0, new Vector2(0f, 0f), 14f, 0.3f);
			bodyChunkConnections = new BodyChunkConnection[4];
			bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[1], base.bodyChunks[0], chunkDistMax, BodyChunkConnection.Type.Pull, 0.9f, -1f);
			bodyChunkConnections[1] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], chunkDistMax, BodyChunkConnection.Type.Pull, 0.9f, -1f);
			bodyChunkConnections[2] = new BodyChunkConnection(base.bodyChunks[1], base.bodyChunks[0], chunkDistMin, BodyChunkConnection.Type.Push, 0.9f, -1f);
			bodyChunkConnections[3] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], chunkDistMin, BodyChunkConnection.Type.Push, 0.9f, -1f);
			base.airFriction = 0.999f;
			base.gravity = 0.9f;
			surfaceFriction = 0.1f;
			bounce = 0.3f;
			collisionLayer = 1;
			base.waterFriction = 0.95f;
			base.buoyancy = 1.1f;
			numberOfNails = UnityEngine.Random.Range(7, 13);
			nailOffsets = new List<Vector2>();
			nailVariations = new Vector2[numberOfNails];
			for (int i = 0; i < numberOfNails; i++)
			{
				nailOffsets.Add(Custom.RNV() * UnityEngine.Random.Range(0.3f, 0.7f));
				nailVariations[i] = new Vector2(UnityEngine.Random.value, UnityEngine.Random.value);
			}
			nailOffsets.Sort((Vector2 s1, Vector2 s2) => s2.y.CompareTo(s1.y));
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
		}

		public override void PlaceInRoom(Room placeRoom)
		{
			base.PlaceInRoom(placeRoom);
			base.bodyChunks[0].HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			base.bodyChunks[1].HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos) + Vector2.right * base.bodyChunks[1].rad);
		}

		public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
		{
			if (firstContact)
			{
				float num = Mathf.Min(speed / 6f, 1f);
				room.PlaySound(SoundID.Rock_Hit_Wall, base.bodyChunks[chunk], loop: false, 0.6f * num, UnityEngine.Random.Range(0.05f, 0.1f) + 0.05f * num);
				if (wasPickedUp)
				{
					interactedWith = true;
				}
			}
			base.TerrainImpact(chunk, direction, speed, firstContact);
		}

		public override void PickedUp(Creature upPicker)
		{
			wasPickedUp = true;
			base.PickedUp(upPicker);
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			float num = base.firstChunk.rad / 8f;
			sLeaser.sprites = new FSprite[5 + numberOfNails * spritesPerNail];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].scale = num * 1f;
			sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["JaggedCircle"];
			sLeaser.sprites[0].rotation = UnityEngine.Random.Range(0, 360);
			sLeaser.sprites[3] = new FSprite("Futile_White");
			sLeaser.sprites[3].scale = num * 0.7f;
			sLeaser.sprites[3].shader = Custom.rainWorld.Shaders["JaggedCircle"];
			sLeaser.sprites[3].rotation = UnityEngine.Random.Range(0, 360);
			sLeaser.sprites[1] = new FSprite("Futile_White");
			sLeaser.sprites[1].scale = num * 1f;
			sLeaser.sprites[4] = new FSprite("Futile_White");
			sLeaser.sprites[4].scale = num * 0.7f;
			sLeaser.sprites[4].anchorY = 0f;
			sLeaser.sprites[1].shader = Custom.rainWorld.Shaders["JaggedCircle"];
			sLeaser.sprites[2] = new FSprite("Futile_White");
			sLeaser.sprites[2].scale = num * 1f;
			sLeaser.sprites[2].anchorY = 0f;
			for (int i = nailsIndex; i < sLeaser.sprites.Length; i += spritesPerNail)
			{
				sLeaser.sprites[i] = new FSprite("Futile_White");
				sLeaser.sprites[i].scale = num * 0.15f;
				sLeaser.sprites[i].shader = Custom.rainWorld.Shaders["JaggedCircle"];
				sLeaser.sprites[i + 1] = new FSprite("SmallSpear");
				sLeaser.sprites[i + 1].scale = num * 0.5f;
				sLeaser.sprites[i + 1].anchorY = 0.08f;
				sLeaser.sprites[i + 1].anchorX = 0.55f;
				sLeaser.sprites[i + 2] = new FSprite("Futile_White");
				sLeaser.sprites[i + 2].scale = num * 0.04f;
				sLeaser.sprites[i + 2].anchorY = -0.2f;
				sLeaser.sprites[i + 2].anchorX = 0.55f;
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(base.bodyChunks[0].lastLastPos, base.bodyChunks[0].lastPos, timeStacker);
			Vector2 vector2 = Vector2.Lerp(base.bodyChunks[1].lastLastPos, base.bodyChunks[1].lastPos, timeStacker);
			float num = Custom.AimFromOneVectorToAnother(vector, vector2);
			float num2 = (chunkDistMax - chunkDistMin) / 8f;
			float num3 = chunkDistMax / 8f;
			float a = Mathf.InverseLerp(chunkDistMax, chunkDistMin, Vector2.Distance(vector, vector2));
			a = Mathf.Max(a, 0.1f);
			float num4 = Vector2.Distance(vector, vector2) / chunkDistMax;
			Vector2 vector3 = new Vector2(-1f, 1f);
			float num5 = Mathf.Abs(Custom.Angle(Custom.DirVec(vector2, vector), vector3)) / 180f;
			sLeaser.sprites[0].SetPosition(vector - camPos);
			sLeaser.sprites[3].SetPosition(vector + vector3 * (base.firstChunk.rad * 0.2f) - camPos);
			sLeaser.sprites[1].SetPosition(vector2 - camPos);
			sLeaser.sprites[1].rotation = num;
			sLeaser.sprites[1].scaleY = num2 * a;
			sLeaser.sprites[2].SetPosition(vector - camPos);
			sLeaser.sprites[2].rotation = num;
			sLeaser.sprites[2].scaleY = num3 * 0.5f * num4;
			sLeaser.sprites[4].SetPosition(vector + vector3 * (base.firstChunk.rad * 0.2f) - camPos);
			sLeaser.sprites[4].rotation = num;
			sLeaser.sprites[4].scaleY = num3 * 0.5f * num4 * (1.2f - num5 * 0.4f);
			for (int i = nailsIndex; i < sLeaser.sprites.Length; i += spritesPerNail)
			{
				int num6 = (i - nailsIndex) / spritesPerNail;
				Vector2 vector4 = vector2 + Custom.rotateVectorDeg(nailOffsets[num6] * new Vector2(1f, a), num) * base.firstChunk.rad - camPos;
				float rotation = num + (nailVariations[num6].y * 2f - 1f) * 10f * (1f + a * a * 2f);
				sLeaser.sprites[i].SetPosition(vector4);
				sLeaser.sprites[i].rotation = rotation;
				sLeaser.sprites[i].scaleY = sLeaser.sprites[i].scaleX * a;
				sLeaser.sprites[i + 1].SetPosition(vector4);
				sLeaser.sprites[i + 1].rotation = rotation;
				sLeaser.sprites[i + 1].scaleY = num2 * (0.2f + nailVariations[num6].x * 0.25f) * (1f - a * a * 0.7f);
				sLeaser.sprites[i + 2].SetPosition(vector4 + vector3 * 0.8f);
				sLeaser.sprites[i + 2].rotation = rotation;
				sLeaser.sprites[i + 2].scaleY = num2 * (0.2f + nailVariations[num6].x * 0.25f) * (1f - a * a * 0.7f) * 2.5f;
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Items");
			}
			newContatiner.AddChild(sLeaser.sprites[0]);
			newContatiner.AddChild(sLeaser.sprites[2]);
			newContatiner.AddChild(sLeaser.sprites[3]);
			newContatiner.AddChild(sLeaser.sprites[4]);
			newContatiner.AddChild(sLeaser.sprites[1]);
			for (int i = nailsIndex; i < sLeaser.sprites.Length; i += spritesPerNail)
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
				newContatiner.AddChild(sLeaser.sprites[i + 1]);
				newContatiner.AddChild(sLeaser.sprites[i + 2]);
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			Color b = new Color(0.244f, 0.149f, 0.071f);
			Color b2 = new Color(0.125f, 0.053f, 0.036f);
			Color b3 = new Color(0.15f, 0.206f, 0.176f);
			b = Color.Lerp(Color.Lerp(palette.blackColor, b, 0.75f), palette.fogColor, 0.2f);
			b2 = Color.Lerp(Color.Lerp(palette.blackColor, b2, 0.75f), palette.fogColor, 0.2f);
			b3 = Color.Lerp(Color.Lerp(palette.blackColor, b3, 0.75f), palette.fogColor, 0.2f);
			Color color = b * 1.3f;
			sLeaser.sprites[0].color = b;
			sLeaser.sprites[1].color = b * 0.8f;
			sLeaser.sprites[2].color = b;
			sLeaser.sprites[3].color = color;
			sLeaser.sprites[4].color = color;
			for (int i = nailsIndex; i < sLeaser.sprites.Length; i += spritesPerNail)
			{
				int num = (i - nailsIndex) / spritesPerNail;
				Color color2 = Color.Lerp(b2, b3, nailVariations[num].y);
				sLeaser.sprites[i].color = color2;
				sLeaser.sprites[i + 1].color = color2;
				sLeaser.sprites[i + 2].color = color2 * 1.7f;
			}
		}
	}
}
