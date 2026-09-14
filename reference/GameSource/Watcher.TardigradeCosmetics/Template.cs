using System;
using UnityEngine;

namespace Watcher.TardigradeCosmetics;

public abstract class Template
{
	public class SpritesOverlap : ExtEnum<SpritesOverlap>
	{
		public static SpritesOverlap Behind = new SpritesOverlap("Behind", register: true);

		public static SpritesOverlap InFront = new SpritesOverlap("InFront", register: true);

		public SpritesOverlap(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class FloatCurve
	{
		public virtual float curve(float x)
		{
			return x;
		}
	}

	public class FlatOneCurve : FloatCurve
	{
		public override float curve(float x)
		{
			return 1f;
		}
	}

	public class SharpCurve : FloatCurve
	{
		public override float curve(float x)
		{
			return (1f - Mathf.Pow(1f - x - 1f, 4f)) * 0.7f + Mathf.Sin(x * (float)Math.PI) * 0.3f;
		}
	}

	public class RoundedCurve : FloatCurve
	{
		public override float curve(float x)
		{
			return 1f - Mathf.Pow(x, 7f);
		}
	}

	public class SlayerSpineCurve : FloatCurve
	{
		public override float curve(float x)
		{
			return 6.75f * x * Mathf.Pow(1f - x, 2f) * 0.5f + 0.5f;
		}
	}

	public TardigradeGraphics bGraphics;

	public int numberOfSprites;

	public int startSprite;

	public RoomPalette palette;

	public Template(TardigradeGraphics bGraphics, int startSprite)
	{
		this.bGraphics = bGraphics;
		this.startSprite = startSprite;
	}

	public virtual void Update(TardigradeGraphics.Spine spine)
	{
	}

	public virtual void Reset()
	{
	}

	public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
	}

	public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, TardigradeGraphics.Spine spine)
	{
	}

	public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		this.palette = palette;
	}

	public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner, SpritesOverlap overlap)
	{
		if (overlap == SpritesOverlap.Behind)
		{
			for (int i = startSprite; i < startSprite + numberOfSprites; i++)
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
	}
}
