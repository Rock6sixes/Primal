using HUD;
using UnityEngine;
using Watcher;

namespace Menu;

public class WarpSafariMenu : MenuObject, IOwnAHUD
{
	public global::HUD.HUD hud;

	public Map.MapData mapData;

	public MenuContainer[] hudContainers;

	public bool returnToMenu;

	public SlugcatStats.Name name;

	public MenuLabel buttonInstruction;

	public MenuLabel regionName;

	public string SelectedRegion => hud?.warpMap?.selectedRegion?.region;

	public int CurrentFood => 0;

	public Player.InputPackage MapInput => RWInput.PlayerInput(0);

	public bool RevealMap
	{
		get
		{
			if (hud != null)
			{
				return !returnToMenu;
			}
			return false;
		}
	}

	public Vector2 MapOwnerInRoomPosition => Vector2.zero;

	public bool MapDiscoveryActive => false;

	public int MapOwnerRoom => 0;

	public WarpSafariMenu(Menu menu, MenuObject owner, SlugcatStats.Name name)
		: base(menu, owner)
	{
		this.name = name;
		hudContainers = new MenuContainer[2];
		for (int i = 0; i < 2; i++)
		{
			hudContainers[i] = new MenuContainer(menu, this, new Vector2(0f, 0f));
			subObjects.Add(hudContainers[i]);
		}
		hud = new global::HUD.HUD(new FContainer[2]
		{
			hudContainers[1].Container,
			hudContainers[0].Container
		}, menu.manager.rainWorld, this);
		mapData = new Map.MapData(null, menu.manager.rainWorld);
		mapData.InitSafariWarpData(menu.manager.rainWorld.progression, name);
		hud.InitFastTravelHud(mapData);
		buttonInstruction = new MenuLabel(menu, this, "", new Vector2(582.99f, Mathf.Max(menu.manager.rainWorld.options.SafeScreenOffset.y, 5f)), new Vector2(200f, 30f), bigText: false);
		buttonInstruction.label.color = Menu.MenuRGB(Menu.MenuColors.White);
		UpdateButtonInstructions();
		subObjects.Add(buttonInstruction);
		regionName = new MenuLabel(menu, this, "", new Vector2(582.99f, 682.99f), new Vector2(200f, 30f), bigText: true);
		regionName.label.color = Menu.MenuRGB(Menu.MenuColors.White);
		subObjects.Add(regionName);
	}

	public override void Update()
	{
		base.Update();
		hud.Update();
		if (hud.warpMap.selectedRegion == null)
		{
			regionName.label.text = "";
		}
		else
		{
			WarpMap.WarpRegionIcon selectedRegion = hud.warpMap.selectedRegion;
			if (selectedRegion != null)
			{
				regionName.label.text = menu.Translate(Region.GetRegionFullName(selectedRegion.region, null));
			}
		}
		UpdateButtonInstructions();
		if (returnToMenu && !hud.warpMap.visible)
		{
			Singal(this, "ENDSAFWARPMAP");
		}
		if (!inactive)
		{
			if (MapInput.jmp && hud.warpMap.selectedRegion != null)
			{
				Singal(this, "PLAY!");
				inactive = true;
			}
			if (MapInput.thrw)
			{
				returnToMenu = true;
				inactive = true;
				menu.PlaySound(SoundID.MENU_Switch_Page_Out);
			}
		}
	}

	public void UpdateButtonInstructions()
	{
		string text = "";
		if (SelectedRegion != null)
		{
			text = menu.Translate("Jump Button") + " - " + menu.Translate("PLAY");
			text += "<LINE>";
		}
		text = text + menu.Translate("Throw Button") + " - " + menu.Translate("BACK");
		text = text.Replace("<LINE>", "     ");
		buttonInstruction.text = text;
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (hud != null)
		{
			hud.Draw(timeStacker);
		}
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		if (hud != null)
		{
			hud.ClearAllSprites();
		}
	}

	public void FoodCountDownDone()
	{
	}

	public global::HUD.HUD.OwnerType GetOwnerType()
	{
		return global::HUD.HUD.OwnerType.WarpSafariMenu;
	}

	public void PlayHUDSound(SoundID soundID)
	{
		menu.PlaySound(soundID);
	}

	public static bool WarpExclusiveSafari(MultiplayerUnlocks.SafariUnlockID unlock)
	{
		if (unlock == WatcherEnums.SafariUnlockID.WARA || unlock == WatcherEnums.SafariUnlockID.WARB || unlock == WatcherEnums.SafariUnlockID.WARC || unlock == WatcherEnums.SafariUnlockID.WARD || unlock == WatcherEnums.SafariUnlockID.WARE || unlock == WatcherEnums.SafariUnlockID.WARF || unlock == WatcherEnums.SafariUnlockID.WARG || unlock == WatcherEnums.SafariUnlockID.WMPA || unlock == WatcherEnums.SafariUnlockID.WBLA || unlock == WatcherEnums.SafariUnlockID.WPTA || unlock == WatcherEnums.SafariUnlockID.WPGA || unlock == WatcherEnums.SafariUnlockID.WRFA || unlock == WatcherEnums.SafariUnlockID.WRFB || unlock == WatcherEnums.SafariUnlockID.WSKA || unlock == WatcherEnums.SafariUnlockID.WSKB || unlock == WatcherEnums.SafariUnlockID.WSKC || unlock == WatcherEnums.SafariUnlockID.WSKD || unlock == WatcherEnums.SafariUnlockID.WTDA || unlock == WatcherEnums.SafariUnlockID.WTDB || unlock == WatcherEnums.SafariUnlockID.WRRA || unlock == WatcherEnums.SafariUnlockID.WVWA || unlock == WatcherEnums.SafariUnlockID.WVWB || unlock == WatcherEnums.SafariUnlockID.WAUA || unlock == WatcherEnums.SafariUnlockID.WSUR || unlock == WatcherEnums.SafariUnlockID.WDSR || unlock == WatcherEnums.SafariUnlockID.WHIR || unlock == WatcherEnums.SafariUnlockID.WGWR || unlock == WatcherEnums.SafariUnlockID.WORA)
		{
			return true;
		}
		return false;
	}
}
