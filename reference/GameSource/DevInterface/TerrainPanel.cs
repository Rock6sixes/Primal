using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DevInterface;

public class TerrainPanel : Panel, IDevUISignals
{
	public class FadeSlider : Slider
	{
		public int index;

		public FadeSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, int index)
			: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 65f)
		{
			this.index = index;
		}

		public override void Refresh()
		{
			base.Refresh();
			if (base.RoomSettings.terrainFadePalette == null)
			{
				base.NumberText = " - ";
				RefreshNubPos(0f);
			}
			else
			{
				base.NumberText = (int)(base.RoomSettings.terrainFadePalette.fades[index] * 100f) + "%";
				RefreshNubPos(base.RoomSettings.terrainFadePalette.fades[index]);
			}
		}

		public override void NubDragged(float nubPos)
		{
			if (base.RoomSettings.terrainFadePalette != null)
			{
				base.RoomSettings.terrainFadePalette.fades[index] = nubPos;
				if (owner.room.game.cameras[0].currentCameraPosition == index)
				{
					owner.room.game.cameras[0].ChangeTerrainPaletteFade();
				}
				Refresh();
			}
		}
	}

	public TerrainPaletteSelector paletteSelector;

	public Button paletteButton;

	public Button fadePaletteButton;

	public TerrainPanel(DevUI owner, string id, DevUINode parentNode, Vector2 pos)
		: base(owner, id, parentNode, pos, new Vector2(310f, 5f), "TERRAIN")
	{
		Vector2 p = new Vector2(5f, 5f);
		for (int num = owner.room.cameraPositions.Length - 1; num >= 0; num--)
		{
			Add(new FadeSlider(owner, "Fade_" + num, this, p, "Screen " + num + ":", num));
		}
		Add(fadePaletteButton = new Button(owner, "Pick_Fade_Palette", this, p, 300f, ""));
		p.y += 10f;
		size.y += 10f;
		Add(new RoomSettingSlider(owner, "Terrain_Sky_Fade", this, p, "Sky Fade", RoomSettingSlider.Type.TerrainSkyFade));
		Add(new RoomSettingSlider(owner, "Terrain_Depth", this, p, "Depth", RoomSettingSlider.Type.TerrainDepth));
		Add(new RoomSettingSlider(owner, "Terrain_Waves", this, p, "Waves", RoomSettingSlider.Type.TerrainWaves));
		Add(new RoomSettingSlider(owner, "Terrain_Edge_Radius", this, p, "Edge Radius", RoomSettingSlider.Type.TerrainEdgeRadius));
		Add(new RoomSettingSlider(owner, "Terrain_Grain", this, p, "Grain", RoomSettingSlider.Type.TerrainGrain));
		Add(new RoomSettingSlider(owner, "Terrain_Goo", this, p, "Goo Height", RoomSettingSlider.Type.TerrainGooHeight));
		Add(new RoomSettingSlider(owner, "Terrain_Stain_Height", this, p, "Stain Height", RoomSettingSlider.Type.TerrainStainHeight));
		Add(new RoomSettingSlider(owner, "Terrain_Stain_Brightness", this, p, "Stain Brightness", RoomSettingSlider.Type.TerrainStainBrightness));
		Add(new RoomSettingSlider(owner, "Terrain_Stain_Amount", this, p, "Stain Amount", RoomSettingSlider.Type.TerrainStainAmount));
		Add(new RoomSettingSlider(owner, "Terrain_Light", this, p, "Light", RoomSettingSlider.Type.TerrainLight));
		Add(paletteButton = new Button(owner, "Pick_Palette", this, p, 300f, ""));
		void Add(DevUINode child)
		{
			subNodes.Add(child);
			p.y += 20f;
			size.y += 20f;
		}
	}

	public List<string> GetPalettes(bool fade)
	{
		List<string> list = (from name in AssetManager.ListDirectory("terrainpalettes")
			where !name.EndsWith(".meta")
			select name).Select(Path.GetFileNameWithoutExtension).ToList();
		list.Insert(0, "NO PALETTE");
		if (!fade)
		{
			list.Insert(0, "INHERIT");
		}
		return list;
	}

	public void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		if (!(type == DevUISignalType.ButtonClick))
		{
			return;
		}
		if (sender.IDstring == "Pick_Palette" || sender.IDstring == "Pick_Fade_Palette")
		{
			if (paletteSelector == null)
			{
				bool fade = sender.IDstring == "Pick_Fade_Palette";
				paletteSelector = new TerrainPaletteSelector(owner, this, new Vector2(-300f, 0f), GetPalettes(fade), fade);
				subNodes.Add(paletteSelector);
			}
			else
			{
				paletteSelector.ClearSprites();
				subNodes.Remove(paletteSelector);
				paletteSelector = null;
			}
		}
		else if (paletteSelector != null)
		{
			if (paletteSelector.fade)
			{
				if (sender.IDstring == "NO PALETTE")
				{
					base.RoomSettings.terrainFadePalette = null;
				}
				else if (base.RoomSettings.terrainFadePalette == null)
				{
					base.RoomSettings.terrainFadePalette = new RoomSettings.TerrainFadePalette(sender.IDstring, owner.room.cameraPositions.Length);
				}
				else
				{
					base.RoomSettings.terrainFadePalette.palette = sender.IDstring;
				}
			}
			else if (sender.IDstring == "INHERIT")
			{
				base.RoomSettings.terrainPalette = null;
			}
			else
			{
				base.RoomSettings.TerrainPalette = sender.IDstring;
			}
			paletteSelector.ClearSprites();
			subNodes.Remove(paletteSelector);
			paletteSelector = null;
			owner.room.game.cameras[0].ReloadTerrainPalette();
		}
		Refresh();
	}

	public override void Refresh()
	{
		base.Refresh();
		string text = base.RoomSettings.TerrainPalette;
		if (base.RoomSettings.terrainPalette == null && base.RoomSettings.parent.terrainPalette != null && !base.RoomSettings.parent.isAncestor)
		{
			text = "<T> " + text;
		}
		else if (base.RoomSettings.terrainPalette == null)
		{
			text = "<A> " + text;
		}
		paletteButton.Text = text;
		fadePaletteButton.Text = "Fade: " + base.RoomSettings.terrainFadePalette?.palette;
	}
}
