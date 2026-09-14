using System.Collections.Generic;
using Menu;
using UnityEngine;

namespace MoreSlugcats;

public class DyeableRect : RoundedRect
{
	public List<FSprite> tabInvisible;

	public Color color;

	public new bool filled;

	public bool tab;

	public DyeableRect(global::Menu.Menu menu, MenuObject owner, Vector2 pos, Vector2 size, bool filled = true)
		: base(menu, owner, pos, size, filled)
	{
		color = global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.MediumGrey);
		this.filled = filled;
		tab = false;
		if (filled)
		{
			tabInvisible = new List<FSprite>();
			tabInvisible.Add(sprites[2]);
			tabInvisible.Add(sprites[6]);
			tabInvisible.Add(sprites[7]);
			tabInvisible.Add(sprites[11]);
			tabInvisible.Add(sprites[15]);
			tabInvisible.Add(sprites[16]);
		}
	}

	public new int SideSprite(int side)
	{
		return (filled ? 9 : 0) + side;
	}

	public new int CornerSprite(int corner)
	{
		return (filled ? 9 : 0) + 4 + corner;
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (tab)
		{
			foreach (FSprite item in tabInvisible)
			{
				item.isVisible = false;
			}
		}
		for (int i = 0; i < 4; i++)
		{
			sprites[SideSprite(i)].color = color;
			sprites[CornerSprite(i)].color = color;
		}
	}
}
