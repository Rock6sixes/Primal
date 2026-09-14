namespace Menu;

public class ScoreCalculatorPhase : ExtEnum<ScoreCalculatorPhase>
{
	public static ScoreCalculatorPhase Setup = new ScoreCalculatorPhase("Setup", register: true);

	public static ScoreCalculatorPhase Challenges = new ScoreCalculatorPhase("Challenges", register: true);

	public static ScoreCalculatorPhase Multipliers = new ScoreCalculatorPhase("Multipliers", register: true);

	public static ScoreCalculatorPhase Burdens = new ScoreCalculatorPhase("Burdens", register: true);

	public static ScoreCalculatorPhase Finalise = new ScoreCalculatorPhase("Finalise", register: true);

	public static ScoreCalculatorPhase Done = new ScoreCalculatorPhase("Done", register: true);

	public ScoreCalculatorPhase(string value, bool register = false)
		: base(value, register)
	{
	}
}
