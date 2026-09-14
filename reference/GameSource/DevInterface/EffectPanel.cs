using System.Globalization;
using UnityEngine;

namespace DevInterface;

public class EffectPanel : Panel
{
	public class EffectPanelSlider : Slider
	{
		public int index;

		public RoomSettings.RoomEffect effect => (parentNode as EffectPanel).effect;

		public EffectPanelSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
			: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
		{
			int num = IDstring.LastIndexOf('_');
			if (num != -1 && effect.extraAmounts != null && int.TryParse(IDstring.Substring(num + 1), NumberStyles.Any, CultureInfo.InvariantCulture, out var result) && result > 0 && result <= effect.extraAmounts.Length)
			{
				index = result;
			}
		}

		public override void Refresh()
		{
			base.Refresh();
			float num = 0f;
			string iDstring = IDstring;
			if (iDstring != null && iDstring == "Amount_Slider")
			{
				num = effect.amount;
			}
			else if (index > 0)
			{
				num = effect.extraAmounts[index - 1];
			}
			base.NumberText = (int)(num * 100f) + "%";
			RefreshNubPos(num);
		}

		public override void NubDragged(float nubPos)
		{
			if (!effect.inherited)
			{
				string iDstring = IDstring;
				if (iDstring != null && iDstring == "Amount_Slider")
				{
					effect.amount = nubPos;
					if (effect.type == RoomSettings.RoomEffect.Type.VoidMelt)
					{
						owner.room.game.cameras[0].levelGraphic.alpha = effect.amount;
						if (owner.room.game.cameras[0].fullScreenEffect != null)
						{
							owner.room.game.cameras[0].fullScreenEffect.alpha = effect.amount;
						}
					}
					if (effect.type == RoomSettings.RoomEffect.Type.HeatWave)
					{
						owner.room.game.cameras[0].levelGraphic.alpha = effect.amount / 2f;
					}
				}
				else if (index > 0)
				{
					effect.extraAmounts[index - 1] = nubPos;
				}
				if (effect.type == RoomSettings.RoomEffect.Type.ModifyEffectColorA || effect.type == RoomSettings.RoomEffect.Type.ModifyEffectColorB)
				{
					owner.room.game.cameras[0].ApplyEffectColorsToAllPaletteTextures(base.RoomSettings.EffectColorA, base.RoomSettings.EffectColorB);
				}
			}
			Refresh();
		}
	}

	public RoomSettings.RoomEffect effect;

	public EffectPanel(DevUI owner, DevUINode parentNode, Vector2 pos, RoomSettings.RoomEffect effect)
		: base(owner, "Effect_Panel_" + effect.type.ToString(), parentNode, pos, new Vector2(250f, 10f + 20f * (float)RoomSettings.RoomEffect.GetSliderCount(effect.type)), "")
	{
		int sliderCount = RoomSettings.RoomEffect.GetSliderCount(effect.type);
		this.effect = effect;
		Move(effect.panelPosition);
		subNodes.Add(new EffectPanelSlider(owner, "Amount_Slider", this, new Vector2(5f, 5f + (float)(sliderCount - 1) * 20f), RoomSettings.RoomEffect.GetSliderName(effect.type, 0) + ": "));
		for (int i = 1; i < sliderCount; i++)
		{
			subNodes.Add(new EffectPanelSlider(owner, "Amount_Slider_" + i, this, new Vector2(5f, 5f + 20f * (float)(sliderCount - i - 1)), RoomSettings.RoomEffect.GetSliderName(effect.type, i) + ": "));
		}
	}

	public override void Move(Vector2 newPos)
	{
		base.Move(newPos);
		effect.panelPosition = absPos;
	}

	public override void Refresh()
	{
		base.Refresh();
		base.Title = "EFFECT " + (effect.inherited ? "<T> " : "") + effect.type.ToString();
		fSprites[0].color = (effect.overWrite ? new Color(0.5f, 0f, 0f) : (effect.inherited ? new Color(0f, 0f, 0.5f) : new Color(0f, 0.5f, 0f)));
	}
}
