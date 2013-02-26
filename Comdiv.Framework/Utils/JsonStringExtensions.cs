using System.Collections.Generic;
using System.IO;
using Comdiv.Extensions;
using Newtonsoft.Json;
using JsonSerializer = Qorpent.Serialization.JsonSerializer;


namespace Comdiv.Framework.Utils
{
	public static class JsonStringExtensions
	{
		public static string toJSON(this object obj)
		{
			return new JsonSerializer().Serialize("", obj);
		}


		public static IDictionary<string, object> fromJSON(this string json)
		{
			var dict = new Dictionary<string, object>();
			if (json.hasContent())
			{
				var s = new Newtonsoft.Json.JsonSerializer();
				s.Populate(new JsonTextReader(new StringReader(json)), dict);
			}
			return dict;
		}
	}
}
