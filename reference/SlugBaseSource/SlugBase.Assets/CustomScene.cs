using System;
using System.IO;
using System.Linq;
using Menu;
using SlugBase.SaveData;
using UnityEngine;

namespace SlugBase.Assets;

/// <summary>
/// A scene added by SlugBase.
/// </summary>
public class CustomScene
{
	/// <summary>
	/// An image from a <see cref="T:SlugBase.Assets.CustomScene" />.
	/// </summary>
	public class Image
	{
		/// <summary>
		/// The file name of the image to load. This is combined with <see cref="P:SlugBase.Assets.CustomScene.SceneFolder" />.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The pixel position of this image's bottom left corner in the scene.
		/// </summary>
		public Vector2 Position { get; set; }

		/// <summary>
		/// The depth of this image in the scene.
		/// </summary>
		public float Depth { get; set; } = -1f;


		/// <summary>
		/// The shader to use when rendering. Defaults to <see cref="F:Menu.MenuDepthIllustration.MenuShader.Normal" />.
		/// </summary>
		public MenuShader Shader { get; set; }

		/// <summary>
		/// If <c>true</c>, this image will display when in flat mode and will be hidden otherwise.
		/// </summary>
		public bool Flatmode { get; set; }

		/// <summary>
		/// Creates a new image.
		/// </summary>
		/// <param name="name">The file name.</param>
		/// <param name="position">The pixel position of the bottom left corner.</param>
		/// <exception cref="T:System.ArgumentNullException"></exception>
		public Image(string name, Vector2 position)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			Name = name.Replace('/', Path.DirectorySeparatorChar);
			Position = position;
		}

		/// <summary>
		/// Creates a new image from JSON.
		/// </summary>
		/// <param name="json">The JSON data to load from.</param>
		public Image(JsonObject json)
			: this(json.GetString("name"), JsonUtils.ToVector2(json.Get("pos")))
		{
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			Depth = json.TryGet("depth")?.AsFloat() ?? (-1f);
			string text = json.TryGet("shader")?.AsString();
			Shader = (MenuShader)((text == null) ? ((object)MenuShader.Normal) : ((object)new MenuShader(text, false)));
			Flatmode = json.TryGet("flatmode")?.AsBool() ?? false;
		}
	}

	/// <summary>
	/// Stores all registered <see cref="T:SlugBase.Assets.CustomScene" />s.
	/// </summary>
	public static JsonRegistry<SceneID, CustomScene> Registry { get; } = new JsonRegistry<SceneID, CustomScene>((SceneID key, JsonObject json) => new CustomScene(key, json));


	/// <summary>
	/// This scene's unique ID.
	/// </summary>
	public SceneID ID { get; }

	/// <summary>
	/// An array of images in this scene.
	/// </summary>
	public Image[] Images { get; }

	/// <summary>
	/// An array of depths that the camera may focus on.
	/// </summary>
	public float[] IdleDepths { get; }

	/// <summary>
	/// A path relative to StreamingAssets to load images from.
	/// </summary>
	public string SceneFolder { get; }

	/// <summary>
	/// The position of the glow's center.
	/// <para>Only effective when used on the slugcat select screen.</para>
	/// </summary>
	public Vector2? GlowPos { get; }

	/// <summary>
	/// The position of the mark's center.
	/// <para>Only effective when used on the slugcat select screen.</para>
	/// </summary>
	public Vector2? MarkPos { get; }

	/// <summary>
	/// The pixel offset for this scene in the select menu.
	/// <para>Only effective when used on the slugcat select screen.</para>
	/// </summary>
	public Vector2? SelectMenuOffset { get; }

	/// <summary>
	/// The depth of the slugcat image in this scene.
	/// <para>Only effective when used on the slugcat select screen.</para>
	/// </summary>
	public float? SlugcatDepth { get; }

	/// <summary>
	/// If a scene is used as a dream, should it replace any current dream.
	/// </summary>
	public bool OverrideDream { get; }

	public static void SetSelectMenuScene(SaveState save, SceneID id)
	{
		SlugBaseSaveData slugBaseData = save.deathPersistentSaveData.GetSlugBaseData();
		if ((ExtEnum<SceneID>)(object)id == (ExtEnum<SceneID>)null)
		{
			slugBaseData.RemoveInternal("SELECTSCENE");
		}
		else
		{
			slugBaseData.SetInternal("SELECTSCENE", ((ExtEnumBase)id).value);
		}
	}

	internal CustomScene(SceneID id, JsonObject json)
	{
		ID = id;
		Images = (from img in json.GetList("images")
			select new Image(img.AsObject())).ToArray();
		if (!(this is CustomSlideshow.CustomSlideshowScene))
		{
			IdleDepths = (from depth in json.GetList("idle_depths")
				select depth.AsFloat()).ToArray();
		}
		SceneFolder = json.TryGet("scene_folder")?.AsString().Replace('/', Path.DirectorySeparatorChar);
		JsonAny? jsonAny = json.TryGet("glow_pos");
		if (jsonAny.HasValue)
		{
			JsonAny valueOrDefault = jsonAny.GetValueOrDefault();
			if (true)
			{
				GlowPos = JsonUtils.ToVector2(valueOrDefault);
			}
		}
		jsonAny = json.TryGet("mark_pos");
		if (jsonAny.HasValue)
		{
			JsonAny valueOrDefault2 = jsonAny.GetValueOrDefault();
			if (true)
			{
				MarkPos = JsonUtils.ToVector2(valueOrDefault2);
			}
		}
		jsonAny = json.TryGet("select_menu_pos");
		if (jsonAny.HasValue)
		{
			JsonAny valueOrDefault3 = jsonAny.GetValueOrDefault();
			if (true)
			{
				SelectMenuOffset = JsonUtils.ToVector2(valueOrDefault3);
			}
		}
		SlugcatDepth = json.TryGet("slugcat_depth")?.AsFloat();
		OverrideDream = json.TryGet("dream_override")?.AsBool() ?? true;
	}
}
