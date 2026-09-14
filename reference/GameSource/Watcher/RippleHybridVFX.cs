using RWCustom;
using UnityEngine;

namespace Watcher;

public class RippleHybridVFX : IDrawable
{
	public enum RippleSide
	{
		Both,
		Ripple,
		Normal
	}

	public enum HybridState
	{
		Off,
		Activate,
		Deactivate
	}

	public Vector2[] positions;

	public float[] scales;

	public float[] randomOffsets;

	public bool[] visibility;

	public Room room;

	public FContainer shadows;

	public float activationTime;

	public float maxScale = 15f;

	public bool deactivate;

	public float activationCounter;

	public RippleSide side;

	public bool deleteMe { get; set; }

	public RippleHybridVFX(RoomCamera.SpriteLeaser sLeaser, Room room, float activationTime, RippleSide rippleMaskSide)
	{
		this.room = room;
		shadows = room.game.cameras[0].ReturnFContainer("Shadows");
		this.activationTime = activationTime;
		side = rippleMaskSide;
		Initiate(sLeaser);
	}

	public void Initiate(RoomCamera.SpriteLeaser sLeaser)
	{
		if (activationTime == 0f)
		{
			activationCounter = 1f;
			activationTime = 1f;
		}
		activationCounter = activationTime;
		positions = new Vector2[sLeaser.sprites.Length];
		scales = new float[sLeaser.sprites.Length];
		visibility = new bool[sLeaser.sprites.Length];
		randomOffsets = new float[sLeaser.sprites.Length];
		for (int i = 0; i < randomOffsets.Length; i++)
		{
			randomOffsets[i] = Random.value;
		}
		Update(sLeaser);
		room.drawableObjects.Add(this);
		room.game.cameras[0].NewObjectInRoom(this);
	}

	public void Update(RoomCamera.SpriteLeaser sLeaser)
	{
		UpdateActivationAnimation();
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			positions[i] = sLeaser.sprites[i].GetPosition();
			scales[i] = Mathf.Min(maxScale, Mathf.Abs(Mathf.Max(1f, Mathf.Max(sLeaser.sprites[i].scaleX, sLeaser.sprites[i].scaleY)) * 8f));
			visibility[i] = sLeaser.sprites[i].isVisible && sLeaser.sprites[i].container != shadows && positions[i] != Vector2.zero && sLeaser.sprites[i].scale != 0f;
		}
	}

	public void UpdateActivationAnimation()
	{
		if (deactivate)
		{
			if (activationCounter >= activationTime)
			{
				delete();
			}
			else
			{
				activationCounter += Time.deltaTime;
			}
		}
		else if (!(activationCounter <= 0f))
		{
			activationCounter -= Time.deltaTime;
		}
	}

	public void delete()
	{
		deleteMe = true;
		room.drawableObjects.Remove(this);
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[positions.Length];
		string text = "RippleHybrid";
		switch (side)
		{
		case RippleSide.Ripple:
			text += "Ripple";
			break;
		case RippleSide.Both:
			text += "Both";
			break;
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i] = new FSprite("Futile_White");
			sLeaser.sprites[i].shader = Custom.rainWorld.Shaders[text];
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].SetPosition(positions[i]);
			sLeaser.sprites[i].scale = scales[i];
			Color black = Color.black;
			black.r = randomOffsets[i];
			black.g = 1f - activationCounter / activationTime;
			sLeaser.sprites[i].color = black;
			sLeaser.sprites[i].isVisible = visibility[i];
		}
		if (deleteMe)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		FContainer fContainer = rCam.ReturnFContainer("GrabShaders");
		FSprite[] sprites = sLeaser.sprites;
		foreach (FSprite node in sprites)
		{
			fContainer.AddChild(node);
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}
}
