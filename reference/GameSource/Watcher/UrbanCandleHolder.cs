using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class UrbanCandleHolder : CosmeticSprite, INotifyWhenRoomUnloaded
{
	public class CandlePole : CosmeticSprite
	{
		public DynamicLevelElement pole;

		public float width = 1f;

		public float height = 1f;

		public float rotation;

		public float tilt = 0.5f;

		public float depth;

		public int candles;

		public int index = -1;

		public float variation;

		public Mesh candlePoleMesh;

		public float getHeight
		{
			get
			{
				if (index == -1)
				{
					return height;
				}
				return height * variation;
			}
		}

		public CandlePole(Room room, Vector2 pos, float tilt, float height, float depth, float rotation = 0f, float width = 1f)
		{
			this.height = height;
			this.width = width;
			base.room = room;
			base.pos = pos;
			this.tilt = tilt;
			this.rotation = rotation;
			this.depth = depth;
			variation = Random.value;
			candlePoleMesh = new Mesh
			{
				name = "CandlePole Mesh"
			};
			Vector3[] vertices = MaskSource.quadVerts.Select((Vector3 v) => v - Vector3.down * 0.5f).ToArray();
			candlePoleMesh.vertices = vertices;
			candlePoleMesh.triangles = MaskSource.quadTris;
			candlePoleMesh.uv = MaskSource.quadUV;
			candlePoleMesh.RecalculateBounds();
			pole = new DynamicLevelElement(pos, new Vector2(width, getHeight), null, 0, candlePoleMesh, "DynamicLevelPole")
			{
				rotation = this.rotation,
				dontSetProps = true,
				dontDrawUpdate = true
			};
			room.AddObject(pole);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
		}

		public override void Destroy()
		{
			base.Destroy();
			pole.Destroy();
			if (candlePoleMesh != null)
			{
				Object.Destroy(candlePoleMesh);
				candlePoleMesh = null;
			}
		}

		public static Vector2 ApplyDepth(Room room, Vector2 pos, float depth)
		{
			return room.game.cameras[0].ApplyDepth(pos, (1f - depth) * 30f - 5f);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			if (!base.slatedForDeletetion)
			{
				Vector2 vector = ApplyDepth(room, Vector2.Lerp(lastPos, pos, timeStacker), depth);
				if (pole.element != null)
				{
					pole.element.SetProperty(0, depth);
					pole.element.SetProperty(1, variation);
					pole.element.SetProperty(2, tilt);
					pole.element.SetProperty(3, width / getHeight);
					pole.element.ApplyVertexColor();
					pole.element.setGameObjectPos = vector - camPos;
					pole.element.setGameObjectRotation = Vector3.back * rotation;
					pole.element.setGameObjectScale = new Vector3(width, getHeight, 1f);
				}
			}
		}
	}

	public class CandleBowl : CosmeticSprite
	{
		public class Slot
		{
			public int maxSlots;

			public int slot;

			public float initialOffset;

			public float tilt;

			public float scale;

			public float individualOffset;

			public float randomRotation;

			public Vector2 pos { get; set; }

			public float depthOffset { get; set; }

			public float rotation { get; set; }

			public Slot(int slot, int maxSlots, float initialOffset, float tilt, float scale)
			{
				this.slot = slot;
				this.maxSlots = maxSlots;
				this.initialOffset = initialOffset;
				this.tilt = tilt;
				this.scale = scale;
				randomRotation = Random.Range(-20, 20);
				float num = 360f / (float)maxSlots;
				individualOffset = Random.Range(0f - num, num) * 0.5f;
			}

			public void UpdateSlot()
			{
				float num = 360f / (float)maxSlots;
				Vector2 vector = Custom.rotateVectorDeg(Vector2.up, initialOffset + (float)slot * num + individualOffset);
				depthOffset = ((0f - vector.y) * 0.5f + 0.5f) * 0.0666f - 0.0333f;
				rotation = vector.x * 30f;
				pos = new Vector2(vector.x, vector.y * tilt) * (scale * 0.75f * 8f);
			}
		}

		public DynamicLevelElement holder;

		public List<CandlePole> randomPoles;

		public UrbanCandle[] candles;

		public Slot[] slots;

		public float scale = 5f;

		public float rotation;

		public float tilt = 0.5f;

		public float variation;

		public float depth;

		public float candleWidthScale = 1f;

		public CandleBowl(Room room, Vector2 pos, float tilt, float scale, float depth, int minCandles = 5, int maxCandles = 14, bool spawnPoles = false)
		{
			base.room = room;
			base.pos = pos;
			this.tilt = tilt;
			variation = Random.value;
			this.depth = depth;
			this.scale = scale;
			if (spawnPoles)
			{
				randomPoles = new List<CandlePole>();
			}
			slots = new Slot[Random.Range(minCandles, maxCandles)];
			candles = new UrbanCandle[slots.Length];
			holder = new DynamicLevelElement(pos, Vector2.one * scale, null, 0, null, "DynamicLevelBowl")
			{
				rotation = rotation,
				dontSetProps = true,
				dontDrawUpdate = true
			};
			room.AddObject(holder);
			float initialOffset = Random.Range(0, 360);
			for (int i = 0; i < slots.Length; i++)
			{
				slots[i] = new Slot(i, slots.Length, initialOffset, tilt, scale);
				candles[i] = new UrbanCandle(room, new UrbanCandle.UrbanCandleData
				{
					pos = slots[i].pos + pos,
					rotation = slots[i].rotation,
					depth = this.depth + slots[i].depthOffset,
					width = 0.3f,
					height = Random.Range(0.5f, 1f + scale * 0.5f)
				});
				room.AddObject(candles[i]);
				if (spawnPoles && Random.value > 0.5f)
				{
					randomPoles.Add(new CandlePole(room, slots[i].pos + pos, tilt, scale * 0.5f, depth + slots[i].depthOffset, 180f, 0.1f * scale)
					{
						index = i
					});
				}
			}
			if (!spawnPoles)
			{
				return;
			}
			foreach (CandlePole randomPole in randomPoles)
			{
				room.AddObject(randomPole);
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			for (int i = 0; i < slots.Length; i++)
			{
				slots[i].tilt = tilt;
				slots[i].scale = scale;
				slots[i].UpdateSlot();
			}
			for (int j = 0; j < candles.Length; j++)
			{
				candles[j].pos = slots[j].pos + pos;
				candles[j].depth = slots[j].depthOffset + depth;
				candles[j].rotation = slots[j].rotation;
				candles[j].width = 0.6f * candleWidthScale;
			}
			if (randomPoles != null)
			{
				for (int k = 0; k < randomPoles.Count; k++)
				{
					randomPoles[k].pos = pos + slots[randomPoles[k].index].pos;
					randomPoles[k].height = scale * 0.5f;
					randomPoles[k].width = scale * 0.1f;
					randomPoles[k].depth = depth + slots[randomPoles[k].index].depthOffset;
					randomPoles[k].tilt = tilt;
					randomPoles[k].rotation = 180f;
				}
			}
		}

		public override void Destroy()
		{
			base.Destroy();
			holder.Destroy();
			for (int i = 0; i < candles.Length; i++)
			{
				candles[i].Destroy();
			}
			if (randomPoles != null)
			{
				for (int j = 0; j < randomPoles.Count; j++)
				{
					randomPoles[j].Destroy();
				}
			}
		}

		public static Vector2 ApplyDepth(Room room, Vector2 pos, float depth)
		{
			return room.game.cameras[0].ApplyDepth(pos, (1f - depth) * 30f - 5f);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			if (!base.slatedForDeletetion)
			{
				Vector2 vector = ApplyDepth(room, Vector2.Lerp(lastPos, pos, timeStacker), depth);
				if (holder.element != null)
				{
					holder.element.SetProperty(0, depth);
					holder.element.SetProperty(1, tilt);
					holder.element.SetProperty(2, variation);
					holder.element.ApplyVertexColor();
					holder.element.setGameObjectPos = vector - camPos;
					holder.element.setGameObjectRotation = Vector3.back * rotation;
					holder.element.setGameObjectScale = Vector3.one * scale;
				}
			}
		}
	}

	public class UrbanCandleHolderData : PlacedObject.Data
	{
		public float tilt;

		public float height;

		public float scale;

		public Vector2 pos;

		public float depth;

		public int seed;

		public float candleWidth;

		public UrbanCandleHolder obj;

		public UrbanCandleHolderData(PlacedObject owner)
			: base(owner)
		{
			height = 1f;
			scale = 1f;
			depth = 0.5f;
			tilt = 0.3f;
			seed = 1;
			candleWidth = 1f;
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			int num = -1;
			pos.x = ((array.Length > ++num) ? ParseFloat(array[num]) : pos.x);
			pos.y = ((array.Length > ++num) ? ParseFloat(array[num]) : pos.y);
			height = ((array.Length > ++num) ? ParseFloat(array[num]) : height);
			scale = ((array.Length > ++num) ? ParseFloat(array[num]) : scale);
			seed = ((array.Length > ++num) ? ParseInt(array[num]) : seed);
			tilt = ((array.Length > ++num) ? ParseFloat(array[num]) : tilt);
			depth = ((array.Length > ++num) ? ParseFloat(array[num]) : depth);
			candleWidth = ((array.Length > ++num) ? ParseFloat(array[num]) : candleWidth);
		}

		public float ParseFloat(string s)
		{
			return float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
		}

		public int ParseInt(string s)
		{
			return int.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}", pos.x, pos.y, height, scale, seed, tilt, depth, candleWidth);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public CandleBowl[] bowls;

	public CandleBowl topBowl;

	public CandlePole pole;

	public float[] bowlOffsets;

	public float tilt = 0.3f;

	public float depth;

	public float height = 1f;

	public float scale = 1f;

	public float candleWidthScale = 1f;

	public float scaleMax = 5f;

	public float scaleMin = 3f;

	public int minCandles = 5;

	public int maxCandles = 14;

	public UrbanCandleHolder(Room room, UrbanCandleHolderData data)
	{
		pos = data.pos;
		tilt = data.tilt;
		depth = data.depth;
		height = data.height;
		scale = data.scale;
		candleWidthScale = data.candleWidth;
		Random.State state = Random.state;
		Random.InitState(data.seed);
		int num = Random.Range(1, 4);
		pole = new CandlePole(room, pos, tilt + 0.1f, (float)num * 4.5f * height, depth);
		room.AddObject(pole);
		bowls = new CandleBowl[num];
		bowlOffsets = new float[num];
		for (int i = 0; i < num; i++)
		{
			bowlOffsets[i] = Random.Range(50, 80) * i;
			float num2 = (float)(i + 1) / (float)num;
			bowls[i] = new CandleBowl(room, pos + Vector2.up * (bowlOffsets[i] * height), GetTilt(num2), Mathf.Lerp(scaleMax, scaleMin, num2) * scale, depth, minCandles, maxCandles, i != 0);
			room.AddObject(bowls[i]);
		}
		topBowl = new CandleBowl(room, pos + Vector2.up * ((float)(70 * num) * height), tilt - 1f, depth, 1f, 3, 6);
		room.AddObject(topBowl);
		Random.state = state;
	}

	public float GetTilt(float p)
	{
		return Mathf.Lerp(tilt, Mathf.Max(tilt - 0.1f * height, 0f), p);
	}

	public override void Destroy()
	{
		base.Destroy();
		topBowl.Destroy();
		pole.Destroy();
		for (int i = 0; i < bowls.Length; i++)
		{
			bowls[i].Destroy();
		}
	}

	public void RoomUnloaded()
	{
		Destroy();
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		for (int i = 0; i < bowlOffsets.Length; i++)
		{
			float num = (float)(i + 1) / (float)bowlOffsets.Length;
			bowls[i].pos = pos + Vector2.up * (bowlOffsets[i] * height);
			bowls[i].tilt = GetTilt(num);
			bowls[i].depth = depth;
			bowls[i].scale = Mathf.Lerp(scaleMax, scaleMin, num) * scale;
			bowls[i].candleWidthScale = candleWidthScale;
		}
		topBowl.pos = pos + Vector2.up * ((float)(70 * bowlOffsets.Length) * height);
		topBowl.tilt = GetTilt(1f);
		topBowl.depth = depth;
		pole.pos = pos;
		pole.depth = depth;
		pole.tilt = GetTilt(1f);
		pole.height = (float)bowlOffsets.Length * 4.5f * height;
	}
}
