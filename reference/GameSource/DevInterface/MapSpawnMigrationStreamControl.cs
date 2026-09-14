using System.Collections.Generic;
using System.Globalization;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class MapSpawnMigrationStreamControl : BezierSplineControl
{
	public class WorldMigrationStreamPanel : Panel, IDevUISignals
	{
		public class ChooseNextStreamPanel : Panel
		{
			public ChooseNextStreamPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, Vector2 size, WorldSpawnMigrationStream stream, MapPage mapPage)
				: base(owner, IDstring, parentNode, pos, size, stream.name)
			{
				if (mapPage.owner.room.world.voidSpawnWorldAI?.worldMigrationStreams == null || mapPage.owner.room.world.voidSpawnWorldAI.worldMigrationStreams.Count <= 1)
				{
					subNodes.Add(new DevUILabel(owner, "label", this, new Vector2(5f, 5f), 50f, "no valid nextStreams :("));
					return;
				}
				Vector2 vector = new Vector2(5f, 5f);
				subNodes.Add(new Button(owner, "NextStream_None", this, vector, 90f, "None"));
				foreach (WorldSpawnMigrationStream worldMigrationStream in mapPage.owner.room.world.voidSpawnWorldAI.worldMigrationStreams)
				{
					if (worldMigrationStream != stream)
					{
						vector.y += 20f;
						subNodes.Add(new Button(owner, "NextStream_" + worldMigrationStream.name, this, vector, 90f, worldMigrationStream.name));
					}
				}
			}
		}

		public class ChooseDestRoomPanel : Panel
		{
			public static List<string> GetAllPossibleWarps(RainWorld rainWorld, string region)
			{
				region = region.ToLowerInvariant();
				List<string> list = new List<string>();
				if (rainWorld.regionWarpRooms.ContainsKey(region))
				{
					list.AddRange(rainWorld.regionWarpRooms[region]);
				}
				if (rainWorld.regionSpinningTopRooms.ContainsKey(region))
				{
					list.AddRange(rainWorld.regionSpinningTopRooms[region]);
				}
				for (int i = 0; i < list.Count; i++)
				{
					list[i] = list[i].Split(':')[0];
				}
				return list;
			}

			public ChooseDestRoomPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, Vector2 size, WorldSpawnMigrationStream stream, MapPage mapPage)
				: base(owner, IDstring, parentNode, pos, size, stream.name)
			{
				List<string> allPossibleWarps = GetAllPossibleWarps(mapPage.owner.room.game.rainWorld, mapPage.owner.room.world.name);
				if (allPossibleWarps.Count == 0)
				{
					subNodes.Add(new DevUILabel(owner, "label", this, new Vector2(5f, 5f), 50f, "no valid warpPoints :("));
					return;
				}
				Vector2 vector = new Vector2(5f, 5f);
				subNodes.Add(new Button(owner, "DestRoom_None", this, vector, 90f, "None"));
				foreach (string item in allPossibleWarps)
				{
					vector.y += 20f;
					subNodes.Add(new Button(owner, "DestRoom_" + item, this, vector, 90f, item));
				}
			}
		}

		public MapPage mapPage;

		public WorldSpawnMigrationStream stream;

		public SignalSlider widthSlider;

		public SignalSlider rateSlider;

		public Button[] layers = new Button[3];

		public Button nextStreamButton;

		public Button destRoomButton;

		public ChooseNextStreamPanel nextStreamPanel;

		public ChooseDestRoomPanel destRoomPanel;

		public string NextStreamText
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(stream.nextStreamName))
				{
					return stream.nextStreamName;
				}
				return "None";
			}
		}

		public string DestRoomText
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(stream.destRoom))
				{
					return stream.destRoom;
				}
				return "None";
			}
		}

		public WorldMigrationStreamPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, WorldSpawnMigrationStream stream, MapPage mapPage)
			: base(owner, IDstring, parentNode, pos + new Vector2(0f, -90f), new Vector2(190f, 110f), stream.name)
		{
			this.stream = stream;
			this.mapPage = mapPage;
			Vector2 vector = new Vector2(5f, 5f);
			widthSlider = new SignalSlider(owner, IDstring, this, vector, "width", inheritButton: false, 50f, 0f)
			{
				NumberText = stream.width.ToString()
			};
			widthSlider.RefreshNubPos(Mathf.InverseLerp(1f, 200f, stream.width));
			vector.y += 20f;
			rateSlider = new SignalSlider(owner, IDstring, this, vector, "rate", inheritButton: false, 50f, 0f)
			{
				NumberText = stream.rate.ToString()
			};
			rateSlider.RefreshNubPos(Mathf.InverseLerp(5f, 200f, stream.rate));
			vector.y += 20f;
			DevUILabel item = new DevUILabel(owner, "NextStreamLabel", this, vector, 75f, "Next Stream");
			nextStreamButton = new Button(owner, "NextStreamButton", this, vector + new Vector2(80f, 0f), 99f, NextStreamText);
			vector.y += 20f;
			DevUILabel item2 = new DevUILabel(owner, "DestRoomLabel", this, vector, 75f, "Warp Dest");
			destRoomButton = new Button(owner, "DestRoomButton", this, vector + new Vector2(80f, 0f), 99f, DestRoomText);
			vector.y += 20f;
			DevUILabel item3 = new DevUILabel(owner, "layers", this, vector, 50f, "Layers");
			vector.x += 61f;
			for (int i = 0; i < layers.Length; i++)
			{
				layers[i] = new Button(owner, "Layer_" + (i + 1), this, vector, 20f, LayerButtonText(i));
				vector.x += 25f;
			}
			subNodes.AddRange(new List<DevUINode> { widthSlider, rateSlider, item3, item, nextStreamButton, item2, destRoomButton });
			subNodes.AddRange(layers);
		}

		public string LayerButtonText(int i)
		{
			if (!stream.layers[i])
			{
				return "-";
			}
			return (i + 1).ToString();
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			if (sender == widthSlider)
			{
				float width = Mathf.Lerp(1f, 200f, float.Parse(message, NumberStyles.Any, CultureInfo.InvariantCulture));
				stream.width = width;
				widthSlider.NumberText = width.ToString();
				return;
			}
			if (sender == rateSlider)
			{
				int rate = (int)Mathf.Lerp(5f, 200f, float.Parse(message, NumberStyles.Any, CultureInfo.InvariantCulture));
				stream.rate = rate;
				rateSlider.NumberText = rate.ToString();
				return;
			}
			switch (sender.IDstring)
			{
			case "Layer_1":
				stream.layers[0] = !stream.layers[0];
				layers[0].Text = LayerButtonText(0);
				if (parentNode is MapSpawnMigrationStreamControl mapSpawnMigrationStreamControl2)
				{
					mapSpawnMigrationStreamControl2.ColorSpline();
				}
				break;
			case "Layer_2":
				stream.layers[1] = !stream.layers[1];
				layers[1].Text = LayerButtonText(1);
				if (parentNode is MapSpawnMigrationStreamControl mapSpawnMigrationStreamControl3)
				{
					mapSpawnMigrationStreamControl3.ColorSpline();
				}
				break;
			case "Layer_3":
				stream.layers[2] = !stream.layers[2];
				layers[2].Text = LayerButtonText(2);
				if (parentNode is MapSpawnMigrationStreamControl mapSpawnMigrationStreamControl)
				{
					mapSpawnMigrationStreamControl.ColorSpline();
				}
				break;
			case "NextStreamButton":
				if (nextStreamPanel == null && mapPage.owner.room.world.voidSpawnWorldAI?.worldMigrationStreams != null)
				{
					int num2 = mapPage.owner.room.world.voidSpawnWorldAI.worldMigrationStreams.Count * 20 + 10;
					nextStreamPanel = new ChooseNextStreamPanel(owner, "StreamPanel", this, new Vector2(-100f, 100f), new Vector2(100f, num2), stream, mapPage);
					subNodes.Add(nextStreamPanel);
				}
				else
				{
					nextStreamPanel.ClearSprites();
					subNodes.Remove(nextStreamPanel);
					nextStreamPanel = null;
				}
				break;
			case "DestRoomButton":
				if (destRoomPanel == null)
				{
					int num = (ChooseDestRoomPanel.GetAllPossibleWarps(mapPage.owner.room.game.rainWorld, mapPage.owner.room.world.name).Count + 1) * 20 + 10;
					destRoomPanel = new ChooseDestRoomPanel(owner, "DestRoomPanel", this, new Vector2(-100f, 100f), new Vector2(100f, num), stream, mapPage);
					subNodes.Add(destRoomPanel);
				}
				else
				{
					destRoomPanel.ClearSprites();
					subNodes.Remove(destRoomPanel);
					destRoomPanel = null;
				}
				break;
			}
			if (sender.IDstring.StartsWith("NextStream_"))
			{
				if (nextStreamPanel != null)
				{
					nextStreamPanel.ClearSprites();
					subNodes.Remove(nextStreamPanel);
					nextStreamPanel = null;
				}
				string text = sender.IDstring.Substring("NextStream_".Length);
				stream.nextStream = owner.room.world.voidSpawnWorldAI?.GetWorldMigrationStream(text);
				if (text == "None")
				{
					stream.nextStreamName = null;
					stream.nextStream = null;
				}
				else
				{
					stream.nextStreamName = text;
				}
				nextStreamButton.Text = NextStreamText;
			}
			if (sender.IDstring.StartsWith("DestRoom_"))
			{
				if (destRoomPanel != null)
				{
					destRoomPanel.ClearSprites();
					subNodes.Remove(destRoomPanel);
					destRoomPanel = null;
				}
				string text2 = sender.IDstring.Substring("DestRoom_".Length);
				stream.destRoom = text2;
				if (text2 == "None")
				{
					stream.destRoom = null;
				}
				else
				{
					stream.destRoom = text2;
				}
				destRoomButton.Text = DestRoomText;
			}
		}

		public override void Refresh()
		{
			base.Refresh();
			if (parentNode is MapSpawnMigrationStreamControl mapSpawnMigrationStreamControl)
			{
				mapSpawnMigrationStreamControl.RefreshLineSprite();
			}
		}
	}

	public MapPage mapPage;

	public WorldSpawnMigrationStream stream;

	public WorldMigrationStreamPanel panel;

	public FSprite lineSprite;

	public Vector2 PanPos
	{
		get
		{
			if (mapPage == null)
			{
				return Vector2.zero;
			}
			return mapPage.panPos;
		}
	}

	public override Vector2 absPos
	{
		get
		{
			return base.absPos + PanPos;
		}
		set
		{
			base.absPos = value - PanPos;
		}
	}

	public Color layerColor
	{
		get
		{
			if (stream == null)
			{
				return Color.white;
			}
			return new Color(stream.layers[2] ? 1f : 0f, stream.layers[0] ? 1f : 0f, stream.layers[1] ? 1f : 0f);
		}
	}

	public Color color => Color.Lerp(RainWorld.RippleColor, layerColor, 0.5f);

	public override void Move(Vector2 newPos)
	{
		base.Move(newPos - PanPos);
	}

	public MapSpawnMigrationStreamControl(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, WorldSpawnMigrationStream stream, MapPage mapPage)
		: base(owner, IDstring, parentNode, pos, stream.spline, originHandle: true)
	{
		this.mapPage = mapPage;
		this.stream = stream;
		MoveAllSpritesToMainStage(this);
		ColorSpline();
		subNodes.Add(panel = new WorldMigrationStreamPanel(owner, "streamPanel", this, stream.spline.posA, stream, mapPage));
		fSprites.Add(lineSprite = new FSprite("pixel"));
		lineSprite.anchorY = 0f;
		Futile.stage.AddChild(lineSprite);
	}

	public static void MoveAllSpritesToMainStage(DevUINode node)
	{
		foreach (FSprite fSprite in node.fSprites)
		{
			fSprite.RemoveFromContainer();
			Futile.stage.AddChild(fSprite);
		}
		foreach (DevUINode subNode in node.subNodes)
		{
			MoveAllSpritesToMainStage(subNode);
		}
	}

	public void ColorSpline()
	{
		foreach (FSprite fSprite in fSprites)
		{
			if (fSprite != ghostSprite && fSprite != lineSprite)
			{
				fSprite.color = color;
			}
		}
		foreach (DevUINode subNode in subNodes)
		{
			if (!(subNode is Panel) && subNode != posANode)
			{
				ColorAllSprites(subNode, color);
			}
		}
	}

	public static void ColorAllSprites(DevUINode node, Color color)
	{
		if (node is Handle handle)
		{
			handle.defaultColor = color;
		}
		foreach (FSprite fSprite in node.fSprites)
		{
			fSprite.color = color;
		}
		foreach (DevUINode subNode in node.subNodes)
		{
			ColorAllSprites(subNode, color);
		}
	}

	public override void SplitSegment(int segment, float posOnSegment)
	{
		base.SplitSegment(segment, posOnSegment);
		MoveAllSpritesToMainStage(subNodes[subNodes.Count - 1]);
		ColorAllSprites(subNodes[subNodes.Count - 1], color);
		FSprite fSprite = fSprites[fSprites.Count - 1];
		fSprite.RemoveFromContainer();
		Futile.stage.AddChild(fSprite);
		fSprite.color = color;
	}

	public override void AddMidpoint(int segment, BezierSpline.Midpoint midpoint)
	{
		base.AddMidpoint(segment, midpoint);
		MoveAllSpritesToMainStage(subNodes[subNodes.Count - 1]);
		ColorAllSprites(subNodes[subNodes.Count - 1], color);
		FSprite fSprite = fSprites[fSprites.Count - 1];
		fSprite.RemoveFromContainer();
		Futile.stage.AddChild(fSprite);
		fSprite.color = color;
	}

	public override void Refresh()
	{
		base.Refresh();
		RefreshLineSprite();
	}

	public void RefreshLineSprite()
	{
		if (lineSprite != null)
		{
			lineSprite.scaleY = Vector2.Distance(posANode.absPos, panel.absPos);
			lineSprite.rotation = Custom.AimFromOneVectorToAnother(posANode.absPos, panel.absPos);
			lineSprite.SetPosition(posANode.absPos);
		}
	}

	public override void Update()
	{
		bool dragged = posBNode.dragged;
		base.Update();
		if (!(posBNode.dragged || dragged))
		{
			return;
		}
		foreach (DevUINode subNode in mapPage.subNodes)
		{
			if (!(subNode is RoomPanel roomPanel))
			{
				continue;
			}
			foreach (var warpPoint in roomPanel.miniMap.warpPoints)
			{
				FSprite item = warpPoint.Item1;
				if (Vector2.Distance(posBNode.absPos, item.GetPosition()) < 20f)
				{
					posBNode.AbsMove(item.GetPosition());
					syncedSpline?.SetBezier(base.Segments - 1, GetBezier(base.Segments - 1));
					Refresh();
					stream.destRoom = roomPanel.roomRep.room.name;
					panel.destRoomButton.Text = panel.DestRoomText;
					return;
				}
			}
		}
	}
}
