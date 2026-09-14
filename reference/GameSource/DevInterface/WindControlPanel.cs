using System;
using UnityEngine;

namespace DevInterface;

public class WindControlPanel : Panel, IDevUISignals
{
	public class WindAffectGroupCycler : Button
	{
		public const string prefix = "Affects:";

		public string[] options;

		public PlacedObject.WindData Data
		{
			get
			{
				if (!(parentNode.parentNode is QuadObjectRepresentation quadObjectRepresentation) || !(quadObjectRepresentation.pObj.data is PlacedObject.WindData result))
				{
					return null;
				}
				return result;
			}
		}

		public WindAffectGroupCycler(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, float width)
			: base(owner, IDstring, parentNode, pos, width, string.Empty)
		{
			subNodes.Add(new DevUILabel(owner, "Title", this, new Vector2(-50f, 0f), 45f, "Affects:"));
			options = ExtEnum<PlacedObject.WindData.AffectGroup>.values.entries.ToArray();
		}

		public override void Refresh()
		{
			base.Refresh();
			SwitchOption(Data.affectGroup);
		}

		public void SwitchOption(PlacedObject.WindData.AffectGroup ag)
		{
			Data.affectGroup = ag;
			base.Text = ag.value;
		}

		public override void Clicked()
		{
			int num = Array.IndexOf(options, Data.affectGroup.value) + 1;
			SwitchOption(new PlacedObject.WindData.AffectGroup(options[num % options.Length]));
		}
	}

	public class WindStrengthSlider : Slider
	{
		public WindStrengthSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
			: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
		{
			subNodes[0].fSprites[0].scaleX -= 30f;
			subNodes[1].fSprites[0].scaleX += 30f;
			((DevUILabel)subNodes[1]).Move(((DevUILabel)subNodes[1]).pos + new Vector2(-30f, 0f));
		}

		public override void Refresh()
		{
			base.Refresh();
			if (parentNode.parentNode is QuadObjectRepresentation quadObjectRepresentation && quadObjectRepresentation.pObj.data is PlacedObject.WindData windData && IDstring == "Wind_Strength")
			{
				base.NumberText = windData.velocity.ToString("0.00");
				RefreshNubPos(windData.velocity * 0.5f + 0.5f);
			}
		}

		public override void NubDragged(float nubPos)
		{
			base.NubDragged(nubPos);
			if (parentNode.parentNode is QuadObjectRepresentation quadObjectRepresentation && quadObjectRepresentation.pObj.data is PlacedObject.WindData windData)
			{
				windData.velocity = nubPos * 2f - 1f;
				base.NumberText = windData.velocity.ToString("0.00");
			}
		}
	}

	public WindControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
		: base(owner, IDstring, parentNode, pos, new Vector2(250f, 50f), "Wind Rect")
	{
		subNodes.Add(new WindStrengthSlider(owner, "Wind_Strength", this, new Vector2(5f, 5f), "Velocity"));
		subNodes.Add(new WindAffectGroupCycler(owner, "Wind_Affect_Group", this, new Vector2(55f, 25f), 190f));
	}

	public void Signal(DevUISignalType type, DevUINode sender, string message)
	{
	}
}
