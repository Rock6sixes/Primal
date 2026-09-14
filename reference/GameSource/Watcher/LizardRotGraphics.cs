using System.Collections.Generic;
using LizardCosmetics;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class LizardRotGraphics : Template, DaddyGraphics.IHaveRotGraphics
{
	public DaddyGraphics.DaddyLegGraphic[] legs;

	public DaddyGraphics.DaddyDeadLeg[] deadLegs;

	public DaddyGraphics.Eye[] eyes;

	public Vector2[] eyePositions;

	public float[] eyeRads;

	public float[] eyeBubbleUp;

	public float[] deadLegYs;

	public bool legsVisible = true;

	public DaddyLongLegs.IHaveRotParts RotPartsOwner => lGraphics.lizard.rotModule;

	public bool ApplyPaletteInDraw => lGraphics.lizard.Template.type == CreatureTemplate.Type.WhiteLizard;

	public float BumpSize => 1f;

	public float Digesting => 0f;

	public DaddyTentacle[] Tentacles => lGraphics.lizard.rotModule.tentacles;

	public DaddyGraphics.Eye[] Eyes => eyes;

	public LizardRotGraphics(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		spritesOverlap = SpritesOverlap.BodySurface;
		int num = startSprite;
		LizardState.RotType rotType = lGraphics.lizard.LizardState.rotType;
		legs = new DaddyGraphics.DaddyLegGraphic[((DaddyLongLegs.IHaveRotParts)lGraphics.lizard.rotModule).RotTentacles.Length];
		for (int i = 0; i < legs.Length; i++)
		{
			legs[i] = new DaddyGraphics.DaddyLegGraphic(lGraphics, this, i, num);
			num += legs[i].sprites;
		}
		List<int> list = new List<int>();
		deadLegs = new DaddyGraphics.DaddyDeadLeg[Random.Range((!(rotType == LizardState.RotType.Slight)) ? 1 : 0, Random.Range(2, 4))];
		deadLegYs = new float[deadLegs.Length];
		for (int j = 0; j < deadLegs.Length; j++)
		{
			if (list.Count == 0)
			{
				for (int k = 0; k < lGraphics.owner.bodyChunks.Length; k++)
				{
					list.Add(k);
				}
			}
			deadLegs[j] = new DaddyGraphics.DaddyDeadLeg(lGraphics, this, j, Random.Range(4, Random.Range(8, 17)) / ((!(rotType != LizardState.RotType.Full)) ? 1 : 2), num, lGraphics.lizard.bodyChunks[list[Random.Range(0, list.Count)]], lGraphics.lizard.bodyChunks[Random.Range(0, lGraphics.lizard.bodyChunks.Length)]);
			list.Remove(deadLegs[j].bodyParts[0].connection.index);
			num += deadLegs[j].sprites;
			deadLegYs[j] = 0f - (Random.value * 1.5f - 1f);
		}
		eyes = new DaddyGraphics.Eye[Random.Range(2, Random.Range(4, 7))];
		eyeRads = new float[eyes.Length];
		eyeBubbleUp = new float[eyes.Length];
		eyePositions = new Vector2[eyes.Length];
		for (int l = 0; l < eyes.Length; l++)
		{
			eyePositions[l] = new Vector2(Mathf.Pow(Random.value, 2f) * 0.95f, 0f - (Random.value * 1.5f - 1f));
			eyeRads[l] = 2f + Random.value * lGraphics.SpinePosition(eyePositions[l].x, 1f).rad * 1.1f;
			eyes[l] = new DaddyGraphics.Eye(lGraphics, this, l, eyeRads[l], HDmode: false, num);
			num += eyes[l].sprites;
		}
		numberOfSprites = num - startSprite;
	}

	public override void Update()
	{
		_ = (RotPartsOwner as LizardRotModule).transforming;
		if (lGraphics.lizard.rotModule.daddyMode)
		{
			for (int i = 0; i < legs.Length; i++)
			{
				legs[i].Update();
			}
		}
		for (int j = 0; j < deadLegs.Length; j++)
		{
			deadLegs[j].Update();
		}
		for (int k = 0; k < eyes.Length; k++)
		{
			eyes[k].rad = eyeRads[k] * ((RotPartsOwner as LizardRotModule).daddyMode ? 1.25f : 1f);
			eyes[k].Update();
		}
		if (!(RotPartsOwner as LizardRotModule).transforming)
		{
			return;
		}
		for (int l = 0; l < eyes.Length; l++)
		{
			if ((double)Random.value < 0.2)
			{
				eyeBubbleUp[l] += Random.value * 0.7f;
			}
			if (eyeBubbleUp[l] >= 1f)
			{
				eyeBubbleUp[l] = 0f;
			}
			eyes[l].rad = eyeRads[l] + eyeBubbleUp[l] * eyeRads[l] * 0.75f;
		}
	}

	public override void Reset()
	{
		base.Reset();
		Vector2 pos = lGraphics.lizard.mainBodyChunk.pos;
		for (int i = 0; i < legs.Length; i++)
		{
			legs[i].Reset(pos);
		}
		for (int j = 0; j < deadLegs.Length; j++)
		{
			deadLegs[j].Reset(pos);
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		for (int i = 0; i < legs.Length; i++)
		{
			legs[i].InitiateSprites(sLeaser, rCam);
		}
		for (int j = 0; j < deadLegs.Length; j++)
		{
			deadLegs[j].InitiateSprites(sLeaser, rCam);
		}
		for (int k = 0; k < eyes.Length; k++)
		{
			eyes[k].InitiateSprites(sLeaser, rCam);
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (lGraphics.lizard.rotModule.daddyMode)
		{
			for (int i = 0; i < legs.Length; i++)
			{
				legs[i].DrawSprite(sLeaser, rCam, timeStacker, camPos);
			}
		}
		if (legsVisible != lGraphics.lizard.rotModule.daddyMode)
		{
			legsVisible = lGraphics.lizard.rotModule.daddyMode;
			for (int j = 0; j < legs.Length; j++)
			{
				for (int k = 0; k < legs[j].sprites; k++)
				{
					sLeaser.sprites[legs[j].firstSprite + k].isVisible = legsVisible;
				}
			}
		}
		for (int l = 0; l < deadLegs.Length; l++)
		{
			deadLegs[l].DrawSprite(sLeaser, rCam, timeStacker, camPos);
		}
		for (int m = 0; m < eyes.Length; m++)
		{
			eyes[m].DrawSprite(sLeaser, rCam, timeStacker, camPos);
			sLeaser.sprites[eyes[m].firstSprite].scale = (eyes[m].rad * 1.1f + 2f) / 8f;
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		for (int i = 0; i < legs.Length; i++)
		{
			legs[i].ApplyPalette(sLeaser, rCam, palette);
		}
		for (int j = 0; j < deadLegs.Length; j++)
		{
			deadLegs[j].ApplyPalette(sLeaser, rCam, palette);
		}
		for (int k = 0; k < eyes.Length; k++)
		{
			eyes[k].ApplyPalette(sLeaser, rCam, palette);
		}
	}

	public float TubeRadius(DaddyGraphics.DaddyTubeGraphic tube)
	{
		return 2f;
	}

	public Color RotBodyColor()
	{
		return lGraphics.BodyColor(0f);
	}

	public Color TubeBodyColor(DaddyGraphics.DaddyTubeGraphic tube)
	{
		return RotBodyColor();
	}

	public Color TubeEffectColor(DaddyGraphics.DaddyTubeGraphic tube)
	{
		return lGraphics.lizard.rotModule.rotColor;
	}

	public Color TubeBumpColor(DaddyGraphics.DaddyTubeGraphic tube)
	{
		return lGraphics.lizard.rotModule.rotColor;
	}

	public Vector2 TubeDeadLegPushOutFromPos(DaddyGraphics.DaddyTubeGraphic tube)
	{
		return lGraphics.lizard.bodyChunks[1].pos - Custom.PerpendicularVector(Custom.DirVec(lGraphics.lizard.firstChunk.pos, lGraphics.lizard.bodyChunks[2].pos)) * (deadLegYs[tube.index] * lGraphics.depthRotation * 50f);
	}

	public Color EyeBulbColor(int index)
	{
		return lGraphics.BodyColor(eyePositions[index].x);
	}

	public Vector2 EyePosition(int index, float timeStacker)
	{
		LizardGraphics.LizardSpineData lizardSpineData = lGraphics.SpinePosition(eyePositions[index].x, timeStacker);
		return lizardSpineData.pos + lizardSpineData.perp * ((lizardSpineData.rad + 2f) * lizardSpineData.depthRotation * eyePositions[index].y);
	}

	public Vector2 EyePushOutFromPos(int index, float timeStacker)
	{
		LizardGraphics.LizardSpineData lizardSpineData = lGraphics.SpinePosition(eyePositions[index].x, timeStacker);
		return lizardSpineData.pos - lizardSpineData.perp * (200f * lizardSpineData.depthRotation * eyePositions[index].y);
	}

	public void TentOwnerFeelSomethingWithTentacle(Tracker.CreatureRepresentation creatureRep, Vector2 feelPos)
	{
		Vector2 pos = lGraphics.lizard.firstChunk.pos;
		float num = float.MinValue;
		int num2 = -1;
		for (int i = 0; i < eyes.Length; i++)
		{
			float num3 = Vector2.Dot(Custom.DirVec(pos, EyePosition(i, 1f)), Custom.DirVec(pos, feelPos));
			if (eyes[i].soundSource != null)
			{
				num3 -= 1f;
			}
			if (eyes[i].creatureRep != null)
			{
				num3 -= 2f;
			}
			if (num3 > num)
			{
				num = num3;
				num2 = i;
			}
		}
		eyes[num2].ReactToCreature(creatureRep);
		lGraphics.lizard.room.PlaySound(SoundID.Daddy_React_To_Tentacle_Touch, pos, lGraphics.lizard.abstractPhysicalObject);
	}
}
