using UnityEngine;

namespace RWCustom;

public class Counter
{
	public int counter;

	public bool countsUp;

	public bool needReset;

	public int max { get; set; }

	public int min { get; set; }

	public float normalized => (float)(counter - min) / (float)(max - min);

	public bool isFinished
	{
		get
		{
			if (!countsUp)
			{
				return counter <= min;
			}
			return counter >= max;
		}
	}

	public Counter(int max, int min = 0, bool countsUp = false)
	{
		this.max = max;
		this.min = min;
		this.countsUp = countsUp;
		Reset();
	}

	public void Tick()
	{
		if (needReset)
		{
			Reset();
			needReset = false;
		}
		else if (countsUp)
		{
			if (counter < max)
			{
				counter++;
			}
			else if (needReset)
			{
				counter = min;
				needReset = false;
			}
		}
		else if (counter > min)
		{
			counter--;
		}
		else if (needReset)
		{
			counter = max;
			needReset = false;
		}
	}

	public void Add(int amount)
	{
		counter = Mathf.Clamp(counter + amount, min, max);
	}

	public void Reset()
	{
		counter = (countsUp ? min : max);
	}

	public void Finish()
	{
		counter = (countsUp ? max : min);
	}

	public void ResetIfFinished()
	{
		needReset = isFinished;
	}

	public void ResetOnNextTick()
	{
		needReset = true;
	}

	public void Set(int time)
	{
		counter = time;
	}

	public void SetClamped(int time)
	{
		counter = Mathf.Clamp(time, min, max);
	}

	public void SetToMax()
	{
		counter = max;
	}

	public void SetToMin()
	{
		counter = min;
	}

	public void SetMax(int max)
	{
		this.max = Mathf.Max(max, min + 1);
	}

	public void SetMin(int min)
	{
		this.min = Mathf.Min(min, max - 1);
	}

	public static implicit operator int(Counter input)
	{
		return input.counter;
	}

	public override string ToString()
	{
		return counter.ToString();
	}
}
