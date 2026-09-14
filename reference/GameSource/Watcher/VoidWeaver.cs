using System;
using System.Collections.Generic;
using System.Linq;
using HUD;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class VoidWeaver : CosmeticSprite
{
	public abstract class BodyAttachment
	{
		public int totalSprites = 1;

		public float posAlongBody;

		public Vector2 offset;

		public float relAngle;

		public float darkness;

		public float rootDarkness;

		public Vector2 pos;

		public Vector2 lastPos;

		public float rotation;

		public float lastRotation;

		public VoidWeaver owner;

		public float Depth => owner.GetDepthAlongBody(posAlongBody);

		public BodyAttachment(VoidWeaver owner, float posAlongBody, Vector2 offset, float relAngle, float darkness, float rootDarkness)
		{
			this.owner = owner;
			this.posAlongBody = posAlongBody;
			this.relAngle = relAngle;
			this.offset = offset;
			this.darkness = darkness;
			this.rootDarkness = rootDarkness;
		}

		public virtual void Update()
		{
			lastRotation = rotation;
			lastPos = pos;
			Vector2 pointAlongBody = owner.GetPointAlongBody(posAlongBody, 1f);
			Vector2 vector = Custom.DirVec(owner.GetPointAlongBody(posAlongBody + 0.0375f, 1f), owner.GetPointAlongBody(posAlongBody - 0.0375f, 1f));
			Vector2 vector2 = -Custom.PerpendicularVector(vector);
			pos = pointAlongBody + vector * offset.y + vector2 * offset.x;
			rotation = Custom.VecToDeg(-vector) + relAngle;
		}

		public virtual void Reset(Vector2 pos)
		{
			this.pos = pos;
			lastPos = pos;
		}

		public abstract void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, int firstSprite);

		public abstract void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, int firstSprite);

		public abstract void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette, int firstSprite);
	}

	public class Whisker : BodyAttachment
	{
		public float length;

		public bool large;

		public Segment[] segments;

		public Whisker(VoidWeaver owner, float posAlongBody, Vector2 offset, float relAngle, float darkness, float rootDarkness, float length, bool large)
			: base(owner, posAlongBody, offset, relAngle, darkness, rootDarkness)
		{
			this.length = length;
			this.large = large;
			segments = new Segment[Mathf.Max(Mathf.RoundToInt(length / 20f), 5)];
			for (int i = 0; i < segments.Length; i++)
			{
				float num = (float)i / (float)(segments.Length - 1);
				segments[i] = new Segment((large ? 0f : 2f) * (1f - num) + 0.5f, length / (float)segments.Length, 0f);
			}
		}

		public override void Reset(Vector2 pos)
		{
			base.Reset(pos);
			Segment[] array = segments;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Reset(pos);
			}
		}

		public override void Update()
		{
			base.Update();
			Segment[] array = segments;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Update();
			}
			segments[0].pos = pos;
			segments[0].vel = Vector2.zero;
			Vector2 vector = Custom.DegToVec(rotation);
			Vector2 vector2 = Custom.PerpendicularVector(vector);
			for (int j = 1; j < segments.Length; j++)
			{
				float num = (float)j / (float)(segments.Length - 1);
				if (large)
				{
					segments[j].vel += vector * (Mathf.Pow(Mathf.InverseLerp(80f, 0f, num * length), 3f) * 2f);
					continue;
				}
				segments[j].vel += vector * (Mathf.Pow(Mathf.InverseLerp(0.75f, 0f, num), 2f) * 8f);
				float num2 = offset.x / owner.bodyThickness * Mathf.Sin(num * (float)Math.PI * 2f);
				segments[j].vel += vector2 * (num2 * 0.6f);
			}
			owner.DoRopePhysics(segments, body: false);
			segments[0].pos = pos;
			segments[0].vel = segments[0].pos - segments[0].lastPos;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, int firstSprite)
		{
			sLeaser.sprites[firstSprite] = TriangleMesh.MakeLongMeshAtlased(segments.Length, pointyTip: true, customColor: true);
			sLeaser.sprites[firstSprite].shader = rCam.game.rainWorld.Shaders[owner.inEndingRoom ? "Basic" : "WeaverFeather"];
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, int firstSprite)
		{
			TriangleMesh triangleMesh = sLeaser.sprites[firstSprite] as TriangleMesh;
			Vector2 vector = Vector2.Lerp(segments[0].lastPos, segments[0].pos, timeStacker);
			float num = segments[0].rad;
			for (int i = 0; i < segments.Length; i++)
			{
				int num2 = i * 4;
				Vector2 vector2 = Vector2.Lerp(segments[i].lastPos, segments[i].pos, timeStacker);
				Vector2 vector3 = Custom.DirVec(vector, vector2);
				Vector2 vector4 = Custom.PerpendicularVector(vector3);
				float num3 = Vector2.Distance(vector, vector2) / 5f;
				float rad = segments[i].rad;
				Vector2 vector5 = vector4 * ((num + rad) * 0.5f);
				triangleMesh.MoveVertice(num2, rCam.ApplyDepth(vector - vector5 + vector3 * num3, base.Depth * 30f) - camPos);
				triangleMesh.MoveVertice(num2 + 1, rCam.ApplyDepth(vector + vector5 + vector3 * num3, base.Depth * 30f) - camPos);
				if (i < segments.Length - 1)
				{
					Vector2 vector6 = vector4 * rad;
					triangleMesh.MoveVertice(num2 + 2, rCam.ApplyDepth(vector2 - vector6 - vector3 * num3, base.Depth * 30f) - camPos);
					triangleMesh.MoveVertice(num2 + 3, rCam.ApplyDepth(vector2 + vector6 - vector3 * num3, base.Depth * 30f) - camPos);
				}
				else
				{
					triangleMesh.MoveVertice(num2 + 2, rCam.ApplyDepth(vector2, base.Depth * 30f) - camPos);
				}
				vector = vector2;
				num = rad;
			}
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette, int firstSprite)
		{
			TriangleMesh triangleMesh = sLeaser.sprites[firstSprite] as TriangleMesh;
			Color[] verticeColors = triangleMesh.verticeColors;
			for (int i = 0; i < verticeColors.Length; i += 2)
			{
				float num = (float)i / (float)(verticeColors.Length - 1);
				Color color = (verticeColors[i] = ((!owner.inEndingRoom) ? new Color(num, darkness, rootDarkness, base.Depth) : Color.Lerp(endingRoomLight, endingRoomDark, Mathf.Lerp(rootDarkness, darkness, num))));
				if (i + 1 < verticeColors.Length)
				{
					verticeColors[i + 1] = color;
				}
			}
			triangleMesh.Refresh();
		}
	}

	public class Feather : BodyAttachment
	{
		public int variant;

		public float lengthFac;

		public Feather(VoidWeaver owner, float posAlongBody, Vector2 offset, float relAngle, float darkness, float rootDarkness, float lengthFac)
			: base(owner, posAlongBody, offset, relAngle, darkness, rootDarkness)
		{
			this.lengthFac = lengthFac;
			variant = UnityEngine.Random.Range(0, 3);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, int firstSprite)
		{
			sLeaser.sprites[firstSprite] = new VertexColorSprite("WeaverFeather" + variant, quadType: false)
			{
				shader = rCam.game.rainWorld.Shaders[owner.inEndingRoom ? "Basic" : "WeaverFeather"],
				anchorY = 0.1f,
				scaleX = Mathf.Lerp(lengthFac, 1f, 0.25f) * ((offset.x > 0f) ? 1f : (-1f)) * 0.8f,
				scaleY = lengthFac
			};
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, int firstSprite)
		{
			Vector2 ps = Vector2.Lerp(lastPos, pos, timeStacker);
			sLeaser.sprites[firstSprite].SetPosition(rCam.ApplyDepth(ps, base.Depth * 30f) - camPos);
			sLeaser.sprites[firstSprite].rotation = Mathf.LerpAngle(lastRotation, rotation, timeStacker);
			float depthScale = Custom.GetDepthScale(base.Depth);
			sLeaser.sprites[firstSprite].scaleX = depthScale * Mathf.Lerp(lengthFac, 1f, 0.25f) * ((offset.x > 0f) ? 1f : (-1f)) * 0.8f;
			sLeaser.sprites[firstSprite].scaleY = depthScale * lengthFac;
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette, int firstSprite)
		{
			VertexColorSprite vertexColorSprite = sLeaser.sprites[firstSprite] as VertexColorSprite;
			float depth = base.Depth;
			if (owner.inEndingRoom)
			{
				vertexColorSprite.verticeColors[0] = Color.Lerp(endingRoomLight, endingRoomDark, darkness);
				vertexColorSprite.verticeColors[1] = Color.Lerp(endingRoomLight, endingRoomDark, darkness);
				vertexColorSprite.verticeColors[2] = Color.Lerp(endingRoomLight, endingRoomDark, rootDarkness);
				vertexColorSprite.verticeColors[3] = Color.Lerp(endingRoomLight, endingRoomDark, rootDarkness);
			}
			else
			{
				vertexColorSprite.verticeColors[0] = new Color(1f, darkness, rootDarkness, depth);
				vertexColorSprite.verticeColors[1] = new Color(1f, darkness, rootDarkness, depth);
				vertexColorSprite.verticeColors[2] = new Color(0f, darkness, rootDarkness, depth);
				vertexColorSprite.verticeColors[3] = new Color(0f, darkness, rootDarkness, depth);
			}
		}
	}

	public class Halo
	{
		public int totalSprites = 2;

		public VoidWeaver parent;

		public int firstSprite;

		public const int divs = 64;

		public Halo(VoidWeaver parent, int firstSprite)
		{
			this.parent = parent;
			this.firstSprite = firstSprite;
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[256];
			for (int i = 0; i < 64; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					int num = i * 4 + j * 2;
					int num2 = i * 3 + j;
					int num3 = (i + 1) % 64 * 3 + j;
					array[num] = new TriangleMesh.Triangle(num2, num2 + 1, num3);
					array[num + 1] = new TriangleMesh.Triangle(num3, num2 + 1, num3 + 1);
				}
			}
			sLeaser.sprites[firstSprite] = new TriangleMesh("Futile_White", array, customColor: false)
			{
				shader = rCam.game.rainWorld.Shaders[parent.inEndingRoom ? "WeaverGlowNoGrab" : "WeaverGlow"]
			};
			sLeaser.sprites[firstSprite + 1] = new FSprite("Futile_White")
			{
				shader = rCam.game.rainWorld.Shaders["FlatLightNoisy"]
			};
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			TriangleMesh triangleMesh = sLeaser.sprites[firstSprite] as TriangleMesh;
			Vector2 vector = Vector2.Lerp(parent.body[0].lastPos, parent.body[0].pos, timeStacker);
			float num = Mathf.Lerp(parent.LastHaloFlare, parent.HaloFlare, timeStacker);
			float num2 = 20f;
			float num3 = 50f;
			float num4 = 100f + (400f - 150f * UnityEngine.Random.value * Mathf.Sqrt(num)) * Mathf.SmoothStep(0f, 1f, num);
			float num5 = (Mathf.PerlinNoise(((float)parent.animCounter + timeStacker + num * num * 1000f) / 40f * 0.2f, 20f) - 0.46f) * 2f * 0.5f;
			float f = (Mathf.PerlinNoise(((float)parent.animCounter + timeStacker) / 40f * 0.1f + 0.5f, 40f) - 0.46f) * 2f;
			f = Mathf.Pow(Mathf.Abs(f), 0.75f) * Mathf.Sign(f);
			f = Mathf.Lerp(f, 0f, Mathf.Sqrt(num));
			if (parent.talking > 0f)
			{
				float num6 = 0.5f - 0.5f * Mathf.Cos(Mathf.Lerp(parent.lastTalking, parent.talking, timeStacker) * (float)Math.PI);
				num5 *= 1f - num6;
				f *= 1f - num6;
				num3 -= 5f * num6;
				num4 -= 20f * num6;
				if (parent.GetVoiceSpectrum(out var lows, out var highs))
				{
					num5 = Mathf.Clamp01(Mathf.Lerp(num5, (highs * 2f - lows) * 3f, num6));
					f = Mathf.Lerp(f, 0.5f + (highs + lows) * 2f, num6);
					num3 += num6 * (highs + lows) * 40f;
					num4 += num6 * (highs + lows) * 300f;
				}
			}
			for (int i = 0; i < 64; i++)
			{
				float num7 = (float)i / 64f;
				Vector2 vector2 = new Vector2(Mathf.Cos((0f - num7) * (float)Math.PI * 2f), Mathf.Sin((0f - num7) * (float)Math.PI * 2f));
				for (int j = 0; j < 3; j++)
				{
					float y = (float)j / 2f;
					triangleMesh.MoveVertice(i * 3 + j, vector + vector2 * j switch
					{
						1 => num3, 
						0 => num2, 
						_ => num4, 
					} - camPos);
					triangleMesh.UVvertices[i * 3 + j] = new Vector2(Mathf.Lerp(-0.1f, -0.05f, f) + Mathf.Pow(Mathf.PingPong(num7 * 8f, 1f), 2f + f) * (0.25f + num5 * 0.1f) + Mathf.Pow(Mathf.PingPong(num7 * 8f + 1f, 1f), 2f + f) * (0.25f - num5 * 0.1f), y);
				}
			}
			triangleMesh.alpha = Mathf.Lerp(0.25f, 1f, Mathf.Pow(parent.HaloFlare, 2f));
			sLeaser.sprites[firstSprite + 1].SetPosition(vector - camPos);
			sLeaser.sprites[firstSprite + 1].scale = 50f;
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			sLeaser.sprites[firstSprite].color = RainWorld.SaturatedGold * (parent.inEndingRoom ? 2f : 1f);
			sLeaser.sprites[firstSprite + 1].color = RainWorld.GoldRGB;
			sLeaser.sprites[firstSprite + 1].alpha = 0.1f;
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			rCam.ReturnFContainer(parent.inEndingRoom ? "Background" : "Water").AddChild(sLeaser.sprites[firstSprite]);
			rCam.ReturnFContainer("ForegroundLights").AddChild(sLeaser.sprites[firstSprite + 1]);
		}
	}

	public class Segment
	{
		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		public float depth;

		public float rad;

		public float connectionRad;

		public Segment(float rad, float connectionRad, float depth)
		{
			this.rad = rad;
			this.connectionRad = connectionRad;
			this.depth = depth;
		}

		public void Reset(Vector2 pos)
		{
			this.pos = pos;
			lastPos = pos;
			vel = Vector2.zero;
		}

		public void Connect(Segment other)
		{
			Vector2 normalized = (pos - other.pos).normalized;
			float num = Vector2.Distance(pos, other.pos);
			float num2 = 0.55f;
			float num3 = connectionRad + other.connectionRad;
			Vector2 vector = normalized * ((num3 - num) * num2 * 0.5f);
			Vector2 vector2 = normalized * ((num3 - num) * (1f - num2) * 0.5f);
			pos += vector;
			vel += vector;
			other.pos -= vector2;
			other.vel -= vector2;
		}

		public void Update()
		{
			lastPos = pos;
		}
	}

	public abstract class Behavior : Conversation.IOwnAConversation
	{
		public VoidWeaver owner;

		public virtual bool Vanish => false;

		public virtual bool NoticedPlayer => false;

		public Player FocusPlayer { get; set; }

		public Behavior(VoidWeaver owner)
		{
			this.owner = owner;
		}

		public virtual void Update()
		{
			if (NoticedPlayer && (FocusPlayer == null || FocusPlayer.room != owner.room))
			{
				FocusPlayer = FindClosestPlayer();
			}
		}

		public virtual string ReplaceParts(string s)
		{
			return s;
		}

		public virtual void SpecialEvent(string eventName)
		{
		}

		public Player FindClosestPlayer()
		{
			Player player = null;
			foreach (AbstractCreature player3 in owner.room.game.Players)
			{
				if (player3.realizedObject is Player player2 && player2.room == owner.room && (player == null || Vector2.Distance(player2.mainBodyChunk.pos, owner.body[0].pos) < Vector2.Distance(player.mainBodyChunk.pos, owner.body[0].pos)))
				{
					player = player2;
				}
			}
			return player;
		}
	}

	public class DefaultBehavior : Behavior
	{
		public class Stage : ExtEnum<Stage>
		{
			public static Stage Idle = new Stage("Idle", register: true);

			public static Stage Noticed = new Stage("Noticed", register: true);

			public static Stage Talking = new Stage("Talking", register: true);

			public static Stage Pause = new Stage("Pause", register: true);

			public static Stage Disappear = new Stage("Disappear", register: true);

			public Stage(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public int onScreenCounter;

		public const int noticePlayerDelay = 60;

		public const int speakDelay = 100;

		public const int disappearDelay = 160;

		public Stage stage = Stage.Idle;

		public int timeInStage;

		public VoidWeaverConversation conversation;

		public override bool Vanish => stage == Stage.Disappear;

		public override bool NoticedPlayer => stage != Stage.Idle;

		public bool IsOnScreen
		{
			get
			{
				int num = owner.room.CameraViewingPoint(owner.targetPos);
				RoomCamera[] cameras = owner.room.game.cameras;
				foreach (RoomCamera roomCamera in cameras)
				{
					if (roomCamera.room == owner.room && roomCamera.currentCameraPosition == num)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool AnyCameraInRoom
		{
			get
			{
				RoomCamera[] cameras = owner.room.game.cameras;
				for (int i = 0; i < cameras.Length; i++)
				{
					if (cameras[i].room == owner.room)
					{
						return true;
					}
				}
				return false;
			}
		}

		public DialogBox DialogBox
		{
			get
			{
				if (conversation != null)
				{
					return conversation.dialogBox;
				}
				global::HUD.HUD hud = owner.room.game.cameras[0].hud;
				if (hud.dialogBox == null)
				{
					hud.InitDialogBox();
				}
				return hud.dialogBox;
			}
		}

		public DefaultBehavior(VoidWeaver owner)
			: base(owner)
		{
		}

		public override void Update()
		{
			base.Update();
			timeInStage++;
			conversation?.Update();
			if (stage == Stage.Idle)
			{
				if (IsOnScreen)
				{
					onScreenCounter++;
				}
				else
				{
					onScreenCounter = 0;
				}
				if (onScreenCounter > 60)
				{
					NextStage();
				}
			}
			else if (stage == Stage.Noticed)
			{
				if (timeInStage > 100)
				{
					StartMonologue();
					NextStage();
				}
			}
			else if (stage == Stage.Talking)
			{
				if (!AnyCameraInRoom)
				{
					owner.StopSpeaking();
					conversation?.Destroy();
					conversation = null;
					stage = Stage.Idle;
					onScreenCounter = 0;
					timeInStage = 0;
				}
				else
				{
					if (!conversation.slatedForDeletion)
					{
						return;
					}
					conversation = null;
					Player player = null;
					float num = 0f;
					for (int i = 0; i < owner.room.game.Players.Count; i++)
					{
						if (owner.room.game.Players[i].realizedCreature != null)
						{
							Player player2 = owner.room.game.Players[i].realizedCreature as Player;
							if (player2.controller == null)
							{
								player2.controller = new Player.NullController();
							}
							if (player2.room == owner.room && (player == null || Vector2.Distance(player2.mainBodyChunk.pos, owner.pos) < num))
							{
								player = player2;
								num = Vector2.Distance(player2.mainBodyChunk.pos, owner.pos);
							}
						}
					}
					if (player != null)
					{
						owner.room.game.cameras[0].EnterCutsceneMode(player.abstractCreature, RoomCamera.CameraCutsceneType.Standard);
					}
					NextStage();
				}
			}
			else if (stage == Stage.Pause)
			{
				if (timeInStage > 160)
				{
					NextStage();
				}
			}
			else
			{
				_ = stage == Stage.Disappear;
			}
		}

		public Conversation.ID GetConversationID()
		{
			return (owner.room.game.GetStorySession?.saveState.miscWorldSaveData.numberOfVoidWeaverEncounters ?? 0) switch
			{
				0 => WatcherEnums.ConversationID.Weaver_1, 
				1 => WatcherEnums.ConversationID.Weaver_2, 
				2 => WatcherEnums.ConversationID.Weaver_3, 
				_ => WatcherEnums.ConversationID.Weaver_4, 
			};
		}

		public void StartMonologue()
		{
			conversation = new VoidWeaverConversation(this, GetConversationID(), DialogBox);
			owner.Speak(conversation.Voiceline);
		}

		public void StartVanish()
		{
			owner.StartDeactivate();
		}

		public void NextStage()
		{
			if (stage == Stage.Idle)
			{
				stage = Stage.Noticed;
			}
			else if (stage == Stage.Noticed)
			{
				stage = Stage.Talking;
			}
			else if (stage == Stage.Talking)
			{
				stage = Stage.Pause;
			}
			else
			{
				if (!(stage == Stage.Pause))
				{
					return;
				}
				stage = Stage.Disappear;
			}
			timeInStage = 0;
		}
	}

	public int firstBodySprite;

	public int firstAttachmentSprite;

	public int firstEyeSprite;

	public int distortionSprite;

	public int totalSprites;

	public Segment[] body;

	public float bodyThickness = 28f;

	public Halo halo;

	public Vector2 targetPos;

	public Vector2 targetDir;

	public PlacedObject placedObject;

	public VoidWeaverFade fadeOut;

	public int fadeOutCounter;

	public PositionedSoundEmitter voice;

	public int jangleCooldown;

	public int rustleCooldown;

	public int animCounter;

	public float talking;

	public float lastTalking;

	public float lean;

	public List<BodyAttachment> bodyAttachments = new List<BodyAttachment>();

	public const string endingRoom = "WRSA_WEAVER";

	public const int lengthMultiplier = 4;

	public const int maxJangleDelay = 400;

	public const int maxRustleDelay = 240;

	public Behavior behavior;

	public bool inEndingRoom;

	public static Color endingRoomDark = VoidWeaverEndingScene.lightSkyColor;

	public static Color endingRoomLight = Custom.RGB2RGBA(RainWorld.SaturatedGold * 1.5f, 1f);

	public float[] freqSamples = new float[64];

	public float LastHaloFlare => Mathf.Clamp01((float)(fadeOutCounter - 1) / 160f);

	public float HaloFlare => Mathf.Clamp01((float)fadeOutCounter / 160f);

	public VoidWeaver(PlacedObject placedObject)
		: this(placedObject.pos, -(placedObject.data as PlacedObject.ResizableObjectData).handlePos.normalized)
	{
		this.placedObject = placedObject;
	}

	public VoidWeaver(Vector2 targetPos, Vector2 targetDir)
	{
		this.targetPos = targetPos;
		this.targetDir = targetDir;
		animCounter = UnityEngine.Random.Range(0, 100000);
		behavior = new DefaultBehavior(this);
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(0);
		int num = 72;
		for (int i = 0; i < num; i++)
		{
			float num2 = 1f - (float)i / (float)(num - 1);
			if (i == num - 2)
			{
				AddMegaWhiskers(num2);
			}
			int num3 = 4 + i % 2;
			for (int j = 0; j < num3; j++)
			{
				float num4 = ((float)j - (float)(num3 - 1) / 2f) * 0.4f;
				float num5 = (Mathf.InverseLerp(0.2f, 0f, num2) * 0.6f + Mathf.InverseLerp(0.3f, 0.5f, num2)) * Mathf.InverseLerp(1f, 0.9f, num2);
				float lengthFac = (BodyContour(num2) * 0.9f + 0.1f) * Mathf.Cos(num4 * (float)Math.PI * 0.25f);
				Vector2 offset = new Vector2(bodyThickness * num4 * BodyContour(num2), 0f);
				float relAngle = num4 * (0f - Mathf.Lerp(50f, 110f, Mathf.Pow(UnityEngine.Random.value, 2f) * num5)) * (num5 * 0.3f + 0.7f);
				float num6 = UnityEngine.Random.value * Mathf.Min(Mathf.Abs(num4), 1f) * 0.5f + 0.25f * num2;
				float num7 = Mathf.Max((Mathf.Max(1f - Mathf.Abs(num4), 0f) * 0.5f + 0.5f) * Mathf.InverseLerp(0f, 0.0375f, num2), num6);
				bodyAttachments.Add(new Feather(this, num2, offset, relAngle, num6, num7, lengthFac));
				if (UnityEngine.Random.value < 0.1f + Mathf.Pow(1f - num2, 2f) * 0.15f && num2 < 0.9f)
				{
					float length = Mathf.Lerp(35f, 60f, Mathf.Pow(UnityEngine.Random.value, 1.5f)) * BodyContour(num2);
					float relAngle2 = num4 * -50f;
					bodyAttachments.Add(new Whisker(this, num2 + 0.005f, offset, relAngle2, num6, Mathf.Lerp(num7, num6, 0.5f), length, large: false));
				}
			}
		}
		num = 6;
		for (int k = 0; k < num; k++)
		{
			int num8 = 4 + k % 2;
			float num9 = (float)(k + 1) / (float)num;
			for (int l = 0; l < num8; l++)
			{
				float num10 = ((float)l - (float)(num8 - 1) / 2f) * 0.4f;
				float num11 = 0f;
				float lengthFac2 = BodyContour(num11) * Mathf.Cos(num10 * (float)Math.PI * 0.25f) * (1f - num9 * 0.65f);
				Vector2 offset2 = new Vector2(bodyThickness * num10 * BodyContour(num11) * Mathf.Cos(num9 * (float)Math.PI * 0.5f), bodyThickness * num9 * BodyContour(num11));
				float relAngle3 = num10 * (0f - (Mathf.Lerp(50f, 105f, num9) + Mathf.Pow(UnityEngine.Random.value, 2f) * 45f));
				float num12 = UnityEngine.Random.value * Mathf.Min(Mathf.Abs(num10), 1f) * 0.5f;
				float rootDarkness = num12;
				bodyAttachments.Add(new Feather(this, num11, offset2, relAngle3, num12, rootDarkness, lengthFac2));
			}
		}
		UnityEngine.Random.state = state;
		halo = new Halo(this, totalSprites);
		totalSprites += halo.totalSprites;
		firstBodySprite = totalSprites;
		totalSprites += 3;
		firstAttachmentSprite = totalSprites;
		totalSprites += bodyAttachments.Sum((BodyAttachment attachment) => attachment.totalSprites);
		firstEyeSprite = totalSprites;
		totalSprites += 2;
		distortionSprite = totalSprites;
		totalSprites++;
		body = new Segment[28];
		for (int m = 0; m < body.Length; m++)
		{
			float depth = Mathf.InverseLerp(0.2f, 1f, (float)m / (float)(body.Length - 1));
			body[m] = new Segment(bodyThickness * BodyContour((float)m / ((float)body.Length - 1f)), 80f / (float)body.Length * 4f, depth);
		}
		Reset(targetPos);
	}

	public void AddMegaWhiskers(float alongBody)
	{
		for (int i = 0; i < 6; i++)
		{
			float num = Mathf.Sin(((float)i / 5f * 2f - 1f) * (float)Math.PI * 0.5f);
			Vector2 offset = new Vector2(bodyThickness * num * BodyContour(alongBody), 0f);
			float relAngle = num * -50f;
			float darkness = 0f;
			float rootDarkness = 0.1f;
			float length = 240f;
			bodyAttachments.Add(new Whisker(this, alongBody, offset, relAngle, darkness, rootDarkness, length, large: true));
		}
	}

	public void StartDeactivate()
	{
		room.PlaySound(WatcherEnums.WatcherSoundID.Void_Weaver_Flare, body[0].pos);
		if (fadeOutCounter == 0)
		{
			fadeOutCounter = 1;
		}
	}

	public void DeactivateWeaver()
	{
		room.world.DeactivateWeaverPresence();
		if (base.slatedForDeletetion)
		{
			return;
		}
		for (int i = 0; i < room.game.Players.Count; i++)
		{
			if (room.game.Players[i].realizedCreature != null)
			{
				(room.game.Players[i].realizedCreature as Player).controller = null;
			}
		}
		if (room.game.session is StoryGameSession storyGameSession && !storyGameSession.voidWeaverEncountersThisCycle.Contains(room.abstractRoom.name))
		{
			storyGameSession.voidWeaverEncountersThisCycle.Add(room.abstractRoom.name);
			room.game.GetStorySession.saveState.miscWorldSaveData.numberOfVoidWeaverEncounters++;
		}
		room.game.cameras[0].ExitCutsceneMode();
		room.PlaySound(WatcherEnums.WatcherSoundID.Void_Weaver_Vanish);
		Destroy();
	}

	public void Speak(SoundID line)
	{
		voice = new PositionedSoundEmitter(body[0].pos, 1f, 1f)
		{
			requireActiveUpkeep = true
		};
		room.PlaySound(line, voice, loop: false, 1f, 1f, randomStartPosition: false);
	}

	public void StopSpeaking()
	{
		voice = null;
	}

	public void SetBodyPos(Vector2 position)
	{
		body[0].pos = position;
		body[0].lastPos = position;
		targetPos = position;
		if (placedObject != null)
		{
			placedObject.pos = position;
		}
		Reset(position);
	}

	public bool GetVoiceSpectrum(out float lows, out float highs)
	{
		highs = 0f;
		lows = 0f;
		if (voice?.currentSoundObject == null || !voice.currentSoundObject.IsPlaying)
		{
			return false;
		}
		voice.currentSoundObject.GetSpectrumData(freqSamples, 0, FFTWindow.BlackmanHarris);
		for (int i = 0; i < 5; i++)
		{
			lows += freqSamples[i];
		}
		for (int j = 5; j < freqSamples.Length; j++)
		{
			highs += freqSamples[j];
		}
		return true;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (base.slatedForDeletetion)
		{
			return;
		}
		if (placedObject != null)
		{
			targetPos = placedObject.pos;
			targetDir = -(placedObject.data as PlacedObject.ResizableObjectData).handlePos.normalized;
		}
		animCounter++;
		behavior?.Update();
		if (fadeOutCounter == 0 && behavior != null && behavior.Vanish)
		{
			StartDeactivate();
		}
		if (voice != null)
		{
			voice.alive = true;
			voice.pos = body[0].pos;
			if (voice.slatedForDeletetion)
			{
				voice = null;
			}
		}
		lastTalking = talking;
		talking = Mathf.MoveTowards(talking, (voice != null) ? 1f : 0f, 1f / 30f);
		if (jangleCooldown > 0)
		{
			jangleCooldown--;
		}
		else
		{
			jangleCooldown = UnityEngine.Random.Range(133, 400);
			room.PlaySound(WatcherEnums.WatcherSoundID.Void_Weaver_Jangle, body[UnityEngine.Random.Range(0, body.Length)].pos);
		}
		if (rustleCooldown > 0)
		{
			rustleCooldown--;
		}
		else
		{
			rustleCooldown = UnityEngine.Random.Range(80, 240);
			room.PlaySound(WatcherEnums.WatcherSoundID.Void_Weaver_Rustle, body[UnityEngine.Random.Range(0, body.Length)].pos);
		}
		Segment[] array = body;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Update();
		}
		foreach (BodyAttachment bodyAttachment in bodyAttachments)
		{
			bodyAttachment.Update();
		}
		body[0].vel += Vector2.ClampMagnitude(targetPos - body[0].pos, 200f) * 0.08f;
		for (int j = 0; j < body.Length; j++)
		{
			float num = (float)j / (float)(body.Length - 1);
			float num2 = Mathf.Sin(Mathf.InverseLerp(0.03f, 1f, num) * (float)Math.PI);
			float num3 = (float)animCounter / 40f - num * (float)Math.PI * 4.7f;
			body[j].vel -= targetDir * Mathf.Lerp(0.6f, 0.45f, num);
			body[j].vel += Custom.PerpendicularVector(targetDir) * (Mathf.Sin(num3) * 2f * num2) + targetDir * (0.6f * Mathf.Abs(Mathf.Sin(num3)) * num2) + targetDir * (-0.5f * Mathf.Sign(Mathf.Sin(2f * num3)) * Mathf.Abs(Mathf.Sin(2f * num3)) * num2);
		}
		if (behavior?.FocusPlayer != null)
		{
			lean = Mathf.Min(lean + 1f / 30f, 1f);
			Vector2 vector = behavior.FocusPlayer.mainBodyChunk.pos;
			for (int k = 0; k < 6; k++)
			{
				float num4 = (float)k / 5f * 2f - 1f;
				body[k].vel += (body[k].pos - vector).normalized * (num4 * lean);
			}
		}
		DoRopePhysics(body, body: true);
		foreach (BodyAttachment bodyAttachment2 in bodyAttachments)
		{
			bodyAttachment2.Update();
		}
		foreach (AbstractCreature player2 in room.game.Players)
		{
			if (player2.realizedCreature is Player player && player.room == room && player.graphicsModule is PlayerGraphics playerGraphics)
			{
				playerGraphics.LookAtPoint(body[0].pos, 10000f);
			}
		}
		if (fadeOutCounter > 0)
		{
			fadeOutCounter++;
		}
		if (fadeOutCounter > 80)
		{
			if (fadeOut == null)
			{
				fadeOut = new VoidWeaverFade(body[0].pos);
				fadeOut.giveAchievement = true;
				room.AddObject(fadeOut);
			}
			fadeOut.setPos = body[0].pos;
			if (fadeOut.RampingDown)
			{
				DeactivateWeaver();
			}
		}
		pos = body[0].pos;
	}

	public void Reset(Vector2 pos)
	{
		Vector2 vector = pos;
		Segment[] array = body;
		foreach (Segment segment in array)
		{
			segment.Reset(vector);
			vector -= targetDir * (segment.connectionRad * 2f);
		}
		foreach (BodyAttachment bodyAttachment in bodyAttachments)
		{
			bodyAttachment.Reset(pos);
		}
		base.pos = pos;
		lastPos = pos;
	}

	public void DoRopePhysics(Segment[] segments, bool body)
	{
		for (int i = 1; i < segments.Length; i++)
		{
			segments[i].Connect(segments[i - 1]);
		}
		foreach (Segment segment in segments)
		{
			segment.vel = Vector2.ClampMagnitude(segment.vel, 30f);
			segment.pos += segment.vel;
		}
		if (body)
		{
			for (int k = 0; k < segments.Length; k++)
			{
				segments[k].vel.y -= 0.025f;
				segments[k].vel *= 0.8f;
			}
			for (int l = 0; l < segments.Length; l++)
			{
				Segment segment2 = segments[l];
				for (int m = l + 2; m < segments.Length; m++)
				{
					Segment segment3 = segments[m];
					Vector2 vector = segment3.pos - segment2.pos;
					float magnitude = vector.magnitude;
					float num = (segment2.rad + segment3.rad) * 0.75f;
					vector /= magnitude;
					if (magnitude > 0f && magnitude < num)
					{
						Vector2 vector2 = vector * ((magnitude - num) * 0.2f);
						segment2.vel += vector2;
						segment3.vel -= vector2;
					}
				}
			}
			return;
		}
		for (int n = 0; n < segments.Length; n++)
		{
			segments[n].vel += 0.05f * GetWind(segments[n].pos, animCounter);
			segments[n].vel *= 0.925f;
		}
		for (int num2 = 1; num2 < segments.Length - 1; num2++)
		{
			Segment segment4 = segments[num2 - 1];
			Segment segment5 = segments[num2 + 1];
			Vector2 vector3 = Custom.DirVec(segment5.pos, segment4.pos);
			float num3 = Vector2.Distance(segment4.pos, segment5.pos);
			float num4 = (segment4.connectionRad + segment5.connectionRad) * 1.25f;
			if (num3 < num4)
			{
				Vector2 vector4 = vector3 * ((num4 - num3) * 0.1f);
				segment4.vel += vector4;
				segment5.vel -= vector4;
			}
		}
	}

	public static Vector2 GetWind(Vector2 pos, int time)
	{
		float num = (float)time / 40f * 0.5f;
		return new Vector2((Mathf.PerlinNoise(pos.x * 0.01f + num, pos.y * 0.01f) - 0.46535593f) * 2f, (Mathf.PerlinNoise(pos.x * 0.01f + num, pos.y * 0.01f + 200f) - 0.46535593f) * 2f);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites = new FSprite[totalSprites];
		sLeaser.sprites[firstBodySprite] = TriangleMesh.MakeLongMeshAtlased(body.Length, pointyTip: false, customColor: true);
		sLeaser.sprites[firstBodySprite + 1] = new FSprite("Futile_White")
		{
			shader = rCam.game.rainWorld.Shaders["VectorCircle"],
			scale = body[0].rad / 8f
		};
		sLeaser.sprites[firstBodySprite + 2] = new FSprite("Futile_White")
		{
			shader = rCam.game.rainWorld.Shaders["VectorCircle"],
			scale = body[body.Length - 1].rad / 8f
		};
		int num = firstAttachmentSprite;
		foreach (BodyAttachment bodyAttachment in bodyAttachments)
		{
			bodyAttachment.InitiateSprites(sLeaser, rCam, num);
			num += bodyAttachment.totalSprites;
		}
		sLeaser.sprites[firstEyeSprite] = TriangleMesh.MakeLongMeshAtlased(8, pointyTip: false, customColor: false);
		sLeaser.sprites[firstEyeSprite].shader = Custom.rainWorld.Shaders["RippleBasicBothSides"];
		sLeaser.sprites[firstEyeSprite + 1] = new FSprite("Circle20");
		sLeaser.sprites[firstEyeSprite + 1].shader = Custom.rainWorld.Shaders["RippleBasicBothSides"];
		sLeaser.sprites[distortionSprite] = new FSprite("Futile_White")
		{
			shader = rCam.game.rainWorld.Shaders["GhostDistortion"]
		};
		halo.InitiateSprites(sLeaser, rCam);
		AddToContainer(sLeaser, rCam, null);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		FSprite[] sprites = sLeaser.sprites;
		for (int i = 0; i < sprites.Length; i++)
		{
			sprites[i].RemoveFromContainer();
		}
		rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[distortionSprite]);
		halo.AddToContainer(sLeaser, rCam, newContatiner);
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		sprites = sLeaser.sprites;
		foreach (FSprite fSprite in sprites)
		{
			if (fSprite.container == null)
			{
				newContatiner.AddChild(fSprite);
			}
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (base.slatedForDeletetion)
		{
			return;
		}
		TriangleMesh triangleMesh = sLeaser.sprites[firstBodySprite] as TriangleMesh;
		Vector2 vector = Vector2.Lerp(body[0].lastPos, body[0].pos, timeStacker);
		float num = body[0].rad;
		float num2 = body[0].depth;
		for (int i = 0; i < body.Length; i++)
		{
			int num3 = i * 4;
			Vector2 vector2 = Vector2.Lerp(body[i].lastPos, body[i].pos, timeStacker);
			Vector2 vector3 = Custom.DirVec(vector, vector2);
			Vector2 vector4 = Custom.PerpendicularVector(vector3);
			float num4 = Vector2.Distance(vector, vector2) / 5f;
			float rad = body[i].rad;
			float depth = body[i].depth;
			Vector2 vector5 = vector4 * ((num + rad) * 0.5f);
			Vector2 vector6 = vector4 * rad;
			triangleMesh.MoveVertice(num3, rCam.ApplyDepth(vector - vector5 + vector3 * num4, num2 * 30f) - camPos);
			triangleMesh.MoveVertice(num3 + 1, rCam.ApplyDepth(vector + vector5 + vector3 * num4, num2 * 30f) - camPos);
			triangleMesh.MoveVertice(num3 + 2, rCam.ApplyDepth(vector2 - vector6 - vector3 * num4, depth * 30f) - camPos);
			triangleMesh.MoveVertice(num3 + 3, rCam.ApplyDepth(vector2 + vector6 - vector3 * num4, depth * 30f) - camPos);
			vector = vector2;
			num = rad;
			num2 = depth;
		}
		sLeaser.sprites[firstBodySprite + 1].SetPosition(Vector2.Lerp(body[0].lastPos, body[0].pos, timeStacker) - camPos);
		sLeaser.sprites[firstBodySprite + 2].SetPosition(Vector2.Lerp(body[body.Length - 1].lastPos, body[body.Length - 1].pos, timeStacker) - camPos);
		sLeaser.sprites[firstBodySprite + 2].scale = Custom.GetDepthScale(body[body.Length - 1].depth) * body[body.Length - 1].rad / 8f;
		TriangleMesh triangleMesh2 = sLeaser.sprites[firstEyeSprite] as TriangleMesh;
		Vector2 pointAlongBody = GetPointAlongBody(0f, timeStacker);
		Vector2 vector7 = Custom.DirVec(GetPointAlongBody(0.05f, timeStacker), GetPointAlongBody(0f, timeStacker));
		Vector2 vector8 = -Custom.PerpendicularVector(vector7);
		pointAlongBody -= vector7 * 3f;
		for (int j = 0; j < triangleMesh2.vertices.Length; j += 2)
		{
			float num5 = (float)j / (float)(triangleMesh2.vertices.Length - 2) * 2f - 1f;
			Vector2 vector9 = pointAlongBody + vector7 * (num5 * 16f);
			float num6 = (1f - Mathf.Pow(Mathf.Abs(num5), 2.4f)) * 11f;
			triangleMesh2.MoveVertice(j, vector9 + vector8 * num6 - camPos);
			triangleMesh2.MoveVertice(j + 1, vector9 - vector8 * num6 - camPos);
		}
		Vector2 vector10 = room.lightAngle.normalized * new Vector2(-1f, 1f);
		sLeaser.sprites[firstEyeSprite + 1].SetPosition(pointAlongBody + vector10 * 7f - camPos);
		sLeaser.sprites[firstEyeSprite + 1].scaleY = 0.3f;
		sLeaser.sprites[firstEyeSprite + 1].scaleX = 0.5f;
		sLeaser.sprites[firstEyeSprite + 1].alpha = 0.25f;
		sLeaser.sprites[firstEyeSprite + 1].rotation = Custom.VecToDeg(vector10);
		int num7 = firstAttachmentSprite;
		foreach (BodyAttachment bodyAttachment in bodyAttachments)
		{
			bodyAttachment.DrawSprites(sLeaser, rCam, timeStacker, camPos, num7);
			num7 += bodyAttachment.totalSprites;
		}
		sLeaser.sprites[distortionSprite].SetPosition(GetPointAlongBody(0f, timeStacker) - camPos - targetDir * 120f);
		sLeaser.sprites[distortionSprite].scale = 58.3125f;
		halo.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public float BodyContour(float t)
	{
		return 0.4f + 0.6f * Mathf.Pow(t, 1.5f) + 0.35f * Mathf.Pow(1f - t, 3f) - Mathf.Pow(t, 5f);
	}

	public Vector2 GetPointAlongBody(float t, float timeStacker)
	{
		t *= (float)body.Length;
		int num = (int)t;
		t -= (float)num;
		if (num > body.Length - 2)
		{
			num = body.Length - 2;
			t = 1f;
		}
		else if (num < 0)
		{
			num = 0;
			t = 0f;
		}
		return Vector2.Lerp(Vector2.Lerp(body[num].lastPos, body[num].pos, timeStacker), Vector2.Lerp(body[num + 1].lastPos, body[num + 1].pos, timeStacker), t);
	}

	public float GetDepthAlongBody(float t)
	{
		t *= (float)body.Length;
		int num = (int)t;
		t -= (float)num;
		if (num > body.Length - 2)
		{
			num = body.Length - 2;
			t = 1f;
		}
		else if (num < 0)
		{
			num = 0;
			t = 0f;
		}
		return Mathf.Lerp(body[num].depth, body[num + 1].depth, t);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		Color goldRGB = RainWorld.GoldRGB;
		Color color = Color.Lerp(palette.blackColor, goldRGB, 0.5f);
		Color b = palette.skyColor;
		if (inEndingRoom)
		{
			goldRGB = endingRoomLight;
			color = endingRoomDark;
			b = VoidWeaverEndingScene.lightSkyColor;
		}
		TriangleMesh triangleMesh = sLeaser.sprites[firstBodySprite] as TriangleMesh;
		for (int i = 0; i < triangleMesh.verticeColors.Length; i += 2)
		{
			float t = (float)i / (float)(triangleMesh.verticeColors.Length - 2);
			GetDepthAlongBody(t);
			float t2 = 0f;
			float t3 = 0f;
			triangleMesh.verticeColors[i] = (triangleMesh.verticeColors[i + 1] = Color.Lerp(Color.Lerp(color, palette.blackColor, t2), b, t3));
		}
		sLeaser.sprites[firstBodySprite + 1].color = goldRGB;
		sLeaser.sprites[firstBodySprite + 2].color = color;
		sLeaser.sprites[firstEyeSprite].color = palette.blackColor;
		sLeaser.sprites[firstEyeSprite + 1].color = Color.white;
		sLeaser.sprites[distortionSprite].color = Color.white;
		int num = firstAttachmentSprite;
		foreach (BodyAttachment bodyAttachment in bodyAttachments)
		{
			bodyAttachment.ApplyPalette(sLeaser, rCam, palette, num);
			num += bodyAttachment.totalSprites;
		}
		halo.ApplyPalette(sLeaser, rCam, palette);
	}
}
