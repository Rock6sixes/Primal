using UnityEngine;

public class GraffitiSplat : CosmeticSprite
{
	public Color environmentColor;

	public Color color;

	public Rect rect;

	public int time;

	public GraffitiSplat(Color color, Vector2 pos, Rect rect)
	{
		this.color = color;
		this.rect = rect;
		base.pos = pos;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		time++;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		TriangleMesh triangleMesh = TriangleMesh.MakeGridMesh("Futile_White", 3);
		triangleMesh.shader = rCam.game.rainWorld.Shaders["MudDecal"];
		for (int i = 0; i < triangleMesh.UVvertices.Length; i++)
		{
			ref Vector2 reference = ref triangleMesh.UVvertices[i];
			reference.x = ((reference.x == 1f || reference.x == 0f) ? reference.x : 0.5f);
			reference.y = ((reference.y == 1f || reference.y == 0f) ? reference.y : 0.5f);
		}
		sLeaser.sprites[0] = triangleMesh;
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		TriangleMesh triangleMesh = sLeaser.sprites[0] as TriangleMesh;
		Rect rect = this.rect.CloneWithExpansion(90f);
		for (int i = 0; i < 4; i++)
		{
			float y = i switch
			{
				1 => pos.y, 
				2 => pos.y, 
				3 => rect.yMax, 
				_ => rect.yMin, 
			};
			for (int j = 0; j < 4; j++)
			{
				triangleMesh.MoveVertice(j + i * 4, new Vector2(j switch
				{
					1 => pos.x, 
					2 => pos.x, 
					3 => rect.xMax, 
					_ => rect.xMin, 
				}, y) - camPos);
			}
		}
		Color a = Color.Lerp(color, environmentColor, Mathf.Pow(Mathf.InverseLerp(160f, 800f, (float)time + timeStacker), 3f));
		sLeaser.sprites[0].color = Color.Lerp(a, environmentColor, rCam.currentPalette.darkness * 0f);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		environmentColor = palette.texture.GetPixel(11, 4);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Sand");
		}
		int num = 0;
		for (int i = 0; i < newContatiner.GetChildCount() && newContatiner.GetChildAt(i) is TriangleMesh triangleMesh; i++)
		{
			if (triangleMesh.shader != rCam.game.rainWorld.Shaders["MudDecal"])
			{
				break;
			}
			num++;
		}
		newContatiner.AddChildAtIndex(sLeaser.sprites[0], num);
	}
}
