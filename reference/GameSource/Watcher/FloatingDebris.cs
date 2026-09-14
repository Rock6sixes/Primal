using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class FloatingDebris : UpdatableAndDeletable
{
	public class UIText
	{
		public class Text
		{
			public bool hide;

			public string title;

			public Text(string title)
			{
				this.title = title;
			}
		}

		public Text depthFar;

		public Text depthNear;

		public Text amount;

		public Text scaleMin;

		public Text scaleMax;

		public Text movementAmount;

		public Text scatter;

		public Text depthOffset;

		public Text scaleOffset;

		public Text extraSlider1;

		public Text extraSlider2;

		public Text extraSlider3;

		public Text extraSlider4;

		public static Text hide = new Text(null)
		{
			hide = true
		};

		public static Text Title(string title)
		{
			return new Text(title);
		}
	}

	public class FloaterData
	{
		public int index;

		public (int, float) influence;

		public Vector2 offset;

		public float offsetAmount;

		public Vector2 origPos;

		public float scale;

		public float scaleOffset;

		public Vector4 extraOffsets;

		public float depth;

		public float depthOffset;

		public float movement;

		public FloatingDebris owner;

		public FloaterData(FloatingDebris owner, int index, (int, float) influence, Vector2 origPos, Vector2 offset, float offsetAmount, float depth, float depthOffset, float scale, float scaleOffset, Vector4 extraOffsets, float movement)
		{
			this.owner = owner;
			this.index = index;
			this.influence = influence;
			this.origPos = origPos;
			this.offset = offset;
			this.offsetAmount = offsetAmount;
			this.depth = depth;
			this.depthOffset = depthOffset;
			this.scale = scale;
			this.scaleOffset = scaleOffset;
			this.extraOffsets = extraOffsets;
			this.movement = movement;
		}
	}

	public class Floater : UpdatableAndDeletable
	{
		public interface IFloaterSpawner
		{
			UIText GetUIText();

			Floater Spawn(FloaterData data);
		}

		public Vector2 pos;

		public int posInArray;

		public int index;

		public float influence;

		public Vector2 offset;

		public float offsetAmount;

		public Vector2 origPos;

		public Vector2 lastPos;

		public Vector2 preMovementPos;

		public float defaultScale = 10f;

		public float scale;

		public float scaleOffset;

		public Vector4 extraOffsets;

		public float depthOffset;

		public float movement;

		public float time;

		public FloatingDebris owner;

		public float depth
		{
			get
			{
				return Mathf.Clamp01(_depth + depthOffset);
			}
			set
			{
				_depth = value;
			}
		}

		public float finalScale => Mathf.Clamp(defaultScale * (scale - scaleOffset), 0f, 99999f);

		public float _depth { get; set; }

		public Vector2 getPos
		{
			get
			{
				preMovementPos = origPos + offset * offsetAmount;
				float num = Mathf.Sin(time + offset.x * (float)Math.PI * 2f) * offset.y * 10f;
				num += Mathf.Sin(time + origPos.x / 20f) * 10f;
				num *= 1f - depth;
				return preMovementPos + Vector2.up * (num * movement);
			}
		}

		public Floater(FloaterData data)
		{
			owner = data.owner;
			posInArray = data.index;
			(int, float) tuple = data.influence;
			index = tuple.Item1;
			influence = tuple.Item2;
			origPos = data.origPos;
			offset = data.offset;
			offsetAmount = data.offsetAmount;
			depth = data.depth;
			depthOffset = data.depthOffset;
			scale = data.scale;
			scaleOffset = data.scaleOffset;
			extraOffsets = data.extraOffsets;
			pos = getPos;
			lastPos = pos;
			movement = data.movement;
		}

		public override void Update(bool eu)
		{
			if (room.BeingViewed)
			{
				time += Time.fixedDeltaTime * 2f;
			}
			lastPos = pos;
			pos = getPos;
			base.Update(eu);
		}

		public virtual void OnParentChange()
		{
		}
	}

	public class TestSquares : Floater, IDrawable
	{
		public class TestSquaresSpawner : IFloaterSpawner
		{
			public UIText GetUIText()
			{
				return null;
			}

			public Floater Spawn(FloaterData data)
			{
				return new TestSquares(data);
			}
		}

		public TestSquares(FloaterData data)
			: base(data)
		{
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[0]);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
			sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
			sLeaser.sprites[0].scale = base.finalScale;
			sLeaser.sprites[0].color = Color.Lerp(Color.black, Color.white, base.depth);
			sLeaser.sprites[0].alpha = 1f - base.depth;
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["CustomDepth"];
			sLeaser.sprites[0].x = pos.x - rCam.pos.x;
			sLeaser.sprites[0].y = pos.y - rCam.pos.y;
			sLeaser.sprites[0].color = Color.Lerp(Color.white, Color.black, base.depth);
			sLeaser.sprites[0].scale = base.finalScale;
			sLeaser.sprites[0].alpha = 1f - base.depth;
			AddToContainer(sLeaser, rCam, null);
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}
	}

	public class Aurora : Floater, IDrawable
	{
		public class AuroraSpawner : IFloaterSpawner
		{
			public static UIText uiText = new UIText
			{
				depthFar = UIText.Title("Intensity Max"),
				depthNear = UIText.Title("Intensity Min"),
				scaleMax = UIText.hide,
				scaleMin = UIText.Title("Parallax Amount"),
				scaleOffset = UIText.Title("Height Offset"),
				depthOffset = UIText.Title("Hue Offset")
			};

			public UIText GetUIText()
			{
				return uiText;
			}

			public virtual Floater Spawn(FloaterData data)
			{
				if (data.index == 0)
				{
					return new Aurora(data)
					{
						rippleLayer = data.owner.data.rippleLayer
					};
				}
				return new Floater(data);
			}
		}

		public class AuroraForegroundSpawner : AuroraSpawner
		{
			public override Floater Spawn(FloaterData data)
			{
				if (data.index == 0)
				{
					return new Aurora(data)
					{
						foreground = true,
						rippleLayer = data.owner.data.rippleLayer
					};
				}
				return new Floater(data);
			}
		}

		public bool foreground;

		public int rippleLayer;

		public int numberOfPoints;

		public int numberOfFloaters;

		public int oldNumberOfFloaters;

		public int subdivide = 3;

		public bool needRefresh;

		public int step;

		public List<int> NumberOfPointsEachIteration = new List<int>();

		public float cameraOffsetAmount => 1f - scale / 3f;

		public Vector2 GetPos(Floater floater)
		{
			float f = time * 0.25f * Custom.Mod(floater.offset.y * 1000f, 1f) * 2f - 1f;
			float num = Mathf.Sin(time + floater.offset.x * (float)Math.PI * 2f) * floater.offset.y * 10f;
			num += Mathf.Sin(time + floater.origPos.x / 20f) * 10f;
			Vector2 vector = new Vector2(Mathf.Sin(f), Mathf.Cos(f)) * 10f;
			return floater.origPos + floater.offset * new Vector2(floater.offsetAmount, floater.offsetAmount * 0.5f) + (vector + Vector2.up * num) * (floater.movement * 3f);
		}

		public Aurora(FloaterData data)
			: base(data)
		{
			SetNumberOfPoints();
		}

		public override void Update(bool eu)
		{
			oldNumberOfFloaters = numberOfFloaters;
			numberOfFloaters = owner.positions.Count;
			if (numberOfFloaters != oldNumberOfFloaters)
			{
				SetNumberOfPoints();
			}
			base.Update(eu);
		}

		public void SetNumberOfPoints()
		{
			NumberOfPointsEachIteration.Clear();
			if (numberOfFloaters < 3)
			{
				numberOfPoints = 0;
				needRefresh = true;
				return;
			}
			numberOfPoints = numberOfFloaters;
			NumberOfPointsEachIteration.Add(numberOfPoints);
			for (int i = 1; i <= subdivide; i++)
			{
				numberOfPoints += numberOfPoints - 1;
				NumberOfPointsEachIteration.Add(numberOfPoints);
			}
			step = GetStep(0);
			needRefresh = true;
		}

		public int GetStep(int a)
		{
			return (int)Mathf.Pow(2f, subdivide + 1 - a);
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			if (sLeaser.sprites == null)
			{
				sLeaser.sprites = new FSprite[1];
			}
			sLeaser.sprites[0]?.RemoveFromContainer();
			if (numberOfPoints == 0)
			{
				sLeaser.sprites[0] = new FSprite("Futile_White");
				sLeaser.sprites[0].isVisible = true;
				AddToContainer(sLeaser, rCam, null);
				needRefresh = false;
				return;
			}
			TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[(numberOfPoints - 1) * 2];
			for (int i = 0; i < numberOfPoints - 1; i++)
			{
				int num = i * 2;
				array[num] = new TriangleMesh.Triangle(num, num + 1, num + 2);
				array[num + 1] = new TriangleMesh.Triangle(num + 1, num + 2, num + 3);
			}
			TriangleMesh triangleMesh = new TriangleMesh("Futile_White", array, customColor: true);
			sLeaser.sprites[0] = triangleMesh;
			sLeaser.sprites[0].shader = Custom.rainWorld.Shaders[(foreground ? "AuroraForeground" : "Aurora") + ((rippleLayer == 0) ? "" : "Ripple")];
			for (int j = 0; j < triangleMesh.vertices.Length / 2; j++)
			{
				int num2 = j * 2;
				triangleMesh.UVvertices[num2] = Vector2.Lerp(Vector2.zero, Vector2.right, (float)j / (float)(triangleMesh.vertices.Length / 2));
				triangleMesh.UVvertices[num2 + 1] = new Vector2(triangleMesh.UVvertices[num2].x, 1f);
				triangleMesh.verticeColors[num2].r = (float)numberOfFloaters / 100f;
			}
			needRefresh = false;
			AddToContainer(sLeaser, rCam, null);
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			rCam.ReturnFContainer("ForegroundLights").AddChild(sLeaser.sprites[0]);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
				return;
			}
			if (needRefresh)
			{
				InitiateSprites(sLeaser, rCam);
			}
			if (numberOfPoints == 0)
			{
				return;
			}
			TriangleMesh triangleMesh = (TriangleMesh)sLeaser.sprites[0];
			if (owner.floaters.Count < numberOfFloaters)
			{
				return;
			}
			for (int i = 0; i < numberOfFloaters; i++)
			{
				Vector2 vector = GetPos(owner.floaters[i]) - camPos * cameraOffsetAmount;
				int num = i * step;
				triangleMesh.MoveVertice(num, vector);
				triangleMesh.verticeColors[num].g = owner.floaters[i].depthOffset * 0.5f + 0.5f;
				triangleMesh.verticeColors[num].b = owner.floaters[i]._depth;
			}
			for (int j = 0; j < NumberOfPointsEachIteration.Count - 1; j++)
			{
				int num2 = GetStep(j);
				for (int k = 0; k < NumberOfPointsEachIteration[j] - 1; k++)
				{
					int num3 = k * num2;
					Vector2 vector2 = new Vector2(triangleMesh.vertices[num3].x, triangleMesh.vertices[num3].y);
					triangleMesh.MoveVertice(pos: (vector2 + new Vector2(triangleMesh.vertices[num3 + num2].x, triangleMesh.vertices[num3 + num2].y)) * 0.5f, ind: num3 + num2 / 2);
					triangleMesh.verticeColors[num3 + num2 / 2].g = (triangleMesh.verticeColors[num3].g + triangleMesh.verticeColors[num3 + num2].g) * 0.5f;
					triangleMesh.verticeColors[num3 + num2 / 2].b = (triangleMesh.verticeColors[num3].b + triangleMesh.verticeColors[num3 + num2].b) * 0.5f;
				}
				for (int l = 1; l < NumberOfPointsEachIteration[j] - 1; l++)
				{
					int num4 = l * num2;
					int num5 = num2 / 2;
					Vector2 a = new Vector2(triangleMesh.vertices[num4].x, triangleMesh.vertices[num4].y);
					Vector2 vector3 = new Vector2(triangleMesh.vertices[num4 - num5].x, triangleMesh.vertices[num4 - num5].y);
					Vector2 vector4 = new Vector2(triangleMesh.vertices[num4 + num5].x, triangleMesh.vertices[num4 + num5].y);
					Vector2 b = (vector3 + vector4) * 0.5f;
					Vector2 zero = Vector2.zero;
					zero = Vector2.Lerp(a, b, 0.75f);
					triangleMesh.MoveVertice(num4, zero);
					triangleMesh.verticeColors[num4].g = triangleMesh.verticeColors[num4].g * 0.25f + (triangleMesh.verticeColors[num4 - num5].g + triangleMesh.verticeColors[num4 + num5].g) * 0.37f;
					triangleMesh.verticeColors[num4].b = triangleMesh.verticeColors[num4].b * 0.25f + (triangleMesh.verticeColors[num4 - num5].b + triangleMesh.verticeColors[num4 + num5].b) * 0.37f;
				}
			}
			for (int m = 1; m < triangleMesh.vertices.Length / 2 - 1; m++)
			{
				int num6 = m * 2;
				float x = triangleMesh.vertices[num6 - 2].x;
				float x2 = triangleMesh.vertices[num6 + 2].x;
				triangleMesh.verticeColors[num6].a = Mathf.Abs(0f - x + x2) * 0.01f;
			}
			triangleMesh.verticeColors[0].a = 0f;
			triangleMesh.verticeColors[triangleMesh.vertices.Length - 2].a = 0f;
			int num7 = GetStep(0);
			for (int n = 0; n < numberOfFloaters - 1; n++)
			{
				for (int num8 = 0; num8 <= num7 / 2; num8++)
				{
					int num9 = n * num7 + num8 * 2;
					float num10 = Mathf.Lerp(owner.floaters[n].scaleOffset, owner.floaters[n + 1].scaleOffset, (float)num8 * 2f / (float)num7);
					Vector2 vector5 = new Vector2(triangleMesh.vertices[num9].x, triangleMesh.vertices[num9].y);
					triangleMesh.MoveVertice(num9 + 1, Vector2.up * (180f + 180f * num10) + vector5);
					triangleMesh.verticeColors[num9 + 1] = triangleMesh.verticeColors[num9];
				}
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}
	}

	public class BigTube : Floater, INotifyWhenRoomUnloaded
	{
		public class TubeSegment
		{
			public BigTube tube;

			public DynamicLevelElement levelElement;

			public Vector3[] verts;

			public Vector2[] randomness;

			public Color32[] colors;

			public Mesh mesh;

			public bool initialized;

			public bool slatedForDeletion;

			public bool isVisible => levelElement?.isVisible ?? false;

			public TubeSegment(BigTube tube)
			{
				this.tube = tube;
			}

			public void AddDynamicLevelElement()
			{
				levelElement = new DynamicLevelElement(tube.curvePos, Vector2.one, null, 0, mesh, "DynamicLevelTubeSegment")
				{
					dontSetProps = true,
					dontApplyColors = true
				};
				tube.room.AddObject(levelElement);
			}

			public void Init()
			{
				CreateMesh();
				UpdateMesh(ignoreBounds: true);
				initialized = true;
			}

			public void Update()
			{
				if (!slatedForDeletion)
				{
					if (!initialized)
					{
						Init();
					}
					if (levelElement == null)
					{
						AddDynamicLevelElement();
					}
					levelElement.mesh = mesh;
					if (tube.room.BeingViewed)
					{
						UpdateMesh();
						levelElement.pos = tube.curvePos;
					}
				}
			}

			public void UpdateMesh(bool ignoreBounds = false)
			{
				if (isVisible || ignoreBounds)
				{
					float num = 1f - Mathf.Clamp01((float)tube.owner.depthNear / 30f + tube.depthOffset);
					float num2 = tube.finalScale * (0.5f + num * 0.5f);
					Vector2 vector = tube.curveDeriv;
					if (vector.sqrMagnitude == 0f)
					{
						vector = Vector2.up;
					}
					else
					{
						float magnitude = vector.magnitude;
						vector /= magnitude;
						magnitude = Mathf.Min(1f, magnitude / num2);
						vector *= magnitude;
					}
					Vector2 vector2 = Vector2.Perpendicular(vector.normalized);
					verts[0] = (vector + vector2) * (0f - num2);
					verts[1] = (Vector2)verts[0] + num2 * 2f * vector;
					verts[2] = (Vector2)verts[0] + num2 * 2f * vector2;
					verts[3] = (vector + vector2) * num2;
					colors[0] = (colors[1] = (colors[2] = (colors[3] = new Color(num, 1f, 1f, 0f))));
					mesh.vertices = verts;
					mesh.colors32 = colors;
					mesh.RecalculateBounds();
					if (!(levelElement?.element?.meshFilter == null))
					{
						levelElement.element.meshFilter.sharedMesh = mesh;
					}
				}
			}

			public void CreateMesh()
			{
				mesh = new Mesh
				{
					name = "BigTube Mesh"
				};
				mesh.MarkDynamic();
				verts = new Vector3[4];
				colors = new Color32[verts.Length];
				int[] array = new int[6];
				Vector2[] array2 = new Vector2[verts.Length];
				array[0] = 0;
				array[1] = (array[3] = 2);
				array[2] = (array[5] = 1);
				array[4] = 3;
				array2[0] = Vector2.zero;
				array2[1] = array2[0] + Vector2.up;
				array2[2] = Vector2.right;
				array2[3] = array2[2] + Vector2.up;
				mesh.vertices = verts;
				mesh.triangles = array;
				mesh.uv = array2;
				mesh.colors32 = colors;
			}

			public void DisposeMesh()
			{
				if (mesh != null)
				{
					UnityEngine.Object.Destroy(mesh);
					mesh = null;
				}
			}

			public void Destroy()
			{
				levelElement?.Destroy();
				levelElement = null;
				slatedForDeletion = true;
				DisposeMesh();
			}
		}

		public class BigTubeSpawner : IFloaterSpawner
		{
			public static UIText uiText = new UIText
			{
				depthFar = UIText.hide,
				depthNear = UIText.Title("Depth"),
				depthOffset = UIText.Title("Depth Offset"),
				scaleMax = UIText.hide,
				scaleMin = UIText.Title("Radius"),
				scaleOffset = UIText.Title("Radius Offset"),
				scatter = UIText.hide,
				movementAmount = UIText.Title("Relaxation")
			};

			public UIText GetUIText()
			{
				return uiText;
			}

			public virtual Floater Spawn(FloaterData data)
			{
				return new BigTube(data);
			}
		}

		public TubeSegment tubeSegment;

		public bool initializedGraphics;

		public Vector2 curvePos;

		public Vector2 curveDeriv;

		public new float finalScale => Mathf.Clamp(defaultScale * (owner.scaleMinimum - scaleOffset), 0f, 99999f);

		public BigTube(FloaterData data)
			: base(data)
		{
			curvePos = getPos();
			curveDeriv = getDeriv();
		}

		public override void Destroy()
		{
			base.Destroy();
			tubeSegment?.Destroy();
		}

		public void RoomUnloaded()
		{
			Destroy();
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			curvePos = getPos();
			curveDeriv = getDeriv();
			if (!initializedGraphics)
			{
				tubeSegment = new TubeSegment(this);
				initializedGraphics = true;
			}
			tubeSegment.Update();
		}

		public new Vector2 getPos()
		{
			return catmullRomPosition(owner.data.ConPos(index - 1), owner.data.ConPos(index), owner.data.ConPos(index + 1), owner.data.ConPos(index + 2), influence) + owner.pos;
		}

		public Vector2 getDeriv()
		{
			float num = (float)owner.data.controlPointPosX.Count / (float)owner.floaters.Count;
			Vector2 vector = getPos();
			return catmullRomPosition(owner.data.ConPos(index - 1), owner.data.ConPos(index), owner.data.ConPos(index + 1), owner.data.ConPos(index + 2), influence + num) + owner.pos - vector;
		}

		public Vector2 catmullRomPosition(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
		{
			Vector2 vector = (c - a) * (0.5f * owner.data.movement) + b;
			Vector2 vector2 = (b - d) * (0.5f * owner.data.movement) + c;
			float num = t * t;
			float num2 = t * t * t;
			return b * (-1f * num2 + 3f * num + -3f * t + 1f) + vector * (3f * num2 + -6f * num + 3f * t) + vector2 * (-3f * num2 + 3f * num) + c * num2;
		}

		public Vector2 catmullRomDerivative(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
		{
			Vector2 vector = (c - a) * (0.5f * owner.data.movement) + b;
			Vector2 vector2 = (b - d) * (0.5f * owner.data.movement) + c;
			float num = t * t;
			return b * (-3f * num + 6f * t - 3f) + vector * (9f * num + -12f * t + 3f) + vector2 * (-9f * num + 6f * t) + c * (3f * num);
		}
	}

	public class GildedWind : Floater, IDrawable
	{
		public class GildedWindSpawner : IFloaterSpawner
		{
			public static UIText uiText = new UIText
			{
				depthFar = UIText.Title("Depth Far"),
				depthNear = UIText.Title("Depth Near"),
				scaleMax = UIText.Title("Scale Max"),
				scaleMin = UIText.Title("Scale Min"),
				scaleOffset = UIText.Title("Scale Offset"),
				depthOffset = UIText.Title("Depth Offset"),
				extraSlider1 = UIText.Title("Intensity"),
				extraSlider2 = UIText.Title("Wind Speed")
			};

			public UIText GetUIText()
			{
				return uiText;
			}

			public virtual Floater Spawn(FloaterData data)
			{
				return new GildedWind(data);
			}
		}

		public float rotation;

		public float lastRotation;

		public int step;

		public int totalFloaters;

		public int totalConPoints;

		public bool debug;

		public Vector2 splinePos;

		public Vector2 lastOrigPos;

		public DebugSprite debugPosSprite;

		public DebugSprite debugTargetSprite;

		public DebugSprite debugLineSprite;

		public Vector2 random;

		public float shaderTime;

		public float speed;

		public float intensity;

		public Vector2 smoothPos;

		public new Vector2 getPos
		{
			get
			{
				preMovementPos = origPos + offset * offsetAmount;
				float num = Mathf.Sin(time + offset.x * (float)Math.PI * 2f) * offset.y * 10f;
				num += Mathf.Sin(time + origPos.x / 20f) * 10f;
				num *= 1f - base.depth;
				return preMovementPos + Vector2.up * (num * movement);
			}
		}

		public GildedWind(FloaterData data)
			: base(data)
		{
			random = new Vector2(UnityEngine.Random.value, UnityEngine.Random.value);
		}

		public override void Update(bool eu)
		{
			InitDebug();
			UpdateCounts();
			UpdateRotation();
			intensity = Mathf.Clamp01((extraOffsets.x + 1f) * 0.5f * (1f + random.y * 0.3f));
			speed = Mathf.Clamp01((extraOffsets.y + 1f) * 0.5f);
			lastOrigPos = origPos;
			if (room.BeingViewed)
			{
				time += Time.fixedDeltaTime * 2f * Mathf.Clamp01(Mathf.Abs(speed * 2f - 1f) * 3f);
			}
			lastPos = pos;
			pos = getPos;
		}

		public void UpdateCounts()
		{
			step = Mathf.Max(totalFloaters / Mathf.Max(totalConPoints, 1), 2);
			totalFloaters = owner.floaters.Count;
			totalConPoints = owner.data.controlPointPosX.Count;
		}

		public override void Destroy()
		{
			base.Destroy();
			if (debug)
			{
				debugTargetSprite.Destroy();
				debugPosSprite.Destroy();
				debugLineSprite.Destroy();
			}
		}

		public void InitDebug()
		{
			if (debug && debugPosSprite == null)
			{
				debugPosSprite = new DebugSprite(Vector2.zero, new FSprite("Futile_White")
				{
					color = Color.red
				}, room);
				debugTargetSprite = new DebugSprite(Vector2.zero, new FSprite("Futile_White")
				{
					color = Color.green
				}, room);
				debugLineSprite = new DebugSprite(Vector2.zero, new FSprite("Futile_White")
				{
					color = Color.blue,
					anchorY = 0f,
					scaleX = 0.12f
				}, room);
				room.AddObject(debugPosSprite);
				room.AddObject(debugTargetSprite);
				room.AddObject(debugLineSprite);
			}
		}

		public void UpdateRotation()
		{
			lastRotation = rotation;
			Vector2 vector = offset;
			rotation = GetAngle() + 90f;
			if (debug)
			{
				debugPosSprite.pos = preMovementPos;
				debugTargetSprite.pos = vector;
				debugLineSprite.pos = preMovementPos;
				debugLineSprite.sprite.rotation = rotation;
				debugLineSprite.sprite.scaleY = (vector - pos).magnitude;
			}
		}

		public Vector2 ConPos(int i)
		{
			return owner.data.ConPos(i);
		}

		public float GetAngle()
		{
			if (totalConPoints <= 1)
			{
				return Custom.VecToDeg(offset);
			}
			float a = Custom.VecToDeg(Vector2.Perpendicular(ConPos(index) - ConPos(index - 1)));
			float num = Custom.VecToDeg(Vector2.Perpendicular(ConPos(index + 1) - ConPos(index)));
			if (index == 0 && influence <= 0.5f)
			{
				return num;
			}
			if (index == totalConPoints - 2 && influence > 0.5f)
			{
				return num;
			}
			float b = Custom.VecToDeg(Vector2.Perpendicular(ConPos(index + 2) - ConPos(index + 1)));
			if (influence > 0.5f)
			{
				return Mathf.LerpAngle(num, b, influence - 0.5f);
			}
			return Mathf.LerpAngle(a, num, influence + 0.5f);
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["GildedWind"];
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].SetPosition(Vector2.Lerp(lastPos, pos, timeStacker) - camPos);
			sLeaser.sprites[0].rotation = Mathf.LerpAngle(lastRotation, rotation, timeStacker);
			sLeaser.sprites[0].color = new Color(random.x, Mathf.Clamp01(base._depth + depthOffset * 0.5f), speed, intensity);
			sLeaser.sprites[0].scaleX = 2f * base.finalScale;
			sLeaser.sprites[0].scaleY = 4f * base.finalScale;
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[0]);
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}
	}

	public class Ripple : Floater
	{
		public class RippleSpawner : IFloaterSpawner
		{
			public static UIText uiText = new UIText
			{
				depthFar = UIText.Title("Intensity Max"),
				depthNear = UIText.Title("Intensity Min"),
				depthOffset = UIText.Title("Intensity Offset")
			};

			public UIText GetUIText()
			{
				return uiText;
			}

			public virtual Floater Spawn(FloaterData data)
			{
				return new Ripple(data);
			}
		}

		public class WatcherRippleSpawner : RippleSpawner
		{
			public override Floater Spawn(FloaterData data)
			{
				return new Ripple(data)
				{
					isGameplay = true
				};
			}
		}

		public class WavyRippleSpawner : RippleSpawner
		{
			public override Floater Spawn(FloaterData data)
			{
				return new Ripple(data)
				{
					maskShader = "WavesShiftMask"
				};
			}
		}

		public class WavyWatcherRippleSpawner : RippleSpawner
		{
			public override Floater Spawn(FloaterData data)
			{
				return new Ripple(data)
				{
					isGameplay = true,
					maskShader = "WavesShiftMask"
				};
			}
		}

		public CosmeticRipple ripple;

		public CosmeticRippleData data;

		public float oldScale;

		public float oldScaleOffset;

		public bool isGameplay;

		public string maskShader;

		public new float finalScale => Mathf.Clamp(defaultScale * (scale + scaleOffset), 0f, 99999f);

		public new Vector2 getPos
		{
			get
			{
				float num = Mathf.Sin(Custom.rainWorld.processManager.shadersTime + offset.x * (float)Math.PI * 2f) * offset.y * 10f;
				num += Mathf.Sin(Custom.rainWorld.processManager.shadersTime + origPos.x / 20f) * 10f;
				return origPos + offset * offsetAmount + offset * (num * movement);
			}
		}

		public Ripple(FloaterData data)
			: base(data)
		{
			defaultScale = 100f;
		}

		public override void Destroy()
		{
			ripple.RemoveObject();
			ripple.Destroy();
			base.Destroy();
		}

		public void Init()
		{
			PlacedObject placedObject = new PlacedObject(PlacedObject.Type.CosmeticRipple, null);
			data = new CosmeticRippleData(placedObject);
			data.fallOff = 0.95f;
			data.pos = getPos;
			data.intensity = base.depth;
			data.scale = finalScale;
			data.leavesTrail = false;
			data.isGameplay = isGameplay;
			placedObject.data = data;
			ripple = new CosmeticRipple(placedObject, maskShader);
			room.ripple = true;
			room.AddObject(ripple);
		}

		public override void Update(bool eu)
		{
			if (room != null && ripple == null)
			{
				Init();
			}
			base.Update(eu);
			ripple.pos = getPos;
			ripple.intensity = base.depth;
			ripple.scale = finalScale;
			oldScale = scale;
			oldScaleOffset = scaleOffset;
		}
	}

	public class RippleFlow : Floater, IDrawable, INotifyWhenRoomUnloaded
	{
		public class RippleFlowSpawner : IFloaterSpawner
		{
			public static UIText uiText = new UIText
			{
				depthFar = UIText.Title("Intensity Max"),
				depthNear = UIText.Title("Intensity Min"),
				depthOffset = UIText.Title("Flow Speed"),
				scatter = UIText.Title("Random Offset"),
				scaleOffset = UIText.Title("Depth Offset")
			};

			public UIText GetUIText()
			{
				return uiText;
			}

			public virtual Floater Spawn(FloaterData data)
			{
				return new RippleFlow(data);
			}
		}

		public class WatcherRippleFlowSpawner : RippleFlowSpawner
		{
			public override Floater Spawn(FloaterData data)
			{
				return new RippleFlow(data)
				{
					watcherRipple = true
				};
			}
		}

		public MaskSource rippleFlow;

		public string maskShader;

		public bool watcherRipple;

		public int n = 3;

		public int w = 4;

		public int size;

		public bool isDebug;

		public Vector3[] verts;

		public Vector2[] uv;

		public Color32[] colors;

		public Mesh mesh;

		public float[] randomness;

		public bool initialize = true;

		public new float finalScale => Mathf.Clamp(defaultScale * scale, 0f, 99999f);

		public bool isVisible => rippleFlow?.isVisible ?? false;

		public bool slowUpdateFrame => Time.frameCount % 10 == posInArray % 10;

		public new Vector2 getPos
		{
			get
			{
				float num = 1f;
				preMovementPos = origPos + offset * offsetAmount;
				if (posInArray > 1 && posInArray < owner.floaters.Count - 2)
				{
					Vector2 vector = owner.floaters[posInArray - 2].preMovementPos;
					Vector2 vector2 = owner.floaters[posInArray - 1].preMovementPos;
					Vector2 vector3 = preMovementPos;
					Vector2 vector4 = owner.floaters[posInArray + 1].preMovementPos;
					Vector2 vector5 = owner.floaters[posInArray + 2].preMovementPos;
					float magnitude = (vector - vector2).magnitude;
					float magnitude2 = (vector2 - vector3).magnitude;
					float magnitude3 = (vector3 - vector4).magnitude;
					num = Mathf.Min(b: (vector4 - vector5).magnitude, a: Mathf.Min(Mathf.Min(magnitude, magnitude2), magnitude3)) * 0.45f;
				}
				float num2 = time + randomness[0] * 33f;
				num2 = num2 * (randomness[3] * 2f - 1f) + randomness[2];
				Vector2 vector6 = new Vector2(Mathf.Sin(num2), Mathf.Cos(num2)) * num;
				return preMovementPos + vector6 * movement;
			}
		}

		public RippleFlow(FloaterData data)
			: base(data)
		{
			defaultScale = 20f;
		}

		public override void Destroy()
		{
			rippleFlow?.DeleteMe();
			base.Destroy();
			DisposeMesh();
		}

		public void GenerateRandomness()
		{
			UnityEngine.Random.State state = UnityEngine.Random.state;
			randomness = new float[4];
			UnityEngine.Random.InitState((int)(owner.offsets[posInArray].x * 1000f));
			for (int i = 0; i < randomness.Length; i++)
			{
				randomness[i] = UnityEngine.Random.value;
			}
			UnityEngine.Random.state = state;
		}

		public void Init()
		{
			GenerateRandomness();
			CreateMesh();
			UpdateMesh(ignoreBounds: true);
			initialize = false;
		}

		public void CreateMaskObject()
		{
			room.ripple = true;
			rippleFlow = MaskMaker.MakeSource(watcherRipple ? "GameplayRippleGrab" : "RippleGrab", "RippleFlow", isSecondLayer: false, mesh, pos);
			UpdateMesh(ignoreBounds: true);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (!owner.initialized)
			{
				return;
			}
			if (posInArray >= owner.floaters.Count - 2 || posInArray <= 1)
			{
				if (rippleFlow != null)
				{
					rippleFlow.DeleteMe();
					rippleFlow = null;
				}
				return;
			}
			if (initialize)
			{
				Init();
			}
			pos = getPos;
			if (!room.BeingViewed)
			{
				return;
			}
			MaskSource maskSource = rippleFlow;
			if (maskSource == null || maskSource.beingDeleted)
			{
				if (!watcherRipple || !room.game.cameras[0].rippleData.gameplayRippleActive || (room.game.cameras[0].rippleData.transitionRippleSpawned && !(room.game.cameras[0].rippleData.gameplayRippleAnimation < 0.1f)))
				{
					CreateMaskObject();
				}
			}
			else
			{
				UpdateMesh();
				rippleFlow.SetPos(pos);
			}
		}

		public void UpdateMesh(bool ignoreBounds = false)
		{
			if (base.slatedForDeletetion || (!isVisible && !ignoreBounds && !slowUpdateFrame))
			{
				return;
			}
			RippleFlow rippleFlow = (RippleFlow)owner.floaters[Mathf.Max(posInArray - 2, 0)];
			RippleFlow rippleFlow2 = (RippleFlow)owner.floaters[posInArray - 1];
			RippleFlow rippleFlow3 = (RippleFlow)owner.floaters[posInArray + 1];
			RippleFlow rippleFlow4 = (RippleFlow)owner.floaters[Mathf.Min(posInArray + 2, owner.floaters.Count - 1)];
			Vector2 zero = Vector2.zero;
			Vector2 vector = rippleFlow.pos - pos;
			Vector2 vector2 = rippleFlow2.pos - pos;
			Vector2 vector3 = rippleFlow3.pos - pos;
			Vector2 vector4 = rippleFlow4.pos - pos;
			vector /= 16f;
			vector2 /= 16f;
			vector3 /= 16f;
			vector4 /= 16f;
			int num = 3;
			Vector2 vector5 = Custom.PerpendicularVector(vector2 - vector);
			Vector2 vector6 = Custom.PerpendicularVector(-vector2);
			Vector2 vector7 = Custom.PerpendicularVector(vector3);
			Vector2 vector8 = Custom.PerpendicularVector(vector4 - vector3);
			float a = Angle(vector2 - vector, vector2);
			float b = Angle(vector2, vector3);
			float num2 = Angle(vector3 - vector4, vector3);
			float num3 = Angle(vector, vector4);
			Smin2((vector2 - vector).magnitude, vector.magnitude * 0.5f, rippleFlow.finalScale);
			Smin2(a, num3);
			float value = Smin2((vector2 - vector).magnitude, vector.magnitude * 0.5f, vector2.magnitude, rippleFlow2.finalScale) * Smin2(a, b, num3);
			float value2 = Smin2(vector2.magnitude, finalScale) * Smin2(a, b, num2, num3);
			float value3 = Smin2((vector4 - vector3).magnitude, vector4.magnitude * 0.5f, vector3.magnitude, rippleFlow3.finalScale) * Smin2(num2, b, num3);
			Smin2((vector4 - vector3).magnitude, vector4.magnitude * 0.5f, rippleFlow4.finalScale);
			Smin2(num2, num3);
			float amount = 1f;
			float num4 = 0f;
			Vector2 vector9 = vector5 * Smooth(value, amount, num4);
			Vector2 vector10 = Half(vector5, vector6) * Smooth(value, amount, num4);
			Vector2 vector11 = Half(vector6, vector7) * Smooth(value2, amount, num4);
			Vector2 vector12 = Half(vector7, vector8) * Smooth(value3, amount, num4);
			Vector2 vector13 = vector8 * Smooth(value3, amount, num4);
			int num5 = (int)Mathf.Pow(2f, n) * w;
			int num6 = 0;
			verts[0] = vector - vector9;
			verts[w - 1] = vector + vector9;
			num6 += num5;
			verts[num6] = vector2 - vector10;
			verts[num6 + w - 1] = vector2 + vector10;
			num6 += num5;
			verts[num6] = zero - vector11;
			verts[num6 + w - 1] = zero + vector11;
			num6 += num5;
			verts[num6] = vector3 - vector12;
			verts[num6 + w - 1] = vector3 + vector12;
			verts[size * w - w] = vector4 - vector13;
			verts[size * w - 1] = vector4 + vector13;
			for (int num7 = n; num7 > 0; num7--)
			{
				int num8 = Mathf.Max((int)Mathf.Pow(2f, num7 - 1), 1);
				int num9 = (int)Mathf.Pow(2f, num7);
				for (int i = 1; i <= num; i++)
				{
					int num10 = i * num9;
					int num11 = num10 - num8;
					int num12 = Mathf.Min(num10 + num9, size - 1);
					num11 *= w;
					num10 *= w;
					num12 *= w;
					verts[num11] = verts[num10];
					verts[num10] = Vector3.Lerp(verts[num11], verts[num12], 0.25f);
					verts[num11 + w - 1] = verts[num10 + w - 1];
					verts[num10 + w - 1] = Vector3.Lerp(verts[num11 + w - 1], verts[num12 + w - 1], 0.25f);
				}
				for (int num13 = num; num13 > 0; num13--)
				{
					int num14 = num13 * num9 - num8;
					int num15 = Mathf.Max(num14 - num9, 0);
					num15 *= w;
					num14 *= w;
					verts[num14] = Vector3.Lerp(verts[num14], verts[num15], 0.25f);
					verts[num14 + w - 1] = Vector3.Lerp(verts[num14 + w - 1], verts[num15 + w - 1], 0.25f);
				}
				num *= 2;
			}
			for (int j = 0; j < size; j++)
			{
				int num16 = j * w;
				for (int k = 1; k < w - 1; k++)
				{
					float t = (float)k / (float)(w - 1);
					verts[num16 + k] = Vector3.Lerp(verts[num16], verts[num16 + w - 1], t);
				}
			}
			for (int l = 0; l < colors.Length; l++)
			{
				colors[l] = new Color(depthOffset * 0.5f + 0.5f + offset.y * 0.05f, base._depth, scaleOffset * 0.5f + 0.5f);
			}
			mesh.vertices = verts;
			mesh.colors32 = colors;
			mesh.RecalculateBounds();
			if (this.rippleFlow != null)
			{
				this.rippleFlow.meshFilter.sharedMesh = mesh;
			}
		}

		public void DisposeMesh()
		{
			if (mesh != null)
			{
				UnityEngine.Object.Destroy(mesh);
				mesh = null;
			}
		}

		public void RoomUnloaded()
		{
			Destroy();
		}

		public void CreateMesh()
		{
			size = 3 * (int)Mathf.Pow(2f, n) + 2;
			DisposeMesh();
			mesh = new Mesh
			{
				name = "RippleFlow Mesh"
			};
			mesh.MarkDynamic();
			verts = new Vector3[size * w];
			colors = new Color32[verts.Length];
			for (int i = 0; i < size; i++)
			{
				int num = i * w;
				verts[num] = Vector3.right * (i * 3) + Vector3.down * 40f;
				verts[num + w - 1] = verts[num] + Vector3.up;
				for (int j = 1; j < w - 1; j++)
				{
					verts[num + j] = Vector3.Lerp(verts[num], verts[num + w - 1], (float)j / (float)(w - 1));
				}
			}
			mesh.vertices = verts;
			int num2 = (size - 1) * (w - 1);
			int[] array = new int[num2 * 6];
			for (int k = 0; k < num2; k++)
			{
				int num3 = k * 6;
				int num4 = (array[num3] = k % (w - 1) + k / (w - 1) * w);
				array[num3 + 1] = (array[num3 + 3] = num4 + w);
				array[num3 + 2] = (array[num3 + 5] = num4 + 1);
				array[num3 + 4] = num4 + w + 1;
			}
			mesh.triangles = array;
			uv = new Vector2[size * w];
			for (int l = 0; l < size; l++)
			{
				int num5 = l * w;
				float num6 = (float)l / (float)(size - 1);
				uv[num5] = Vector2.right * num6;
				uv[num5 + w - 1] = uv[num5] + Vector2.up;
				for (int m = 1; m < w - 1; m++)
				{
					float num7 = (float)m / (float)(w - 1);
					uv[num5 + m] = uv[num5] + Vector2.up * num7;
				}
			}
			mesh.uv = uv;
		}

		public float Angle(Vector2 a, Vector2 b)
		{
			return Mathf.Clamp01(Vector2.Angle(a.normalized, b.normalized) / 90f);
		}

		public Vector2 Half(Vector2 a, Vector2 b)
		{
			return ((a + b) * 0.5f).normalized;
		}

		public float Smin2(float a, float b)
		{
			return Smin(a, b, 0.5f);
		}

		public float Smin2(float a, float b, float c)
		{
			float a2 = Smin2(a, b);
			return Smin2(a2, c);
		}

		public float Smin2(float a, float b, float c, float d)
		{
			float a2 = Smin2(a, b, c);
			return Smin2(a2, d);
		}

		public float Smin(float a, float b, float v)
		{
			float num = Mathf.Clamp01(0.5f + 0.5f * (a - b) / v);
			return Mathf.Lerp(a, b, num) - v * num * (1f - num);
		}

		public float Smooth(float value, float amount, float offset)
		{
			amount = Mathf.Max(amount, 0f);
			if (value > amount)
			{
				return value + offset;
			}
			if (value < 0f - amount)
			{
				return offset;
			}
			return ((value / amount + 2f) * value + amount) * 0.25f + offset;
		}

		public float Smooth2(float value, float amount, float offset, float multiplier)
		{
			amount *= multiplier;
			amount = Mathf.Max(amount, 0f);
			if (value > amount * 2f)
			{
				return value - amount + offset;
			}
			if (value < 0f)
			{
				return offset;
			}
			return 0.25f * value * value / amount + offset;
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].isVisible = isDebug;
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			rippleFlow?.DrawUpdate(timeStacker, rCam, camPos);
			sLeaser.sprites[0].x = pos.x - camPos.x;
			sLeaser.sprites[0].y = pos.y - camPos.y;
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[0]);
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}
	}

	public class Rock : Floater
	{
		public class RockSpawner : IFloaterSpawner
		{
			public UIText GetUIText()
			{
				return null;
			}

			public Floater Spawn(FloaterData data)
			{
				return new Rock(data);
			}
		}

		public DynamicLevelElement rockGraphics;

		public float oldMovement;

		public bool movementChange;

		public Vector4 randomness;

		public float rotationSpeed = 20f;

		public float rotation => randomness.w * 360f + time * rotationSpeed * randomness.w * movement;

		public new Vector2 finalScale => base.finalScale * new Vector2(1f + randomness.x * 0.25f, 1f + randomness.y * 0.25f);

		public Rock(FloaterData data)
			: base(data)
		{
			defaultScale = 8f;
			oldMovement = movement;
			randomness = new Vector4(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value * 2f - 1f);
		}

		public void GenerateRandomness()
		{
			_ = UnityEngine.Random.state;
			UnityEngine.Random.InitState((int)(offset.x * 1000f));
		}

		public void Init()
		{
			GenerateRandomness();
			rockGraphics = new DynamicLevelElement(base.getPos, finalScale, null, 0, null, (movement == 0f) ? "DynamicLevelRock_NoMovement" : "DynamicLevelRock")
			{
				rotation = rotation,
				dontSetProps = true
			};
			room.AddObject(rockGraphics);
		}

		public override void Destroy()
		{
			base.Destroy();
			rockGraphics.Destroy();
		}

		public override void Update(bool eu)
		{
			if (room != null && rockGraphics == null)
			{
				Init();
			}
			base.Update(eu);
			if (room.BeingViewed)
			{
				rockGraphics.element?.SetProperty(0, 1f - base.depth);
				rockGraphics.element?.SetProperty(1, randomness.x);
				rockGraphics.element?.SetProperty(2, randomness.y);
				rockGraphics.element?.SetProperty(3, randomness.z);
				rockGraphics.pos = pos;
				rockGraphics.rotation = rotation;
				rockGraphics.scale = finalScale;
				if (oldMovement != movement && (oldMovement == 0f || movement == 0f) && rockGraphics != null)
				{
					rockGraphics.Destroy();
					rockGraphics = null;
				}
				oldMovement = movement;
			}
		}
	}

	public class RotFloater : Floater, INotifyWhenRoomUnloaded
	{
		public class Blob
		{
			public RotFloater owner;

			public DynamicLevelElement blob;

			public Vector3[] verts;

			public Vector2[] randomness;

			public Color32[] colors;

			public Mesh mesh;

			public bool initialize = true;

			public int numberOfBlobs;

			public float[] scaleVariation;

			public float blobScale = 3f;

			public bool slatedForDeletion;

			public bool isVisible => blob?.isVisible ?? false;

			public Blob(RotFloater owner)
			{
				numberOfBlobs = owner.blobOffsets.Length;
				this.owner = owner;
			}

			public void GenerateRandomness()
			{
				randomness = new Vector2[numberOfBlobs];
				scaleVariation = new float[numberOfBlobs];
				UnityEngine.Random.State state = UnityEngine.Random.state;
				UnityEngine.Random.InitState((int)(owner.offset.x * 1000f));
				for (int i = 0; i < numberOfBlobs; i++)
				{
					randomness[i] = new Vector2(UnityEngine.Random.value, UnityEngine.Random.value);
					scaleVariation[i] = 0.75f + UnityEngine.Random.value * 0.25f;
				}
				UnityEngine.Random.state = state;
			}

			public void AddDynamicLevelElement()
			{
				blob = new DynamicLevelElement(owner.pos, Vector2.one, null, 0, mesh, "DynamicLevelBlob")
				{
					dontSetProps = true,
					dontApplyColors = true
				};
				owner.room.AddObject(blob);
			}

			public void Init()
			{
				GenerateRandomness();
				CreateMesh();
				UpdateBlobPositions();
				UpdateMesh(ignoreBounds: true);
				initialize = false;
			}

			public void Update()
			{
				if (!slatedForDeletion)
				{
					if (initialize)
					{
						Init();
					}
					if (blob == null)
					{
						AddDynamicLevelElement();
					}
					blob.mesh = mesh;
					UpdateBlobPositions();
					if (owner.room.BeingViewed)
					{
						UpdateMesh();
						blob.pos = owner.pos;
					}
				}
			}

			public void UpdateBlobPositions()
			{
				for (int i = 0; i < numberOfBlobs; i++)
				{
					float num = (Mathf.Sin(owner.time + randomness[i].x * 11f) * 0.5f + 0.5f) * 30f * (randomness[i].y * 0.5f + 0.5f) * owner.movement;
					owner.blobDepths[i] = Mathf.Clamp01(1f - Mathf.Lerp(owner.owner.depthNear, owner.owner.depthFar, owner.blobRandomDepthOffsets[i]) / 30f + owner.depthOffset);
					owner.finalBlobPositions[i] = owner.blobOffsets[i] * (num + owner._finalScale.x * (0.75f + owner.blobDepths[i] * 0.25f)) / 16f;
				}
			}

			public void UpdateMesh(bool ignoreBounds = false)
			{
				if (isVisible || ignoreBounds || owner.slowUpdateFrame)
				{
					for (int i = 0; i < numberOfBlobs; i++)
					{
						float num = owner.blobDepths[i];
						Vector2 vector = owner.finalBlobPositions[i];
						float num2 = blobScale * scaleVariation[i] * (0.5f + num * 0.5f);
						int num3 = i * 4;
						verts[num3] = vector - Vector2.one * num2;
						verts[num3 + 3] = vector + Vector2.one * num2;
						verts[num3 + 1] = verts[num3] + Vector3.up * (num2 * 2f);
						verts[num3 + 2] = verts[num3] + Vector3.right * (num2 * 2f);
						colors[num3] = (colors[num3 + 1] = (colors[num3 + 2] = (colors[num3 + 3] = new Color(owner.blobDepths[i], randomness[i].x, randomness[i].y, 0f))));
					}
					mesh.vertices = verts;
					mesh.colors32 = colors;
					mesh.RecalculateBounds();
					if (!(blob?.element?.meshFilter == null))
					{
						blob.element.meshFilter.sharedMesh = mesh;
					}
				}
			}

			public void CreateMesh()
			{
				mesh = new Mesh
				{
					name = "RotFloater Blob Mesh"
				};
				mesh.MarkDynamic();
				verts = new Vector3[numberOfBlobs * 4];
				int[] array = new int[6 * numberOfBlobs];
				colors = new Color32[verts.Length];
				Vector2[] array2 = new Vector2[verts.Length];
				for (int i = 0; i < numberOfBlobs; i++)
				{
					int num = i * 6;
					int num2 = (array[num] = i * 4);
					array[num + 1] = (array[num + 3] = num2 + 2);
					array[num + 2] = (array[num + 5] = num2 + 1);
					array[num + 4] = num2 + 3;
					array2[num2] = Vector2.zero;
					array2[num2 + 1] = array2[num2] + Vector2.up;
					array2[num2 + 2] = Vector2.right;
					array2[num2 + 3] = array2[num2 + 2] + Vector2.up;
				}
				mesh.vertices = verts;
				mesh.triangles = array;
				mesh.uv = array2;
				mesh.colors32 = colors;
			}

			public void DisposeMesh()
			{
				if (mesh != null)
				{
					UnityEngine.Object.Destroy(mesh);
					mesh = null;
				}
			}

			public void Destroy()
			{
				slatedForDeletion = true;
				blob?.Destroy();
				blob = null;
				DisposeMesh();
			}
		}

		public class Wire
		{
			public int wireSegments = 13;

			public float wireWidth = 1f;

			public float wireLength = 15f;

			public float wireC = 0.333f;

			public float[] lengths;

			public float[] widths;

			public float[] randomness;

			public int numberOfWires;

			public DynamicLevelElement wire;

			public RotFloater owner;

			public Vector2 end;

			public Vector2 start;

			public Vector3[] verts;

			public Color32[] colors;

			public Mesh mesh;

			public bool initialize = true;

			public bool slatedForDeletion;

			public bool isVisible => wire?.isVisible ?? false;

			public Wire(RotFloater owner)
			{
				numberOfWires = owner.smallWiresConnections.Length - 1;
				this.owner = owner;
			}

			public void GenerateRandomness()
			{
				UnityEngine.Random.State state = UnityEngine.Random.state;
				lengths = new float[numberOfWires];
				widths = new float[numberOfWires];
				randomness = new float[numberOfWires];
				UnityEngine.Random.InitState((int)(owner.offset.x * 1000f));
				for (int i = 0; i < numberOfWires; i++)
				{
					lengths[i] = 1f + UnityEngine.Random.value * 1f;
					widths[i] = UnityEngine.Random.value;
					randomness[i] = UnityEngine.Random.value;
				}
				UnityEngine.Random.state = state;
			}

			public void AddDynamicLevelElement()
			{
				wire = new DynamicLevelElement(owner.pos, Vector3.one * 1f, null, 0, mesh, "DynamicLevelWire")
				{
					dontSetProps = true,
					dontApplyColors = true
				};
				owner.room.AddObject(wire);
			}

			public void Init()
			{
				GenerateRandomness();
				CreateMesh();
				UpdateMesh(ignoreBounds: true);
				initialize = false;
			}

			public void Update()
			{
				if (!slatedForDeletion)
				{
					if (initialize)
					{
						Init();
					}
					if (wire == null)
					{
						AddDynamicLevelElement();
					}
					wire.mesh = mesh;
					if (owner.room.BeingViewed)
					{
						UpdateMesh();
						wire.pos = owner.pos;
					}
				}
			}

			public Vector2 Spline((Vector2, Vector2, Vector2, Vector2) abcd, float t)
			{
				(Vector2, Vector2, Vector2, Vector2) tuple = abcd;
				Vector2 item = tuple.Item1;
				Vector2 item2 = tuple.Item2;
				Vector2 item3 = tuple.Item3;
				Vector2 item4 = tuple.Item4;
				float num = t * t;
				float num2 = num * t;
				return item + item2 * t + item3 * num + item4 * num2;
			}

			public (Vector2, Vector2, Vector2, Vector2) Spline(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
			{
				return (a, 3f * (b - a), 3f * (c - 2f * b + a), d - 3f * c + 3f * b - a);
			}

			public void UpdateMesh(bool ignoreBounds = false)
			{
				if (!isVisible && !ignoreBounds && !owner.slowUpdateFrame)
				{
					return;
				}
				for (int i = 0; i < numberOfWires; i++)
				{
					int num = owner.smallWiresConnections[i];
					int num2 = owner.smallWiresConnections[i + 1];
					float num3 = owner.blobDepths[num];
					float num4 = owner.blobDepths[num2];
					start = owner.finalBlobPositions[num];
					end = owner.finalBlobPositions[num2];
					Vector2 vector = owner.finalBlobPositions[num];
					Vector2 vector2 = owner.finalBlobPositions[num2];
					Vector2 vector3 = vector2 - vector;
					float num5 = Mathf.Sign((owner.blobOffsets[num2] - owner.blobOffsets[num]).x);
					float g = ((num5 > 0f) ? (num3 - num4) : (num4 - num3)) * 0.5f + 0.5f;
					float num6 = Mathf.Max(0.001f, wireLength * lengths[i] * ((num3 + num4) * 0.125f + 0.75f) - vector3.magnitude);
					float num7 = 1f - Mathf.Max(vector3.x, 0f) / wireLength * lengths[i];
					Vector2 vector4 = Vector2.one - new Vector2(Mathf.Abs(Mathf.Min(vector3.y, 0f)), Mathf.Max(vector3.y, 0f)) / wireLength * (lengths[i] * 0.75f * num7);
					Vector2 b = vector + (vector2 - vector) * wireC - new Vector2(num6 * 0.1f * num5, num6 * 0.4f) * vector4.y;
					Vector2 c = vector2 + (vector - vector2) * wireC - new Vector2(num6 * -0.1f * num5, num6 * 0.4f) * vector4.x;
					(Vector2, Vector2, Vector2, Vector2) abcd = Spline(vector, b, c, vector2);
					int num8 = i * wireSegments;
					int num9 = (i + 1) * wireSegments;
					for (int j = num8; j < num9; j++)
					{
						int num10 = j * 2;
						float num11 = (float)(j - num8) / (float)(wireSegments - 1);
						if (num5 > 0f)
						{
							num11 = 1f - num11;
						}
						colors[num10] = (colors[num10 + 1] = new Color(Mathf.Lerp(num3, num4, num11) - 0.08f, g, widths[i], randomness[i]));
						verts[num10] = Spline(abcd, num11);
					}
					for (int k = num8; k < num9; k++)
					{
						int num12 = k * 2;
						int num13 = Mathf.Max(k, num8 + 1) * 2 - 2;
						int num14 = Mathf.Min(k, num9 - 2) * 2 + 2;
						float t = (float)(k - num8) / (float)(wireSegments - 1);
						Vector2 vector5 = (verts[num13] - verts[num14]).normalized;
						vector5 = new Vector2(0f - vector5.y, vector5.x);
						vector5 *= wireWidth * (1f + widths[i] * 0.3f) * (0.75f + Mathf.Lerp(num3, num4, t) * 0.25f);
						verts[num12 + 1] = (Vector2)verts[num12] - vector5;
					}
				}
				mesh.colors32 = colors;
				mesh.vertices = verts;
				mesh.RecalculateBounds();
				if (!(wire?.element?.meshFilter == null))
				{
					wire.element.meshFilter.sharedMesh = mesh;
				}
			}

			public void CreateMesh()
			{
				mesh = new Mesh
				{
					name = "RotFloater Wire Mesh"
				};
				mesh.MarkDynamic();
				verts = new Vector3[wireSegments * 2 * numberOfWires];
				colors = new Color32[verts.Length];
				int num = wireSegments - 1;
				int[] array = new int[num * 6 * numberOfWires];
				Vector2[] array2 = new Vector2[verts.Length];
				int num2 = 0;
				for (int i = 0; i < numberOfWires; i++)
				{
					int num3 = i * (num + 1);
					int num4 = num3 + num;
					for (int j = num3; j < num4; j++)
					{
						int num5 = (array[num2] = j * 2);
						array[num2 + 1] = (array[num2 + 3] = num5 + 2);
						array[num2 + 2] = (array[num2 + 5] = num5 + 1);
						array[num2 + 4] = num5 + 2 + 1;
						num2 += 6;
					}
					num3 = i * wireSegments;
					num4 = num3 + wireSegments;
					for (int k = num3; k < num4; k++)
					{
						int num6 = k * 2;
						float num7 = (float)(k - num3) / (float)(wireSegments - 1);
						array2[num6] = Vector2.right * num7;
						array2[num6 + 1] = array2[num6] + Vector2.up;
					}
				}
				mesh.vertices = verts;
				mesh.triangles = array;
				mesh.uv = array2;
			}

			public void DisposeMesh()
			{
				if (mesh != null)
				{
					UnityEngine.Object.Destroy(mesh);
					mesh = null;
				}
			}

			public void Destroy()
			{
				slatedForDeletion = true;
				wire?.Destroy();
				wire = null;
				DisposeMesh();
			}
		}

		public class RotFloaterSpawner : IFloaterSpawner
		{
			public UIText GetUIText()
			{
				return null;
			}

			public Floater Spawn(FloaterData data)
			{
				return new RotFloater(data);
			}
		}

		public Vector2[] blobOffsets;

		public Vector2[] finalBlobPositions;

		public float[] blobRandomDepthOffsets;

		public float[] blobDepths;

		public int[] smallWiresConnections;

		public Blob blob;

		public Wire wire;

		public float maxBlobs = 10f;

		public float maxSmalWires = 20f;

		public bool initializeGraphics = true;

		public Vector2 _finalScale;

		public new Vector2 finalScale => base.finalScale * Vector2.one;

		public bool slowUpdateFrame => Time.frameCount % 10 == posInArray % 10;

		public new Vector2 getPos
		{
			get
			{
				float num = Mathf.Sin(time + offset.x * (float)Math.PI * 2f) * offset.y * 10f;
				float num2 = Mathf.Sin(time + origPos.x / 20f) * 10f;
				num2 *= 1f - base.depth;
				return origPos + offset * offsetAmount + Vector2.up * num2 * movement + offset * num * movement;
			}
		}

		public RotFloater(FloaterData data)
			: base(data)
		{
			Init();
		}

		public void Init()
		{
			defaultScale = 40f;
			GenerateRandomness();
		}

		public void GenerateRandomness()
		{
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState((int)(offset.x * 1000f));
			blobOffsets = new Vector2[(int)((0.5f + UnityEngine.Random.value * 0.5f) * maxBlobs)];
			blobRandomDepthOffsets = new float[blobOffsets.Length];
			blobDepths = new float[blobOffsets.Length];
			finalBlobPositions = new Vector2[blobOffsets.Length];
			for (int i = 0; i < blobOffsets.Length; i++)
			{
				blobOffsets[i] = new Vector2(UnityEngine.Random.value, UnityEngine.Random.value) * 2f - Vector2.one;
				blobRandomDepthOffsets[i] = UnityEngine.Random.value;
			}
			smallWiresConnections = new int[(int)((0.5f + UnityEngine.Random.value * 0.5f) * maxSmalWires)];
			int num = 0;
			for (int j = 0; j < smallWiresConnections.Length; j++)
			{
				int num2 = 0;
				for (num2 = num; num2 == num; num2 = (int)(UnityEngine.Random.value * (float)(blobOffsets.Length - 1)))
				{
					smallWiresConnections[j] = num2;
				}
				num = num2;
			}
			UnityEngine.Random.state = state;
		}

		public override void Destroy()
		{
			base.Destroy();
			blob?.Destroy();
			wire?.Destroy();
		}

		public void RoomUnloaded()
		{
			Destroy();
		}

		public void GenerateGraphics()
		{
			blob = new Blob(this);
			wire = new Wire(this);
			initializeGraphics = false;
		}

		public void UpdateGraphics()
		{
			_finalScale = finalScale;
			blob.Update();
			wire.Update();
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (initializeGraphics)
			{
				GenerateGraphics();
			}
			base.depth = Mathf.Clamp01(base._depth + depthOffset);
			UpdateGraphics();
		}
	}

	public class Stardust : Floater, IDrawable
	{
		public class StardustSpawner : IFloaterSpawner
		{
			public static UIText uiText = new UIText
			{
				depthFar = UIText.Title("Depth Far"),
				depthNear = UIText.Title("Depth Near"),
				scaleMax = UIText.Title("Scale Max"),
				scaleMin = UIText.Title("Scale Min"),
				scaleOffset = UIText.Title("Scale Offset"),
				depthOffset = UIText.Title("Depth Offset"),
				extraSlider1 = UIText.Title("Intensity"),
				extraSlider2 = UIText.Title("Wind Speed")
			};

			public UIText GetUIText()
			{
				return uiText;
			}

			public virtual Floater Spawn(FloaterData data)
			{
				return new Stardust(data);
			}
		}

		public float rotation;

		public float lastRotation;

		public int step;

		public int totalFloaters;

		public int totalConPoints;

		public bool debug;

		public Vector2 splinePos;

		public Vector2 lastOrigPos;

		public DebugSprite debugPosSprite;

		public DebugSprite debugTargetSprite;

		public DebugSprite debugLineSprite;

		public Vector2 random;

		public float shaderTime;

		public float speed;

		public float intensity;

		public Vector2 smoothPos;

		public new Vector2 getPos
		{
			get
			{
				preMovementPos = origPos + offset * offsetAmount;
				float num = Mathf.Sin(time + offset.x * (float)Math.PI * 2f) * offset.y * 10f;
				num += Mathf.Sin(time + origPos.x / 20f) * 10f;
				num *= 1f - base.depth;
				return preMovementPos + Vector2.up * (num * movement);
			}
		}

		public Stardust(FloaterData data)
			: base(data)
		{
			random = new Vector2(UnityEngine.Random.value, UnityEngine.Random.value);
		}

		public override void Update(bool eu)
		{
			InitDebug();
			UpdateCounts();
			UpdateRotation();
			intensity = Mathf.Clamp01((extraOffsets.x + 1f) * 0.5f * (1f + random.y * 0.3f));
			speed = Mathf.Clamp01((extraOffsets.y + 1f) * 0.5f);
			lastOrigPos = origPos;
			if (room.BeingViewed)
			{
				time += Time.fixedDeltaTime * 2f * Mathf.Clamp01(Mathf.Abs(speed * 2f - 1f) * 3f);
			}
			lastPos = pos;
			pos = getPos;
		}

		public void UpdateCounts()
		{
			step = Mathf.Max(totalFloaters / Mathf.Max(totalConPoints, 1), 2);
			totalFloaters = owner.floaters.Count;
			totalConPoints = owner.data.controlPointPosX.Count;
		}

		public override void Destroy()
		{
			base.Destroy();
			if (debug)
			{
				debugTargetSprite.Destroy();
				debugPosSprite.Destroy();
				debugLineSprite.Destroy();
			}
		}

		public void InitDebug()
		{
			if (debug && debugPosSprite == null)
			{
				debugPosSprite = new DebugSprite(Vector2.zero, new FSprite("Futile_White")
				{
					color = Color.red
				}, room);
				debugTargetSprite = new DebugSprite(Vector2.zero, new FSprite("Futile_White")
				{
					color = Color.green
				}, room);
				debugLineSprite = new DebugSprite(Vector2.zero, new FSprite("Futile_White")
				{
					color = Color.blue,
					anchorY = 0f,
					scaleX = 0.12f
				}, room);
				room.AddObject(debugPosSprite);
				room.AddObject(debugTargetSprite);
				room.AddObject(debugLineSprite);
			}
		}

		public void UpdateRotation()
		{
			lastRotation = rotation;
			Vector2 vector = offset;
			rotation = GetAngle() + 90f;
			if (debug)
			{
				debugPosSprite.pos = preMovementPos;
				debugTargetSprite.pos = vector;
				debugLineSprite.pos = preMovementPos;
				debugLineSprite.sprite.rotation = rotation;
				debugLineSprite.sprite.scaleY = (vector - pos).magnitude;
			}
		}

		public Vector2 ConPos(int i)
		{
			return owner.data.ConPos(i);
		}

		public float GetAngle()
		{
			if (totalConPoints <= 1)
			{
				return Custom.VecToDeg(offset);
			}
			float a = Custom.VecToDeg(Vector2.Perpendicular(ConPos(index) - ConPos(index - 1)));
			float num = Custom.VecToDeg(Vector2.Perpendicular(ConPos(index + 1) - ConPos(index)));
			if (index == 0 && influence <= 0.5f)
			{
				return num;
			}
			if (index == totalConPoints - 2 && influence > 0.5f)
			{
				return num;
			}
			float b = Custom.VecToDeg(Vector2.Perpendicular(ConPos(index + 2) - ConPos(index + 1)));
			if (influence > 0.5f)
			{
				return Mathf.LerpAngle(num, b, influence - 0.5f);
			}
			return Mathf.LerpAngle(a, num, influence + 0.5f);
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["Stardust"];
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].SetPosition(Vector2.Lerp(lastPos, pos, timeStacker) - camPos);
			sLeaser.sprites[0].rotation = Mathf.LerpAngle(lastRotation, rotation, timeStacker);
			sLeaser.sprites[0].color = new Color(random.x, Mathf.Clamp01(base._depth + depthOffset * 0.5f), speed, intensity);
			sLeaser.sprites[0].scaleX = 2f * base.finalScale;
			sLeaser.sprites[0].scaleY = 4f * base.finalScale;
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[0]);
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}
	}

	public List<Vector2> offsets = new List<Vector2>();

	public List<float> offsetAmounts = new List<float>();

	public List<float> depthOffsets = new List<float>();

	public List<Vector2> positions = new List<Vector2>();

	public List<float> depths = new List<float>();

	public List<float> scales = new List<float>();

	public List<float> scaleOffsets = new List<float>();

	public List<Vector4> extraOffsets = new List<Vector4>();

	public List<Floater> floaters = new List<Floater>();

	public List<(int, float)> controlPointInfluence = new List<(int, float)>();

	public float _totalLength;

	public int seed;

	public int _depthNear;

	public int _depthFar;

	public Vector2 pos = Vector2.zero;

	public int _numberOfFloaters;

	public float radius = 150f;

	public float _scaleMinimum;

	public float _scaleMaximum;

	public float movementAmount;

	public string _type;

	public bool initialized;

	public UIText uiText;

	public static Dictionary<string, Floater.IFloaterSpawner> types = new Dictionary<string, Floater.IFloaterSpawner>
	{
		{
			"Test Squares",
			new TestSquares.TestSquaresSpawner()
		},
		{
			"Rock",
			new Rock.RockSpawner()
		}
	};

	public FloatingDebrisData data;

	public float totalLength
	{
		get
		{
			return _totalLength;
		}
		set
		{
			_totalLength = value;
		}
	}

	public int depthNear
	{
		get
		{
			return _depthNear;
		}
		set
		{
			_depthNear = value;
			_depthFar = ((_depthFar < _depthNear) ? _depthNear : _depthFar);
		}
	}

	public int depthFar
	{
		get
		{
			return _depthFar;
		}
		set
		{
			_depthFar = value;
			_depthNear = ((_depthNear > _depthFar) ? _depthFar : _depthNear);
		}
	}

	public int numberOfFloaters
	{
		get
		{
			return _numberOfFloaters;
		}
		set
		{
			_numberOfFloaters = value;
		}
	}

	public int EffectiveNumberOfFloaters
	{
		get
		{
			if (ModManager.Watcher)
			{
				if (types.ContainsKey(type) && (types[type] is RippleFlow.RippleFlowSpawner || types[type] is Ripple.RippleSpawner))
				{
					return _numberOfFloaters;
				}
				return Mathf.FloorToInt((float)_numberOfFloaters * (1f - Watcher.cfgReduceFloatingDebrisDensity.Value));
			}
			return _numberOfFloaters;
		}
	}

	public float scaleMinimum
	{
		get
		{
			return _scaleMinimum;
		}
		set
		{
			_scaleMinimum = value;
			_scaleMaximum = ((_scaleMaximum < _scaleMinimum) ? _scaleMinimum : _scaleMaximum);
		}
	}

	public float scaleMaximum
	{
		get
		{
			return _scaleMaximum;
		}
		set
		{
			_scaleMaximum = value;
			_scaleMinimum = ((_scaleMinimum > _scaleMaximum) ? _scaleMaximum : _scaleMinimum);
		}
	}

	public string type
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value;
			RemoveAllFloaters();
			UpdateUIText();
			AddFloaters();
		}
	}

	public static FloatingDebris TrySpawnFloatingDebris(Room room, PlacedObject pObj)
	{
		if (!types.ContainsKey((pObj.data as FloatingDebrisData).type))
		{
			return null;
		}
		if (ModManager.Watcher && Watcher.cfgDisableCosmeticRipples.Value && (types[(pObj.data as FloatingDebrisData).type] is RippleFlow.RippleFlowSpawner || types[(pObj.data as FloatingDebrisData).type] is Ripple.RippleSpawner))
		{
			return null;
		}
		return new FloatingDebris(room, pObj);
	}

	public FloatingDebris(Room room, PlacedObject pObj)
	{
		data = pObj.data as FloatingDebrisData;
		base.room = room;
		pos = pObj.pos;
		seed = data.seed;
		_depthNear = data.depthNear;
		_depthFar = data.depthFar;
		_scaleMinimum = data.scaleMinimum;
		_scaleMaximum = data.scaleMaximum;
		_numberOfFloaters = data.numberOfFloaters;
		_type = data.type;
		movementAmount = data.movement;
		Init();
	}

	public void Init()
	{
		UpdateUIText();
		UpdateSeededRandomValues();
		UpdateControlPointInfluence();
		UpdatePositions();
		UpdateOffsetAmounts();
		UpdateDepthOffsets();
		UpdateScaleOffsets();
		UpdateExtraOffsets();
		AddFloaters();
		RefreshFloaters();
	}

	public override void Destroy()
	{
		base.Destroy();
		RemoveAllFloaters();
	}

	public void UpdateUIText()
	{
		uiText = types[type].GetUIText();
	}

	public void UpdateSeededRandomValues()
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(seed);
		offsets.Clear();
		depths.Clear();
		scales.Clear();
		for (int i = 0; i < EffectiveNumberOfFloaters; i++)
		{
			Vector2 item = new Vector2(UnityEngine.Random.value, UnityEngine.Random.value) * 2f - Vector2.one;
			offsets.Add(item);
			depths.Add(Mathf.Lerp(_depthNear, _depthFar, UnityEngine.Random.value) / 30f);
			float value = UnityEngine.Random.value;
			scales.Add(Mathf.Lerp(_scaleMinimum, _scaleMaximum, value));
		}
		UnityEngine.Random.state = state;
	}

	public void MoveControlPoint(int pointIndex, Vector2 position)
	{
		if (pointIndex <= data.controlPointPosX.Count - 1)
		{
			data.controlPointPosX[pointIndex] = position.x;
			data.controlPointPosY[pointIndex] = position.y;
			UpdateControlPointInfluence();
			UpdatePositions();
			UpdateOffsetAmounts();
			UpdateDepthOffsets();
			UpdateScaleOffsets();
			UpdateExtraOffsets();
			RefreshFloaters();
		}
	}

	public void UpdateControlPointInfluence()
	{
		controlPointInfluence.Clear();
		int effectiveNumberOfFloaters = EffectiveNumberOfFloaters;
		if (data.controlPointPosX.Count < 2)
		{
			for (int i = 0; i < effectiveNumberOfFloaters; i++)
			{
				controlPointInfluence.Add((0, 0f));
			}
			return;
		}
		totalLength = 0f;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		int num4 = 0;
		if (data.controlPointPosX.Count <= 1)
		{
			return;
		}
		for (int j = 0; j < data.controlPointPosX.Count - 1; j++)
		{
			totalLength += Vector2.Distance(new Vector2(data.controlPointPosX[j], data.controlPointPosY[j]), new Vector2(data.controlPointPosX[j + 1], data.controlPointPosY[j + 1]));
		}
		num = totalLength / Mathf.Max(effectiveNumberOfFloaters - 1, 0.0001f) - 0.001f;
		num3 = Vector2.Distance(new Vector2(data.controlPointPosX[0], data.controlPointPosY[0]), new Vector2(data.controlPointPosX[1], data.controlPointPosY[1]));
		for (int k = 0; k < effectiveNumberOfFloaters; k++)
		{
			float num5 = (float)k * num;
			if (num5 > num3 + num2)
			{
				k--;
				num4++;
				num2 += num3;
				num3 = Vector2.Distance(new Vector2(data.controlPointPosX[num4], data.controlPointPosY[num4]), new Vector2(data.controlPointPosX[num4 + 1], data.controlPointPosY[num4 + 1]));
			}
			else
			{
				num5 -= num2;
				num5 /= num3;
				controlPointInfluence.Add((num4, num5));
			}
		}
	}

	public void UpdatePositions()
	{
		positions.Clear();
		for (int i = 0; i < EffectiveNumberOfFloaters; i++)
		{
			var (num, t) = controlPointInfluence[i];
			if (data.controlPointPosX.Count <= 1)
			{
				positions.Add(pos + new Vector2(data.controlPointPosX[0], data.controlPointPosY[0]));
				continue;
			}
			Vector2 vector = Vector2.Lerp(new Vector2(data.controlPointPosX[num], data.controlPointPosY[num]), new Vector2(data.controlPointPosX[num + 1], data.controlPointPosY[num + 1]), t);
			positions.Add(pos + vector);
		}
	}

	public void UpdateDepthOffsets()
	{
		depthOffsets.Clear();
		for (int i = 0; i < EffectiveNumberOfFloaters; i++)
		{
			var (num, t) = controlPointInfluence[i];
			if (data.controlPointPosX.Count <= 1)
			{
				depthOffsets.Add(data.controlPointDepthOffset[num]);
				continue;
			}
			float item = Mathf.Lerp(data.controlPointDepthOffset[num], data.controlPointDepthOffset[num + 1], t);
			depthOffsets.Add(item);
		}
	}

	public void UpdateScaleOffsets()
	{
		scaleOffsets.Clear();
		for (int i = 0; i < EffectiveNumberOfFloaters; i++)
		{
			var (num, t) = controlPointInfluence[i];
			if (data.controlPointPosX.Count <= 1)
			{
				scaleOffsets.Add(data.controlPointScaleOffset[num]);
				continue;
			}
			float item = Mathf.Lerp(data.controlPointScaleOffset[num], data.controlPointScaleOffset[num + 1], t);
			scaleOffsets.Add(item);
		}
	}

	public void UpdateExtraOffsets()
	{
		extraOffsets.Clear();
		for (int i = 0; i < EffectiveNumberOfFloaters; i++)
		{
			var (num, t) = controlPointInfluence[i];
			if (data.controlPointPosX.Count <= 1)
			{
				extraOffsets.Add(data.controlPointExtraOffset[num]);
				continue;
			}
			Vector4 item = Vector4.Lerp(data.controlPointExtraOffset[num], data.controlPointExtraOffset[num + 1], t);
			extraOffsets.Add(item);
		}
	}

	public void UpdateOffsetAmounts()
	{
		offsetAmounts.Clear();
		for (int i = 0; i < EffectiveNumberOfFloaters; i++)
		{
			var (num, t) = controlPointInfluence[i];
			if (data.controlPointPosX.Count <= 1)
			{
				offsetAmounts.Add(data.controlPointOffsetAmount[num] * radius);
				continue;
			}
			float num2 = Mathf.Lerp(data.controlPointOffsetAmount[num], data.controlPointOffsetAmount[num + 1], t);
			offsetAmounts.Add(num2 * radius);
		}
	}

	public void RefreshFloaters()
	{
		for (int i = 0; i < EffectiveNumberOfFloaters; i++)
		{
			floaters[i].offset = offsets[i];
			floaters[i].depth = depths[i];
			floaters[i].scale = scales[i];
			floaters[i].origPos = positions[i];
			floaters[i].offsetAmount = offsetAmounts[i];
			floaters[i].depthOffset = depthOffsets[i];
			floaters[i].scaleOffset = scaleOffsets[i];
			floaters[i].extraOffsets = extraOffsets[i];
			floaters[i].movement = movementAmount;
			Floater floater = floaters[i];
			Floater floater2 = floaters[i];
			(int, float) tuple = controlPointInfluence[i];
			floater.index = tuple.Item1;
			floater2.influence = tuple.Item2;
		}
	}

	public Vector2 GetFloaterPosition(int index)
	{
		return Vector2.zero;
	}

	public void RemoveAllFloaters()
	{
		for (int i = 0; i < floaters.Count; i++)
		{
			floaters[i].Destroy();
		}
		floaters.Clear();
	}

	public void AddFloaters()
	{
		Floater.IFloaterSpawner value;
		Floater.IFloaterSpawner floaterSpawner = (types.TryGetValue(_type, out value) ? value : types["TestSquares"]);
		if (floaters.Count < positions.Count)
		{
			for (int i = floaters.Count; i < positions.Count; i++)
			{
				FloaterData floaterData = new FloaterData(this, i, controlPointInfluence[i], positions[i], offsets[i], offsetAmounts[i], depths[i], depthOffsets[i], scales[i], scaleOffsets[i], extraOffsets[i], movementAmount);
				Floater floater = floaterSpawner.Spawn(floaterData);
				room.AddObject(floater);
				floaters.Add(floater);
			}
		}
		else if (floaters.Count > positions.Count)
		{
			for (int num = floaters.Count - 1; num > positions.Count - 1; num--)
			{
				floaters[num].RemoveFromRoom();
				floaters[num].Destroy();
				floaters.Pop();
			}
		}
		initialized = true;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
	}

	public static void AddWatcherFloaters()
	{
		types["Aurora"] = new Aurora.AuroraSpawner();
		types["Aurora Foreground"] = new Aurora.AuroraForegroundSpawner();
		types["Gilded Wind"] = new GildedWind.GildedWindSpawner();
		types["Stardust"] = new Stardust.StardustSpawner();
		types["Ripple"] = new Ripple.RippleSpawner();
		types["Ripple Watcher"] = new Ripple.WatcherRippleSpawner();
		types["Wavy Ripple"] = new Ripple.WavyRippleSpawner();
		types["Wavy Ripple Watcher"] = new Ripple.WavyWatcherRippleSpawner();
		types["Ripple Flow"] = new RippleFlow.RippleFlowSpawner();
		types["Ripple Flow Watcher"] = new RippleFlow.WatcherRippleFlowSpawner();
		types["Rot Islands"] = new RotFloater.RotFloaterSpawner();
		types["Big Tube"] = new BigTube.BigTubeSpawner();
	}

	public static void RemoveWatcherFloaters()
	{
		types.Remove("Aurora");
		types.Remove("Aurora Foreground");
		types.Remove("Gilded Wind");
		types.Remove("Stardust");
		types.Remove("Ripple");
		types.Remove("Ripple Watcher");
		types.Remove("Wavy Ripple");
		types.Remove("Wavy Ripple Watcher");
		types.Remove("Ripple Flow");
		types.Remove("Ripple Flow Watcher");
		types.Remove("Rot Islands");
		types.Remove("Big Tube");
	}
}
