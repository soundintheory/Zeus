using Zeus.Web;

namespace Zeus.Engine
{
	/// <summary>
	/// Base interface for user overridable controllers of various aspects. 
	/// </summary>
	public interface IContentAdapter
	{
		/// <summary>The path associated with this controller instance.</summary>
		PathData Path { get; set; }
	}
}