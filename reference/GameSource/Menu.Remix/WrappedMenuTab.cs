namespace Menu.Remix;

public class WrappedMenuTab : MenuTab
{
	public MenuTabWrapper wrapper;

	public WrappedMenuTab(MenuTabWrapper wrapper)
	{
		this.wrapper = wrapper;
		this.wrapper.Container.AddChild(_container);
	}
}
