using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public static class DebugDrawing
{
	public class DebugInstance : UpdatableAndDeletable, IDrawable
	{
		public struct RectInfo
		{
			public Vector2 center;

			public Vector2 size;

			public float rotation;

			public Color color;
		}

		public struct LabelInfo
		{
			public string text;

			public Vector2 pos;

			public Vector2 anchor;

			public float rotation;

			public Color color;
		}

		public List<RectInfo> rects = new List<RectInfo>();

		public int rectsDrawn;

		public List<LabelInfo> labels = new List<LabelInfo>();

		public int labelsDrawn;

		public void AddRect(Vector2 center, Vector2 size, float rotation, Color color)
		{
			rects.Add(new RectInfo
			{
				center = center,
				size = size,
				rotation = rotation,
				color = color
			});
		}

		public void AddLabel(string text, Vector2 pos, Vector2 anchor, float rotation, Color color)
		{
			labels.Add(new LabelInfo
			{
				text = text,
				pos = pos,
				anchor = anchor,
				rotation = rotation,
				color = color
			});
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			rects.RemoveRange(0, rectsDrawn);
			rectsDrawn = 0;
			labels.RemoveRange(0, labelsDrawn);
			labelsDrawn = 0;
			if (!room.BeingViewed)
			{
				rects.Clear();
				labels.Clear();
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[0];
			sLeaser.containers = new FContainer[2]
			{
				new FContainer(),
				new FContainer()
			};
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			DrawRects(sLeaser.containers[0], camPos);
			DrawLabels(sLeaser.containers[1], camPos);
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawRects(FContainer c, Vector2 camPos)
		{
			int i;
			for (i = 0; i < rects.Count; i++)
			{
				FSprite fSprite;
				if (i >= c.GetChildCount())
				{
					c.AddChild(fSprite = new FSprite("pixel"));
				}
				else
				{
					fSprite = (FSprite)c.GetChildAt(i);
				}
				RectInfo rectInfo = rects[i];
				fSprite.SetPosition(rectInfo.center - camPos);
				fSprite.rotation = rectInfo.rotation;
				fSprite.color = rectInfo.color;
				fSprite.scaleX = rectInfo.size.x;
				fSprite.scaleY = rectInfo.size.y;
				float magnitude = rectInfo.size.magnitude;
				Rect other = new Rect(fSprite.x - magnitude, fSprite.y - magnitude, magnitude * 2f, magnitude * 2f);
				fSprite.isVisible = new Rect(0f, 0f, Futile.screen.pixelWidth, Futile.screen.pixelHeight).Overlaps(other);
			}
			for (; i < c.GetChildCount(); i++)
			{
				c.GetChildAt(i).isVisible = false;
			}
			rectsDrawn = rects.Count;
		}

		public void DrawLabels(FContainer c, Vector2 camPos)
		{
			int i;
			for (i = 0; i < labels.Count; i++)
			{
				FLabel fLabel;
				if (i >= c.GetChildCount())
				{
					c.AddChild(fLabel = new FLabel("font", ""));
				}
				else
				{
					fLabel = (FLabel)c.GetChildAt(i);
				}
				LabelInfo labelInfo = labels[i];
				fLabel.text = labelInfo.text;
				fLabel.SetPosition(labelInfo.pos - camPos);
				fLabel.rotation = labelInfo.rotation;
				fLabel.color = labelInfo.color;
				fLabel.SetAnchor(labelInfo.anchor);
				fLabel.isVisible = true;
			}
			for (; i < c.GetChildCount(); i++)
			{
				c.GetChildAt(i).isVisible = false;
			}
			labelsDrawn = labels.Count;
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("HUD2");
			}
			newContatiner.AddChild(sLeaser.containers[0]);
			newContatiner.AddChild(sLeaser.containers[1]);
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}
	}

	public static DebugInstance GetInstance(Room room)
	{
		List<UpdatableAndDeletable> updateList = room.updateList;
		for (int num = updateList.Count - 1; num >= 0; num--)
		{
			if (updateList[num] is DebugInstance result)
			{
				return result;
			}
		}
		DebugInstance debugInstance = new DebugInstance();
		room.AddObject(debugInstance);
		return debugInstance;
	}

	public static void DrawLine(Room room, Vector2 start, Vector2 end, Color color, float thickness = 1f)
	{
		if (room != null)
		{
			Vector2 center = (start + end) / 2f;
			Vector2 size = new Vector2(thickness, Vector2.Distance(start, end));
			float rotation = Custom.VecToDeg(end - start);
			GetInstance(room).AddRect(center, size, rotation, color);
		}
	}

	public static void DrawArrow(Room room, Vector2 start, Vector2 end, Color color, float thickness = 1f, float headSize = 3f)
	{
		Vector2 normalized = (end - start).normalized;
		DrawLine(room, start, end, color, thickness);
		DrawLine(room, end, end + Custom.RotateAroundOrigo(normalized, 135f) * headSize, color, thickness);
		DrawLine(room, end, end + Custom.RotateAroundOrigo(normalized, -135f) * headSize, color, thickness);
	}

	public static void DrawCross(Room room, Vector2 pos, Color color, float size = 3f, float thickness = 1f)
	{
		if (room != null)
		{
			float num = size * 0.35f;
			DrawLine(room, pos + new Vector2(num, num), pos + new Vector2(0f - num, 0f - num), color, thickness);
			DrawLine(room, pos + new Vector2(0f - num, num), pos + new Vector2(num, 0f - num), color, thickness);
		}
	}

	public static void DrawText(Room room, string text, Vector2 pos, Color color)
	{
		DrawText(room, text, pos, Vector2.zero, 0f, color);
	}

	public static void DrawText(Room room, string text, Vector2 pos, Vector2 anchor, float rotation, Color color)
	{
		if (room != null)
		{
			GetInstance(room).AddLabel(text, pos, anchor, rotation, color);
		}
	}
}
