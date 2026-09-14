using System;
using System.Collections.Generic;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpSimpleImageButton : OpSimpleButton
{
	public class QueueTexture : FocusableQueue
	{
		public string description = "";

		public Texture2D image;

		public string label;

		public OnSignalHandler onPressInit;

		public OnSignalHandler onPressHold;

		public OnSignalHandler onClick;

		public override float sizeY => (float)image.height + 6f;

		public QueueTexture(Texture2D image, string label = "", object sign = null)
			: base(sign)
		{
			if (!(image != null))
			{
				throw new ArgumentNullException("There is no Texture2D for OpSimpleImageButton");
			}
			this.image = image;
			this.label = label;
		}

		public override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			float num = (float)image.width + 6f;
			List<UIelement> list = new List<UIelement>();
			OpSimpleImageButton opSimpleImageButton = new OpSimpleImageButton(new Vector2(posX, posY), new Vector2(num, sizeY), image)
			{
				sign = sign,
				description = UIQueue.Translate(description)
			};
			if (onChange != null)
			{
				opSimpleImageButton.OnChange += onChange;
			}
			if (onHeld != null)
			{
				opSimpleImageButton.OnHeld += onHeld;
			}
			if (onPressInit != null)
			{
				opSimpleImageButton.OnPressInit += onPressInit;
			}
			if (onPressHold != null)
			{
				opSimpleImageButton.OnPressHold += onPressHold;
			}
			if (onClick != null)
			{
				opSimpleImageButton.OnClick += onClick;
			}
			mainFocusable = opSimpleImageButton;
			list.Add(opSimpleImageButton);
			if (!string.IsNullOrEmpty(label))
			{
				OpLabel item = new OpLabel(new Vector2(posX + num + 10f, posY), new Vector2(holder.CanvasSize.x - posX - num - 10f, 30f), UIQueue.Translate(label), FLabelAlignment.Left)
				{
					autoWrap = true,
					bumpBehav = opSimpleImageButton.bumpBehav,
					description = opSimpleImageButton.description
				};
				list.Add(item);
			}
			hasInitialized = true;
			return list;
		}
	}

	public class QueueSprite : FocusableQueue
	{
		public string description = "";

		public string elementName;

		public Texture2D image;

		public string label;

		public OnSignalHandler onPressInit;

		public OnSignalHandler onPressHold;

		public OnSignalHandler onClick;

		public override float sizeY => Futile.atlasManager.GetElementWithName(elementName).sourcePixelSize.y + 6f;

		public QueueSprite(string elementName, string label = "", object sign = null)
			: base(sign)
		{
			if (!Futile.atlasManager.DoesContainElementWithName(elementName))
			{
				throw new ArgumentNullException("There is no such FAtlasElement called [" + elementName + "]");
			}
			this.elementName = elementName;
			this.label = label;
		}

		public override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			float num = Futile.atlasManager.GetElementWithName(elementName).sourcePixelSize.x + 6f;
			List<UIelement> list = new List<UIelement>();
			OpSimpleImageButton opSimpleImageButton = new OpSimpleImageButton(new Vector2(posX, posY), new Vector2(num, sizeY), elementName)
			{
				sign = sign,
				description = UIQueue.Translate(description)
			};
			if (onChange != null)
			{
				opSimpleImageButton.OnChange += onChange;
			}
			if (onHeld != null)
			{
				opSimpleImageButton.OnHeld += onHeld;
			}
			if (onPressInit != null)
			{
				opSimpleImageButton.OnPressInit += onPressInit;
			}
			if (onPressHold != null)
			{
				opSimpleImageButton.OnPressHold += onPressHold;
			}
			if (onClick != null)
			{
				opSimpleImageButton.OnClick += onClick;
			}
			mainFocusable = opSimpleImageButton;
			list.Add(opSimpleImageButton);
			if (!string.IsNullOrEmpty(label))
			{
				OpLabel item = new OpLabel(new Vector2(posX + num + 10f, posY), new Vector2(holder.CanvasSize.x - posX - num - 10f, 30f), UIQueue.Translate(label), FLabelAlignment.Left)
				{
					autoWrap = true,
					bumpBehav = opSimpleImageButton.bumpBehav,
					description = opSimpleImageButton.description
				};
				list.Add(item);
			}
			hasInitialized = true;
			return list;
		}
	}

	public FSprite sprite;

	public bool isTexture;

	public OpSimpleImageButton(Vector2 pos, Vector2 size, string fAtlasElement)
		: base(pos, size)
	{
		isTexture = false;
		FAtlasElement elementWithName;
		try
		{
			elementWithName = Futile.atlasManager.GetElementWithName(fAtlasElement);
		}
		catch (Exception ex)
		{
			throw new ElementFormatException(this, string.Concat("There is no such FAtlasElement called ", fAtlasElement, " : ", "\r\n", ex, ")"));
		}
		sprite = new FSprite(elementWithName.name);
		myContainer.AddChild(sprite);
		sprite.SetAnchor(0.5f, 0.5f);
		sprite.SetPosition(base.size.x / 2f, base.size.y / 2f);
	}

	public OpSimpleImageButton(Vector2 pos, Vector2 size, Texture2D image)
		: base(pos, size)
	{
		isTexture = true;
		if (image == null)
		{
			throw new ElementFormatException(this, "There is no Texture2D for OpSimpleImageButton");
		}
		sprite = new FTexture(image, "sib" + image.GetHashCode());
		sprite.SetAnchor(0.5f, 0.5f);
		myContainer.AddChild(sprite);
		sprite.SetPosition(base.size.x / 2f, base.size.y / 2f);
	}

	public override void Change()
	{
		base.Change();
		sprite.SetPosition(base.size.x / 2f, base.size.y / 2f);
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (!isTexture)
		{
			sprite.color = base.bumpBehav.GetColor(colorEdge);
		}
	}

	public void ChangeImage(Texture2D newImage)
	{
		if (!isTexture)
		{
			throw new InvalidActionException(this, "You must construct this with Texture2D to use this function");
		}
		if (newImage == null)
		{
			MachineConnector.LogError("newImage is null in OpSimpleImageButton.ChangeImage!");
			return;
		}
		(sprite as FTexture).SetTexture(newImage);
		myContainer.AddChild(sprite);
	}

	public void ChangeElement(string newElement)
	{
		if (isTexture)
		{
			throw new InvalidActionException(this, "You must construct this with a name of FAtlasElement to use this function");
		}
		if (string.IsNullOrEmpty(newElement))
		{
			MachineConnector.LogError("newElement is null in OpImage.ChangeElement!");
		}
		else if (!(sprite.element.name == newElement))
		{
			if (!Futile.atlasManager.DoesContainElementWithName(newElement))
			{
				MachineConnector.LogError("There is no such FAtlasElement called [" + newElement + "]");
			}
			else
			{
				sprite.element = Futile.atlasManager.GetElementWithName(newElement);
			}
		}
	}

	public override void Unload()
	{
		base.Unload();
		if (isTexture)
		{
			(sprite as FTexture).Destroy();
		}
	}
}
