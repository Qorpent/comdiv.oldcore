#region LICENSE

// Copyright 2012-2013 Media Technology LTD 
// Original file : BxlXmlRoleProvider.cs
// Project: Comdiv.Core
// This code cannot be used without agreement from 
// Media Technology LTD 

#endregion

using Comdiv.IO;

namespace Comdiv.Security {
	/// <summary>
	/// 	Стандартная имплементация для чтения security.map.config
	/// </summary>
	public class BxlXmlRoleProvider : XmlRoleProvider<BxlApplicationXmlReader> {}
}