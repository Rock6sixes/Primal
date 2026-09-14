using UnityEngine;
using Watcher;

namespace DevInterface;

public class SandGrubNetworkControlPanel : ConsumableRepresentation.ConsumableControlPanel
{
	public class NetworkSlider : Slider
	{
		public ConsumableRepresentation ConsumableRep => parentNode.parentNode as ConsumableRepresentation;

		public SandGrubNetwork.NetworkData Data => ConsumableRep.pObj.data as SandGrubNetwork.NetworkData;

		public NetworkSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
			: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
		{
		}

		public override void Refresh()
		{
			base.Refresh();
			float nubPos = 0f;
			switch (IDstring)
			{
			case "Network_Density":
				nubPos = Data.density;
				base.NumberText = Mathf.RoundToInt(Data.density * 100f) + "%";
				break;
			case "Network_Chance":
				nubPos = Data.chance;
				base.NumberText = Mathf.RoundToInt(Data.chance * 100f) + "%";
				break;
			case "Network_Adult_Chance":
				nubPos = Data.adultChance;
				base.NumberText = Mathf.RoundToInt(Data.adultChance * 100f) + "%";
				break;
			}
			RefreshNubPos(nubPos);
		}

		public override void NubDragged(float nubPos)
		{
			switch (IDstring)
			{
			case "Network_Density":
				Data.density = nubPos;
				break;
			case "Network_Chance":
				Data.chance = nubPos;
				break;
			case "Network_Adult_Chance":
				Data.adultChance = nubPos;
				break;
			}
			parentNode.parentNode.Refresh();
			Refresh();
		}
	}

	public SandGrubNetworkControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string name)
		: base(owner, IDstring, parentNode, pos, name)
	{
		size.y += 60f;
		subNodes.Add(new NetworkSlider(owner, "Network_Adult_Chance", this, new Vector2(5f, 85f), "Adult Chance: "));
		subNodes.Add(new NetworkSlider(owner, "Network_Density", this, new Vector2(5f, 65f), "Density: "));
		subNodes.Add(new NetworkSlider(owner, "Network_Chance", this, new Vector2(5f, 45f), "Spawn Chance: "));
	}
}
