using System;
using System.Collections.Generic;
using System.Text;
using RWCustom;
using SlugBase.Assets;
using UnityEngine;

namespace SlugBase.Interface;

internal class ErrorList
{
	private class ErrorInfo
	{
		public readonly ErrorIcon Icon;

		public readonly string Message;

		public readonly string File;

		public readonly Action Retry;

		public ErrorInfo(ErrorIcon icon, string message, string file, Action retry)
		{
			Icon = icon;
			Message = message;
			File = file;
			Retry = retry;
		}
	}

	public enum ErrorIcon
	{
		Character,
		Scene,
		Plugin,
		Timeline
	}

	public static ErrorList Instance;

	private readonly List<ErrorInfo> _errors = new List<ErrorInfo>();

	private readonly FContainer _container;

	private readonly FSprite _icon;

	private readonly FContainer _list;

	private FSprite _listBack;

	private bool _open;

	private bool _listDirty;

	private const float X_MARGIN = 10f;

	private const float Y_MARGIN = 10f;

	private const float TEXT_WIDTH = 400f;

	private const float ENTRY_HEIGHT = 47f;

	private const float ICON_SIZE = 16f;

	private const float ELEMENT_SPACING = 7f;

	private const float Y_TEXT_OFFSET = 2f;

	public FContainer Container => _container;

	public ErrorList()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		_container = new FContainer();
		_icon = new FSprite("slugbase/ui/error", true)
		{
			anchorX = 0f,
			anchorY = 1f
		};
		_list = new FContainer();
		_container.AddChild((FNode)(object)_icon);
		_container.AddChild((FNode)(object)_list);
	}

	public static ErrorList Attach()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		ErrorList list = new ErrorList();
		((FNode)list.Container).ListenForUpdate(new FutileUpdateDelegate(list.Update));
		((FContainer)Futile.stage).AddChild((FNode)(object)list.Container);
		SlugBaseCharacter.Registry.LoadFailed += delegate(object _, JsonRegistry<Name, SlugBaseCharacter>.LoadErrorEventArgs args)
		{
			Action retry4 = null;
			if (args.Path != null)
			{
				retry4 = delegate
				{
					SlugBaseCharacter.Registry.TryAddFromFile(args.Path);
				};
			}
			list.AddError(ErrorIcon.Character, args.ErrorMessage, args.Path, retry4);
			Debug.LogException(args.Exception);
		};
		CustomScene.Registry.LoadFailed += delegate(object _, JsonRegistry<SceneID, CustomScene>.LoadErrorEventArgs args)
		{
			Action retry3 = null;
			if (args.Path != null)
			{
				retry3 = delegate
				{
					CustomScene.Registry.TryAddFromFile(args.Path);
				};
			}
			list.AddError(ErrorIcon.Scene, args.ErrorMessage, args.Path, retry3);
			Debug.LogException(args.Exception);
		};
		CustomSlideshow.Registry.LoadFailed += delegate(object _, JsonRegistry<SlideShowID, CustomSlideshow>.LoadErrorEventArgs args)
		{
			Action retry2 = null;
			if (args.Path != null)
			{
				retry2 = delegate
				{
					CustomSlideshow.Registry.TryAddFromFile(args.Path);
				};
			}
			list.AddError(ErrorIcon.Scene, args.ErrorMessage, args.Path, retry2);
			Debug.LogException(args.Exception);
		};
		CustomTimeline.Registry.LoadFailed += delegate(object _, JsonRegistry<Timeline, CustomTimeline>.LoadErrorEventArgs args)
		{
			Action retry = null;
			if (args.Path != null)
			{
				retry = delegate
				{
					CustomTimeline.Registry.TryAddFromFile(args.Path);
				};
			}
			list.AddError(ErrorIcon.Timeline, args.ErrorMessage, args.Path, retry);
			Debug.LogException(args.Exception);
		};
		return list;
	}

	public void ClearFileErrors()
	{
		lock (_errors)
		{
			_errors.RemoveAll((ErrorInfo error) => error.File != null);
		}
	}

	public void MarkDirty()
	{
		_listDirty = true;
	}

	public void AddError(ErrorIcon icon, string message, string file, Action retry)
	{
		lock (_errors)
		{
			_listDirty = true;
			if (file != null)
			{
				_errors.RemoveAll((ErrorInfo error) => error.File == file);
			}
			_errors.Add(new ErrorInfo(icon, message, file, retry));
		}
	}

	private void RemoveError(ErrorInfo error)
	{
		lock (_errors)
		{
			_errors.Remove(error);
			_listDirty = true;
		}
	}

	public void Update()
	{
		((FNode)Container).SetPosition(10.1f, (float)Futile.screen.pixelHeight - 10f + 0.1f);
		((FNode)Container).MoveToFront();
		lock (_errors)
		{
			if (_listDirty)
			{
				RefreshList();
				_listDirty = false;
			}
			if (_icon.localRect.Contains(((FNode)_icon).GetLocalMousePosition()))
			{
				_open = true;
			}
			else if (_listBack != null && !_listBack.localRect.Contains(((FNode)_listBack).GetLocalMousePosition()))
			{
				_open = false;
			}
			if (_errors.Count == 0)
			{
				_open = false;
			}
			((FNode)_container).isVisible = _errors.Count > 0;
			((FNode)_icon).isVisible = !_open;
			((FNode)_list).isVisible = _open;
		}
	}

	private void RefreshList()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected O, but got Unknown
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Expected O, but got Unknown
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Expected O, but got Unknown
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Expected O, but got Unknown
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Expected O, but got Unknown
		_list.RemoveAllChildren();
		float num = 400f;
		FSprite val = (_listBack = new FSprite("pixel", true)
		{
			anchorX = 0f,
			anchorY = 1f,
			height = 7f + 54f * (float)_errors.Count
		});
		_list.AddChild((FNode)(object)val);
		float y = -7f;
		float x;
		foreach (ErrorInfo error in _errors)
		{
			x = 7f;
			FSprite val2 = new FSprite(GetIconSprite(error.Icon), true)
			{
				anchorX = 0f,
				anchorY = 1f
			};
			((FNode)val2).SetPosition(x, y);
			_list.AddChild((FNode)(object)val2);
			x += 23f;
			if (error.Retry != null)
			{
				AddButton("slugbase/ui/reload", (ButtonSignalDelegate)delegate
				{
					RemoveError(error);
					error.Retry();
				});
			}
			x += 23f;
			if (error.File != null)
			{
				AddButton("slugbase/ui/edit", (ButtonSignalDelegate)delegate
				{
					Application.OpenURL("file:///" + error.File);
				});
			}
			x += 23f;
			AddButton("slugbase/ui/cancel", (ButtonSignalDelegate)delegate
			{
				RemoveError(error);
			});
			x += 23f;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(error.Message);
			if (error.File != null)
			{
				string text = error.File;
				if (error.File.Replace('\\', '/').StartsWith(Custom.RootFolderDirectory().Replace('\\', '/'), StringComparison.InvariantCultureIgnoreCase))
				{
					text = text.Substring(Custom.RootFolderDirectory().Length);
				}
				stringBuilder.AppendLine("File: " + text);
			}
			FLabel val3 = new FLabel(Custom.GetFont(), stringBuilder.ToString())
			{
				anchorX = 0f,
				anchorY = 1f,
				color = Color.black
			};
			((FNode)val3).SetPosition(x, y + 2f);
			_list.AddChild((FNode)(object)val3);
			num = Math.Max(num, val3.textRect.width);
			y -= 54f;
		}
		val.width = 106f + num;
		void AddButton(string sprite, ButtonSignalDelegate onClick)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			FButton val4 = new FButton(sprite, sprite, sprite, (string)null)
			{
				anchorX = 0f,
				anchorY = 1f
			};
			val4.SetColors(SlugBasePlugin.SlugBaseBlue, Color.black, SlugBasePlugin.SlugBaseGray);
			((FNode)val4).SetPosition(x, y);
			_list.AddChild((FNode)(object)val4);
			val4.SignalRelease += onClick;
		}
	}

	private string GetIconSprite(ErrorIcon icon)
	{
		if (1 == 0)
		{
		}
		string result = icon switch
		{
			ErrorIcon.Character => "slugbase/ui/character", 
			ErrorIcon.Scene => "slugbase/ui/scene", 
			ErrorIcon.Plugin => "slugbase/ui/plugin", 
			ErrorIcon.Timeline => "slugbase/ui/timeline", 
			_ => "pixel", 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	public void Remove()
	{
		((FNode)_container).RemoveFromContainer();
	}
}
