using MicrosoftTeams.DataOperations;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Common;
using Terrasoft.Configuration.Tests;
using Terrasoft.Core.DB;

namespace MicrosoftTeams.Tests
{
	[TestFixture]
	[MockSettings(RequireMock.All)]
	internal class MiscTests : BaseConfigurationTestFixture
	{

		private Misc sut;

		protected override void SetUp()
		{
			base.SetUp();
			UserConnection.DBEngine = Substitute.ForPartsOf<DBEngine>();
			sut = new Misc(UserConnection);

			EntitySchemaManager.AddCustomizedEntitySchema("OAuthApplications",
				new Dictionary<string, string>() {
					{ "Name", "Text" },
				}
			);
			SetUpTestData("OAuthApplications", query => query.Has(Consts.ApplicationId), new Dictionary<string, object>() {
				{ "Id", Consts.ApplicationId },
				{ "Name", "Microsoft Graph"}
			});
		}


		[Test]
		public void GetLookupValue_ShouldReturn()
		{
			var result = sut.GetLookupValue<string>("OAuthApplications", "Id", Consts.ApplicationId, "Name");
			Assert.That(result, Is.EqualTo("Microsoft Graph"));
		}

		protected void SetUpTestData(string schemaName, Action<SelectData> filterAction, params Dictionary<string, object>[] items)
		{
			var selectData = new SelectData(UserConnection, schemaName);
			items.ForEach(values => selectData.AddRow(values));
			filterAction.Invoke(selectData);
			selectData.MockUp();
		}
	}
}
