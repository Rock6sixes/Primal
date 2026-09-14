using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class CompetitiveFilterRepresentation : ResizeableObjectRepresentation
{
	public class CompetitiveFilterControlPanel : Panel, IDevUISignals
	{
		public ButtonWithSelectPanel groupButton;

		public PlacedObject.CompetitiveFilterData data;

		public SelectPanel MakeSpawnSelect(ButtonWithSelectPanel button)
		{
			string[] allSpawnFilters = GetAllSpawnFilters();
			SelectPanel selectPanel = new SelectPanel(button.owner, "Select_Group_Panel", button, new Vector2(50f, 15f) - button.pos, new Vector2(155f, 80f + (float)allSpawnFilters.Length * 20f), "Select Group", allSpawnFilters);
			selectPanel.columns = 1;
			selectPanel.PopulateItems(0);
			return selectPanel;
		}

		public string[] GetAllSpawnFilters()
		{
			List<string> list = new List<string>();
			list.Add("NONE");
			foreach (ArenaCreatureSpawner.FilterSpawner spawnFilter in (owner.game.session as CompetitiveGameSession).spawnFilters)
			{
				list.Add(spawnFilter.name);
			}
			return list.ToArray();
		}

		public CompetitiveFilterControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, PlacedObject.CompetitiveFilterData data)
			: base(owner, IDstring, parentNode, pos, new Vector2(150f, 26f), "CompFilter")
		{
			this.data = data;
			groupButton = new ButtonWithSelectPanel(owner, "filterSelect", this, new Vector2(5f, 5f), 140f, data.name, MakeSpawnSelect);
			subNodes.Add(groupButton);
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			string iDstring = sender.IDstring;
			if (iDstring != null && iDstring == "filterSelect")
			{
				data.name = message;
				groupButton.Text = data.name;
			}
			Refresh();
		}
	}

	public int lineSprite;

	public CompetitiveFilterControlPanel controlPanel;

	public CompetitiveFilterRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj)
		: base(owner, IDstring, parentNode, pObj, "CompFilter", showRing: true)
	{
		subNodes.Add(controlPanel = new CompetitiveFilterControlPanel(owner, "CompFilter_Panel", this, new Vector2(0f, 100f), pObj.data as PlacedObject.CompetitiveFilterData));
		controlPanel.pos = (pObj.data as PlacedObject.CompetitiveFilterData).panelPos;
		fSprites.Add(new FSprite("pixel"));
		lineSprite = fSprites.Count - 1;
		owner.placedObjectsContainer.AddChild(fSprites[lineSprite]);
		fSprites[lineSprite].anchorY = 0f;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(lineSprite, absPos);
		fSprites[lineSprite].scaleY = controlPanel.pos.magnitude;
		fSprites[lineSprite].rotation = Custom.AimFromOneVectorToAnother(absPos, controlPanel.absPos);
		(pObj.data as PlacedObject.CompetitiveFilterData).panelPos = controlPanel.pos;
	}
}
