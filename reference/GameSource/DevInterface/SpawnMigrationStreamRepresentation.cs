using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace DevInterface;

public class SpawnMigrationStreamRepresentation : SplineObjectRepresentation, IDevUISignals
{
	public Button typeButton;

	public SignalSlider widthSlider;

	public SignalSlider capacitySlider;

	public SignalSlider rateSlider;

	public new PlacedObject.SpawnMigrationStreamData Data => pObj.data as PlacedObject.SpawnMigrationStreamData;

	public SpawnMigrationStreamRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		bool flag = false;
		foreach (UpdatableAndDeletable update in owner.room.updateList)
		{
			if (update is VoidSpawnMigrationStream voidSpawnMigrationStream && voidSpawnMigrationStream.data == Data)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			owner.room.AddObject(new VoidSpawnMigrationStream(owner.room, Data));
		}
		Panel panel = new Panel(owner, IDstring, this, Vector2.zero, new Vector2(200f, 128f), "SpawnType");
		subNodes.Add(panel);
		Vector2 vector = new Vector2(5f, 5f);
		typeButton = new Button(owner, IDstring + "_button", panel, vector, 80f, Data.type.value);
		vector.y += 30f;
		widthSlider = new SignalSlider(owner, IDstring, panel, vector, "width", inheritButton: false, 50f, 0f)
		{
			NumberText = Data.width.ToString()
		};
		widthSlider.RefreshNubPos(Mathf.InverseLerp(1f, 200f, Data.width));
		vector.y += 30f;
		capacitySlider = new SignalSlider(owner, IDstring, panel, vector, "maxAmount", inheritButton: false, 50f, 0f)
		{
			NumberText = Data.maxCapacity.ToString()
		};
		capacitySlider.RefreshNubPos(Mathf.InverseLerp(1f, 400f, Data.maxCapacity));
		vector.y += 30f;
		rateSlider = new SignalSlider(owner, IDstring, panel, vector, "rate", inheritButton: false, 50f, 0f)
		{
			NumberText = Data.rate.ToString()
		};
		rateSlider.RefreshNubPos(Mathf.InverseLerp(5f, 400f, Data.rate));
		panel.subNodes.AddRange(new List<DevUINode> { typeButton, widthSlider, capacitySlider, rateSlider });
	}

	public void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		if (sender == typeButton)
		{
			string[] array = ExtEnumBase.GetExtEnumType(typeof(VoidSpawn.SpawnType)).entries.ToArray();
			int num = array.IndexOf(Data.type.value);
			num++;
			if (num >= array.Length)
			{
				num = 0;
			}
			Data.type = new VoidSpawn.SpawnType(array[num]);
			typeButton.Text = Data.type.value;
		}
		else if (sender == widthSlider)
		{
			float width = Mathf.Lerp(1f, 200f, float.Parse(message, NumberStyles.Any, CultureInfo.InvariantCulture));
			Data.width = width;
			widthSlider.NumberText = width.ToString();
		}
		else if (sender == capacitySlider)
		{
			int maxCapacity = (int)Mathf.Lerp(1f, 400f, float.Parse(message, NumberStyles.Any, CultureInfo.InvariantCulture));
			Data.maxCapacity = maxCapacity;
			capacitySlider.NumberText = maxCapacity.ToString();
		}
		else if (sender == rateSlider)
		{
			int rate = (int)Mathf.Lerp(5f, 400f, float.Parse(message, NumberStyles.Any, CultureInfo.InvariantCulture));
			Data.rate = rate;
			rateSlider.NumberText = rate.ToString();
		}
	}
}
