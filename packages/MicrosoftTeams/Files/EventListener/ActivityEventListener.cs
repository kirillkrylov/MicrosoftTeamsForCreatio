using Common.Logging;
using Microsoft.Graph;
using MicrosoftTeams.DataOperations;
using MicrosoftTeams.Interfaces;
using MicrosoftTeams.MsGraph;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Terrasoft.Core;
using Terrasoft.Core.Entities;
using Terrasoft.Core.Entities.Events;
using Terrasoft.Core.Factories;
using Ts = Terrasoft.Core.Entities;

namespace MicrosoftTeams.EventListener
{
	/// <summary>
	/// Listener for 'EntityName' entity events.
	/// </summary>
	/// <seealso cref="Terrasoft.Core.Entities.Events.BaseEntityEventListener" />
	[EntityEventListener(SchemaName = "Activity")]
	internal class ActivityEventListener : BaseEntityEventListener
	{
		
		#region Methods : Public : OnInsert
		public override void OnInserted(object sender, EntityAfterEventArgs e)
		{
			base.OnInserted(sender, e);
			Ts.Entity entity = (Ts.Entity)sender;
			UserConnection userConnection = entity.UserConnection;

			//if (!IsChangeInteresting(e.ModifiedColumnValues)) return;

			ILog logger = LogManager.GetLogger("MsTeamsConnector");
			IApplication application = ClassFactory.Get<IBuilder<IApplication>>()
				.ConfigureUserConnection(userConnection)
				.ConfigureLogger(logger)
				.Build();

			var @event = GetEvent(entity, application);
			Event result = default;
			Task.Run(async () =>
			{
				result = await AddTeamsEvent(@event, application);
			}).Wait();

			entity.FetchFromDB(entity.PrimaryColumnValue);
			entity.SetColumnValue("TeamsMeetingId", result.Id);
			entity.SetColumnValue("TeamsJoinUrl", result.OnlineMeeting.JoinUrl);
			
			string NoteHtml = $"<div><a href='{result.OnlineMeeting.JoinUrl}' style='color: #0000EE' title='{result.OnlineMeeting.JoinUrl}'><span>Join Teams Meeting</span></a></div>";
			entity.SetColumnValue("Notes", $"{NoteHtml}");
			entity.Save();
		}
		#endregion

		#region Methods : Public : OnInsert
		public override void OnUpdated(object sender, EntityAfterEventArgs e)
		{
			base.OnUpdated(sender, e);
			Ts.Entity entity = (Ts.Entity)sender;
			UserConnection userConnection = entity.UserConnection;

			string meetingId = entity.GetTypedColumnValue<string>("TeamsMeetingId");
			if (string.IsNullOrEmpty(meetingId)) return;

			if (!IsChangeInteresting(e.ModifiedColumnValues)) return;

			ILog logger = LogManager.GetLogger("MsTeamsConnector");
			IApplication application = ClassFactory.Get<IBuilder<IApplication>>()
				.ConfigureUserConnection(userConnection)
				.ConfigureLogger(logger)
				.Build();

			var @event = GetEvent(entity, application);
			Event result = default;
			Task.Run(async () =>
			{
				result = await UpdateEvent(meetingId,@event, application);
			}).Wait();


			entity.SetColumnValue("TeamsMeetingId", result.Id);
			entity.SetColumnValue("TeamsJoinUrl", result.OnlineMeeting.JoinUrl);

			string NoteHtml = $"<div><a href='{result.OnlineMeeting.JoinUrl}' style='color: #0000EE' title='{result.OnlineMeeting.JoinUrl}'><span>Join Teams Meeting</span></a></div>";
			entity.SetColumnValue("Notes", $"{NoteHtml}");
			entity.Save();
		}
		#endregion


		#region Methods : Public : OnDelete
		public override void OnDeleting(object sender, EntityBeforeEventArgs e)
		{
			base.OnDeleting(sender, e);
			var entity = (Ts.Entity)sender;
			string meetingId = entity.GetTypedColumnValue<string>("TeamsMeetingId");

			if (string.IsNullOrEmpty(meetingId)) return;

			ILog logger = LogManager.GetLogger("MsTeamsConnector");
			IApplication application = ClassFactory.Get<IBuilder<IApplication>>()
				.ConfigureUserConnection(entity.UserConnection)
				.ConfigureLogger(logger)
				.Build();

			Task.Run(async () =>
			{
				await DeleteEvent(meetingId, application);
			}).Wait();
		}
		#endregion


		internal bool IsChangeInteresting(EntityColumnValueCollection modifiedColumns)
		{
			string[] columnsToObserve = new string[] { "StartDate", "DueDate", "TimeZoneId", "Title" };
			
			bool isInteresting = false;

			foreach(var column in modifiedColumns)
			{
				int pos = Array.IndexOf(columnsToObserve, column.Name);
				if (pos != -1)
				{
					isInteresting = true;
				}
			}
			return isInteresting;
		}
		

		internal async Task<Event> AddTeamsEvent(Event @event, IApplication application)
		{
			var gclient = application.GetService<IGClient>();
			var client = await gclient.GetGraphServiceClient();

			try
			{
				return await client.Me.Events.Request().AddAsync(@event).ConfigureAwait(false);			
			}
			catch (Exception ex)
			{
				ILog logger = LogManager.GetLogger("MsTeamsConnector");
				logger.ErrorFormat("Could not AddTeamsEvent {0}\n{1}", ex.Message, ex.StackTrace);
				throw;
			}
		}

		internal async Task DeleteEvent(string meetingId, IApplication application)
		{
			var gclient = application.GetService<IGClient>();
			var client = await gclient.GetGraphServiceClient();

			try
			{
				await client.Me.Events[meetingId].Request().DeleteAsync().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				ILog logger = LogManager.GetLogger("MsTeamsConnector");
				logger.ErrorFormat("Could not AddTeamsEvent {0}\n{1}", ex.Message, ex.StackTrace);
				throw;
			}
		}

		internal async Task<Event> UpdateEvent(string meetingId, Event @event, IApplication application)
		{
			var gclient = application.GetService<IGClient>();
			var client = await gclient.GetGraphServiceClient();

			try
			{
				return await client.Me.Events[meetingId].Request().UpdateAsync(@event).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				ILog logger = LogManager.GetLogger("MsTeamsConnector");
				logger.ErrorFormat("Could not UpdateTeamsEvent {0}\n{1}", ex.Message, ex.StackTrace);
				throw;
			}
		}

		internal Event GetEvent(Ts.Entity entity, IApplication application)
		{
			TimeZoneInfo userTimeZoneInfo = entity.UserConnection.CurrentUser.TimeZone;
			DateTime startTime = TimeZoneInfo.ConvertTimeToUtc(entity.GetTypedColumnValue<DateTime>("StartDate"), userTimeZoneInfo);
			DateTime endTime = TimeZoneInfo.ConvertTimeToUtc(entity.GetTypedColumnValue<DateTime>("DueDate"), userTimeZoneInfo);

			Guid tz = entity.GetTypedColumnValue<Guid>("TimeZoneId");

			TimeZoneInfo tzInfo = userTimeZoneInfo;

			var misc = application.GetService<IMisc>();
			if (tz != Guid.Empty)
			{
				string tzCode = misc.GetLookupValue<string>("TimeZone", "Id", tz, "Code");
				tzInfo = TimeZoneInfo.FindSystemTimeZoneById(tzCode);
			}

			var tzStart = TimeZoneInfo.ConvertTimeFromUtc(startTime, tzInfo);
			var tzEnd = TimeZoneInfo.ConvertTimeFromUtc(endTime, tzInfo);
			string title = entity.GetTypedColumnValue<string>("Title");

			Event @event = new Event
			{
				Subject = title,
				Body = new ItemBody
				{
					ContentType = BodyType.Html,
					Content = "Does this time work for you?"
				},
				Start = new DateTimeTimeZone
				{

					DateTime = tzStart.ToString("s", DateTimeFormatInfo.InvariantInfo),
					TimeZone = tzInfo.Id
				},
				End = new DateTimeTimeZone
				{
					DateTime = tzEnd.ToString("s", DateTimeFormatInfo.InvariantInfo),
					TimeZone = tzInfo.Id
				},
				Location = new Location
				{
					DisplayName = "Teams Meeting"
				},
				IsOnlineMeeting = true,
				OnlineMeetingProvider = OnlineMeetingProviderType.TeamsForBusiness
			};
			return @event;
		}
	}
}
