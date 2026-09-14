using System;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public abstract class UIconfig : UIfocusable
{
	public abstract class ConfigQueue : FocusableQueue
	{
		public ConfigurableBase config;

		public OnValueChangeHandler onValueUpdate;

		public OnValueChangeHandler onValueChanged;

		public ConfigQueue(ConfigurableBase config, object sign = null)
			: base(sign)
		{
			this.config = config;
		}
	}

	public const string errorNull = "config cannot be null. If you want a cosmetic UIconfig, generate cosmetic Configurable with OptionInterface.config.Bind.";

	public ConfigurableBase cfgEntry;

	public bool cosmetic;

	public string lastValue;

	public string _value;

	public string defaultValue { get; set; }

	public string Key => cfgEntry.key;

	public virtual string value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value != value)
			{
				FocusMoveDisallow();
				string oldValue = _value;
				_value = value;
				if (!UIelement.ContextWrapped && ConfigContainer.instance != null)
				{
					ConfigContainer.instance.NotifyConfigChange(this, oldValue, value);
				}
				this.OnValueUpdate?.Invoke(this, value, oldValue);
				Change();
				if (!held)
				{
					this.OnValueChanged?.Invoke(this, _value, lastValue);
					lastValue = _value;
				}
			}
		}
	}

	public override bool held
	{
		get
		{
			return base.held;
		}
		set
		{
			base.held = value;
			if (!value && base.Focused && lastValue != _value)
			{
				this.OnValueChanged?.Invoke(this, _value, lastValue);
			}
			lastValue = _value;
		}
	}

	public event OnValueChangeHandler OnValueUpdate;

	public event OnValueChangeHandler OnValueChanged;

	public UIconfig(ConfigurableBase config, Vector2 pos, Vector2 size)
		: base(pos, size)
	{
		if (config == null)
		{
			throw new ArgumentNullException("config cannot be null. If you want a cosmetic UIconfig, generate cosmetic Configurable with OptionInterface.config.Bind.");
		}
		if (config.BoundUIconfig != null)
		{
			throw new MultiuseConfigurableException(config);
		}
		config.BoundUIconfig = this;
		cfgEntry = config;
		cosmetic = cfgEntry.IsCosmetic;
		defaultValue = cfgEntry.defaultValue;
		_value = defaultValue;
		lastValue = _value;
	}

	public UIconfig(ConfigurableBase config, Vector2 pos, float rad)
		: base(pos, rad)
	{
		if (config == null)
		{
			throw new ArgumentNullException("config cannot be null. If you want a cosmetic UIconfig, generate cosmetic Configurable with OptionInterface.config.Bind.");
		}
		if (config.BoundUIconfig != null)
		{
			throw new MultiuseConfigurableException(config);
		}
		config.BoundUIconfig = this;
		cfgEntry = config;
		cosmetic = cfgEntry.IsCosmetic;
		defaultValue = cfgEntry.defaultValue;
		_value = defaultValue;
		lastValue = _value;
	}

	public override void Reset()
	{
		base.Reset();
		value = defaultValue;
		held = false;
	}

	public void ForceValue(string newValue)
	{
		_value = newValue;
	}

	public void ShowConfig()
	{
		value = (cfgEntry.OI.config.pendingReset ? cfgEntry.defaultValue : ValueConverter.ConvertToString(cfgEntry.BoxedValue, cfgEntry.settingType));
	}

	public virtual bool CopyFromClipboard(string value)
	{
		try
		{
			this.value = value;
			held = false;
			return this.value == value;
		}
		catch
		{
			return false;
		}
	}

	public virtual string CopyToClipboard()
	{
		held = false;
		return value;
	}

	public override void Deactivate()
	{
		held = false;
		base.Deactivate();
	}

	public override void Unload()
	{
		base.Unload();
		cfgEntry.BoundUIconfig = null;
	}

	public void _UndoCallChanges()
	{
		this.OnValueUpdate?.Invoke(this, _value, lastValue);
		this.OnValueChanged?.Invoke(this, _value, lastValue);
		lastValue = _value;
	}
}
