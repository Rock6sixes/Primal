using System;
using UnityEngine;

namespace DevInterface;

public class EnumCycler<T> : MouseOverSwitchColorLabel where T : ExtEnum<T>
{
	public string baseName;

	public Func<T> getValue;

	public Action<T> setValue;

	public EnumCycler(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, float width, string baseName, Func<T> getValue, Action<T> setValue)
		: base(owner, IDstring, parentNode, pos, width, baseName + getValue().value)
	{
		this.baseName = baseName;
		this.getValue = getValue;
		this.setValue = setValue;
	}

	public override void Update()
	{
		base.Update();
		if (base.MouseOver && owner != null && owner.mouseClick)
		{
			int index = (getValue().Index + 1) % ExtEnum<T>.values.Count;
			setValue((T)Activator.CreateInstance(typeof(T), ExtEnum<T>.values.entries[index], false));
			base.Text = baseName + getValue().value;
		}
	}
}
