namespace Comdiv.Persistence {
	/// <summary>
	/// Called by HQL controllers in save entity phase - can apply additional validation before save
	/// </summary>
	public interface IDatabaseVerificator {
		/// <summary>
		/// Validates state of object before save
		/// </summary>
		void VerifySaving();
	}
}