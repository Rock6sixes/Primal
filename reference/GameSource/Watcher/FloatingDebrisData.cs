using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Watcher;

public class FloatingDebrisData : PlacedObject.Data
{
	public FloatingDebris obj;

	public Vector2 panelPos = Vector2.one * 50f;

	public int seed;

	public int _depthNear;

	public int _depthFar = 30;

	public int numberOfFloaters = 6;

	public float _scaleMinimum = 0.25f;

	public float _scaleMaximum = 0.5f;

	public float movement = 0.5f;

	public string type = "Test Squares";

	public int rippleLayer;

	public List<float> controlPointPosX = new List<float>();

	public List<float> controlPointPosY = new List<float>();

	public List<float> controlPointOffsetAmount = new List<float>();

	public List<float> controlPointDepthOffset = new List<float>();

	public List<float> controlPointScaleOffset = new List<float>();

	public List<Vector4> controlPointExtraOffset = new List<Vector4>();

	public int depthNear
	{
		get
		{
			return _depthNear;
		}
		set
		{
			_depthNear = value;
			_depthFar = ((_depthFar < _depthNear) ? _depthNear : _depthFar);
		}
	}

	public int depthFar
	{
		get
		{
			return _depthFar;
		}
		set
		{
			_depthFar = value;
			_depthNear = ((_depthNear > _depthFar) ? _depthFar : _depthNear);
		}
	}

	public float scaleMinimum
	{
		get
		{
			return _scaleMinimum;
		}
		set
		{
			_scaleMinimum = value;
			_scaleMaximum = ((_scaleMaximum < _scaleMinimum) ? _scaleMinimum : _scaleMaximum);
		}
	}

	public float scaleMaximum
	{
		get
		{
			return _scaleMaximum;
		}
		set
		{
			_scaleMaximum = value;
			_scaleMinimum = ((_scaleMinimum > _scaleMaximum) ? _scaleMaximum : _scaleMinimum);
		}
	}

	public Vector2 ConPos(int i)
	{
		i = Mathf.Clamp(i, 0, controlPointPosX.Count - 1);
		return new Vector2(controlPointPosX[i], controlPointPosY[i]);
	}

	public FloatingDebrisData(PlacedObject owner)
		: base(owner)
	{
		AddControlPoint(-50f, 50f, 0.5f, 0f, 0f, Vector4.zero);
	}

	public void AddControlPoint(float x, float y, float offsetAmount, float depthOffset, float scaleOffset, Vector4 extraOffsets)
	{
		controlPointPosX.Add(x);
		controlPointPosY.Add(y);
		controlPointOffsetAmount.Add(offsetAmount);
		controlPointDepthOffset.Add(depthOffset);
		controlPointScaleOffset.Add(scaleOffset);
		controlPointExtraOffset.Add(extraOffsets);
	}

	public void AddControlPoint(float x, float y, float offsetAmount, float depthOffset, float scaleOffset)
	{
		AddControlPoint(x, y, offsetAmount, depthOffset, scaleOffset, Vector2.zero);
	}

	public void AddControlPointRight()
	{
		if (controlPointPosX.Count == 0)
		{
			AddControlPoint(0f, 0f, 1f, 0f, 0f, Vector4.zero);
			return;
		}
		controlPointPosX.Add(controlPointPosX[controlPointPosX.Count - 1] + 50f);
		controlPointPosY.Add(controlPointPosY[controlPointPosY.Count - 1]);
		controlPointOffsetAmount.Add(0.25f);
		controlPointDepthOffset.Add(0f);
		controlPointScaleOffset.Add(0f);
		controlPointExtraOffset.Add(Vector4.zero);
	}

	public void AddControlPointLeft()
	{
		if (controlPointPosX.Count == 0)
		{
			AddControlPoint(0f, 0f, 1f, 0f, 0f, Vector4.zero);
			return;
		}
		controlPointPosX.Insert(0, controlPointPosX[0] - 50f);
		controlPointPosY.Insert(0, controlPointPosY[0]);
		controlPointOffsetAmount.Insert(0, 0.25f);
		controlPointDepthOffset.Insert(0, 0f);
		controlPointScaleOffset.Insert(0, 0f);
		controlPointExtraOffset.Insert(0, Vector4.zero);
	}

	public void RemoveControlPointRight()
	{
		if (controlPointPosX.Count > 1)
		{
			controlPointPosX.Pop();
			controlPointPosY.Pop();
			controlPointOffsetAmount.Pop();
			controlPointDepthOffset.Pop();
			controlPointScaleOffset.Pop();
			controlPointExtraOffset.Pop();
		}
	}

	public void RemoveControlPointLeft()
	{
		if (controlPointPosX.Count > 1)
		{
			controlPointPosX.RemoveAt(0);
			controlPointPosY.RemoveAt(0);
			controlPointOffsetAmount.RemoveAt(0);
			controlPointDepthOffset.RemoveAt(0);
			controlPointScaleOffset.RemoveAt(0);
			controlPointExtraOffset.RemoveAt(0);
		}
	}

	public void ClearControlPoints()
	{
		controlPointPosX.Clear();
		controlPointPosY.Clear();
		controlPointOffsetAmount.Clear();
		controlPointDepthOffset.Clear();
		controlPointScaleOffset.Clear();
		controlPointExtraOffset.Clear();
	}

	public override void FromString(string s)
	{
		string[] array = Regex.Split(s, "~");
		int num = -1;
		panelPos.x = ((array.Length > ++num) ? float.Parse(array[num], NumberStyles.Any, CultureInfo.InvariantCulture) : panelPos.x);
		panelPos.y = ((array.Length > ++num) ? float.Parse(array[num], NumberStyles.Any, CultureInfo.InvariantCulture) : panelPos.y);
		seed = ((array.Length > ++num) ? int.Parse(array[num], NumberStyles.Any, CultureInfo.InvariantCulture) : 0);
		_depthNear = ((array.Length > ++num) ? int.Parse(array[num], NumberStyles.Any, CultureInfo.InvariantCulture) : _depthNear);
		_depthFar = ((array.Length > ++num) ? int.Parse(array[num], NumberStyles.Any, CultureInfo.InvariantCulture) : _depthFar);
		_scaleMinimum = ((array.Length > ++num) ? float.Parse(array[num], NumberStyles.Any, CultureInfo.InvariantCulture) : _scaleMinimum);
		_scaleMaximum = ((array.Length > ++num) ? float.Parse(array[num], NumberStyles.Any, CultureInfo.InvariantCulture) : _scaleMaximum);
		numberOfFloaters = ((array.Length > ++num) ? int.Parse(array[num], NumberStyles.Any, CultureInfo.InvariantCulture) : numberOfFloaters);
		controlPointPosX = ((array.Length > ++num) ? (from e in array[num].Split('|')
			select float.Parse(e, NumberStyles.Any, CultureInfo.InvariantCulture)).ToList() : controlPointPosX);
		controlPointPosY = ((array.Length > ++num) ? (from e in array[num].Split('|')
			select float.Parse(e, CultureInfo.InvariantCulture)).ToList() : controlPointPosY);
		controlPointOffsetAmount = ((array.Length > ++num) ? (from e in array[num].Split('|')
			select float.Parse(e, NumberStyles.Any, CultureInfo.InvariantCulture)).ToList() : controlPointOffsetAmount);
		controlPointDepthOffset = ((array.Length > ++num) ? (from e in array[num].Split('|')
			select float.Parse(e, NumberStyles.Any, CultureInfo.InvariantCulture)).ToList() : controlPointDepthOffset);
		controlPointScaleOffset = ((array.Length > ++num) ? (from e in array[num].Split('|')
			select float.Parse(e, NumberStyles.Any, CultureInfo.InvariantCulture)).ToList() : controlPointScaleOffset);
		type = ((array.Length > ++num) ? array[num] : type);
		movement = ((array.Length > ++num) ? float.Parse(array[num], NumberStyles.Any, CultureInfo.InvariantCulture) : movement);
		controlPointExtraOffset = ((array.Length > ++num) ? LoadExtraOffsets(array[num]) : controlPointPosX.Select((float _) => Vector4.zero).ToList());
		rippleLayer = ((array.Length > ++num) ? int.Parse(array[num], NumberStyles.Any, CultureInfo.InvariantCulture) : rippleLayer);
	}

	public List<Vector4> LoadExtraOffsets(string e)
	{
		string[] array = e.Split('|').ToArray();
		int num = Mathf.Min(controlPointPosX.Count, array.Length);
		List<Vector4> list = new List<Vector4>();
		for (int i = 0; i < num; i++)
		{
			if (i >= num)
			{
				list.Add(Vector4.zero);
				continue;
			}
			float[] array2 = (from s in array[i].Split('v')
				select float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture)).ToArray();
			list.Add(new Vector4(array2[0], array2[1], array2[2], array2[3]));
		}
		return list;
	}

	public override string ToString()
	{
		string baseString = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}~{11}~{12}~{13}~{14}~{15}~{16}", panelPos.x, panelPos.y, seed, _depthNear, _depthFar, _scaleMinimum, _scaleMaximum, numberOfFloaters, string.Join("|", controlPointPosX), string.Join("|", controlPointPosY), string.Join("|", controlPointOffsetAmount), string.Join("|", controlPointDepthOffset), string.Join("|", controlPointScaleOffset), type, movement, string.Join("|", controlPointExtraOffset.Select((Vector4 v) => $"{v.x}v{v.y}v{v.z}v{v.w}")), rippleLayer);
		baseString = SaveState.SetCustomData(this, baseString);
		return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
	}
}
