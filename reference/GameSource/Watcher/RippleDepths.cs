using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class RippleDepths : CosmeticSprite
{
	public Counter rippleVisionsCooldown = new Counter(40);

	public List<PearlContent> visions;

	public string[] properties;

	public float strength;

	public bool useDarkEffect;

	public RippleDepths(Room room, bool darkEffect = false)
	{
		base.room = room;
		visions = new List<PearlContent>();
		properties = PearlContent.LoadPearlProperties(PearlContent.ResolveFilePath("ripplevisions"));
		useDarkEffect = darkEffect;
		rippleVisionsCooldown.Finish();
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		SpawnRippleVisions();
		RemoveRippleVisions();
	}

	public void SpawnRippleVisions()
	{
		if (strength == 1f || strength == 0f)
		{
			return;
		}
		rippleVisionsCooldown.Tick();
		if (!rippleVisionsCooldown.isFinished)
		{
			return;
		}
		for (int i = 0; i < 100; i++)
		{
			IntVector2 intVector = room.RandomTile();
			if (room.GetTile(intVector).IsSolid())
			{
				Vector2 position = room.MiddleOfTile(intVector);
				if (room.VisibleInAnyCameraScreenBounds(position) && !AnyVisionsClose(position))
				{
					visions.Add(PearlContent.Load(properties, Random.Range(0, 10000)));
					visions[visions.Count - 1].PlayInRoom(room, position);
					rippleVisionsCooldown.Reset();
					break;
				}
			}
		}
	}

	public void RemoveRippleVisions()
	{
		for (int num = visions.Count - 1; num > 0; num--)
		{
			if (visions[num].life.isFinished)
			{
				visions.RemoveAt(num);
			}
		}
	}

	public bool AnyVisionsClose(Vector2 position)
	{
		if (visions.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < visions.Count; i++)
		{
			if ((double)Vector2.Distance(position, visions[i].pos) < (double)visions[i].size.x * 0.8)
			{
				return true;
			}
		}
		return false;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		if (useDarkEffect)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["RippleDepths"];
			sLeaser.sprites[0].scaleX = Custom.rainWorld.options.ScreenSize.x / 16f + 0.5f;
			sLeaser.sprites[0].scaleY = Custom.rainWorld.options.ScreenSize.y / 16f + 0.5f;
			sLeaser.sprites[0].anchorX = 0f;
			sLeaser.sprites[0].anchorY = 0f;
			sLeaser.sprites[0].color = new Color(strength, 0f, 0f, 0f);
			rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[0]);
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		Shader.SetGlobalFloat("_ForceRippleTrailSwirl", 1f);
		if (rCam.rippleData != null)
		{
			rCam.rippleData.TrailAmount = 1f;
		}
	}

	public static void SpawnRippleDephts(Room room, float strength)
	{
		for (int i = 0; i < room.updateList.Count; i++)
		{
			if (room.updateList[i] is RippleDepths)
			{
				return;
			}
		}
		room.AddObject(new RippleDepths(room)
		{
			strength = strength
		});
	}
}
