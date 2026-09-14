using RWCustom;
using UnityEngine;

namespace Watcher.TardigradeCosmetics;

public class Spikes : Template
{
	public float startPoint;

	public float endPoint;

	public int rows;

	public int numberInRow;

	public int rowsOnBottom;

	public float wrap;

	public float length;

	public float width;

	public float layBack;

	public float puffOut;

	public FloatCurve spikeLengthsCurve;

	public FloatCurve spikeCurve;

	public Vector2[,,] spikeTips;

	public int SpriteOfSpike(int row, int index)
	{
		return startSprite + row * numberInRow + index;
	}

	public Spikes(TardigradeGraphics tGraphics, int startSprite, float startPoint, float endPoint, float wrap, float length, float width, float layBack, float puffOut, int rows, int numberInRow, FloatCurve spikeLengthsCurve, FloatCurve spikeCurve)
		: base(tGraphics, startSprite)
	{
		this.startPoint = startPoint;
		this.endPoint = endPoint;
		this.rows = rows;
		this.numberInRow = numberInRow;
		this.wrap = wrap;
		this.length = length;
		this.width = width;
		this.layBack = layBack;
		this.puffOut = puffOut;
		this.spikeLengthsCurve = spikeLengthsCurve;
		this.spikeCurve = spikeCurve;
		spikeTips = new Vector2[rows, numberInRow, 3];
		numberOfSprites = rows * numberInRow;
		rowsOnBottom = Mathf.CeilToInt((float)rows / 2f);
	}

	public Vector2 GetConnectionPos(int row, int index, TardigradeGraphics.Spine spine)
	{
		return GetConnectionPos(row, index, spine.GetSpineData(Mathf.Lerp(startPoint, endPoint, Mathf.InverseLerp(0f, numberInRow - 1, index))));
	}

	public Vector2 GetConnectionPos(int row, int index, TardigradeGraphics.SpineData data)
	{
		float roll = GetRoll(row);
		return data.pos + data.perp * (roll * 8f * bGraphics.bunny.scale) - data.dir * ((float)row / ((float)rows - 1f) * 4f * bGraphics.bunny.scale * (1f - Mathf.Abs(roll)) * (IsRowOnTop(row) ? 1f : (-1f)));
	}

	public float GetRoll(int row)
	{
		float num = ((bGraphics.bunny.flip > 0f) ? 1f : (-1f));
		if (IsRowOnTop(row))
		{
			return bGraphics.bunny.flip - num * ((float)(row - rowsOnBottom) + 1f) / (float)(rows - rowsOnBottom) * wrap;
		}
		return Mathf.Clamp(num - bGraphics.bunny.flip * (((float)row + 1f) / (float)rowsOnBottom) * wrap, -1f, 1f);
	}

	public bool IsRowOnTop(int row)
	{
		return row - rowsOnBottom >= 0;
	}

	public Vector2 GetSpikeGoal(int row, int index, TardigradeGraphics.SpineData data)
	{
		float roll = GetRoll(row);
		float num = Mathf.InverseLerp(0f, numberInRow - 1, index);
		return (data.perp * roll - data.dir * ((IsRowOnTop(row) ? 0.8f : 0.5f) * layBack + (num - 0.5f) * puffOut)).normalized * (length * spikeLengthsCurve.curve(num) * (Mathf.Abs(roll) * 0.3f + 0.7f) * bGraphics.bunny.scale);
	}

	public override void Update(TardigradeGraphics.Spine spine)
	{
		bool flag = bGraphics.bunny.dryness > 0.1f;
		for (int i = 0; i < numberInRow; i++)
		{
			float t = Mathf.InverseLerp(0f, numberInRow - 1, i);
			float f = Mathf.Lerp(startPoint, endPoint, t);
			TardigradeGraphics.SpineData spineData = spine.GetSpineData(f);
			for (int j = 0; j < rows; j++)
			{
				int row = j;
				if (bGraphics.bunny.flip <= 0f)
				{
					row = rows - j - 1;
				}
				spikeTips[j, i, 1] = spikeTips[j, i, 0];
				spikeTips[j, i, 0] += spikeTips[j, i, 2];
				spikeTips[j, i, 2] *= 0.8f;
				Vector2 connectionPos = GetConnectionPos(row, i, spineData);
				Vector2 spikeGoal = GetSpikeGoal(row, i, spineData);
				Vector2 vector = (connectionPos + spikeGoal - spikeTips[j, i, 0]) * 0.1f;
				spikeTips[j, i, 0] += vector;
				spikeTips[j, i, 2] += vector;
				if (flag)
				{
					spikeTips[j, i, 0] = Vector2.Lerp(spikeTips[j, i, 0], Vector2.Lerp(connectionPos + spikeGoal, connectionPos, 0.9f), bGraphics.bunny.dryness);
					spikeTips[j, i, 2] *= 1f - bGraphics.bunny.dryness * 0.9f;
				}
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < numberInRow; i++)
		{
			for (int j = 0; j < rows; j++)
			{
				spikeTips[j, i, 0] = bGraphics.bunny.firstChunk.pos;
				spikeTips[j, i, 1] = spikeTips[j, i, 0];
				spikeTips[j, i, 2] *= 0f;
			}
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < numberInRow; j++)
			{
				int num = SpriteOfSpike(i, j);
				sLeaser.sprites[num] = TriangleMesh.MakeLongMesh(5, pointyTip: false, customColor: true);
			}
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, TardigradeGraphics.Spine spine)
	{
		float num = 1f - (float)bGraphics.bunny.BitesLeft / 3f;
		for (int i = 0; i < rows; i++)
		{
			int row = i;
			if (bGraphics.bunny.flip <= 0f)
			{
				row = rows - i - 1;
			}
			for (int j = 0; j < numberInRow; j++)
			{
				int num2 = SpriteOfSpike(row, j);
				float num3 = Mathf.Lerp(startPoint, endPoint, Mathf.InverseLerp(0f, numberInRow - 1, j));
				sLeaser.sprites[num2].isVisible = num3 >= num;
				TardigradeGraphics.SpineData spineData = spine.GetSpineData(num3);
				Vector2 connectionPos = GetConnectionPos(row, j, spineData);
				Vector2 vector = Vector2.Lerp(spikeTips[i, j, 1], spikeTips[i, j, 0], timeStacker);
				Vector2 vector2 = (connectionPos + vector) * 0.5f;
				Vector2 vector3 = connectionPos + GetSpikeGoal(row, j, spineData) * 0.5f + spineData.dir * 5f;
				vector3 = vector2 + (vector3 - vector2) * 2f;
				Vector2 vector4 = connectionPos - Custom.DirVec(connectionPos, vector) * 2f;
				float num4 = 1f;
				for (int k = 0; k < 5; k++)
				{
					float num5 = Mathf.InverseLerp(0f, 4f, k);
					float num6 = spikeCurve.curve(num5) * width * bGraphics.bunny.scale;
					Vector2 vector5 = Vector2.Lerp(connectionPos, Vector2.Lerp(vector3, vector, num5), num5);
					Vector2 normalized = (vector5 - vector4).normalized;
					Vector2 vector6 = Custom.PerpendicularVector(normalized);
					float num7 = Vector2.Distance(vector5, vector4) / 5f;
					Vector2 vector7 = vector6 * ((num6 + num4) * 0.5f);
					Vector2 vector8 = vector6 * num6;
					if (k == 0)
					{
						(sLeaser.sprites[num2] as TriangleMesh).MoveVertice(k * 4, vector4 - vector7 - camPos);
						(sLeaser.sprites[num2] as TriangleMesh).MoveVertice(k * 4 + 1, vector4 + vector7 - camPos);
					}
					else
					{
						(sLeaser.sprites[num2] as TriangleMesh).MoveVertice(k * 4, vector4 - vector7 + normalized * num7 - camPos);
						(sLeaser.sprites[num2] as TriangleMesh).MoveVertice(k * 4 + 1, vector4 + vector7 + normalized * num7 - camPos);
					}
					(sLeaser.sprites[num2] as TriangleMesh).MoveVertice(k * 4 + 2, vector5 - vector8 - normalized * num7 - camPos);
					(sLeaser.sprites[num2] as TriangleMesh).MoveVertice(k * 4 + 3, vector5 + vector8 - normalized * num7 - camPos);
					vector4 = vector5;
					num4 = num6;
				}
			}
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Color b = bGraphics.secondaryColor;
		if (bGraphics.bunny.IsSlayer && rows == 4)
		{
			b = Color.Lerp(bGraphics.bodyColor, bGraphics.secondaryColor, 0.4f);
		}
		for (int i = 0; i < rows; i++)
		{
			if (bGraphics.bunny.IsSlayer && rows == 4)
			{
				b = Color.Lerp(bGraphics.bodyColor, bGraphics.secondaryColor, (i == 1 || i == 2) ? 0.5f : 0.7f);
			}
			for (int j = 0; j < numberInRow; j++)
			{
				int num = SpriteOfSpike(i, j);
				for (int k = 0; k < (sLeaser.sprites[num] as TriangleMesh).verticeColors.Length; k++)
				{
					(sLeaser.sprites[num] as TriangleMesh).verticeColors[k] = Color.Lerp(bGraphics.bodyColor, b, (float)k / ((float)(sLeaser.sprites[num] as TriangleMesh).verticeColors.Length - 1f));
				}
			}
		}
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner, SpritesOverlap overlap)
	{
		for (int i = 0; i < rows; i++)
		{
			if (IsRowOnTop(i))
			{
				if (overlap == SpritesOverlap.InFront)
				{
					for (int num = numberInRow - 1; num >= 0; num--)
					{
						newContatiner.AddChild(sLeaser.sprites[SpriteOfSpike(i, num)]);
					}
				}
			}
			else if (overlap == SpritesOverlap.Behind)
			{
				for (int j = 0; j < numberInRow; j++)
				{
					newContatiner.AddChild(sLeaser.sprites[SpriteOfSpike(i, j)]);
				}
			}
		}
	}
}
