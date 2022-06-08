using System.Collections.Generic;

namespace MicrosoftTeams.Tests
{
	public static class SqlParamFinder
	{
		public static Dictionary<string, string> GetParamsForInsert(string sqlText, string tableName)
		{
			int startIndex = sqlText.IndexOf(tableName) + tableName.Length;
			int endIndex = sqlText.IndexOf(')', startIndex);
			var ps = sqlText.Substring(startIndex + 1, endIndex - startIndex - 1);
			var psArray = ps.Split(',');

			int startVIndex = sqlText.IndexOf("VALUES") + 6;
			int endVIndex = sqlText.IndexOf(')', startVIndex);
			string psn = sqlText.Substring(startVIndex + 1, endVIndex - startVIndex - 1);
			var psnArray = psn.Split(',');


			Dictionary<string, string> myNames = new Dictionary<string, string>();
			for (int i = 0; i < psArray.Length; i++)
			{
				myNames.Add(psArray[i].Trim(), psnArray[i].Trim());
			}
			return myNames;
		}
		public static Dictionary<string, string> GetParamsForUpdate(string sqlText, string tableName)
		{
			int startIndex = sqlText.IndexOf(tableName) + tableName.Length + 6;
			int endIndex = sqlText.IndexOf("WHERE", startIndex);
			var ps = sqlText.Substring(startIndex, endIndex - startIndex - 1);
			ps = ps.Replace("\n\t", string.Empty);


			var pairs = ps.Split(',');
			Dictionary<string, string> myNames = new Dictionary<string, string>();

			foreach (var pair in pairs)
			{
				var segments = pair.Split('=');
				string colName = segments[0].Trim();
				string colValue = segments[1].Trim();
				myNames.Add(colName, colValue);
			}
			return myNames;
		}


		public static Dictionary<string, string> GetFiltersForUpdate(string sqlText, string tableName)
		{
			int startIndex = sqlText.IndexOf(tableName) + tableName.Length + 6;
			int endIndex = sqlText.IndexOf("WHERE", startIndex);
			var ps = sqlText.Substring(endIndex, sqlText.Length - endIndex);
			ps = ps.Replace("\n\t", string.Empty);
			ps = ps.Substring(5, ps.Length - 5);


			var segments = ps.Split('=');

			Dictionary<string, string> myNames = new Dictionary<string, string>();
			myNames.Add(segments[0].Trim(), segments[1].Trim());

			return myNames;
		}
	}
}
