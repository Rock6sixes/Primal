using RWCustom;
using UnityEngine;

namespace Watcher.TardigradeCosmetics;

public class Plume : Template
{
	public Plume(TardigradeGraphics tGraphics, int startSprite)
		: base(tGraphics, startSprite)
	{
		numberOfSprites = 1;
	}

	public override void Update(TardigradeGraphics.Spine spine)
	{
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites[startSprite] = TriangleMesh.MakeLongMesh(4, pointyTip: false, customColor: true);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, TardigradeGraphics.Spine spine)
	{
		TardigradeGraphics.SpineData spineData = spine.GetSpineData(0.2f);
		Vector2 vector = spineData.pos + Custom.PerpendicularVector(spineData.dir) * (8f * bGraphics.bunny.flip);
		Vector2 normalized = (Custom.PerpendicularVector(spineData.dir) * bGraphics.bunny.flip - spineData.dir * 0.1f).normalized;
		Vector2 vector2 = spine.chunkPositions[0];
		float num = 0f;
		for (int i = 0; i < 4; i++)
		{
			float num2 = Mathf.InverseLerp(0f, 3f, i);
			float num3 = Mathf.Lerp(1.5f, 0.5f, num2);
			Vector2 vector3 = Custom.Bezier(vector, vector + normalized * 3f + spineData.dir * 5f, vector + normalized * 7f, vector + normalized * 5f - spineData.dir * 1f, num2);
			Vector2 normalized2 = (vector3 - vector2).normalized;
			Vector2 vector4 = Custom.PerpendicularVector(normalized2);
			float num4 = Vector2.Distance(vector3, vector2) / 5f;
			if (i == 0)
			{
				(sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 4, vector2 - vector4 * ((num3 + num) * 0.5f) - camPos);
				(sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector2 + vector4 * ((num3 + num) * 0.5f) - camPos);
			}
			else
			{
				(sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 4, vector2 - vector4 * ((num3 + num) * 0.5f) + normalized2 * num4 - camPos);
				(sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector2 + vector4 * ((num3 + num) * 0.5f) + normalized2 * num4 - camPos);
			}
			(sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector3 - vector4 * num3 - normalized2 * num4 - camPos);
			(sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector3 + vector4 * num3 - normalized2 * num4 - camPos);
			vector2 = vector3;
			num = num3;
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		for (int i = 0; i < 4; i++)
		{
			(sLeaser.sprites[startSprite] as TriangleMesh).verticeColors[i * 4 + 2] = Color.red;
			(sLeaser.sprites[startSprite] as TriangleMesh).verticeColors[i * 4 + 3] = Color.red;
		}
	}
}
