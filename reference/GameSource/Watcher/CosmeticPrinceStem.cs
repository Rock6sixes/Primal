using RWCustom;
using UnityEngine;

namespace Watcher;

public class CosmeticPrinceStem : CosmeticSprite
{
	public PrinceBulb bulb;

	public Vector2 startTangent;

	public Vector2 endPos;

	public Vector2 endTangent;

	public float seed;

	public Color effectColor;

	public float endRotation;

	public float lastEndRotation;

	public float targetEndRotation;

	public Vector2 IdealPos
	{
		get
		{
			Vector2 direction = bulb.GetDirection(1f);
			Vector2 vector = Custom.PerpendicularVector(direction);
			return bulb.firstChunk.pos + endPos.x * direction + endPos.y * vector;
		}
	}

	public CosmeticPrinceStem(PrinceBulb bulb, Vector2 startTangent, Vector2 endPos, Vector2 endTangent)
	{
		this.bulb = bulb;
		this.endPos = endPos;
		this.endTangent = endTangent;
		this.startTangent = startTangent;
		seed = Random.value;
		pos = IdealPos;
		lastPos = IdealPos;
		effectColor = bulb.abstractPhysicalObject.world.region?.regionParams.corruptionEffectColor ?? Color.blue;
	}

	public override void Update(bool eu)
	{
		vel += (IdealPos - pos) * 0.03f;
		vel *= 0.8f;
		lastEndRotation = endRotation;
		endRotation = Mathf.Lerp(endRotation, targetEndRotation, 0.01f);
		if (Random.value < 0.01f)
		{
			targetEndRotation = Random.Range(-20f, 20f);
		}
		base.Update(eu);
	}

	public float StemRad(float t)
	{
		return (15f - 10f * t) * Mathf.Sqrt(Mathf.InverseLerp(1f, 0.9f, t));
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		string stemTexture = PrinceStem.GetStemTexture();
		TriangleMesh triangleMesh = TriangleMesh.MakeLongMesh(8, pointyTip: false, customColor: true, stemTexture);
		sLeaser.sprites[0] = triangleMesh;
		triangleMesh.shader = rCam.game.rainWorld.Shaders["PrinceStem"];
		for (int i = 0; i < triangleMesh.UVvertices.Length; i++)
		{
			triangleMesh.UVvertices[i].y = seed - 1f;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 direction = bulb.GetDirection(timeStacker);
		Vector2 vector = Custom.PerpendicularVector(direction);
		Vector2 vector2 = Vector2.Lerp(lastPos, pos, timeStacker);
		float degAng = Mathf.Lerp(lastEndRotation, endRotation, timeStacker);
		Vector2 vector3 = Vector2.Lerp(bulb.firstChunk.lastPos, bulb.firstChunk.pos, timeStacker);
		Vector2 cA = vector3 + direction * startTangent.x + vector * startTangent.y;
		Vector2 vector4 = vector2;
		Vector2 cB = vector4 - Custom.RotateAroundOrigo(direction * endTangent.x + vector * endTangent.y, degAng);
		TriangleMesh triangleMesh = sLeaser.sprites[0] as TriangleMesh;
		for (int i = 0; i < triangleMesh.vertices.Length; i += 2)
		{
			float num = (float)i / (float)(triangleMesh.vertices.Length - 2);
			Vector2 vector5 = Custom.Bezier(vector3, cA, vector4, cB, num);
			Vector2 vector6 = Custom.PerpendicularVector(Custom.BezierDerivative(vector3, cA, vector4, cB, num));
			float num2 = StemRad(num);
			triangleMesh.MoveVertice(i, vector5 + vector6 * num2 - camPos);
			triangleMesh.MoveVertice(i + 1, vector5 - vector6 * num2 - camPos);
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		TriangleMesh triangleMesh = sLeaser.sprites[0] as TriangleMesh;
		Color color = effectColor;
		for (int i = 0; i < triangleMesh.verticeColors.Length; i += 2)
		{
			float a = (float)i / (float)(triangleMesh.verticeColors.Length - 2);
			color.a = a;
			triangleMesh.verticeColors[i] = color;
			triangleMesh.verticeColors[i + 1] = color;
		}
		base.ApplyPalette(sLeaser, rCam, palette);
	}
}
