using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

public class WaterCurrent : UpdatableAndDeletable, IDrawable
{
	public class WaterCurrentData : PlacedObject.ResizableObjectData
	{
		public float velocity;

		public float width;

		public WaterCurrentData(PlacedObject owner)
			: base(owner)
		{
			handlePos = new Vector2(0f, 200f);
			velocity = 20f;
			width = 100f;
		}

		public override void FromString(string s)
		{
			base.FromString(s);
			string[] array = Regex.Split(s, "~");
			velocity = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			width = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 4);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}", velocity, width);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public PlacedObject pObj;

	public float tilt;

	public WaterCurrentData Data => (WaterCurrentData)pObj.data;

	public WaterCurrent(PlacedObject pObj)
	{
		this.pObj = pObj;
		tilt = Random.Range(0.4f, 0.5f);
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[2]
		{
			new TriangleMesh.Triangle(0, 2, 1),
			new TriangleMesh.Triangle(2, 3, 1)
		};
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new TriangleMesh("Futile_White", tris, customColor: false)
		{
			shader = rCam.game.rainWorld.Shaders["WaterCurrent"]
		};
		AddToContainer(sLeaser, rCam, null);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		newContatiner.AddChild(sLeaser.sprites[0]);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		TriangleMesh triangleMesh = (TriangleMesh)sLeaser.sprites[0];
		float a = Custom.LerpMap(Mathf.Abs(Data.velocity), 20f, 200f, 0.1f, 1f) * (0f - Mathf.Sign(Data.velocity));
		float g = Data.handlePos.magnitude / 400f;
		triangleMesh.color = new Color(tilt, g, 0f, a);
		for (int i = 0; i < 4; i++)
		{
			float num = i % 2;
			float num2 = i / 2;
			triangleMesh.MoveVertice(i, pObj.pos - (num - 0.5f) * Data.width * 1.25f * Custom.PerpendicularVector(Data.handlePos) + Data.handlePos * num2 - camPos);
			triangleMesh.UVvertices[i].x = num;
			triangleMesh.UVvertices[i].y = num2 * Data.handlePos.magnitude / 400f;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		List<PhysicalObject>[] physicalObjects = room.physicalObjects;
		for (int i = 0; i < physicalObjects.Length; i++)
		{
			foreach (PhysicalObject item in physicalObjects[i])
			{
				BodyChunk[] bodyChunks = item.bodyChunks;
				foreach (BodyChunk bodyChunk in bodyChunks)
				{
					if (bodyChunk.submersion != 0f)
					{
						PushChunk(bodyChunk);
					}
				}
			}
		}
	}

	public void PushChunk(BodyChunk chunk)
	{
		Vector2 normalized = Data.handlePos.normalized;
		if (room.HasAnySolid(room.GetTilePosition(chunk.pos + normalized * (chunk.rad + 3f))))
		{
			return;
		}
		Vector2 vector = Custom.RotateAroundOrigo(chunk.pos - (pObj.pos + Data.handlePos / 2f), 0f - Custom.VecToDeg(normalized));
		float num = Data.width / 2f;
		float num2 = Data.handlePos.magnitude / 2f;
		float num3 = chunk.submersion * Mathf.InverseLerp(num + chunk.rad, num, Mathf.Abs(vector.x)) * Mathf.InverseLerp(num2 + chunk.rad, num2, Mathf.Abs(vector.y));
		num3 *= chunk.owner.currentAffectiveness;
		if (num3 == 0f)
		{
			return;
		}
		foreach (PlacedObject noCurrentZone in room.noCurrentZones)
		{
			float rad = ((PlacedObject.ResizableObjectData)noCurrentZone.data).Rad;
			num3 *= Mathf.InverseLerp(rad, rad + chunk.rad, Vector2.Distance(chunk.pos, noCurrentZone.pos));
		}
		Vector2 vector2 = Data.velocity / 300f * num3 * normalized;
		chunk.vel += vector2;
	}
}
