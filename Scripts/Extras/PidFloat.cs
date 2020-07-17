[System.Serializable]
public class PidFloat
{
	public float pGain = 1f;
	public float iGain = 1f;
	public float dGain = 1f;

	private float prevError;
	private float P, I, D;

	public float Update(float currentError, float timeFrame)
	{
		if (timeFrame < float.Epsilon)
			return prevError;

		P = currentError;
		I += P * timeFrame;
		D = (P - prevError) / timeFrame;
		prevError = currentError;

		return P * pGain + I * iGain + D * dGain;
	}
}
// lore: https://forum.unity3d.com/threads/rigidbody-lookat-torque.146625/

