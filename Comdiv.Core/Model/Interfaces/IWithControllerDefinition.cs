namespace Comdiv.Model.Interfaces {
	/// <summary>
	/// Опредение контроллера
	/// </summary>
	public interface IWithControllerDefinition{
		string Action { get; set; }
		string Controller { get; set; }
		string Area { get; set; }
	}
}