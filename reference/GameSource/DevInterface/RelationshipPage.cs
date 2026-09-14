using System.Collections.Generic;
using System.Linq;
using ArenaBehaviors;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Watcher;

namespace DevInterface;

public class RelationshipPage : Page
{
	public class DevRelationOrder : ExtEnum<DevRelationOrder>
	{
		public static DevRelationOrder Alphabetical = new DevRelationOrder("Alphabetical", register: true);

		public static DevRelationOrder FoodChain = new DevRelationOrder("FoodChain", register: true);

		public static DevRelationOrder Environment = new DevRelationOrder("Environment", register: true);

		public static DevRelationOrder RegisterOrder = new DevRelationOrder("RegisterOrder", register: true);

		public static DevRelationOrder MyRelationToYou = new DevRelationOrder("MyRelationToYou", register: true);

		public static DevRelationOrder YourRelationToMe = new DevRelationOrder("YourRelationToMe", register: true);

		public DevRelationOrder(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class DevRelationCategory : ExtEnum<DevRelationCategory>
	{
		public static DevRelationCategory CurrentRegion = new DevRelationCategory("CurrentRegion", register: true);

		public static DevRelationCategory Lizards = new DevRelationCategory("Lizards", register: true);

		public static DevRelationCategory Offscreen = new DevRelationCategory("Offscreen", register: true);

		public static DevRelationCategory ApexPredators = new DevRelationCategory("ApexPredators", register: true);

		public static DevRelationCategory Predators = new DevRelationCategory("Predators", register: true);

		public static DevRelationCategory Prey = new DevRelationCategory("Prey", register: true);

		public static DevRelationCategory Food = new DevRelationCategory("Food", register: true);

		public static DevRelationCategory Hazards = new DevRelationCategory("Hazards", register: true);

		public static DevRelationCategory Misc = new DevRelationCategory("Misc", register: true);

		public static DevRelationCategory Desert = new DevRelationCategory("Desert", register: true);

		public static DevRelationCategory Water = new DevRelationCategory("Water", register: true);

		public static DevRelationCategory Dark = new DevRelationCategory("Dark", register: true);

		public static DevRelationCategory Air = new DevRelationCategory("Air", register: true);

		public static DevRelationCategory Artificial = new DevRelationCategory("Artificial", register: true);

		public static DevRelationCategory Neutral = new DevRelationCategory("Neutral", register: true);

		public DevRelationCategory(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class SelectRelationshipPanel : SelectPanel
	{
		public SignalSlider amountSlider;

		public Button removeChangesButton;

		public float amount;

		public SelectRelationshipPanel(DevUI owner, DevUINode parentNode, Vector2 pos, string[] decalNames, float amount)
			: base(owner, "Adjust_Relationship_Panel", parentNode, pos, new Vector2(190f, 80f + (float)decalNames.Length * 20f), "Adjust Relationship", decalNames)
		{
			perpage = decalNames.Length;
			columns = 1;
			this.amount = amount;
		}

		public override void PopulateItems(int offset)
		{
			perpage = items.Length;
			columns = 1;
			base.PopulateItems(offset);
			amountSlider = new SignalSlider(owner, "amount", this, new Vector2(5f, size.y - 25f - 20f * ((float)(perpage / columns) + 1f)), "amount", inheritButton: false, 50f, amount);
			removeChangesButton = new Button(owner, "reset", this, amountSlider.pos + new Vector2(0f, 20f), 100f, "remove changes");
			subNodes.Add(amountSlider);
			subNodes.Add(removeChangesButton);
		}
	}

	public class CritRelationRep : RectangularDevUINode
	{
		public FSprite critSprite;

		public RelationshipButton relationship1;

		public RelationshipButton relationship2;

		public FLabel creatureLabel;

		public CreatureTemplate.Type creatureType;

		public Color defaultRelationColor = new Color(1f, 0f, 0f);

		public Color inheritedRelationColor = new Color(1f, 0.4f, 0f);

		public Color definedRelationColor = new Color(0f, 0.2f, 0.8f);

		public CritRelationRep(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, CreatureTemplate.Type creatureType, CreatureTemplate.Type creatureType2)
			: base(owner, IDstring, parentNode, pos, new Vector2(160f, 60f))
		{
			fSprites.Add(new FSprite("pixel"));
			fSprites[0].color = new Color(0f, 0f, 0f);
			fSprites[0].alpha = 0.5f;
			fSprites[0].anchorX = 0f;
			fSprites[0].anchorY = 0f;
			this.creatureType = creatureType;
			creatureLabel = new FLabel(Custom.GetFont(), creatureType.value);
			creatureLabel.alignment = FLabelAlignment.Left;
			fLabels.Add(creatureLabel);
			Futile.stage.AddChild(fSprites[0]);
			Futile.stage.AddChild(creatureLabel);
			CreatureTemplate creatureTemplate = StaticWorld.GetCreatureTemplate(creatureType);
			CreatureTemplate creatureTemplate2 = StaticWorld.GetCreatureTemplate(creatureType2);
			relationship1 = new RelationshipButton(owner, "relationship1", this, new Vector2(5f, 5f), creatureTemplate, creatureTemplate2);
			relationship2 = new RelationshipButton(owner, "relationship1", this, new Vector2(5f, 25f), creatureTemplate2, creatureTemplate);
			subNodes.Add(relationship1);
			subNodes.Add(relationship2);
		}

		public override void Refresh()
		{
			base.Refresh();
			fSprites[0].scaleY = size.y;
			fSprites[0].scaleX = size.x;
			MoveSprite(0, absPos);
			MoveLabel(0, absPos + new Vector2(20f, size.y - 10f));
		}
	}

	public class RelationshipButton : ButtonWithSelectPanel, IDevUISignals
	{
		public CreatureTemplate.Relationship? relationship;

		public CreatureTemplate A;

		public CreatureTemplate B;

		public DevUILabel amountLabel;

		public Color defaultRelationColor = new Color(1f, 0f, 0f);

		public Color inheritedRelationColor = new Color(1f, 0.4f, 0f);

		public Color definedRelationColor = new Color(0f, 0.2f, 0.8f);

		public Color changedRelationColor = new Color(0f, 0.6f, 0.2f);

		public Color changedInheritedRelationColor = new Color(0.3f, 0.5f, 0f);

		public CreatureTemplate.Relationship Relationship
		{
			get
			{
				return GetEffectiveRelationship(A, B);
			}
			set
			{
				SetChangedRelationship(A.type, B.type, value);
			}
		}

		public static SelectPanel MakePanel(ButtonWithSelectPanel button)
		{
			RelationshipButton relationshipButton = button as RelationshipButton;
			return new SelectRelationshipPanel(relationshipButton.owner, relationshipButton, new Vector2(200f, 15f) - relationshipButton.absPos, ExtEnum<CreatureTemplate.Relationship.Type>.values.entries.ToArray(), relationshipButton.Relationship.intensity);
		}

		public RelationshipButton(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, CreatureTemplate A, CreatureTemplate B)
			: base(owner, IDstring, parentNode, pos, 150f, "", MakePanel)
		{
			this.A = A;
			this.B = B;
			amountLabel = new DevUILabel(owner, IDstring, this, new Vector2(120f, 0f), 20f, Relationship.intensity.ToString());
			subNodes.Add(amountLabel);
			amountLabel.spriteColor = new Color(0f, 0f, 0f, 0f);
		}

		public override void Update()
		{
			base.Update();
			amountLabel.textColor = base.textColor;
		}

		public override void OnValueChange(string value)
		{
			CreatureTemplate.Relationship relationship = Relationship;
			relationship.type = new CreatureTemplate.Relationship.Type(value);
			Relationship = relationship;
			(owner.activePage as RelationshipPage).refresh = true;
			Refresh();
		}

		public override void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			if (selectPanel is SelectRelationshipPanel selectRelationshipPanel && (sender == selectRelationshipPanel.amountSlider || sender == selectRelationshipPanel.removeChangesButton))
			{
				(owner.activePage as RelationshipPage).refresh = true;
				if (sender == selectRelationshipPanel.amountSlider)
				{
					if (Input.GetKey(KeyCode.LeftControl))
					{
						amountLabel.Text = float.Parse(message).ToString("0.0");
					}
					else
					{
						amountLabel.Text = float.Parse(message).ToString("0.00");
					}
					CreatureTemplate.Relationship relationship = Relationship;
					relationship.intensity = float.Parse(amountLabel.Text);
					Relationship = relationship;
					selectRelationshipPanel.amountSlider.NumberText = amountLabel.Text;
					Refresh();
				}
				else if (sender == selectRelationshipPanel.removeChangesButton)
				{
					ResetChangedRelationship(A.type, B.type);
					Refresh();
				}
			}
			else
			{
				base.Signal(type, sender, message);
			}
		}

		public override void Refresh()
		{
			base.Refresh();
			CreatureTemplate.Relationship relationship = Relationship;
			if (TryGetChangedRelationship(A, B, out var relationship2))
			{
				overrideTextColor = changedRelationColor;
			}
			else if (A.ancestor != null && TryGetChangedRelationship(A, B, out relationship2, inherit: true))
			{
				overrideTextColor = changedInheritedRelationColor;
			}
			else if (relationship == A.defaultRelationship)
			{
				overrideTextColor = defaultRelationColor;
			}
			else if (A.ancestor != null && relationship == A.ancestor.CreatureRelationship(B))
			{
				overrideTextColor = inheritedRelationColor;
			}
			else if (B.ancestor != null && relationship == A.CreatureRelationship(B.ancestor))
			{
				overrideTextColor = inheritedRelationColor;
			}
			else
			{
				overrideTextColor = definedRelationColor;
			}
			amountLabel.Text = relationship.intensity.ToString();
			base.Text = relationship.type.value;
		}
	}

	public List<DevRelationCategory> foodChainCategories = new List<DevRelationCategory>
	{
		DevRelationCategory.ApexPredators,
		DevRelationCategory.Predators,
		DevRelationCategory.Prey,
		DevRelationCategory.Food,
		DevRelationCategory.Hazards,
		DevRelationCategory.Misc
	};

	public List<DevRelationCategory> environmentCategories = new List<DevRelationCategory>
	{
		DevRelationCategory.Neutral,
		DevRelationCategory.Air,
		DevRelationCategory.Water,
		DevRelationCategory.Desert,
		DevRelationCategory.Dark,
		DevRelationCategory.Artificial
	};

	public List<CreatureTemplate.Relationship.Type> relationOrder = new List<CreatureTemplate.Relationship.Type>
	{
		CreatureTemplate.Relationship.Type.Eats,
		CreatureTemplate.Relationship.Type.Attacks,
		CreatureTemplate.Relationship.Type.Antagonizes,
		CreatureTemplate.Relationship.Type.AgressiveRival,
		CreatureTemplate.Relationship.Type.PlaysWith,
		CreatureTemplate.Relationship.Type.Pack,
		CreatureTemplate.Relationship.Type.StayOutOfWay,
		CreatureTemplate.Relationship.Type.Uncomfortable,
		CreatureTemplate.Relationship.Type.Afraid,
		CreatureTemplate.Relationship.Type.Ignores,
		CreatureTemplate.Relationship.Type.DoesntTrack,
		CreatureTemplate.Relationship.Type.SocialDependent
	};

	public static Dictionary<CreatureTemplate.Type, Dictionary<CreatureTemplate.Type, CreatureTemplate.Relationship>> changedRelationships = new Dictionary<CreatureTemplate.Type, Dictionary<CreatureTemplate.Type, CreatureTemplate.Relationship>>();

	public bool refresh;

	public CreatureTemplate.Type selectedCreatureType;

	public DevRelationCategory currentCategory;

	public DevRelationOrder currentOrder;

	public ButtonWithSelectPanel selectCreatureButton;

	public ButtonWithSelectPanel selectCategoryButton;

	public ButtonWithSelectPanel selectOrderButton;

	public RelationshipButton defaultRelationLabel;

	public List<CritRelationRep> relationReps = new List<CritRelationRep>();

	public IEnumerable<CreatureTemplate.Type> GetCreaturesInCategory(DevRelationCategory category)
	{
		if (category == DevRelationCategory.CurrentRegion)
		{
			foreach (CreatureTemplate.Type item in GetCreaturesInRegion())
			{
				yield return item;
			}
			yield break;
		}
		foreach (string entry in ExtEnum<CreatureTemplate.Type>.values.entries)
		{
			CreatureTemplate.Type type = new CreatureTemplate.Type(entry);
			if (IsCreatureInCategory(type, category))
			{
				yield return type;
			}
		}
	}

	public bool IsCreatureInCategory(CreatureTemplate.Type type, DevRelationCategory category)
	{
		if (category == DevRelationCategory.Lizards)
		{
			if (!StaticWorld.GetCreatureTemplate(type).IsLizard)
			{
				return type == CreatureTemplate.Type.LizardTemplate;
			}
			return true;
		}
		if (category == DevRelationCategory.Offscreen)
		{
			return StaticWorld.GetCreatureTemplate(type).hibernateOffScreen;
		}
		if (category == DevRelationCategory.CurrentRegion)
		{
			return GetCreaturesInRegion().Contains(type);
		}
		if (foodChainCategories.Contains(category))
		{
			return IsCreatureInFoodChainCategory(type, category);
		}
		if (environmentCategories.Contains(category))
		{
			return IsCreatureInEnvironmentCategory(type, category);
		}
		return false;
	}

	public bool IsCreatureInFoodChainCategory(CreatureTemplate.Type type, DevRelationCategory category)
	{
		return GetCreatureFoodChain(type) == category;
	}

	public DevRelationCategory GetCreatureFoodChain(CreatureTemplate.Type type)
	{
		CreatureTemplate creatureTemplate = StaticWorld.GetCreatureTemplate(type);
		if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Vulture || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.BigEel || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.MirosBird || (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.DaddyLongLegs && (!ModManager.DLCShared || !(type == DLCSharedEnums.CreatureTemplateType.Inspector))) || type == CreatureTemplate.Type.RedLizard || type == CreatureTemplate.Type.RedCentipede || (ModManager.MSC && type == MoreSlugcatsEnums.CreatureTemplateType.TrainLizard) || (ModManager.DLCShared && type == DLCSharedEnums.CreatureTemplateType.MirosVulture) || (ModManager.Watcher && (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.Loach || creatureTemplate.type == WatcherEnums.CreatureTemplateType.BigMoth)) || creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.Angler || creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.DrillCrab)
		{
			return DevRelationCategory.ApexPredators;
		}
		if ((creatureTemplate.IsLizard && type != CreatureTemplate.Type.RedLizard && (!ModManager.MSC || !(type == MoreSlugcatsEnums.CreatureTemplateType.TrainLizard))) || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.BigSpider || creatureTemplate.type == CreatureTemplate.Type.LizardTemplate || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.DropBug || type == CreatureTemplate.Type.Centipede || type == CreatureTemplate.Type.Centiwing || (ModManager.DLCShared && type == DLCSharedEnums.CreatureTemplateType.AquaCenti) || (ModManager.Watcher && type == WatcherEnums.CreatureTemplateType.BigSandGrub))
		{
			return DevRelationCategory.Predators;
		}
		if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Slugcat || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.LanternMouse || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.CicadaA || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Scavenger || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.JetFish || type == CreatureTemplate.Type.BigNeedleWorm || type == CreatureTemplate.Type.EggBug || (ModManager.DLCShared && type == DLCSharedEnums.CreatureTemplateType.Yeek) || (ModManager.MSC && (type == MoreSlugcatsEnums.CreatureTemplateType.FireBug || type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)) || (ModManager.Watcher && type == WatcherEnums.CreatureTemplateType.SmallMoth))
		{
			return DevRelationCategory.Prey;
		}
		if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Fly || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.VultureGrub || type == CreatureTemplate.Type.SmallCentipede || type == CreatureTemplate.Type.SmallNeedleWorm || (ModManager.Watcher && (type == WatcherEnums.CreatureTemplateType.SandGrub || type == WatcherEnums.CreatureTemplateType.Rat || type == WatcherEnums.CreatureTemplateType.Tardigrade)))
		{
			return DevRelationCategory.Food;
		}
		if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Leech || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.TentaclePlant || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.GarbageWorm || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Spider || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Snail || (ModManager.DLCShared && (type == DLCSharedEnums.CreatureTemplateType.BigJelly || type == DLCSharedEnums.CreatureTemplateType.StowawayBug)) || (ModManager.Watcher && (type == WatcherEnums.CreatureTemplateType.Barnacle || type == WatcherEnums.CreatureTemplateType.Frog || type == WatcherEnums.CreatureTemplateType.Rattler)))
		{
			return DevRelationCategory.Hazards;
		}
		if (!(creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Deer) && !(creatureTemplate.TopAncestor().type == CreatureTemplate.Type.TubeWorm) && !(creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Overseer) && !(creatureTemplate.TopAncestor().type == CreatureTemplate.Type.TempleGuard) && (!ModManager.DLCShared || !(type == DLCSharedEnums.CreatureTemplateType.Inspector)))
		{
			if (!ModManager.Watcher)
			{
				return DevRelationCategory.Misc;
			}
			if (!(creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.SkyWhale) && !(creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.BoxWorm) && !(creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.FireSprite) && !(creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.MothGrub))
			{
				_ = creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.RippleSpider;
			}
		}
		return DevRelationCategory.Misc;
	}

	public bool IsCreatureInEnvironmentCategory(CreatureTemplate.Type type, DevRelationCategory category)
	{
		return GetCreatureEnvironment(type) == category;
	}

	public DevRelationCategory GetCreatureEnvironment(CreatureTemplate.Type type)
	{
		CreatureTemplate creatureTemplate = StaticWorld.GetCreatureTemplate(type);
		if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Spider || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.BigSpider || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.LanternMouse || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.MirosBird || type == CreatureTemplate.Type.BlackLizard || (ModManager.DLCShared && type == DLCSharedEnums.CreatureTemplateType.MirosVulture))
		{
			return DevRelationCategory.Dark;
		}
		if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Overseer || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.TempleGuard || (ModManager.DLCShared && type == DLCSharedEnums.CreatureTemplateType.Inspector) || creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.BoxWorm || creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.FireSprite || creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.RippleSpider)
		{
			return DevRelationCategory.Artificial;
		}
		if (ModManager.Watcher && (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.DrillCrab || creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.SandGrub || type == WatcherEnums.CreatureTemplateType.IndigoLizard))
		{
			return DevRelationCategory.Desert;
		}
		if ((creatureTemplate.waterRelationship == CreatureTemplate.WaterRelationship.Amphibious || creatureTemplate.waterRelationship == CreatureTemplate.WaterRelationship.WaterOnly) && !(creatureTemplate.TopAncestor().type == CreatureTemplate.Type.DaddyLongLegs) && !(creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Slugcat) && !(creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Overseer))
		{
			return DevRelationCategory.Water;
		}
		if (creatureTemplate.canFly)
		{
			return DevRelationCategory.Air;
		}
		return DevRelationCategory.Neutral;
	}

	public IEnumerable<CreatureTemplate.Type> GetCreaturesInRegion()
	{
		if (owner?.room?.world == null)
		{
			yield return null;
		}
		List<CreatureTemplate.Type> result = new List<CreatureTemplate.Type>();
		GameSession session = owner.room.world.game.session;
		if (session is ArenaGameSession arenaSession)
		{
			if (arenaSession.arenaSitting.players.Count > 0)
			{
				yield return CreatureTemplate.Type.Slugcat;
			}
			if (arenaSession.arenaSitting.gameTypeSetup.fliesSpawn)
			{
				yield return CreatureTemplate.Type.Fly;
			}
		}
		if (owner.room.world.game.session is SandboxGameSession sandboxGameSession)
		{
			if (sandboxGameSession.editor != null)
			{
				foreach (SandboxEditor.PlacedIcon icon in sandboxGameSession.editor.icons)
				{
					if (icon is SandboxEditor.CreatureOrItemIcon creatureOrItemIcon && !(creatureOrItemIcon.iconData.itemType != AbstractPhysicalObject.AbstractObjectType.Creature) && !result.Contains(creatureOrItemIcon.iconData.critType))
					{
						result.Add(creatureOrItemIcon.iconData.critType);
						yield return creatureOrItemIcon.iconData.critType;
					}
				}
				yield break;
			}
			foreach (SandboxEditor.PlacedIconData symbolsDatum in sandboxGameSession.symbolsData)
			{
				if (!(symbolsDatum.data.itemType != AbstractPhysicalObject.AbstractObjectType.Creature) && !result.Contains(symbolsDatum.data.critType))
				{
					result.Add(symbolsDatum.data.critType);
					yield return symbolsDatum.data.critType;
				}
			}
			yield break;
		}
		if (owner.room.world.singleRoomWorld)
		{
			AbstractRoom[] abstractRooms = owner.room.world.abstractRooms;
			foreach (AbstractRoom abstractRoom in abstractRooms)
			{
				foreach (AbstractCreature creature in abstractRoom.creatures)
				{
					if (!result.Contains(creature.creatureTemplate.type))
					{
						result.Add(creature.creatureTemplate.type);
						yield return creature.creatureTemplate.type;
					}
				}
			}
			yield break;
		}
		yield return CreatureTemplate.Type.Slugcat;
		if (owner.room.world.swarmRooms.Length != 0)
		{
			yield return CreatureTemplate.Type.Fly;
		}
		Region region = owner.room.world.region;
		if (region != null)
		{
			if ((region.regionParams.overseersMax > 0 && region.regionParams.overseersSpawnChance > 0f) || region.regionParams.playerGuideOverseerSpawnChance > 0f)
			{
				yield return CreatureTemplate.Type.Overseer;
			}
			if (region.regionParams.rippleSpiderThreshold < 50)
			{
				yield return WatcherEnums.CreatureTemplateType.RippleSpider;
			}
		}
		World.CreatureSpawner[] spawners = owner.room.world.spawners;
		foreach (World.CreatureSpawner creatureSpawner in spawners)
		{
			if (creatureSpawner is World.SimpleSpawner simpleSpawner && !result.Contains(simpleSpawner.creatureType))
			{
				result.Add(simpleSpawner.creatureType);
				yield return simpleSpawner.creatureType;
			}
			else
			{
				if (!(creatureSpawner is World.Lineage { creatureTypes: var creatureTypes }))
				{
					continue;
				}
				foreach (int index in creatureTypes)
				{
					string entry = ExtEnum<CreatureTemplate.Type>.values.GetEntry(index);
					if (entry != null && !result.Contains(new CreatureTemplate.Type(entry)))
					{
						result.Add(new CreatureTemplate.Type(entry));
						yield return new CreatureTemplate.Type(entry);
					}
				}
			}
		}
	}

	public static bool TryGetChangedRelationship(CreatureTemplate crit1, CreatureTemplate crit2, out CreatureTemplate.Relationship relationship, bool inherit = false)
	{
		relationship = default(CreatureTemplate.Relationship);
		if (changedRelationships.TryGetValue(crit1.type, out var value) && value.TryGetValue(crit2.type, out relationship))
		{
			return true;
		}
		if (inherit)
		{
			if (crit1.ancestor != null && TryGetChangedRelationship(crit1.ancestor, crit2, out relationship, inherit: true))
			{
				return true;
			}
			if (crit2.ancestor != null && TryGetChangedRelationship(crit1, crit2.ancestor, out relationship, inherit: true))
			{
				return true;
			}
		}
		return false;
	}

	public static void SetChangedRelationship(CreatureTemplate.Type crit1, CreatureTemplate.Type crit2, CreatureTemplate.Relationship relationship)
	{
		if (!changedRelationships.ContainsKey(crit1))
		{
			changedRelationships[crit1] = new Dictionary<CreatureTemplate.Type, CreatureTemplate.Relationship>();
		}
		changedRelationships[crit1][crit2] = relationship;
	}

	public static void ResetChangedRelationship(CreatureTemplate.Type crit1, CreatureTemplate.Type crit2)
	{
		if (changedRelationships.ContainsKey(crit1))
		{
			changedRelationships[crit1].Remove(crit2);
			if (changedRelationships[crit1].Count == 0)
			{
				changedRelationships.Remove(crit1);
			}
		}
	}

	public static CreatureTemplate.Relationship GetEffectiveRelationship(CreatureTemplate crit1, CreatureTemplate crit2)
	{
		if (TryGetChangedRelationship(crit1, crit2, out var relationship))
		{
			return relationship;
		}
		if (crit1.ancestor != null && TryGetChangedRelationship(crit1.ancestor, crit2, out var relationship2))
		{
			return relationship2;
		}
		return crit1.CreatureRelationship(crit2);
	}

	public static SelectPanel MakeCreatureSelect(ButtonWithSelectPanel button)
	{
		return new SelectPanel(button.owner, "Select_Creature_Panel", button, new Vector2(200f, 15f) - button.pos, new Vector2(305f, 420f), "Select Creature", ExtEnum<CreatureTemplate.Type>.values.entries.ToArray());
	}

	public static SelectPanel MakeCategorySelect(ButtonWithSelectPanel button)
	{
		return new SelectPanel(button.owner, "Select_Creature_Panel", button, new Vector2(200f, 15f) - button.pos, new Vector2(305f, 420f), "Select Category", ExtEnum<DevRelationCategory>.values.entries.ToArray());
	}

	public static SelectPanel MakeOrderSelect(ButtonWithSelectPanel button)
	{
		return new SelectPanel(button.owner, "Select_Order_Panel", button, new Vector2(200f, 15f) - button.pos, new Vector2(305f, 420f), "Select Order", ExtEnum<DevRelationOrder>.values.entries.ToArray());
	}

	public RelationshipPage(DevUI owner, string IDstring, DevUINode parentNode, string name)
		: base(owner, IDstring, parentNode, name)
	{
		for (int num = subNodes.Count - 1; num >= 0; num--)
		{
			if (subNodes[num] is Button { IDstring: "Save_Specific" } button)
			{
				button.ClearSprites();
				subNodes.Remove(button);
			}
			else if (subNodes[num] is DevUILabel { IDstring: "Specific_Label" } devUILabel)
			{
				devUILabel.ClearSprites();
				subNodes.Remove(devUILabel);
			}
		}
		selectCreatureButton = new ButtonWithSelectPanel(owner, "Select_Crit_Panel_Button", this, new Vector2(100f, 650f), 150f, "Select Creature", MakeCreatureSelect);
		subNodes.Add(selectCreatureButton);
		currentCategory = DevRelationCategory.CurrentRegion;
		selectCategoryButton = new ButtonWithSelectPanel(owner, "Select_Category_Panel_Button", this, new Vector2(260f, 650f), 150f, "Category: " + currentCategory.value, MakeCategorySelect);
		subNodes.Add(selectCategoryButton);
		currentOrder = DevRelationOrder.RegisterOrder;
		selectOrderButton = new ButtonWithSelectPanel(owner, "Select_Order_Panel_Button", this, new Vector2(420f, 650f), 150f, "Order: " + currentOrder.value, MakeOrderSelect);
		subNodes.Add(selectOrderButton);
	}

	public override void Update()
	{
		base.Update();
		if (refresh)
		{
			refresh = false;
			Refresh();
		}
	}

	public override void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		if (sender != null && sender == defaultRelationLabel)
		{
			return;
		}
		if (sender == selectCreatureButton)
		{
			selectedCreatureType = new CreatureTemplate.Type(message);
			selectCreatureButton.Text = "Creature : " + selectedCreatureType;
			if (defaultRelationLabel != null)
			{
				defaultRelationLabel.ClearSprites();
			}
			RefreshRelationships();
		}
		else if (sender == selectCategoryButton)
		{
			DevRelationCategory devRelationCategory;
			DevRelationCategory devRelationCategory2;
			if (foodChainCategories.Contains(devRelationCategory = new DevRelationCategory(message)))
			{
				currentCategory = devRelationCategory;
			}
			else if (environmentCategories.Contains(devRelationCategory2 = new DevRelationCategory(message)))
			{
				currentCategory = devRelationCategory2;
			}
			else
			{
				currentCategory = new DevRelationCategory(message);
			}
			selectCategoryButton.Text = "Category: " + currentCategory.value;
			RefreshRelationships();
		}
		else if (sender == selectOrderButton)
		{
			currentOrder = new DevRelationOrder(message);
			selectOrderButton.Text = "Order: " + currentOrder.value;
			RefreshRelationships();
		}
		else if (sender.IDstring == "Save_Settings")
		{
			LogAllChangedRelationships();
		}
	}

	public static void LogAllChangedRelationships()
	{
		string text = "";
		foreach (KeyValuePair<CreatureTemplate.Type, Dictionary<CreatureTemplate.Type, CreatureTemplate.Relationship>> changedRelationship in changedRelationships)
		{
			foreach (KeyValuePair<CreatureTemplate.Type, CreatureTemplate.Relationship> item in changedRelationship.Value)
			{
				text = text + EstablishRelationshipCodeString(changedRelationship.Key, item.Key, item.Value) + "\n";
			}
			text += "\n";
		}
		Custom.LogImportant(text);
	}

	public static string EstablishRelationshipCodeString(CreatureTemplate.Type A, CreatureTemplate.Type B, CreatureTemplate.Relationship relationship)
	{
		return $"EstablishRelationship({OriginSignature(A) + A}, {OriginSignature(B) + B}, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.{relationship.type}, {relationship.intensity}f));";
	}

	public static string OriginSignature(CreatureTemplate.Type creature)
	{
		if (MoreSlugcatsCreatures.CreatureName(creature) != null)
		{
			if (creature == MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy || creature == MoreSlugcatsEnums.CreatureTemplateType.FireBug || creature == MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing || creature == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC || creature == MoreSlugcatsEnums.CreatureTemplateType.TrainLizard)
			{
				return "MoreSlugcatsEnums.CreatureTemplateType.";
			}
			return "DLCSharedEnums.CreatureTemplateType.";
		}
		if (WatcherCreatures.CreatureName(creature) != null)
		{
			return "WatcherEnums.CreatureTemplateType.";
		}
		return "CreatureTemplate.Type.";
	}

	public void RefreshRelationships()
	{
		RemoveRelationships();
		IntVector2 intVector = new IntVector2(1, 0);
		foreach (CreatureTemplate.Type item in GetCreaturesInCategory(currentCategory).OrderBy(Order).ThenBy(SecondaryOrder))
		{
			relationReps.Add(new CritRelationRep(owner, "id", this, new Vector2(100f + (float)intVector.x * 180f, 550f - (float)intVector.y * 70f), item, selectedCreatureType));
			subNodes.Add(relationReps[relationReps.Count - 1]);
			intVector.x++;
			if (intVector.x > 6)
			{
				intVector.y++;
				intVector.x = 1;
			}
		}
	}

	public object Order(CreatureTemplate.Type type)
	{
		if (currentOrder == DevRelationOrder.Alphabetical)
		{
			return type.value;
		}
		if (currentOrder == DevRelationOrder.RegisterOrder)
		{
			return type.Index;
		}
		if (currentOrder == DevRelationOrder.Environment)
		{
			return environmentCategories.IndexOf(GetCreatureEnvironment(type));
		}
		if (currentOrder == DevRelationOrder.FoodChain)
		{
			return foodChainCategories.IndexOf(GetCreatureEnvironment(type));
		}
		if (currentOrder == DevRelationOrder.MyRelationToYou)
		{
			return relationOrder.IndexOf(GetEffectiveRelationship(StaticWorld.GetCreatureTemplate(selectedCreatureType), StaticWorld.GetCreatureTemplate(type)).type);
		}
		if (currentOrder == DevRelationOrder.YourRelationToMe)
		{
			return relationOrder.IndexOf(GetEffectiveRelationship(StaticWorld.GetCreatureTemplate(type), StaticWorld.GetCreatureTemplate(selectedCreatureType)).type);
		}
		return type.Index;
	}

	public object SecondaryOrder(CreatureTemplate.Type type)
	{
		if (currentOrder == DevRelationOrder.MyRelationToYou)
		{
			return 1f - GetEffectiveRelationship(StaticWorld.GetCreatureTemplate(selectedCreatureType), StaticWorld.GetCreatureTemplate(type)).intensity;
		}
		if (currentOrder == DevRelationOrder.YourRelationToMe)
		{
			return 1f - GetEffectiveRelationship(StaticWorld.GetCreatureTemplate(type), StaticWorld.GetCreatureTemplate(selectedCreatureType)).intensity;
		}
		return type.Index;
	}

	public void RemoveRelationships()
	{
		foreach (CritRelationRep relationRep in relationReps)
		{
			relationRep.ClearSprites();
			subNodes.Remove(relationRep);
		}
		relationReps.Clear();
	}
}
