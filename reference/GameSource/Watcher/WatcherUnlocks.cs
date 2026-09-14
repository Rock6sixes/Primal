namespace Watcher;

public static class WatcherUnlocks
{
	public static void AddUnlocks()
	{
		AddCreature(WatcherEnums.SandboxUnlockID.Barnacle, MultiplayerUnlocks.SandboxUnlockID.Snail);
		AddCreature(WatcherEnums.SandboxUnlockID.Tardigrade, WatcherEnums.SandboxUnlockID.Barnacle);
		AddCreature(WatcherEnums.SandboxUnlockID.Frog, MultiplayerUnlocks.SandboxUnlockID.Hazer);
		AddCreature(WatcherEnums.SandboxUnlockID.DrillCrab, WatcherEnums.SandboxUnlockID.Frog);
		AddCreature(WatcherEnums.SandboxUnlockID.SandGrub, WatcherEnums.SandboxUnlockID.DrillCrab);
		AddCreature(WatcherEnums.SandboxUnlockID.BigSandGrub, WatcherEnums.SandboxUnlockID.SandGrub);
		AddCreature(WatcherEnums.SandboxUnlockID.MothGrub, MultiplayerUnlocks.SandboxUnlockID.SmallNeedleWorm);
		AddCreature(WatcherEnums.SandboxUnlockID.SmallMoth, WatcherEnums.SandboxUnlockID.MothGrub);
		AddCreature(WatcherEnums.SandboxUnlockID.BigMoth, WatcherEnums.SandboxUnlockID.SmallMoth);
		AddCreature(WatcherEnums.SandboxUnlockID.Angler, MultiplayerUnlocks.SandboxUnlockID.BigEel);
		AddCreature(WatcherEnums.SandboxUnlockID.SkyWhale, MultiplayerUnlocks.SandboxUnlockID.Deer);
		AddCreature(WatcherEnums.SandboxUnlockID.AltSkyWhale, WatcherEnums.SandboxUnlockID.SkyWhale);
		AddCreature(WatcherEnums.SandboxUnlockID.Loach, WatcherEnums.SandboxUnlockID.AltSkyWhale);
		AddCreature(WatcherEnums.SandboxUnlockID.BoxWorm, WatcherEnums.SandboxUnlockID.Loach);
		AddCreature(WatcherEnums.SandboxUnlockID.FireSprite, WatcherEnums.SandboxUnlockID.BoxWorm);
		AddCreature(WatcherEnums.SandboxUnlockID.Rat, WatcherEnums.SandboxUnlockID.Frog);
		AddCreature(WatcherEnums.SandboxUnlockID.Rattler, MultiplayerUnlocks.SandboxUnlockID.DaddyLongLegs);
		AddCreature(WatcherEnums.SandboxUnlockID.RotLoach, WatcherEnums.SandboxUnlockID.Rattler);
		AddCreature(WatcherEnums.SandboxUnlockID.ProtoLizard, WatcherEnums.SandboxUnlockID.RotLoach);
		AddCreature(WatcherEnums.SandboxUnlockID.RotLizard, WatcherEnums.SandboxUnlockID.ProtoLizard);
		AddCreature(WatcherEnums.SandboxUnlockID.ScavengerTemplar, MultiplayerUnlocks.SandboxUnlockID.Scavenger);
		AddCreature(WatcherEnums.SandboxUnlockID.ScavengerDisciple, WatcherEnums.SandboxUnlockID.ScavengerTemplar);
		AddCreature(WatcherEnums.SandboxUnlockID.PeachLizard, MultiplayerUnlocks.SandboxUnlockID.YellowLizard);
		AddCreature(WatcherEnums.SandboxUnlockID.IndigoLizard, WatcherEnums.SandboxUnlockID.PeachLizard);
		AddCreature(WatcherEnums.SandboxUnlockID.BasiliskLizard, MultiplayerUnlocks.SandboxUnlockID.CyanLizard);
		AddCreature(WatcherEnums.SandboxUnlockID.BlizzardLizard, WatcherEnums.SandboxUnlockID.BasiliskLizard);
		AddItem(WatcherEnums.SandboxUnlockID.RotSeedCob, MultiplayerUnlocks.SandboxUnlockID.SeedCob);
		AddItem(WatcherEnums.SandboxUnlockID.RotDangleFruit, MultiplayerUnlocks.SandboxUnlockID.DangleFruit);
		AddItem(WatcherEnums.SandboxUnlockID.Pomegranate, WatcherEnums.SandboxUnlockID.RotSeedCob);
		AddItem(WatcherEnums.SandboxUnlockID.Boomerang, MultiplayerUnlocks.SandboxUnlockID.Lantern);
		AddItem(WatcherEnums.SandboxUnlockID.WeirdToy, WatcherEnums.SandboxUnlockID.Pomegranate);
		AddItem(WatcherEnums.SandboxUnlockID.SpinToy, WatcherEnums.SandboxUnlockID.Pomegranate);
		AddItem(WatcherEnums.SandboxUnlockID.SoftToy, WatcherEnums.SandboxUnlockID.Pomegranate);
		AddItem(WatcherEnums.SandboxUnlockID.BallToy, WatcherEnums.SandboxUnlockID.Pomegranate);
		AddItem(WatcherEnums.SandboxUnlockID.GraffitiBomb, MultiplayerUnlocks.SandboxUnlockID.FirecrackerPlant);
	}

	public static void RemoveUnlocks()
	{
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.Barnacle);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.Tardigrade);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.Frog);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.DrillCrab);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.SandGrub);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.BigSandGrub);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.MothGrub);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.SmallMoth);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.BigMoth);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.Angler);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.SkyWhale);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.AltSkyWhale);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.Loach);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.BoxWorm);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.FireSprite);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.Rat);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.Rattler);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.RotLoach);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.RotLizard);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.ProtoLizard);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.ScavengerTemplar);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.ScavengerDisciple);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.BlizzardLizard);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.BasiliskLizard);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.IndigoLizard);
		MultiplayerUnlocks.CreatureUnlockList.Remove(WatcherEnums.SandboxUnlockID.PeachLizard);
		MultiplayerUnlocks.ItemUnlockList.Remove(WatcherEnums.SandboxUnlockID.RotSeedCob);
		MultiplayerUnlocks.ItemUnlockList.Remove(WatcherEnums.SandboxUnlockID.RotDangleFruit);
		MultiplayerUnlocks.ItemUnlockList.Remove(WatcherEnums.SandboxUnlockID.Pomegranate);
		MultiplayerUnlocks.ItemUnlockList.Remove(WatcherEnums.SandboxUnlockID.Boomerang);
		MultiplayerUnlocks.ItemUnlockList.Remove(WatcherEnums.SandboxUnlockID.GraffitiBomb);
		MultiplayerUnlocks.ItemUnlockList.Remove(WatcherEnums.SandboxUnlockID.SoftToy);
		MultiplayerUnlocks.ItemUnlockList.Remove(WatcherEnums.SandboxUnlockID.BallToy);
		MultiplayerUnlocks.ItemUnlockList.Remove(WatcherEnums.SandboxUnlockID.SpinToy);
		MultiplayerUnlocks.ItemUnlockList.Remove(WatcherEnums.SandboxUnlockID.WeirdToy);
	}

	public static void AddCreature(MultiplayerUnlocks.SandboxUnlockID creature, MultiplayerUnlocks.SandboxUnlockID insertAfter)
	{
		if (!MultiplayerUnlocks.CreatureUnlockList.Contains(creature))
		{
			MultiplayerUnlocks.CreatureUnlockList.Insert(MultiplayerUnlocks.CreatureUnlockList.IndexOf(insertAfter) + 1, creature);
		}
	}

	public static void AddItem(MultiplayerUnlocks.SandboxUnlockID item, MultiplayerUnlocks.SandboxUnlockID insertAfter)
	{
		if (!MultiplayerUnlocks.ItemUnlockList.Contains(item))
		{
			MultiplayerUnlocks.ItemUnlockList.Insert(MultiplayerUnlocks.ItemUnlockList.IndexOf(insertAfter) + 1, item);
		}
	}
}
