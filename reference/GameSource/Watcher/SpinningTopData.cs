using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Watcher;

public class SpinningTopData : PlacedObject.Data
{
	public Vector2 panelPos;

	public Vector2? destPos;

	public SlugcatStats.Timeline destTimeline;

	public string destRegion;

	public string destRoom;

	public int spawnIdentifier;

	public bool rippleWarp;

	public string RegionString
	{
		get
		{
			if (destRegion == null && destRoom == null)
			{
				return null;
			}
			if (destRegion != null)
			{
				return destRegion;
			}
			return destRoom.Split('_')[0];
		}
		set
		{
			destRegion = value;
		}
	}

	public SpinningTopData(PlacedObject owner)
		: base(owner)
	{
	}

	public override void FromString(string s)
	{
		string[] array = Regex.Split(s, "~");
		_ = array.Length;
		panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
		panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
		destTimeline = ((array[2] == "NULL") ? null : new SlugcatStats.Timeline(array[2]));
		destRegion = ((array[3] == "NULL") ? null : array[3]);
		destRoom = ((array[4] == "NULL") ? null : array[4]);
		if (array[5] == "NULL" || array[6] == "NULL")
		{
			destPos = null;
		}
		else
		{
			Vector2 zero = Vector2.zero;
			zero.x = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			zero.y = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
			destPos = zero;
		}
		spawnIdentifier = ((array.Length > 7) ? int.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture) : 0);
		rippleWarp = ((array.Length <= 8) ? rippleWarp : (array[8] == "true"));
		unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 9);
	}

	public override string ToString()
	{
		string baseString = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}", panelPos.x, panelPos.y, (destTimeline == null) ? "NULL" : destTimeline.ToString(), (RegionString == null) ? "NULL" : RegionString, (destRoom == null) ? "NULL" : destRoom, (!destPos.HasValue) ? "NULL" : destPos.Value.x.ToString(), (!destPos.HasValue) ? "NULL" : destPos.Value.y.ToString(), spawnIdentifier.ToString(), rippleWarp ? "true" : "false");
		baseString = SaveState.SetCustomData(this, baseString);
		return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
	}

	public WarpPoint.WarpPointData CreateWarpPointData(Room room)
	{
		WarpPoint.WarpPointData warpPointData = new WarpPoint.WarpPointData(null);
		warpPointData.destPos = destPos;
		warpPointData.RegionString = RegionString;
		warpPointData.destRoom = destRoom;
		warpPointData.destTimeline = destTimeline;
		warpPointData.panelPos = panelPos;
		warpPointData.deathPersistentWarpPoint = true;
		warpPointData.rippleWarp = rippleWarp;
		warpPointData.oneWay = rippleWarp || Region.IsWatcherVanillaRegion(room.world.name) || Region.IsVanillaSentientRotRegion(room.world.name);
		if (warpPointData.oneWay)
		{
			warpPointData.oneWayEntrance = true;
			warpPointData.oneWayEntranceIdentified = true;
		}
		if (room.game.IsStorySession)
		{
			warpPointData.cycleSpawnedOn = room.game.GetStorySession.saveState.cycleNumber;
		}
		warpPointData.destCam = WarpPoint.GetDestCam(warpPointData);
		return warpPointData;
	}
}
