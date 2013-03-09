namespace Comdiv.Model.Interfaces {
	/// <summary>
	/// Интерфейс получения параметра по имени
	/// </summary>
	public interface IWithGetParameter {
		object GetParameter(string name);
	}
}