using DevInterface;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class FlameJetRepresentation : PlacedObjectRepresentation
{
	public class FlameJetPanel : Panel, IDevUISignals
	{
		public class FlameJetSlider : Slider
		{
			public FlameJetSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void NubDragged(float nubPos)
			{
				string iDstring = IDstring;
				if (iDstring != null)
				{
					FlameJet.FlameJetData data = (parentNode.parentNode as FlameJetRepresentation).data;
					switch (iDstring)
					{
					case "Intensity":
						data.intensity = nubPos;
						data.obj.intensity = nubPos;
						break;
					case "Width":
						data.width = nubPos;
						data.obj.width = nubPos;
						break;
					case "Temperature":
						data.temperature = nubPos;
						data.obj.temperature = nubPos;
						break;
					case "IntensityAnimSpeed":
						data.intensityAnimSpeed = nubPos;
						data.obj.intensityAnimSpeed = nubPos;
						break;
					case "IntensityAnimOffset":
						data.intensityAnimOffset = nubPos;
						data.obj.intensityAnimOffset = nubPos;
						break;
					case "TemperatureAnimSpeed":
						data.temperatureAnimSpeed = nubPos;
						data.obj.temperatureAnimSpeed = nubPos;
						break;
					case "TemperatureAnimOffset":
						data.temperatureAnimOffset = nubPos;
						data.obj.temperatureAnimOffset = nubPos;
						break;
					case "IntensityMin":
						data.obj.intensityMin = nubPos;
						data.intensityMin = data.obj.intensityMin;
						break;
					case "IntensityMax":
						data.obj.intensityMax = nubPos;
						data.intensityMax = data.obj.intensityMax;
						break;
					}
					switch (iDstring)
					{
					case "TemperatureMin":
						data.obj.temperatureMin = nubPos;
						data.temperatureMin = data.obj.temperatureMin;
						break;
					case "TemperatureMax":
						data.obj.temperatureMax = nubPos;
						data.temperatureMax = data.obj.temperatureMax;
						break;
					case "FireVolumeMax":
						data.obj.fireVolumeMax = nubPos;
						data.fireVolumeMax = data.obj.fireVolumeMax;
						break;
					case "SmokeVolumeMax":
						data.obj.smokeVolumeMax = nubPos;
						data.smokeVolumeMax = data.obj.smokeVolumeMax;
						break;
					}
					parentNode.parentNode.Refresh();
					Refresh();
				}
			}

			public override void Refresh()
			{
				base.Refresh();
				float nubPos = 0f;
				string iDstring = IDstring;
				FlameJet.FlameJetData data = (parentNode.parentNode as FlameJetRepresentation).data;
				switch (iDstring)
				{
				case "Intensity":
					nubPos = data.intensity;
					base.NumberText = nubPos + " ";
					break;
				case "Width":
					nubPos = data.width;
					base.NumberText = nubPos + " ";
					break;
				case "Temperature":
					nubPos = data.temperature;
					base.NumberText = nubPos + " ";
					break;
				case "IntensityAnimSpeed":
					nubPos = data.intensityAnimSpeed;
					base.NumberText = nubPos + " ";
					break;
				case "IntensityAnimOffset":
					nubPos = data.intensityAnimOffset;
					base.NumberText = nubPos + " ";
					break;
				case "TemperatureAnimSpeed":
					nubPos = data.temperatureAnimSpeed;
					base.NumberText = nubPos + " ";
					break;
				case "TemperatureAnimOffset":
					nubPos = data.temperatureAnimOffset;
					base.NumberText = nubPos + " ";
					break;
				case "IntensityMin":
					nubPos = data.intensityMin;
					base.NumberText = nubPos + " ";
					break;
				case "IntensityMax":
					nubPos = data.intensityMax;
					base.NumberText = nubPos + " ";
					break;
				case "TemperatureMin":
					nubPos = data.temperatureMin;
					base.NumberText = nubPos + " ";
					break;
				case "TemperatureMax":
					nubPos = data.temperatureMax;
					base.NumberText = nubPos + " ";
					break;
				case "FireVolumeMax":
					nubPos = data.fireVolumeMax;
					base.NumberText = nubPos + " ";
					break;
				case "SmokeVolumeMax":
					nubPos = data.smokeVolumeMax;
					base.NumberText = nubPos + " ";
					break;
				}
				RefreshNubPos(nubPos);
			}
		}

		public bool soundControlsVisible;

		public FlameJet.FlameJetData data => (parentNode as FlameJetRepresentation).data;

		public FlameJetPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 45f), "Flame Jet")
		{
			AddPanelElements();
		}

		public void RefreshUI()
		{
			for (int num = subNodes.Count - 1; num > -1; num--)
			{
				subNodes[num].ClearSprites();
				subNodes.Pop();
			}
			AddPanelElements();
		}

		public string InteractionTypeText()
		{
			if (data.lethality >= 2)
			{
				return "Lethal";
			}
			if (data.lethality >= 1)
			{
				return "Exhaustion";
			}
			return "Cosmetic";
		}

		public string SoundButtonText()
		{
			if (soundControlsVisible)
			{
				return "Hide Sound";
			}
			return "Show Sound";
		}

		public void AddPanelElements()
		{
			size.y = -15f;
			AddSoundControls();
			subNodes.Add(new Button(owner, "ShowSound", this, new Vector2(125f, base.panelStep), 115f, SoundButtonText()));
			subNodes.Add(new Button(owner, "Lethality", this, new Vector2(5f, size.y), 115f, InteractionTypeText()));
			subNodes.Add(new Button(owner, "LinkTempToIntens", this, new Vector2(125f, base.panelStep), 115f, $"Temp=Intens: {data.linkTempToIntens}"));
			subNodes.Add(new Button(owner, "ActiveDuring", this, new Vector2(5f, size.y), 115f, "Active: " + data.activeDuring.value));
			AddTemperatureControls();
			AddIntensityAnimationControls();
			subNodes.Add(new Button(owner, "IntensityAnim", this, new Vector2(5f, base.panelStep), 245f, "Intensity animation: " + data.intensityAnim.value));
			subNodes.Add(new FlameJetSlider(owner, "Intensity", this, new Vector2(5f, base.panelStep), "Intensity:"));
			subNodes.Add(new FlameJetSlider(owner, "Width", this, new Vector2(5f, base.panelStep), "Jet Width:"));
			size.y += 20f;
		}

		public void AddSoundControls()
		{
			_ = (parentNode as FlameJetRepresentation).data;
			if (soundControlsVisible)
			{
				subNodes.Add(new FlameJetSlider(owner, "SmokeVolumeMax", this, new Vector2(5f, base.panelStep), "Smoke Volume:"));
				subNodes.Add(new FlameJetSlider(owner, "FireVolumeMax", this, new Vector2(5f, base.panelStep), "Fire Volume:"));
			}
		}

		public void AddIntensityAnimationControls()
		{
			if (!((parentNode as FlameJetRepresentation).data.intensityAnim == FlameJet.Animation.None))
			{
				subNodes.Add(new FlameJetSlider(owner, "IntensityAnimOffset", this, new Vector2(5f, base.panelStep), "Intens. Anim Offset:"));
				subNodes.Add(new FlameJetSlider(owner, "IntensityAnimSpeed", this, new Vector2(5f, base.panelStep), "Intens. Anim Speed:"));
				subNodes.Add(new FlameJetSlider(owner, "IntensityMax", this, new Vector2(5f, base.panelStep), "Intensity Max:"));
				subNodes.Add(new FlameJetSlider(owner, "IntensityMin", this, new Vector2(5f, base.panelStep), "Intensity Min:"));
			}
		}

		public void AddTemperatureControls()
		{
			FlameJet.FlameJetData flameJetData = (parentNode as FlameJetRepresentation).data;
			if (!flameJetData.linkTempToIntens)
			{
				AddTemperatureAnimationControls();
				subNodes.Add(new Button(owner, "TemperatureAnim", this, new Vector2(5f, base.panelStep), 245f, "Temperature animation: " + flameJetData.temperatureAnim.value));
				subNodes.Add(new FlameJetSlider(owner, "Temperature", this, new Vector2(5f, base.panelStep), "Temperature:"));
			}
		}

		public void AddTemperatureAnimationControls()
		{
			if (!((parentNode as FlameJetRepresentation).data.temperatureAnim == FlameJet.Animation.None))
			{
				subNodes.Add(new FlameJetSlider(owner, "TemperatureAnimOffset", this, new Vector2(5f, base.panelStep), "Temp. Anim Offset:"));
				subNodes.Add(new FlameJetSlider(owner, "TemperatureAnimSpeed", this, new Vector2(5f, base.panelStep), "Temp. Anim Speed:"));
				subNodes.Add(new FlameJetSlider(owner, "TemperatureMax", this, new Vector2(5f, base.panelStep), "Temperature Max:"));
				subNodes.Add(new FlameJetSlider(owner, "TemperatureMin", this, new Vector2(5f, base.panelStep), "Temperature Min:"));
			}
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			FlameJet.FlameJetData flameJetData = (parentNode as FlameJetRepresentation).data;
			if (sender.IDstring == "ShowSound")
			{
				soundControlsVisible = !soundControlsVisible;
				RefreshUI();
			}
			else if (sender.IDstring == "Lethality")
			{
				flameJetData.lethality = (flameJetData.lethality + 1) % 3;
				flameJetData.obj.lethality = flameJetData.lethality;
				RefreshUI();
			}
			else if (sender.IDstring == "LinkTempToIntens")
			{
				flameJetData.linkTempToIntens = !flameJetData.linkTempToIntens;
				flameJetData.obj.linkTempToIntens = flameJetData.linkTempToIntens;
				RefreshUI();
			}
			else if (sender.IDstring == "ActiveDuring")
			{
				flameJetData.activeDuring = new FlameJet.ActiveDuring(ExtEnum<FlameJet.ActiveDuring>.values.GetEntry((flameJetData.activeDuring.Index + 1) % ExtEnum<FlameJet.ActiveDuring>.values.Count));
				flameJetData.obj.activeDuring = flameJetData.activeDuring;
				(sender as Button).Text = "Active: " + flameJetData.activeDuring.value;
			}
			else if (sender.IDstring == "TemperatureAnim")
			{
				flameJetData.temperatureAnim = new FlameJet.Animation(ExtEnum<FlameJet.Animation>.values.GetEntry((flameJetData.temperatureAnim.Index + 1) % ExtEnum<FlameJet.Animation>.values.Count));
				flameJetData.obj.temperatureAnim = flameJetData.temperatureAnim;
				RefreshUI();
			}
			else if (sender.IDstring == "IntensityAnim")
			{
				flameJetData.intensityAnim = new FlameJet.Animation(ExtEnum<FlameJet.Animation>.values.GetEntry((flameJetData.intensityAnim.Index + 1) % ExtEnum<FlameJet.Animation>.values.Count));
				flameJetData.obj.intensityAnim = flameJetData.intensityAnim;
				RefreshUI();
			}
		}
	}

	public FlameJet flameJet;

	public FlameJet.FlameJetData data;

	public FlameJetRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, "FlameJet")
	{
		data = pObj.data as FlameJet.FlameJetData;
		if (data.obj == null)
		{
			data.obj = FlameJet.FromPlacedObject(owner.room, base.pObj);
			data.obj.setPos = pObj.pos;
			owner.room.AddObject(data.obj);
		}
		flameJet = data.obj;
		FlameJetPanel flameJetPanel = new FlameJetPanel(owner, "FlameJetPanel", this, default(Vector2));
		subNodes.Add(flameJetPanel);
		flameJetPanel.pos = data.panelPos;
		FSprite fSprite = new FSprite("pixel")
		{
			anchorY = 0f
		};
		Handle handle = new Handle(owner, "FlameJetTarget", this, default(Vector2));
		subNodes.Add(handle);
		handle.pos = data.target;
		FSprite fSprite2 = new FSprite("pixel")
		{
			anchorY = 0f
		};
		fSprites.Add(fSprite);
		owner.placedObjectsContainer.AddChild(fSprite);
		fSprites.Add(fSprite2);
		owner.placedObjectsContainer.AddChild(fSprite2);
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(1, absPos);
		Panel panel = subNodes[0] as FlameJetPanel;
		data.panelPos = panel.pos;
		fSprites[1].scaleY = (data.panelPos + (panel.collapsed ? (Vector2.up * panel.size.y) : Vector2.zero)).magnitude;
		fSprites[1].rotation = Custom.AimFromOneVectorToAnother(absPos, absPos + data.panelPos + (panel.collapsed ? (Vector2.up * panel.size.y) : Vector2.zero));
		MoveSprite(2, absPos);
		Handle handle = subNodes[1] as Handle;
		fSprites[2].scaleY = data.target.magnitude;
		fSprites[2].rotation = Custom.AimFromOneVectorToAnother(absPos, absPos + data.target);
		flameJet.setPos = (data.pos = pObj.pos);
		flameJet.setTarget = (data.target = handle.pos);
	}
}
