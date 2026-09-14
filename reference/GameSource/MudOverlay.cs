using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class MudOverlay : CosmeticSprite
{
	public RoomCamera.SpriteLeaser ownerSleaser;

	public Creature ownerCrit;

	public Dictionary<FSprite, FSprite> layeringMap;

	public Dictionary<FSprite, FSprite> originalMap;

	public IMuddableGraphics muddableGraphicsModule => ownerSleaser.drawableObject as IMuddableGraphics;

	public MudOverlay(Creature ownerCrit, RoomCamera.SpriteLeaser ownerSleaser)
	{
		if (!(ownerSleaser.drawableObject is IMuddableGraphics))
		{
			throw new ArgumentException("drawableObject is not an IMuddableGraphics");
		}
		if (ownerCrit.graphicsModule != ownerSleaser.drawableObject)
		{
			Custom.LogWarning("initialized MudOverlay for crit [" + ownerCrit.Template.type.ToString() + "] with a graphics module other than its own");
		}
		this.ownerCrit = ownerCrit;
		this.ownerSleaser = ownerSleaser;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room != ownerCrit.room)
		{
			Destroy();
		}
	}

	public FSprite[] MuddableSprites()
	{
		List<FSprite> list = new List<FSprite>();
		for (int i = 0; i < ownerSleaser.sprites.Length; i++)
		{
			if (muddableGraphicsModule.MuddableSprite(ownerSleaser, i))
			{
				list.Add(ownerSleaser.sprites[i]);
			}
		}
		return list.ToArray();
	}

	public FSprite GetMudSprite(FSprite originalSprite)
	{
		return originalMap[originalSprite];
	}

	public FSprite GetMudSprite(int originalIndex)
	{
		return originalMap[ownerSleaser.sprites[originalIndex]];
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		FSprite[] array = MuddableSprites();
		sLeaser.sprites = new FSprite[array.Length];
		layeringMap = new Dictionary<FSprite, FSprite>();
		originalMap = new Dictionary<FSprite, FSprite>();
		for (int i = 0; i < array.Length; i++)
		{
			sLeaser.sprites[i] = MudUtils.MakeMudSprite(rCam, array[i]);
			layeringMap[sLeaser.sprites[i]] = array[i];
			originalMap[array[i]] = sLeaser.sprites[i];
		}
		muddableGraphicsModule.SetUpSpecialMudSprites(ownerSleaser, sLeaser, this);
		AddToContainer(sLeaser, rCam, null);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			UpdateLayering(sLeaser.sprites[i]);
		}
	}

	public void UpdateLayering(FSprite sprite)
	{
		if (!layeringMap.TryGetValue(sprite, out var value))
		{
			return;
		}
		FContainer container = value.container;
		if (container == null)
		{
			sprite.RemoveFromContainer();
			return;
		}
		if (sprite.container != container)
		{
			sprite.RemoveFromContainer();
			container.AddChild(sprite);
		}
		int childIndex = value.container.GetChildIndex(value);
		if (sprite.container.GetChildIndex(sprite) != childIndex + 1)
		{
			sprite.MoveInFrontOfOtherNode(value);
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		FSprite[] array = MuddableSprites();
		if (sLeaser.sprites.Length != array.Length)
		{
			sLeaser.RemoveAllSpritesFromContainer();
			InitiateSprites(sLeaser, rCam);
			muddableGraphicsModule.SetUpSpecialMudSprites(ownerSleaser, sLeaser, this);
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			UpdateLayering(sLeaser.sprites[i]);
			MudUtils.UpdateMudSprite(ownerCrit, sLeaser.sprites[i], array[i]);
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}
}
