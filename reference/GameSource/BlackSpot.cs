using RWCustom;
using UnityEngine;

public class BlackSpot : CosmeticSprite
{
	public PlacedObject pObj;

	public BlackSpot(PlacedObject pObj)
	{
		this.pObj = pObj;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[2]
		{
			new TriangleMesh.Triangle(0, 1, 2),
			new TriangleMesh.Triangle(0, 2, 3)
		};
		sLeaser.sprites = new FSprite[1]
		{
			new TriangleMesh("Futile_White", tris, customColor: true)
			{
				shader = rCam.game.rainWorld.Shaders["BlackSpot"]
			}
		};
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Sand"));
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		TriangleMesh triangleMesh = (TriangleMesh)sLeaser.sprites[0];
		Vector2 vector = pObj.pos - camPos;
		PlacedObject.QuadObjectData quadObjectData = (PlacedObject.QuadObjectData)pObj.data;
		triangleMesh.MoveVertice(0, vector);
		triangleMesh.MoveVertice(1, vector + quadObjectData.handles[0]);
		triangleMesh.MoveVertice(2, vector + quadObjectData.handles[1]);
		triangleMesh.MoveVertice(3, vector + quadObjectData.handles[2]);
		for (int i = 0; i < 4; i++)
		{
			triangleMesh.UVvertices[i] = (triangleMesh.vertices[i] - vector) / 200f;
		}
		for (int j = 0; j < 4; j++)
		{
			Vector2 l = triangleMesh.vertices[j];
			Vector2 l2 = triangleMesh.vertices[(j + 1) % 4];
			for (int k = 0; k < 4; k++)
			{
				triangleMesh.verticeColors[k][j] = Mathf.Abs(Custom.DistanceToLine(triangleMesh.vertices[k], l, l2) / 15f);
			}
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}
}
