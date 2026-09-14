using System.Collections.Generic;
using UnityEngine;

namespace DevInterface;

public class FluxWaterfallControlPanel : Panel
{
	public class WaterfallSlider : Slider
	{
		public float minDelay = -3f;

		public float maxDelay = 3f;

		public WaterfallSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
			: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
		{
		}

		public override void Refresh()
		{
			base.Refresh();
			if (parentNode.parentNode is WaterFlowRepresentation waterFlowRepresentation && waterFlowRepresentation.pObj.data is PlacedObject.WaterFlowData waterFlowData)
			{
				switch (IDstring)
				{
				case "Waterfall_Flow":
					base.NumberText = waterFlowData.flow.ToString("0.00");
					RefreshNubPos(waterFlowData.flow);
					break;
				case "Waterfall_Pre_Delay":
					base.NumberText = waterFlowData.preDelay.ToString("0.0");
					RefreshNubPos(Mathf.InverseLerp(minDelay, maxDelay, waterFlowData.preDelay));
					break;
				case "Waterfall_Post_Delay":
					base.NumberText = waterFlowData.postDelay.ToString("0.0");
					RefreshNubPos(Mathf.InverseLerp(minDelay, maxDelay, waterFlowData.postDelay));
					break;
				}
			}
		}

		public override void NubDragged(float nubPos)
		{
			base.NubDragged(nubPos);
			if (parentNode.parentNode is WaterFlowRepresentation waterFlowRepresentation && waterFlowRepresentation.pObj.data is PlacedObject.WaterFlowData waterFlowData)
			{
				switch (IDstring)
				{
				case "Waterfall_Flow":
					waterFlowData.flow = nubPos;
					base.NumberText = waterFlowData.flow.ToString("0.00");
					break;
				case "Waterfall_Pre_Delay":
					waterFlowData.preDelay = Mathf.Lerp(minDelay, maxDelay, nubPos);
					base.NumberText = waterFlowData.preDelay.ToString("0.0");
					break;
				case "Waterfall_Post_Delay":
					waterFlowData.postDelay = Mathf.Lerp(minDelay, maxDelay, nubPos);
					base.NumberText = waterFlowData.postDelay.ToString("0.0");
					break;
				}
			}
		}
	}

	public Cycler modeCycler;

	public FluxWaterfallControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
		: base(owner, IDstring, parentNode, pos, new Vector2(250f, 85f), title)
	{
		subNodes.Add(new WaterfallSlider(owner, "Waterfall_Flow", this, new Vector2(5f, 5f), "Flow"));
		subNodes.Add(modeCycler = new Cycler(owner, "Waterfall_Mode", this, new Vector2(5f, 25f), 240f, "Mode: ", new List<string> { "Dynamic", "Static" }));
		subNodes.Add(new WaterfallSlider(owner, "Waterfall_Post_Delay", this, new Vector2(5f, 45f), "Post Delay"));
		subNodes.Add(new WaterfallSlider(owner, "Waterfall_Pre_Delay", this, new Vector2(5f, 65f), "Pre Delay"));
		modeCycler.currentAlternative = ((!((parentNode as WaterFlowRepresentation).pObj.data as PlacedObject.WaterFlowData).dynamic) ? 1 : 0);
		modeCycler.Text = modeCycler.baseName + modeCycler.alternatives[modeCycler.currentAlternative];
	}

	public override void Update()
	{
		base.Update();
		bool dynamic = modeCycler.alternatives[modeCycler.currentAlternative] == "Dynamic";
		((parentNode as WaterFlowRepresentation).pObj.data as PlacedObject.WaterFlowData).dynamic = dynamic;
	}
}
