using System;
using System.Collections.Generic;
using System.Globalization;
using Menu.Remix;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class WatcherOptionInterface : OptionInterface
{
	public const int GENERAL_TAB = 0;

	public const int ACCESS_TAB = 1;

	public const int CHEAT_TAB = 2;

	public float gxo = 20f;

	public float gyo;

	public const float yoi = 30f;

	public const float yoi_big = 35f;

	public Color cheatColor = new Color(0.85f, 0.35f, 0.4f);

	public Configurable<bool>[] boolConfigOrder;

	public Configurable<int>[] spoilerIntConfigs;

	public OpLabel spoilerLabelGeneral;

	public OpHoldButton spoilerHoldGeneral;

	public OpLabel currentPresetLabel;

	public OpSimpleButton standardPresetBtn;

	public OpSimpleButton reducedPresetBtn;

	public OpSimpleButton minimalPresetBtn;

	public List<OpCheckBox> accessCheckboxes;

	public List<OpLabel> accessCheckboxLabels;

	public List<OpFloatSlider> accessSliders;

	public List<OpLabel> accessSliderLabels;

	public OpLabel spoilerLabelAccess;

	public OpHoldButton spoilerHoldAccess;

	public OpLabel cheatHomeTitle;

	public OpLabel cheatHomeLabel;

	public OpHoldButton cheatHomeHold;

	public Configurable<bool>[] cheatBoolConfigOrder;

	public override void Initialize()
	{
		base.Initialize();
		Tabs = new OpTab[3]
		{
			new OpTab(this, OptionInterface.Translate("General")),
			new OpTab(this, OptionInterface.Translate("Accessibility")),
			new OpTab(this, OptionInterface.Translate("Cheats"))
			{
				colorButton = cheatColor
			}
		};
		if (boolConfigOrder == null)
		{
			PopulateConfigurableArrays();
		}
		float num = (float)boolConfigOrder.Length * 30f + (float)spoilerIntConfigs.Length * 35f + 60f;
		gyo = 560f - num;
		OpLabel opLabel = new OpLabel(new Vector2(150f, gyo + num - 40f), new Vector2(300f, 30f), OptionInterface.Translate("The Watcher"), FLabelAlignment.Center, bigText: true);
		Tabs[0].AddItems(opLabel);
		PopulateWithConfigs(0, boolConfigOrder, null);
		spoilerLabelGeneral = new OpLabel(new Vector2(150f, gyo - (float)boolConfigOrder.Length * 30f - 60f), new Vector2(300f, 30f), Custom.ReplaceLineDelimeters(OptionInterface.Translate("The following options contain spoilers.")))
		{
			color = cheatColor
		};
		Tabs[0].AddItems(spoilerLabelGeneral);
		spoilerHoldGeneral = new OpHoldButton(new Vector2(250f, gyo - (float)boolConfigOrder.Length * 30f - 120f), new Vector2(120f, 30f), OptionInterface.Translate("CONTINUE"))
		{
			colorEdge = cheatColor
		};
		spoilerHoldGeneral.OnPressDone += CreateSpoilerLayerGeneral;
		Tabs[0].AddItems(spoilerHoldGeneral);
		OpLabel opLabel2 = new OpLabel(new Vector2(150f, 520f), new Vector2(300f, 30f), base.mod.LocalizedName, FLabelAlignment.Center, bigText: true);
		Tabs[1].AddItems(opLabel2);
		OpLabel opLabel3 = new OpLabel(new Vector2(150f, 420f), new Vector2(300f, 30f), Custom.ReplaceLineDelimeters(OptionInterface.Translate("The presets below can be used to quickly reset settings.") + Environment.NewLine + OptionInterface.Translate("These settings adjust the density of VFX for visual accessibility needs.")));
		Tabs[1].AddItems(opLabel3);
		standardPresetBtn = new OpSimpleButton(new Vector2(110f, 360f), new Vector2(120f, 30f), OptionInterface.Translate("STANDARD"))
		{
			description = OptionInterface.Translate("Heavy VFX that may not be suited to those with sensitivity to flashing lights or prone to eye strain")
		};
		reducedPresetBtn = new OpSimpleButton(new Vector2(250f, 360f), new Vector2(120f, 30f), OptionInterface.Translate("REDUCED"))
		{
			description = OptionInterface.Translate("Less flashing lights, and the density of cosmetic visual effects is reduced to make gameplay more clear")
		};
		minimalPresetBtn = new OpSimpleButton(new Vector2(390f, 360f), new Vector2(120f, 30f), OptionInterface.Translate("MINIMAL"))
		{
			description = OptionInterface.Translate("Some cosmetic visual effects are disabled entirely, and many others are significantly reduced")
		};
		standardPresetBtn.OnClick += PresetStandard;
		reducedPresetBtn.OnClick += PresetReduced;
		minimalPresetBtn.OnClick += PresetMinimal;
		Tabs[1].AddItems(standardPresetBtn);
		Tabs[1].AddItems(reducedPresetBtn);
		Tabs[1].AddItems(minimalPresetBtn);
		gyo = 40f;
		gxo = 40f;
		InitializeAccessibilityConfigs();
		gxo = 20f;
		currentPresetLabel = new OpLabel(new Vector2(150f, 320f), new Vector2(300f, 30f), GetCurrentPresetString());
		Tabs[1].AddItems(currentPresetLabel);
		spoilerLabelAccess = new OpLabel(new Vector2(150f, 240f), new Vector2(300f, 30f), Custom.ReplaceLineDelimeters(OptionInterface.Translate("The following options contain spoilers.")))
		{
			color = cheatColor
		};
		Tabs[1].AddItems(spoilerLabelAccess);
		spoilerHoldAccess = new OpHoldButton(new Vector2(250f, 180f), new Vector2(120f, 30f), OptionInterface.Translate("CONTINUE"))
		{
			colorEdge = cheatColor
		};
		spoilerHoldAccess.OnPressDone += CreateSpoilerLayerAccess;
		Tabs[1].AddItems(spoilerHoldAccess);
		for (int num2 = Watcher.floatPresets.Count - 1; num2 >= 0; num2--)
		{
			accessSliders[Watcher.floatPresets.Count - 1 - num2].value = ValueConverter.ConvertToString(Watcher.floatPresets[num2].config.Value);
		}
		for (int num3 = Watcher.boolPresets.Count - 1; num3 >= 0; num3--)
		{
			accessCheckboxes[Watcher.boolPresets.Count - 1 - num3].value = ValueConverter.ConvertToString(Watcher.boolPresets[num3].config.Value);
		}
		cheatBoolConfigOrder = new Configurable<bool>[6]
		{
			Watcher.chtUnlockCampaign,
			Watcher.chtUnlockArenas,
			Watcher.chtUnlockCreatures,
			Watcher.chtUnlockItems,
			Watcher.chtUnlockArenaClass,
			Watcher.chtUnlockSafari
		};
		cheatHomeTitle = new OpLabel(new Vector2(150f, 520f), new Vector2(300f, 30f), OptionInterface.Translate("Cheats"), FLabelAlignment.Center, bigText: true)
		{
			color = cheatColor
		};
		Tabs[2].AddItems(cheatHomeTitle);
		cheatHomeLabel = new OpLabel(new Vector2(150f, 430f), new Vector2(300f, 30f), Custom.ReplaceLineDelimeters(OptionInterface.Translate("This menu contains cheats to unlock content early.") + Environment.NewLine + OptionInterface.Translate("It is not recommended to use these on a first playthrough.") + Environment.NewLine + OptionInterface.Translate("Are you sure you want to continue?")))
		{
			color = cheatColor
		};
		Tabs[2].AddItems(cheatHomeLabel);
		cheatHomeHold = new OpHoldButton(new Vector2(250f, 370f), new Vector2(120f, 30f), OptionInterface.Translate("CONTINUE"))
		{
			colorEdge = cheatColor
		};
		cheatHomeHold.OnPressDone += CreateCheatLayer;
		Tabs[2].AddItems(cheatHomeHold);
	}

	public void PopulateConfigurableArrays()
	{
		boolConfigOrder = new Configurable<bool>[1] { Watcher.cfgWatcherTutorials };
		spoilerIntConfigs = new Configurable<int>[1] { Watcher.cfgForcedCampaignSeed };
	}

	public void CreateSpoilerLayerGeneral(UIfocusable trigger)
	{
		OpTab.DestroyItems(spoilerLabelGeneral, spoilerHoldGeneral);
		float num = (float)boolConfigOrder.Length * 30f + (float)spoilerIntConfigs.Length * 35f + 60f;
		gyo = 560f - num;
		gyo -= (float)boolConfigOrder.Length * 30f + 30f;
		PopulateWithConfigs(0, null, spoilerIntConfigs);
		ConfigConnector.FocusNewElement(spoilerIntConfigs[0].BoundUIconfig);
	}

	public override string ValidationString()
	{
		if (boolConfigOrder == null)
		{
			PopulateConfigurableArrays();
		}
		string text = "[" + base.mod.id + "]  ";
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < Watcher.boolPresets.Count; i++)
		{
			num += (int)Mathf.Pow(2f, i % 4) * (Watcher.boolPresets[i].config.Value ? 1 : 0);
			if (i % 4 == 3 || i == Watcher.boolPresets.Count - 1)
			{
				text += num.ToString("X");
				num2++;
				if (num2 % 4 == 0)
				{
					text += "  ";
				}
				num = 0;
			}
		}
		for (int j = 0; j < Watcher.floatPresets.Count; j++)
		{
			text = text + " " + Watcher.floatPresets[j].config.Value.ToString("0.00");
		}
		for (int k = 0; k < boolConfigOrder.Length; k++)
		{
			text = text + " " + (boolConfigOrder[k].Value ? "1" : "0");
		}
		for (int l = 0; l < spoilerIntConfigs.Length; l++)
		{
			text = text + " " + spoilerIntConfigs[l].Value;
		}
		text += " ";
		num = 0;
		Configurable<bool>[] array = new Configurable<bool>[5]
		{
			Watcher.chtUnlockCampaign,
			Watcher.chtUnlockArenas,
			Watcher.chtUnlockCreatures,
			Watcher.chtUnlockItems,
			Watcher.chtUnlockArenaClass
		};
		for (int m = 0; m < array.Length; m++)
		{
			num += (int)Mathf.Pow(2f, m % 4) * (array[m].Value ? 1 : 0);
			if (m % 4 == 3 || m == array.Length - 1)
			{
				text += num.ToString("X");
				num = 0;
			}
		}
		return text;
	}

	public string GetCurrentPresetString()
	{
		string text = OptionInterface.Translate("Currently Active Preset") + ": ";
		if (CheckPresetStandard())
		{
			return text + standardPresetBtn.text;
		}
		if (CheckPresetReduced())
		{
			return text + reducedPresetBtn.text;
		}
		if (CheckPresetMinimal())
		{
			return text + minimalPresetBtn.text;
		}
		return text + OptionInterface.Translate("NONE (User Defined)");
	}

	public void PopulateWithConfigs(int tabIndex, Configurable<bool>[] boolConfigs, Configurable<int>[] intConfigs)
	{
		float num = 0f;
		UIfocusable uIfocusable = null;
		if (intConfigs != null)
		{
			for (int num2 = intConfigs.Length - 1; num2 >= 0; num2--)
			{
				OpUpdown opUpdown = new OpUpdown(intConfigs[num2], new Vector2(gxo, gyo + num), 100f);
				opUpdown.description = OptionInterface.Translate(intConfigs[num2].info.description);
				if (uIfocusable != null)
				{
					UIfocusable.MutualVerticalFocusableBind(uIfocusable, opUpdown);
				}
				opUpdown.SetNextFocusable(UIfocusable.NextDirection.Left, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.CurrentTabButton));
				opUpdown.SetNextFocusable(UIfocusable.NextDirection.Right, opUpdown);
				Tabs[tabIndex].AddItems(opUpdown);
				uIfocusable = opUpdown;
				Tabs[tabIndex].AddItems(new OpLabel(gxo + 110f, gyo + num, OptionInterface.Translate(intConfigs[num2].info.Tags[0] as string))
				{
					bumpBehav = opUpdown.bumpBehav,
					description = opUpdown.description
				});
				num += 35f;
			}
		}
		if (boolConfigs == null)
		{
			return;
		}
		for (int num3 = boolConfigs.Length - 1; num3 >= 0; num3--)
		{
			bool flag = boolConfigs[num3] == Watcher.chtUnlockCampaign;
			OpCheckBox opCheckBox = new OpCheckBox(boolConfigs[num3], new Vector2(gxo, gyo + num));
			opCheckBox.description = OptionInterface.Translate(boolConfigs[num3].info.description);
			if (uIfocusable != null)
			{
				UIfocusable.MutualVerticalFocusableBind(uIfocusable, opCheckBox);
			}
			opCheckBox.SetNextFocusable(UIfocusable.NextDirection.Left, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.CurrentTabButton));
			opCheckBox.SetNextFocusable(UIfocusable.NextDirection.Right, opCheckBox);
			if (flag)
			{
				opCheckBox.colorEdge = cheatColor;
			}
			Tabs[tabIndex].AddItems(opCheckBox);
			uIfocusable = opCheckBox;
			OpLabel opLabel = new OpLabel(gxo + 40f, gyo + num, OptionInterface.Translate(boolConfigs[num3].info.Tags[0] as string))
			{
				bumpBehav = opCheckBox.bumpBehav,
				description = opCheckBox.description
			};
			if (flag)
			{
				opLabel.color = cheatColor;
			}
			Tabs[tabIndex].AddItems(opLabel);
			num += 30f;
		}
	}

	public void CreateCheatLayer(UIfocusable trigger)
	{
		OpTab.DestroyItems(cheatHomeLabel, cheatHomeHold);
		PopulateCheatConfigs(2, cheatBoolConfigOrder);
		ConfigConnector.FocusNewElement(cheatBoolConfigOrder[0].BoundUIconfig);
	}

	public void PopulateCheatConfigs(int tabIndex, Configurable<bool>[] boolConfigs)
	{
		float num = 0f;
		UIfocusable uIfocusable = null;
		float num2 = 460f;
		for (int i = 0; i < boolConfigs.Length; i++)
		{
			OpCheckBox opCheckBox = new OpCheckBox(boolConfigs[i], new Vector2(20f, num2 + num))
			{
				colorEdge = cheatColor
			};
			opCheckBox.description = OptionInterface.Translate(boolConfigs[i].info.description);
			opCheckBox.ShowConfig();
			if (uIfocusable != null)
			{
				UIfocusable.MutualVerticalFocusableBind(opCheckBox, uIfocusable);
			}
			opCheckBox.SetNextFocusable(UIfocusable.NextDirection.Left, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.CurrentTabButton));
			opCheckBox.SetNextFocusable(UIfocusable.NextDirection.Right, opCheckBox);
			uIfocusable = opCheckBox;
			Tabs[tabIndex].AddItems(opCheckBox);
			Tabs[tabIndex].AddItems(new OpLabel(60f, num2 + num, OptionInterface.Translate(boolConfigs[i].info.Tags[0] as string))
			{
				color = cheatColor
			});
			num -= 30f;
		}
	}

	public void InitializeAccessibilityConfigs()
	{
		if (Watcher.floatPresets[0].config.BoundUIconfig != null)
		{
			return;
		}
		accessSliders = new List<OpFloatSlider>();
		accessSliderLabels = new List<OpLabel>();
		accessCheckboxes = new List<OpCheckBox>();
		accessCheckboxLabels = new List<OpLabel>();
		float num = 0f;
		UIfocusable uIfocusable = null;
		for (int num2 = Watcher.floatPresets.Count - 1; num2 >= 0; num2--)
		{
			OpFloatSlider opFloatSlider = new OpFloatSlider(Watcher.floatPresets[num2].config, new Vector2(gxo + 15f, gyo + num - 5f), 100, 1);
			opFloatSlider.description = OptionInterface.Translate(Watcher.floatPresets[num2].config.info.description);
			if (uIfocusable != null)
			{
				UIfocusable.MutualVerticalFocusableBind(uIfocusable, opFloatSlider);
			}
			opFloatSlider.SetNextFocusable(UIfocusable.NextDirection.Left, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.CurrentTabButton));
			opFloatSlider.SetNextFocusable(UIfocusable.NextDirection.Right, opFloatSlider);
			opFloatSlider.OnChange += RefreshCurrentPresetLabel;
			uIfocusable = opFloatSlider;
			OpLabel opLabel = new OpLabel(gxo + 125f, gyo + num, OptionInterface.Translate(Watcher.floatPresets[num2].config.info.Tags[0] as string))
			{
				bumpBehav = opFloatSlider.bumpBehav,
				description = opFloatSlider.description
			};
			num += 45f;
			Tabs[1].AddItems(opFloatSlider);
			accessSliders.Add(opFloatSlider);
			opFloatSlider.Hide();
			accessSliderLabels.Add(opLabel);
			Tabs[1].AddItems(opLabel);
			opLabel.Hide();
		}
		for (int num3 = Watcher.boolPresets.Count - 1; num3 >= 0; num3--)
		{
			OpCheckBox opCheckBox = new OpCheckBox(Watcher.boolPresets[num3].config, new Vector2(gxo, gyo + num));
			opCheckBox.description = OptionInterface.Translate(Watcher.boolPresets[num3].config.info.description);
			if (uIfocusable != null)
			{
				UIfocusable.MutualVerticalFocusableBind(uIfocusable, opCheckBox);
			}
			opCheckBox.SetNextFocusable(UIfocusable.NextDirection.Left, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.CurrentTabButton));
			opCheckBox.SetNextFocusable(UIfocusable.NextDirection.Right, opCheckBox);
			opCheckBox.OnChange += RefreshCurrentPresetLabel;
			uIfocusable = opCheckBox;
			OpLabel opLabel2 = new OpLabel(gxo + 40f, gyo + num, OptionInterface.Translate(Watcher.boolPresets[num3].config.info.Tags[0] as string))
			{
				bumpBehav = opCheckBox.bumpBehav,
				description = opCheckBox.description
			};
			num += 30f;
			Tabs[1].AddItems(opCheckBox);
			accessCheckboxes.Add(opCheckBox);
			opCheckBox.Hide();
			Tabs[1].AddItems(opLabel2);
			accessCheckboxLabels.Add(opLabel2);
			opLabel2.Hide();
		}
	}

	public void PopulateWithAccessibilityConfigs()
	{
		for (int i = 0; i < accessSliders.Count; i++)
		{
			accessSliders[i].Show();
			accessSliderLabels[i].Show();
		}
		for (int j = 0; j < accessCheckboxes.Count; j++)
		{
			accessCheckboxes[j].Show();
			accessCheckboxLabels[j].Show();
			if (j == accessCheckboxes.Count - 1)
			{
				UIfocusable.MutualVerticalFocusableBind(accessCheckboxes[j], standardPresetBtn);
			}
		}
	}

	public void CreateSpoilerLayerAccess(UIfocusable trigger)
	{
		OpTab.DestroyItems(spoilerLabelAccess, spoilerHoldAccess);
		PopulateWithAccessibilityConfigs();
		ConfigConnector.FocusNewElement(accessCheckboxes[0]);
	}

	public void RefreshCurrentPresetLabel()
	{
		currentPresetLabel.text = GetCurrentPresetString();
	}

	public void PresetStandard(UIfocusable trigger)
	{
		for (int i = 0; i < Watcher.boolPresets.Count; i++)
		{
			if (Watcher.boolPresets[i].config.BoundUIconfig != null)
			{
				Watcher.boolPresets[i].config.BoundUIconfig.value = ValueConverter.ConvertToString(Watcher.boolPresets[i].standardValue);
			}
		}
		for (int j = 0; j < Watcher.floatPresets.Count; j++)
		{
			if (Watcher.floatPresets[j].config.BoundUIconfig != null)
			{
				Watcher.floatPresets[j].config.BoundUIconfig.value = ValueConverter.ConvertToString(Watcher.floatPresets[j].standardValue);
			}
		}
		RefreshCurrentPresetLabel();
	}

	public void PresetReduced(UIfocusable trigger)
	{
		for (int i = 0; i < Watcher.boolPresets.Count; i++)
		{
			if (Watcher.boolPresets[i].config.BoundUIconfig != null)
			{
				Watcher.boolPresets[i].config.BoundUIconfig.value = ValueConverter.ConvertToString(Watcher.boolPresets[i].reducedValue);
			}
		}
		for (int j = 0; j < Watcher.floatPresets.Count; j++)
		{
			if (Watcher.floatPresets[j].config.BoundUIconfig != null)
			{
				Watcher.floatPresets[j].config.BoundUIconfig.value = ValueConverter.ConvertToString(Watcher.floatPresets[j].reducedValue);
			}
		}
		RefreshCurrentPresetLabel();
	}

	public void PresetMinimal(UIfocusable trigger)
	{
		for (int i = 0; i < Watcher.boolPresets.Count; i++)
		{
			if (Watcher.boolPresets[i].config.BoundUIconfig != null)
			{
				Watcher.boolPresets[i].config.BoundUIconfig.value = ValueConverter.ConvertToString(Watcher.boolPresets[i].minimalValue);
			}
		}
		for (int j = 0; j < Watcher.floatPresets.Count; j++)
		{
			if (Watcher.floatPresets[j].config.BoundUIconfig != null)
			{
				Watcher.floatPresets[j].config.BoundUIconfig.value = ValueConverter.ConvertToString(Watcher.floatPresets[j].minimalValue);
			}
		}
		RefreshCurrentPresetLabel();
	}

	public bool CheckPresetStandard()
	{
		for (int i = 0; i < Watcher.boolPresets.Count; i++)
		{
			if (Watcher.boolPresets[i].config.BoundUIconfig != null && Watcher.boolPresets[i].config.BoundUIconfig.value != ValueConverter.ConvertToString(Watcher.boolPresets[i].standardValue))
			{
				return false;
			}
		}
		for (int j = 0; j < Watcher.floatPresets.Count; j++)
		{
			if (Watcher.floatPresets[j].config.BoundUIconfig != null)
			{
				float result;
				bool num = float.TryParse(Watcher.floatPresets[j].config.BoundUIconfig.value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
				float num2 = float.Parse(ValueConverter.ConvertToString(Watcher.floatPresets[j].standardValue), NumberStyles.Any, CultureInfo.InvariantCulture);
				if (!num || result != num2)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool CheckPresetReduced()
	{
		for (int i = 0; i < Watcher.boolPresets.Count; i++)
		{
			if (Watcher.boolPresets[i].config.BoundUIconfig != null && Watcher.boolPresets[i].config.BoundUIconfig.value != ValueConverter.ConvertToString(Watcher.boolPresets[i].reducedValue))
			{
				return false;
			}
		}
		for (int j = 0; j < Watcher.floatPresets.Count; j++)
		{
			if (Watcher.floatPresets[j].config.BoundUIconfig != null)
			{
				float result;
				bool num = float.TryParse(Watcher.floatPresets[j].config.BoundUIconfig.value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
				float num2 = float.Parse(ValueConverter.ConvertToString(Watcher.floatPresets[j].reducedValue), NumberStyles.Any, CultureInfo.InvariantCulture);
				if (!num || result != num2)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool CheckPresetMinimal()
	{
		for (int i = 0; i < Watcher.boolPresets.Count; i++)
		{
			if (Watcher.boolPresets[i].config.BoundUIconfig != null && Watcher.boolPresets[i].config.BoundUIconfig.value != ValueConverter.ConvertToString(Watcher.boolPresets[i].minimalValue))
			{
				return false;
			}
		}
		for (int j = 0; j < Watcher.floatPresets.Count; j++)
		{
			if (Watcher.floatPresets[j].config.BoundUIconfig != null)
			{
				float result;
				bool num = float.TryParse(Watcher.floatPresets[j].config.BoundUIconfig.value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
				float num2 = float.Parse(ValueConverter.ConvertToString(Watcher.floatPresets[j].minimalValue), NumberStyles.Any, CultureInfo.InvariantCulture);
				if (!num || result != num2)
				{
					return false;
				}
			}
		}
		return true;
	}
}
