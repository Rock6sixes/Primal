using System;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public abstract class FocusPointer : UIfocusable
{
	public override bool CurrentlyFocusableMouse => true;

	public override bool CurrentlyFocusableNonMouse => true;

	public FocusPointer()
		: base(-5000f * Vector2.one, Vector2.one)
	{
	}

	public override void GrafUpdate(float timeStacker)
	{
	}

	public override void Update()
	{
	}

	public virtual UIfocusable GetPointed(NextDirection dir)
	{
		throw new NotImplementedException("Cannot use base FocusPointer. Make a child class and override this method.");
	}
}
