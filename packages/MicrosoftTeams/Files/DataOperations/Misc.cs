using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;
using Terrasoft.Core.Entities;

namespace MicrosoftTeams.DataOperations
{
	internal interface IMisc
	{
		T GetLookupValue<T>(string lookupName, string keyColumn, object keyValue, string columnToFetch);
	}

	internal class Misc : IMisc
	{
		private readonly UserConnection _userConnection;

		public Misc(UserConnection userConnection)
		{
			_userConnection = userConnection;
		}

		public T GetLookupValue<T>(string lookupName, string keyColumn, object keyValue, string columnToFetch)
		{
			Entity lookup = _userConnection.EntitySchemaManager.GetInstanceByName(lookupName).CreateEntity(_userConnection);
			lookup.FetchFromDB(keyColumn, keyValue, new string[] { columnToFetch });
			return lookup.GetTypedColumnValue<T>(columnToFetch);
		}
	}
}
