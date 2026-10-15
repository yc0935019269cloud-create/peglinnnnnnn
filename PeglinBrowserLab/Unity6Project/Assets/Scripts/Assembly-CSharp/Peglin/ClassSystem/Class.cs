using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Peglin.ClassSystem
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum Class
	{
		Peglin = 0,
		Balladin = 1,
		Roundrel = 2,
		Spinventor = 3
	}
}
