using System;
using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class UrbanShadow : CosmeticSprite
{
	public class PuppetPart : CosmeticSprite, INotifyWhenRoomUnloaded
	{
		public class Type : ExtEnum<Type>
		{
			public static Type LowerBody = new Type("LowerBody", register: true);

			public static Type UpperBody = new Type("UpperBody", register: true);

			public static Type BodyDecor = new Type("BodyDecor", register: true);

			public static Type HelmetBase = new Type("HelmetBase", register: true);

			public static Type HelmetDecor = new Type("HelmetDecor", register: true);

			public static Type UpperArm = new Type("UpperArm", register: true);

			public static Type UpperArmAlt = new Type("UpperArmAlt", register: true);

			public static Type LowerArm = new Type("LowerArm", register: true);

			public static Type LowerArmAlt = new Type("LowerArmAlt", register: true);

			public static Type CarriedObject = new Type("CarriedObject", register: true);

			public static Type HangingDecor = new Type("HangingDecor", register: true);

			public static Type Vehicle = new Type("Vehicle", register: true);

			public Type(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public Type type;

		public PuppetPart owner;

		public float depth;

		public Vector2 pivot;

		public Vector2 attachTo;

		public Vector2 scale;

		public FAtlasElement element;

		public float lastFinalRotation;

		public int visibilityDelayFrames = 2;

		public float animPhase;

		public float reverseOffset;

		public string spriteName;

		public float variation;

		public float rotation;

		public float initialRotation;

		public float ownerRotationInfluence;

		public float finalRotation;

		public static float magicScale = 16f;

		public float scaleMultiplier;

		public bool setReversed;

		public Mesh urbanShadowMesh;

		public static float meshOffset = 0.1f;

		public static Vector2[] meshOffsets = new Vector2[8]
		{
			new Vector2(meshOffset, 0f),
			new Vector2(0f - meshOffset, 0f),
			new Vector2(0f, meshOffset),
			new Vector2(0f, 0f - meshOffset),
			new Vector2((0f - meshOffset) * 0.77f, (0f - meshOffset) * 0.77f),
			new Vector2((0f - meshOffset) * 0.77f, meshOffset * 0.77f),
			new Vector2(meshOffset * 0.77f, (0f - meshOffset) * 0.77f),
			new Vector2(meshOffset * 0.77f, meshOffset * 0.77f)
		};

		public static string[] SpritesHelmetBase = new string[13]
		{
			"LizardHead0.2", "LizardHead0.5", "LizardHead0.7", "LizardHead1.4", "LizardHead2.1", "LizardHead2.7", "LizardHead3.2", "LizardHead3.3", "EelJaw2A", "HeadA4",
			"JetFish8", "KrakenShield0", "MirosLeg"
		};

		public static string[] SpritesLowerBody = new string[10] { "JetFish8", "JetFish7", "JetFish0", "Cicada7body", "HeadA11", "HeadA9", "KrakenHead7", "LizardHead1.5", "LizardHead1.6", "LizardHead1.7" };

		public static string[] SpritesUpperBody = new string[13]
		{
			"JetFish8", "JetFish7", "JetFish0", "JetFish1", "Cicada1body", "Cicada2body", "Cicada7body", "HeadA11", "HeadA9", "KrakenHead7",
			"KrakenHead1", "KrakenHead2", "KrakenHead3"
		};

		public static string[] SpritesUpperArm = new string[4] { "deerLegA1", "deerLegA2", "deerLegB1", "deerLegB2" };

		public static string[] SpritesLowerArm = new string[9] { "JetFish4", "LizardArm_09", "LizardArm_27", "LizardArm_36", "LizardArm_45", "LizardArm_45", "LizardArm_54", "PlayerArm12", "PlayerArm11" };

		public static string[] SpritesHelmetDecor = new string[11]
		{
			"ScavengerHandA", "SnailHead", "SpiderLeg0B", "SpiderLeg1A", "SpiderLeg1B", "SpiderLeg2B", "EelJaw2B", "LizardBubble4", "LizardScaleA4", "LizardScaleA3",
			"MirosLeg"
		};

		public float GetFinalRotation => initialRotation + rotation + (owner?.rotation * ownerRotationInfluence).GetValueOrDefault();

		public PuppetPart(Room room, PuppetPart owner, Type type, bool reversed)
		{
			this.type = type;
			this.owner = owner;
			variation = UnityEngine.Random.value;
			base.room = room;
			animPhase = GetPhase(type);
			setReversed = reversed;
			depth = owner?.depth ?? UnityEngine.Random.value;
			scaleMultiplier = 2f - depth * 0.5f;
			scale = GetScale(type) * scaleMultiplier;
			initialRotation = GetRotation(type);
			if (owner != null && InheritInitRotation(type))
			{
				initialRotation += owner.initialRotation;
			}
			ownerRotationInfluence = GetOwnerRotationInfluence(type);
			pivot = GetPivotPoint(type);
			attachTo = GetAttachementPoint(type);
			element = GetSprite(type);
			lastFinalRotation = finalRotation;
			Grow();
		}

		public override void Destroy()
		{
			base.Destroy();
			DisposeMesh();
		}

		public void RoomUnloaded()
		{
			Destroy();
		}

		public override void Update(bool eu)
		{
			if (base.slatedForDeletetion)
			{
				return;
			}
			lastFinalRotation = finalRotation;
			finalRotation = GetFinalRotation;
			if (visibilityDelayFrames > 0)
			{
				visibilityDelayFrames--;
			}
			if (owner != null)
			{
				base.Update(eu);
				reverseOffset = owner.reverseOffset;
				if (owner.slatedForDeletetion)
				{
					Destroy();
				}
				animPhase = owner.animPhase;
				Rotate(type);
				Move();
			}
		}

		public void Move()
		{
			pos = owner.pos + Custom.rotateVectorDeg((attachTo - owner.pivot) * owner.scale * magicScale, owner.finalRotation);
		}

		public void Rotate(Type type)
		{
			if (type == Type.UpperArm)
			{
				rotation = Mathf.Sin((animPhase + 0.35f) * 2f * (float)Math.PI) * 20f;
			}
			if (type == Type.UpperArmAlt)
			{
				rotation = Mathf.Sin((animPhase + 0.5f + 0.35f) * 2f * (float)Math.PI) * 20f;
			}
			if (type == Type.HangingDecor && owner.type == Type.LowerArm)
			{
				rotation = Mathf.Sin((animPhase + 0.25f) * 2f * (float)Math.PI) * 20f;
			}
			if (type == Type.HangingDecor && owner.type == Type.LowerArmAlt)
			{
				rotation = Mathf.Sin((animPhase + 0.5f + 0.25f) * 2f * (float)Math.PI) * 20f;
			}
			if (type == Type.HelmetBase || type == Type.HelmetDecor || type == Type.BodyDecor)
			{
				float num = ((!(type == Type.HelmetDecor) && !(type == Type.BodyDecor)) ? 1 : (-1));
				float num2 = animPhase;
				float num3 = Mathf.Sin(num2 * num * 8f * (float)Math.PI) + 1f;
				float num4 = Mathf.Max(Mathf.Sin((num2 + 1.0461504f * num) * num * 4f * (float)Math.PI), 0f) * num;
				float num5 = Mathf.InverseLerp(3.85f, 7f, owner.scale.y);
				float num6 = Mathf.Sin((num2 - 0.062f * (1f * num5) * num) * 4f * (float)Math.PI);
				rotation = num6 * 4f + num3 * num4 * 1f;
				rotation *= 0.2f + num5 * 0.8f;
				rotation *= num;
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			if (!base.slatedForDeletetion)
			{
				sLeaser.maskSources = new MaskSource[1];
				DisposeMesh();
				urbanShadowMesh = CreateMesh();
				sLeaser.maskSources[0] = MaskMaker.MakeSource("UrbanShadowsGrab", GetShader(type), isSecondLayer: false, urbanShadowMesh);
				sLeaser.maskSources[0].SetProperty(0, 1f);
				sLeaser.maskSources[0].SetProperty(1, 1f);
				sLeaser.maskSources[0].SetProperty(2, 1f);
				sLeaser.maskSources[0].SetProperty(3, 2f / Mathf.Max(meshOffsets.Length, 1f) * (1f - depth * 0.75f));
				sLeaser.maskSources[0].ApplyVertexColor();
				if (element.name != "Futile_White")
				{
					sLeaser.maskSources[0].SetTexture(element.atlas.texture);
				}
				sLeaser.maskSources[0].obj?.SetActive(value: false);
				new FSprite("Futile_White");
				UpdateTranslation(sLeaser, 0f);
			}
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			if (base.slatedForDeletetion)
			{
				if (sLeaser.maskSources.Length != 0)
				{
					sLeaser.maskSources = new MaskSource[0];
				}
			}
			else
			{
				base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
				sLeaser.maskSources[0].obj?.SetActive(visibilityDelayFrames <= 0);
				UpdateTranslation(sLeaser, timeStacker);
			}
		}

		public void UpdateTranslation(RoomCamera.SpriteLeaser sLeaser, float timeStacker)
		{
			Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
			if (setReversed)
			{
				vector = new Vector2(0f - vector.x + reverseOffset, vector.y);
			}
			vector *= UrbanLife.renderScale;
			sLeaser.maskSources[0].setGameObjectPos = vector;
			sLeaser.maskSources[0].setGameObjectScale = scale * UrbanLife.renderScale;
			sLeaser.maskSources[0].setGameObjectRotation = new Vector3(0f, setReversed ? 180 : 0, -1f * Mathf.LerpAngle(lastFinalRotation, finalRotation, timeStacker));
		}

		public void DisposeMesh()
		{
			if (urbanShadowMesh != null)
			{
				UnityEngine.Object.Destroy(urbanShadowMesh);
				urbanShadowMesh = null;
			}
		}

		public Mesh CreateMesh()
		{
			Mesh mesh = new Mesh
			{
				name = "UrbanShadow Mesh"
			};
			Vector3[] array = new Vector3[MaskSource.quadVerts.Length * (meshOffsets.Length + 1)];
			MaskSource.quadVerts.Select((Vector3 v) => v - (Vector3)pivot).ToArray().CopyTo(array, 0);
			int[] array2 = new int[MaskSource.quadTris.Length * (meshOffsets.Length + 1)];
			MaskSource.quadTris.CopyTo(array2, 0);
			Vector2[] array3 = new Vector2[4] { element.uvBottomLeft, element.uvTopLeft, element.uvBottomRight, element.uvTopRight };
			Vector2[] array4 = new Vector2[MaskSource.quadUV.Length * (meshOffsets.Length + 1)];
			array3.CopyTo(array4, 0);
			float num = 1f + depth * 6f;
			for (int i = 1; i <= meshOffsets.Length; i++)
			{
				array3.CopyTo(array4, MaskSource.quadUV.Length * i);
				for (int j = 0; j < MaskSource.quadTris.Length; j++)
				{
					array2[MaskSource.quadTris.Length * i + j] = array2[j] + MaskSource.quadVerts.Length * i;
				}
				for (int k = 0; k < MaskSource.quadVerts.Length; k++)
				{
					array[MaskSource.quadVerts.Length * i + k] = array[k] + new Vector3(meshOffsets[i - 1].x * num / scale.x, meshOffsets[i - 1].y * num / scale.y, 0.1f * (float)i);
				}
			}
			mesh.vertices = array;
			mesh.triangles = array2;
			mesh.uv = array4;
			mesh.RecalculateBounds();
			return mesh;
		}

		public void Grow()
		{
			foreach (Type requiredChild in GetRequiredChildren(type))
			{
				PuppetPart obj = new PuppetPart(room, this, requiredChild, setReversed);
				room.AddObject(obj);
			}
			foreach (Type optionalChild in GetOptionalChildren(type))
			{
				int num = UnityEngine.Random.Range(0, GetMaxPartsAllowed(optionalChild));
				for (int i = 0; i < num; i++)
				{
					PuppetPart obj2 = new PuppetPart(room, this, optionalChild, setReversed);
					room.AddObject(obj2);
				}
			}
		}

		public string GetShader(Type type)
		{
			if (type == Type.Vehicle)
			{
				return "UrbanShadowGradient";
			}
			return "Basic";
		}

		public List<Type> GetRequiredChildren(Type type)
		{
			List<Type> list = new List<Type>();
			if (type == Type.LowerBody)
			{
				list.Add(Type.UpperBody);
				return list;
			}
			if (type == Type.UpperBody)
			{
				list.Add(Type.HelmetBase);
				list.Add(Type.UpperArmAlt);
				list.Add(Type.UpperArm);
				return list;
			}
			if (type == Type.UpperArm)
			{
				list.Add(Type.LowerArm);
				return list;
			}
			if (type == Type.UpperArmAlt)
			{
				list.Add(Type.LowerArmAlt);
				return list;
			}
			return list;
		}

		public FAtlasElement GetSprite(Type type)
		{
			new List<Type>();
			if (type == Type.HelmetBase)
			{
				return GetRandomSprite(SpritesHelmetBase);
			}
			if (type == Type.UpperBody)
			{
				return GetRandomSprite(SpritesUpperBody);
			}
			if (type == Type.LowerBody)
			{
				return GetRandomSprite(SpritesLowerBody);
			}
			if (type == Type.UpperArm || type == Type.UpperArmAlt)
			{
				return GetRandomSprite(SpritesUpperArm);
			}
			if (type == Type.LowerArm || type == Type.LowerArmAlt)
			{
				return GetRandomSprite(SpritesLowerArm);
			}
			if (type == Type.HelmetDecor || type == Type.BodyDecor)
			{
				return GetRandomSprite(SpritesHelmetDecor);
			}
			return Futile.atlasManager.GetElementWithName("Futile_White");
		}

		public FAtlasElement GetRandomSprite(string[] sprites)
		{
			return Futile.atlasManager.GetElementWithName(sprites[UnityEngine.Random.Range(0, sprites.Length)]);
		}

		public List<Type> GetOptionalChildren(Type type)
		{
			List<Type> list = new List<Type>();
			if (type == Type.LowerBody)
			{
				list.Add(Type.BodyDecor);
				return list;
			}
			if (type == Type.UpperBody)
			{
				list.Add(Type.BodyDecor);
				return list;
			}
			if (type == Type.HelmetBase)
			{
				list.Add(Type.HelmetDecor);
				return list;
			}
			if (type == Type.UpperArm || type == Type.UpperArmAlt)
			{
				return list;
			}
			if (type == Type.LowerArm || type == Type.LowerArmAlt)
			{
				list.Add(Type.HangingDecor);
				return list;
			}
			return list;
		}

		public int GetMaxPartsAllowed(Type type)
		{
			if (type == Type.HelmetDecor)
			{
				return 5;
			}
			if (type == Type.BodyDecor)
			{
				return 5;
			}
			if (type == Type.HangingDecor)
			{
				return 3;
			}
			return 1;
		}

		public float GetPhase(Type part)
		{
			if (part == Type.UpperArmAlt || part == Type.LowerArmAlt)
			{
				return 0.25f;
			}
			return 0f;
		}

		public Vector2 GetAttachementPoint(Type attachingPart)
		{
			if (attachingPart == Type.UpperBody)
			{
				return new Vector2(0f, 0.2f);
			}
			if (attachingPart == Type.HelmetBase)
			{
				return new Vector2(0f, 0.4f);
			}
			if (attachingPart == Type.UpperArm)
			{
				return new Vector2(0f, 0.3f);
			}
			if (attachingPart == Type.UpperArmAlt)
			{
				return new Vector2(0f, 0.3f);
			}
			if (attachingPart == Type.LowerArm || attachingPart == Type.LowerArmAlt)
			{
				return new Vector2(0f, 0.4f);
			}
			if (attachingPart == Type.HelmetDecor)
			{
				return new Vector2(-0.1f, UnityEngine.Random.Range(-0.4f, 0.4f));
			}
			if (attachingPart == Type.BodyDecor)
			{
				return new Vector2(-0.1f, UnityEngine.Random.Range(-0.4f, 0.4f));
			}
			if (attachingPart == Type.HangingDecor)
			{
				return new Vector2(0f, UnityEngine.Random.Range(-0.4f, 0.4f));
			}
			return Vector2.zero;
		}

		public Vector2 GetPivotPoint(Type part)
		{
			if (part == Type.Vehicle)
			{
				return new Vector2(0f, -0.1f);
			}
			if (part == Type.LowerBody)
			{
				return new Vector2(0f, -0.5f);
			}
			if (part == Type.UpperBody)
			{
				return new Vector2(0f, -0.4f);
			}
			if (part == Type.HelmetBase)
			{
				return new Vector2(0.2f, UnityEngine.Random.Range(-0.3f, -0.2f));
			}
			if (part == Type.UpperArm || part == Type.UpperArmAlt)
			{
				return new Vector2(0f, -0.4f);
			}
			if (part == Type.LowerArm || part == Type.LowerArmAlt)
			{
				return new Vector2(-0.4f, 0f);
			}
			if (part == Type.HelmetDecor)
			{
				return new Vector2(0f, -0.4f);
			}
			if (part == Type.BodyDecor)
			{
				return new Vector2(0f, -0.4f);
			}
			if (part == Type.HangingDecor)
			{
				return new Vector2(0f, 0.4f);
			}
			return Vector2.zero;
		}

		public Vector2 GetScale(Type part)
		{
			if (part == Type.Vehicle)
			{
				return new Vector2(5f, 1.5f) * (25f - 10f * depth);
			}
			if (part == Type.LowerBody)
			{
				return new Vector2(UnityEngine.Random.Range(0.8f, 1.8f), UnityEngine.Random.Range(2f, 2.8f)) * 3f;
			}
			if (part == Type.UpperBody)
			{
				return new Vector2(UnityEngine.Random.Range(0.8f, 1.4f), UnityEngine.Random.Range(1.6f, 2.9f)) * 3f;
			}
			if (part == Type.HelmetBase)
			{
				return new Vector2(1f, UnityEngine.Random.Range(1.1f, 2f)) * 3.5f;
			}
			if (part == Type.UpperArm)
			{
				return new Vector2(0.8f, 2.9f) * 1.6f;
			}
			if (part == Type.LowerArm)
			{
				return new Vector2(2.8f, 0.8f) * 1.6f;
			}
			if (part == Type.UpperArmAlt)
			{
				return GetScale(Type.UpperArm) * 0.9f;
			}
			if (part == Type.LowerArmAlt)
			{
				return GetScale(Type.LowerArm) * 0.9f;
			}
			if (part == Type.HangingDecor)
			{
				return new Vector2(0.3f, UnityEngine.Random.Range(1f, 4f)) * UnityEngine.Random.Range(1f, 2f);
			}
			if (part == Type.HelmetDecor)
			{
				return new Vector2(0.7f, UnityEngine.Random.Range(2f, 3f)) * UnityEngine.Random.Range(1f, 2f);
			}
			if (part == Type.BodyDecor)
			{
				return new Vector2(0.7f, UnityEngine.Random.Range(2.5f, 3.5f)) * UnityEngine.Random.Range(1f, 2f);
			}
			return Vector2.one;
		}

		public float GetRotation(Type part)
		{
			if (part == Type.LowerBody)
			{
				return UnityEngine.Random.Range(1f, 15f);
			}
			if (part == Type.UpperBody)
			{
				return UnityEngine.Random.Range(-10f, 10f);
			}
			if (part == Type.HelmetBase)
			{
				return UnityEngine.Random.Range(90f, 115f);
			}
			if (part == Type.UpperArm || part == Type.UpperArmAlt)
			{
				return 180f;
			}
			if (part == Type.LowerArm || part == Type.LowerArmAlt)
			{
				return 80f;
			}
			if (part == Type.HelmetDecor)
			{
				return UnityEngine.Random.Range(-120f, -140f);
			}
			if (part == Type.BodyDecor)
			{
				return UnityEngine.Random.Range(-130f, -160f);
			}
			return 0f;
		}

		public float GetOwnerRotationInfluence(Type part)
		{
			if (part == Type.HelmetBase)
			{
				return 0.1f;
			}
			if (part == Type.UpperArm || part == Type.UpperArmAlt)
			{
				return 0.2f;
			}
			if (part == Type.LowerArm || part == Type.LowerArmAlt)
			{
				return 0.9f;
			}
			if (part == Type.HangingDecor)
			{
				return 0.1f;
			}
			return 1f;
		}

		public bool InheritInitRotation(Type type)
		{
			if (type == Type.HelmetDecor || type == Type.BodyDecor)
			{
				return true;
			}
			return false;
		}
	}

	public Vector2 start;

	public Vector2 end;

	public Vector2 root;

	public float stepSize;

	public Counter progress;

	public DebugSprite dbg;

	public bool debug;

	public PuppetPart puppet;

	public UrbanShadow(Room room, Vector2 root, Vector2 start, Vector2 end, bool isVehicle, bool randomStart = false)
	{
		bool reversed = UnityEngine.Random.value > 0.5f;
		this.start = start;
		this.end = end;
		lastPos = start;
		this.root = root;
		if (isVehicle)
		{
			this.start -= (this.end - this.start).normalized * 1200f;
			this.end -= (this.start - this.end).normalized * 1200f;
		}
		float num = Vector2.Distance(start, end) / (isVehicle ? 60000f : 3000f);
		pos = start;
		puppet = new PuppetPart(room, null, isVehicle ? PuppetPart.Type.Vehicle : PuppetPart.Type.LowerBody, reversed);
		progress = new Counter((int)(UnityEngine.Random.Range(900f, 1200f) * num * (1f + puppet.depth * 0.5f)), 0, countsUp: true);
		if (randomStart)
		{
			progress.Set(UnityEngine.Random.Range(100, progress.max - 100));
		}
		stepSize = (float)UnityEngine.Random.Range(110, 150) * (0.8f - puppet.depth * 0.2f);
		puppet.animPhase = Vector2.Distance(pos, end) / (stepSize * 2f);
		room.AddObject(puppet);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (base.slatedForDeletetion)
		{
			return;
		}
		puppet.lastPos = puppet.pos;
		if (progress.isFinished)
		{
			Destroy();
			return;
		}
		progress.Tick();
		puppet.animPhase = Vector2.Distance(pos, end) / (stepSize * 2f);
		pos = root + Vector2.Lerp(start, end, progress.normalized);
		if (puppet.type == PuppetPart.Type.LowerBody)
		{
			float num = Mathf.Abs(Mathf.Sin(puppet.animPhase * (float)Math.PI * 2f));
			pos += Vector2.up * (num * 5f);
		}
		puppet.reverseOffset = 2f * root.x + start.x + end.x;
		puppet.pos = pos;
	}

	public override void Destroy()
	{
		base.Destroy();
		if (puppet != null)
		{
			puppet.Destroy();
			puppet = null;
		}
		dbg?.Destroy();
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (room != null && room.BeingViewed && dbg == null && debug)
		{
			room.AddObject(dbg = new DebugSprite(start, new FSprite("Futile_White")
			{
				color = Color.red,
				scale = 0.5f
			}, room));
		}
		dbg?.sprite.SetPosition(Vector2.Lerp(lastPos, pos, timeStacker) - camPos);
	}
}
