using RWCustom;
using UnityEngine;

namespace ScavengerCosmetic;

public class TemplarBlindfold
{
	public Vector2 drawPos;

	public Vector2 drawRotationA;

	public Vector2 drawRotationB;

	public int totalSprites;

	public int firstSprite;

	public ScavengerGraphics owner;

	public float darkness;

	public Color blackColor;

	public SimpleSegment[] rag;

	public TemplarBlindfold(ScavengerGraphics owner, int firstSprite)
	{
		this.owner = owner;
		this.firstSprite = firstSprite;
		totalSprites = 3;
		rag = new SimpleSegment[Random.Range(3, 5)];
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites[firstSprite] = new FSprite("TemplarBlindfold0");
		sLeaser.sprites[firstSprite + 1] = new FSprite("TemplarBlindfold0");
		sLeaser.sprites[firstSprite + 2] = TriangleMesh.MakeLongMesh(3, pointyTip: false, customColor: false);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		darkness = rCam.room.Darkness(drawPos);
		darkness *= 1f - 0.5f * rCam.room.LightSourceExposure(drawPos);
		float num = Custom.VecToDeg(drawRotationB);
		int num2 = Custom.IntClamp(Mathf.RoundToInt(Mathf.Abs(num / 180f) * 8f), 0, 8);
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[firstSprite + i].element = Futile.atlasManager.GetElementWithName("TemplarBlindfold" + num2);
			sLeaser.sprites[firstSprite + i].scaleX = Mathf.Sign(num);
			sLeaser.sprites[firstSprite + i].anchorY = Custom.LerpMap(Mathf.Abs(num), 0f, 100f, 0.5f, 0.675f, 2.1f);
			sLeaser.sprites[firstSprite + i].anchorX = 0.5f - drawRotationB.x * 0.1f * Mathf.Sign(num);
			sLeaser.sprites[firstSprite + i].rotation = Custom.VecToDeg(drawRotationA);
			sLeaser.sprites[firstSprite + i].x = drawPos.x - camPos.x;
			sLeaser.sprites[firstSprite + i].y = drawPos.y - camPos.y;
		}
		sLeaser.sprites[firstSprite + 1].scaleY = 1.05f;
		sLeaser.sprites[firstSprite + 1].anchorY += 0.015f;
		Color color = Color.Lerp(new Color(0.83f, 0.77f, 0.65f), blackColor, Mathf.Pow(darkness, 2f) * 0.75f + 0.25f);
		sLeaser.sprites[firstSprite].color = color;
		sLeaser.sprites[firstSprite + 1].color = Color.Lerp(color, blackColor, Mathf.Lerp(0.25f, 1f, darkness));
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
	{
		if (newContainer == null)
		{
			newContainer = rCam.ReturnFContainer("Items");
		}
		newContainer.AddChild(sLeaser.sprites[firstSprite + 1]);
		newContainer.AddChild(sLeaser.sprites[firstSprite]);
		newContainer.AddChild(sLeaser.sprites[firstSprite + 2]);
	}

	public void Update()
	{
	}
}
