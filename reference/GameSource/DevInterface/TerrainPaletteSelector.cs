using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class TerrainPaletteSelector : Panel
{
	public bool fade;

	public TerrainPaletteSelector(DevUI owner, DevUINode parentNode, Vector2 pos, List<string> options, bool fade)
		: base(owner, "Terrain_Palette_Panel", parentNode, pos, new Vector2(20f + (float)Mathf.CeilToInt((float)options.Count / 16f) * 150f, 345f), "Select a palette")
	{
		this.fade = fade;
		IntVector2 intVector = new IntVector2(0, 0);
		for (int i = 0; i < options.Count; i++)
		{
			subNodes.Add(new Button(owner, options[i], this, new Vector2(5f + (float)intVector.x * 150f, size.y - 25f - 20f * (float)intVector.y), 145f, options[i]));
			intVector.y++;
			if (intVector.y > 16)
			{
				intVector.x++;
				intVector.y = 0;
			}
		}
	}
}
