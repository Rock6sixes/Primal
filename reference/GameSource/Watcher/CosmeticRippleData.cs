using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Watcher;

public class CosmeticRippleData : PlacedObject.Data
{
	public Vector2 panelPos;

	public Vector2 handlePos;

	public Vector2 pos;

	public CosmeticRipple obj;

	public float scale;

	public float fallOff;

	public float intensity;

	public bool invert;

	public float depthMix;

	public bool square;

	public bool leavesTrail;

	public float squish;

	public bool animateOnSpawn;

	public float animationSpeed;

	public int cycleExpiry;

	public int cycleSpawnedOn;

	public bool isGameplay;

	public bool isTransition;

	public CosmeticRippleData(PlacedObject owner)
		: base(owner)
	{
		panelPos = Vector2.zero;
		handlePos = Vector2.left * 100f;
		scale = 0f;
		fallOff = 0.5f;
		intensity = 1f;
		invert = false;
		depthMix = 0.5f;
		square = false;
		leavesTrail = false;
		squish = 0f;
		animationSpeed = 0.05f;
		cycleExpiry = 0;
		cycleSpawnedOn = -1;
		isGameplay = false;
	}

	public override void FromString(string s)
	{
		string[] array = Regex.Split(s, "~");
		_ = array.Length;
		panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
		panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
		handlePos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
		handlePos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
		pos.x = ((array.Count() > 4) ? float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture) : 0f);
		pos.y = ((array.Count() > 5) ? float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture) : 0f);
		scale = ((array.Count() > 6) ? float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture) : scale);
		fallOff = ((array.Count() > 7) ? float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture) : fallOff);
		intensity = ((array.Count() > 8) ? float.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture) : intensity);
		invert = ((array.Count() > 9) ? bool.Parse(array[9]) : invert);
		depthMix = ((array.Count() > 10) ? float.Parse(array[10], NumberStyles.Any, CultureInfo.InvariantCulture) : depthMix);
		square = ((array.Count() > 11) ? bool.Parse(array[11]) : square);
		squish = ((array.Count() > 12) ? float.Parse(array[12], NumberStyles.Any, CultureInfo.InvariantCulture) : squish);
		leavesTrail = ((array.Count() > 13) ? bool.Parse(array[13]) : leavesTrail);
		cycleSpawnedOn = ((array.Count() > 14) ? int.Parse(array[14], NumberStyles.Any, CultureInfo.InvariantCulture) : cycleSpawnedOn);
		cycleExpiry = ((array.Count() > 15) ? int.Parse(array[15], NumberStyles.Any, CultureInfo.InvariantCulture) : cycleExpiry);
		isGameplay = ((array.Count() > 16) ? bool.Parse(array[16]) : isGameplay);
		unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 17);
	}

	public override string ToString()
	{
		string baseString = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}~{11}~{12}~{13}~{14}~{15}~{16}", panelPos.x, panelPos.y, handlePos.x, handlePos.y, pos.x, pos.y, scale, fallOff, intensity, invert, depthMix, square, squish, leavesTrail, cycleSpawnedOn, cycleExpiry, isGameplay);
		baseString = SaveState.SetCustomData(this, baseString);
		return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
	}
}
