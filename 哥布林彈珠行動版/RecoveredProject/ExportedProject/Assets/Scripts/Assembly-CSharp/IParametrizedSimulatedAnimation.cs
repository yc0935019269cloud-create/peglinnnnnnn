public interface IParametrizedSimulatedAnimation
{
	void SetParameter(IParametrizedSimulatedAnimation parent);

	float GetParameter();

	string GetObjectName();
}
