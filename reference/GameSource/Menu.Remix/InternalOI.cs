using Menu.Remix.MixedUI;

namespace Menu.Remix;

public abstract class InternalOI : OptionInterface
{
	public enum Reason
	{
		NoInterface,
		NoMod,
		Statistics,
		TestOI
	}

	public Reason reason;

	public OpLabel labelID;

	public OpLabel labelVersion;

	public OpLabel labelAuthor;

	public OpLabel labelSluggo0;

	public OpLabel labelSluggo1;

	public InternalOI(ModManager.Mod rwMod, Reason type)
		: base(rwMod)
	{
		reason = type;
	}

	public override void Initialize()
	{
		Tabs = new OpTab[1];
		Tabs[0] = new OpTab(this);
	}
}
