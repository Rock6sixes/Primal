using System.IO;
using System.Linq;
using Menu;
using UnityEngine;

namespace SlugBase.Assets;

/// <summary>
/// An intro cutscene added by SlugBase.
/// </summary>
public class CustomSlideshow
{
	/// <summary>
	/// A scene from a <see cref="T:SlugBase.Assets.CustomSlideshow" /> that holds data about when to appear and what images to use for what amount of time.
	/// </summary>
	public class CustomSlideshowScene : CustomScene
	{
		/// <summary>
		/// The time that this scene will start fading in, in seconds.
		/// </summary>
		public float StartAt { get; set; }

		/// <summary>
		/// The time that this image will finish fading in, in seconds.
		/// </summary>
		public float FadeInDoneAt { get; set; }

		/// <summary>
		/// The time that this image will start fading out at, in seconds.
		/// </summary>
		public float FadeOutStartAt { get; set; }

		/// <summary>
		/// The positions that the camera will focus on when playing this scene.
		/// </summary>
		/// <remarks>
		/// X and Y represent the pixel position of the camera, while Z represents the focus depth.
		/// </remarks>
		public Vector3[] CameraMovement { get; set; }

		/// <summary>
		/// Creates a new Scene from JSON.
		/// </summary>
		/// <param name="json">The JSON data to load from.</param>
		public CustomSlideshowScene(JsonObject json)
			: base(new SceneID(json.GetString("name"), false), json)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Expected O, but got Unknown
			StartAt = json.GetFloat("fade_in_start");
			FadeInDoneAt = json.TryGet("fade_in_end")?.AsFloat() ?? (StartAt + 1f);
			FadeOutStartAt = json.GetFloat("fade_out_start");
			JsonList? jsonList = json.TryGet("camera_path")?.AsList();
			if (jsonList.HasValue)
			{
				JsonList valueOrDefault = jsonList.GetValueOrDefault();
				if (true)
				{
					CameraMovement = new Vector3[valueOrDefault.Count];
					for (int i = 0; i < CameraMovement.Length; i++)
					{
						JsonList list = valueOrDefault.GetList(i);
						CameraMovement[i] = new Vector3(list.GetFloat(0), list.GetFloat(1), list.TryGet(2)?.AsFloat() ?? (-1f));
					}
					return;
				}
			}
			CameraMovement = new Vector3[1]
			{
				new Vector3(0f, 0f, -1f)
			};
		}
	}

	/// <summary>
	/// Data about a song from a <see cref="T:SlugBase.Assets.CustomSlideshow" />.
	/// </summary>
	public class SlideshowMusic
	{
		/// <summary>
		/// The file name of the sound to use. This comes from the `StreamingAssets/music/songs` folder.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The duration of the song's fade in, in seconds.
		/// </summary>
		public float FadeIn { get; set; }

		/// <summary>
		/// Load information about a slideshow song from JSON data.
		/// </summary>
		/// <param name="json">The JSON data to load from.</param>
		public SlideshowMusic(JsonObject json)
		{
			Name = json.GetString("name");
			FadeIn = json.TryGet("fadein")?.AsFloat() ?? 40f;
		}
	}

	/// <summary>
	/// Stores all registered <see cref="T:SlugBase.Assets.CustomSlideshow" />s.
	/// </summary>
	public static JsonRegistry<SlideShowID, CustomSlideshow> Registry { get; } = new JsonRegistry<SlideShowID, CustomSlideshow>((SlideShowID key, JsonObject json) => new CustomSlideshow(key, json));


	/// <summary>
	/// This scene's unique ID.
	/// </summary>
	public SlideShowID ID { get; }

	/// <summary>
	/// A path relative to StreamingAssets to load images from.
	/// </summary>
	public string SlideshowFolder { get; }

	/// <summary>
	/// The music to play during a custom intro or outro
	/// </summary>
	public SlideshowMusic Music { get; }

	/// <summary>
	/// An array of images and other data in this scene.
	/// </summary>
	public CustomSlideshowScene[] Scenes { get; }

	/// <summary>
	/// The process to go to after playing the slideshow
	/// </summary>
	public ProcessID Process { get; }

	private CustomSlideshow(SlideShowID id, JsonObject json)
	{
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		ID = id;
		SlideshowFolder = json.TryGet("slideshow_folder")?.AsString().Replace('/', Path.DirectorySeparatorChar);
		JsonAny? jsonAny = json.TryGet("music");
		if (jsonAny.HasValue)
		{
			JsonAny valueOrDefault = jsonAny.GetValueOrDefault();
			if (true)
			{
				Music = new SlideshowMusic(valueOrDefault.AsObject());
			}
		}
		Scenes = (from img in json.GetList("scenes")
			select new CustomSlideshowScene(img.AsObject())).ToArray();
		Process = new ProcessID(json.GetString("next_process"), false);
	}

	internal CustomSlideshowScene GetScene(SceneID id)
	{
		return Scenes.FirstOrDefault((CustomSlideshowScene scene) => (ExtEnum<SceneID>)(object)scene.ID == (ExtEnum<SceneID>)(object)id);
	}
}
