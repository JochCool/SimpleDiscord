using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDiscord
{
	public class RatelimitManager
	{
		// Contains all buckets, mapped by ID.
		Dictionary<string, RatelimitBucket> buckets = new();

		internal RatelimitManager() { }
	}
}
