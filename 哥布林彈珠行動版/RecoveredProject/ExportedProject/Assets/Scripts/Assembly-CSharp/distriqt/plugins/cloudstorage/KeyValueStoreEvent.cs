using System;
using System.Collections.Generic;

namespace distriqt.plugins.cloudstorage
{
	[Serializable]
	public class KeyValueStoreEvent
	{
		public const string CHANGED = "keyvaluestore:changed";

		public const string OVER_QUOTA = "keyvaluestore:overquota";

		public const string ACCOUNT_CHANGED = "keyvaluestore:accountchanged";

		public const string INITIALSYNC = "keyvaluestore:initialsync";

		public List<string> changedKeys;

		public KeyValueStoreEvent()
		{
			changedKeys = new List<string>();
		}
	}
}
