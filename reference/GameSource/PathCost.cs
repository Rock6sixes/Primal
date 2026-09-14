using System;

public struct PathCost : IComparable<PathCost>, IEquatable<PathCost>
{
	public enum Legality
	{
		Allowed,
		Unwanted,
		IllegalConnection,
		IllegalTile,
		SolidTile,
		Unallowed
	}

	public Legality legality;

	public float resistance;

	public bool Allowed => legality <= Legality.Unwanted;

	public bool Considerable => legality != Legality.Unallowed;

	public PathCost(float resistance, Legality legality)
	{
		this.resistance = resistance;
		this.legality = legality;
	}

	public override bool Equals(object obj)
	{
		if (obj != null && obj is PathCost)
		{
			return Equals((PathCost)obj);
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return (845132026 * -1521134295 + legality.GetHashCode()) * -1521134295 + resistance.GetHashCode();
	}

	public bool Equals(PathCost pathCost)
	{
		if (legality == pathCost.legality)
		{
			return resistance == pathCost.resistance;
		}
		return false;
	}

	public static PathCost operator +(PathCost a, PathCost b)
	{
		return new PathCost(a.resistance + b.resistance, (Legality)Math.Max((int)a.legality, (int)b.legality));
	}

	public static PathCost operator *(PathCost a, float b)
	{
		return new PathCost(a.resistance * b, a.legality);
	}

	public static bool operator ==(PathCost a, PathCost b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(PathCost a, PathCost b)
	{
		return !a.Equals(b);
	}

	public static bool operator >(PathCost a, PathCost b)
	{
		if (a.legality == b.legality)
		{
			return a.resistance > b.resistance;
		}
		return a.legality > b.legality;
	}

	public static bool operator >=(PathCost a, PathCost b)
	{
		if (a.legality == b.legality)
		{
			return a.resistance >= b.resistance;
		}
		return a.legality > b.legality;
	}

	public static bool operator <(PathCost a, PathCost b)
	{
		if (a.legality == b.legality)
		{
			return a.resistance < b.resistance;
		}
		return a.legality < b.legality;
	}

	public static bool operator <=(PathCost a, PathCost b)
	{
		if (a.legality == b.legality)
		{
			return a.resistance <= b.resistance;
		}
		return a.legality < b.legality;
	}

	public override string ToString()
	{
		return "pc{" + resistance + ", " + legality.ToString() + "}";
	}

	public int CompareTo(PathCost other)
	{
		if (this < other)
		{
			return -1;
		}
		if (this > other)
		{
			return 1;
		}
		return 0;
	}
}
