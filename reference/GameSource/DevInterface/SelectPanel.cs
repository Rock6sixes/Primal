using RWCustom;
using UnityEngine;

namespace DevInterface;

public class SelectPanel : Panel
{
	public string[] items;

	public int perpage;

	public int columns;

	public int currentOffset;

	public SelectPanel(DevUI owner, string id, DevUINode parentNode, Vector2 pos, Vector2 size, string name, string[] items)
		: base(owner, id, parentNode, pos, size, name)
	{
		this.items = items;
		currentOffset = 0;
		perpage = 36;
		columns = 2;
		PopulateItems(currentOffset);
	}

	public virtual void PopulateItems(int offset)
	{
		currentOffset = offset;
		foreach (DevUINode subNode in subNodes)
		{
			subNode.ClearSprites();
		}
		subNodes.Clear();
		if (items.Length > perpage)
		{
			subNodes.Add(new Button(owner, "BackPage99289..?/~", this, new Vector2(5f, size.y - 25f - 20f * ((float)(perpage / columns) + 1f)), 145f, "Previous"));
			subNodes.Add(new Button(owner, "NextPage99289..?/~", this, new Vector2(155f, size.y - 25f - 20f * ((float)(perpage / columns) + 1f)), 145f, "Next"));
		}
		IntVector2 intVector = new IntVector2(0, 0);
		for (int i = currentOffset; i < items.Length && i < currentOffset + perpage; i++)
		{
			subNodes.Add(new Button(owner, items[i], this, new Vector2(5f + (float)intVector.x * 150f, size.y - 25f - 20f * (float)intVector.y), 145f, items[i]));
			intVector.y++;
			if (intVector.y >= (int)Mathf.Floor((float)perpage / (float)columns))
			{
				intVector.x++;
				intVector.y = 0;
			}
		}
	}

	public void PrevPage()
	{
		currentOffset -= perpage;
		if (currentOffset < 0)
		{
			currentOffset = 0;
		}
		PopulateItems(currentOffset);
	}

	public void NextPage()
	{
		currentOffset += perpage;
		if (currentOffset > items.Length)
		{
			currentOffset = perpage * (int)Mathf.Floor((float)items.Length / (float)perpage);
		}
		PopulateItems(currentOffset);
	}
}
