using System;
using RWCustom;
using UnityEngine;

public class FollowPathVisualizer : UpdatableAndDeletable, IDrawable
{
	public delegate MovementConnection PathFollower(WorldCoordinate originPos, bool actuallyFollowingThisPath);

	public Creature creature;

	public PathFinder pathFinder;

	public PathFollower followPath;

	public int pathLength = 16;

	public static TriangleMesh.Triangle[] tris;

	public int w = 70;

	public int h = 40;

	public bool ShouldDelete
	{
		get
		{
			if (!creature.slatedForDeletetion && creature.room == room && !creature.abstractCreature.slatedForDeletion)
			{
				return pathFinder.AI.lastRoom != room.abstractRoom.index;
			}
			return true;
		}
	}

	public FollowPathVisualizer(PathFinder pathFinder)
	{
		this.pathFinder = pathFinder;
		creature = pathFinder.AI.creature.realizedCreature;
		try
		{
			followPath = (PathFollower)Delegate.CreateDelegate(typeof(PathFollower), pathFinder, "FollowPath", ignoreCase: false, throwOnBindFailure: true);
		}
		catch
		{
			followPath = null;
		}
		if (tris != null)
		{
			return;
		}
		tris = new TriangleMesh.Triangle[w * h * 2];
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < h; i++)
		{
			for (int j = 0; j < w; j++)
			{
				tris[num] = new TriangleMesh.Triangle(num2, num2 + 2, num2 + 1);
				tris[num + 1] = new TriangleMesh.Triangle(num2 + 1, num2 + 2, num2 + 3);
				num += 2;
				num2 += 4;
			}
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (ShouldDelete)
		{
			Destroy();
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1 + pathLength];
		sLeaser.sprites[0] = new TriangleMesh("Futile_White", tris, customColor: true);
		for (int i = 0; i < pathLength; i++)
		{
			float num = (float)i / (float)(pathLength - 1);
			sLeaser.sprites[1 + i] = new FSprite("pixel")
			{
				anchorY = 0f,
				scaleX = 2f,
				color = Color.Lerp(Color.green, Color.black, 0.5f * num)
			};
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("HUD");
		}
		FSprite[] sprites = sLeaser.sprites;
		foreach (FSprite node in sprites)
		{
			newContatiner.AddChild(node);
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (!sLeaser.deleteMeNextFrame && (base.slatedForDeletetion || room != rCam.room))
		{
			sLeaser.CleanSpritesAndRemove();
		}
		else
		{
			if (room == null || ShouldDelete)
			{
				return;
			}
			TriangleMesh triangleMesh = sLeaser.sprites[0] as TriangleMesh;
			Vector2[] vertices = triangleMesh.vertices;
			Color[] verticeColors = triangleMesh.verticeColors;
			int num = Mathf.Max(pathFinder.coveredArea.left, Mathf.FloorToInt(camPos.x / 20f));
			int num2 = Mathf.Min(pathFinder.coveredArea.right, Mathf.FloorToInt((camPos.x + 1366f) / 20f));
			int num3 = Mathf.Max(pathFinder.coveredArea.bottom, Mathf.FloorToInt(camPos.y / 20f));
			int num4 = Mathf.Min(pathFinder.coveredArea.top, Mathf.FloorToInt((camPos.y + 768f) / 20f));
			int index = room.abstractRoom.index;
			int num5 = 0;
			for (int i = num3; i <= num4; i++)
			{
				for (int j = num; j <= num2; j++)
				{
					float num6 = 8f;
					float num7 = 8f;
					Color color = new Color(1f, 1f, 1f, 0.5f);
					WorldCoordinate coord = new WorldCoordinate(index, j, i, -1);
					PathFinder.PathingCell pathingCell = pathFinder.PathingCellAtWorldCoordinate(coord);
					if (pathingCell.generation >= 0)
					{
						color = new HSLColor((float)pathingCell.generation * 0.32f % 1f, 1f, 0.5f).rgb;
						color.a = 0.5f;
						float num8 = Mathf.Max(1f - 0.2f * (float)(pathFinder.pathGeneration - pathingCell.generation), 0.2f);
						num6 *= num8;
						num7 *= num8;
					}
					else
					{
						color.a = 0f;
					}
					if (!pathingCell.reachable)
					{
						color.a *= 0.2f;
					}
					else if (!pathingCell.possibleToGetBackFrom)
					{
						color.a *= 0.5f;
					}
					Vector2 vector = room.MiddleOfTile(j, i) - camPos;
					vertices[num5] = vector + new Vector2(0f - num6, 0f - num7);
					vertices[num5 + 1] = vector + new Vector2(num6, 0f - num7);
					vertices[num5 + 2] = vector + new Vector2(0f - num6, num7);
					vertices[num5 + 3] = vector + new Vector2(num6, num7);
					verticeColors[num5] = color;
					verticeColors[num5 + 1] = color;
					verticeColors[num5 + 2] = color;
					verticeColors[num5 + 3] = color;
					num5 += 4;
				}
			}
			Array.Clear(triangleMesh.vertices, num5, triangleMesh.vertices.Length - num5);
			triangleMesh.Refresh();
			if (followPath == null)
			{
				return;
			}
			IntVector2 tilePosition = room.GetTilePosition(creature.mainBodyChunk.pos);
			if (Input.GetKey(KeyCode.Mouse1))
			{
				tilePosition = room.GetTilePosition((Vector2)Input.mousePosition + camPos);
			}
			MovementConnection movementConnection = followPath(new WorldCoordinate(index, tilePosition.x, tilePosition.y, -1), actuallyFollowingThisPath: false);
			for (int k = 0; k < pathLength; k++)
			{
				if (movementConnection != default(MovementConnection))
				{
					Vector2 vector2 = room.MiddleOfTile(movementConnection.StartTile);
					Vector2 vector3 = room.MiddleOfTile(movementConnection.DestTile);
					sLeaser.sprites[1 + k].isVisible = movementConnection.destinationCoord.TileDefined && movementConnection.startCoord.TileDefined;
					sLeaser.sprites[1 + k].SetPosition(vector2 - camPos);
					sLeaser.sprites[1 + k].rotation = Custom.AimFromOneVectorToAnother(vector2, vector3);
					sLeaser.sprites[1 + k].scaleY = Vector2.Distance(vector2, vector3);
					movementConnection = followPath(movementConnection.destinationCoord, actuallyFollowingThisPath: false);
				}
				else
				{
					sLeaser.sprites[1 + k].isVisible = false;
				}
			}
		}
	}
}
