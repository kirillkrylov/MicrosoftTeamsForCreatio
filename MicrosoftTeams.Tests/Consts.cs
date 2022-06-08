using MicrosoftTeams.Models.OAuthApplicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicrosoftTeams.Dto;

namespace MicrosoftTeams.Tests
{
	internal static class Consts
	{
		public static string BaseUrl => "http://localhost:8050/0";
		public static string ExpectedLoginUrl => "https://login.microsoftonline.com/7b5ca2fb-a643-4385-8b39-36934270c716/oauth2/v2.0/authorize?client_id=a0275e0d-374d-4f32-aa90-84c713df11a6&response_type=code&redirect_uri=http://localhost:8050/0/rest/OAuthHandlerService/OAuthCallBack&scope=Calendars.ReadWrite OnlineMeetingArtifact.Read.All OnlineMeetings.ReadWrite offline_access User.Read";
		public static string ClientId => "a0275e0d-374d-4f32-aa90-84c713df11a6";
		public static string SecretKey = "6Tin7aIuztNVrWsQx6pIy9nYh13IR45vkx9rqGtWZ3Bybgz81BJ+0dkedMsmCuOqAX4hryNfY8kBnuBE8OWqeLI5LqXf3Vj3htaPip3ZiuWqhcC8zZCgwZRv6F+TPWAEmff0II6Zh928VzxEGYufFLAH5sMreddBcLnYyk/aSwB3jxV93fLQme3sXmfeNNnX";

		public static string Tenantid = "7b5ca2fb-a643-4385-8b39-36934270c716";

		#region Params after successful Oauth
		public static string Code => "0.ATsA-6Jce0OmhUOLOTaTQnDHFg1eJ6BNNzJPqpCExxPfEaY7AO4.AgABAAIAAAD--DLA3VO7QrddgJg7WevrAgDs_wQA9P_DhRL279g6_7AwExKATm6Zz8uTBaV0I854mVr9mVVdGpJwzI2VbtX5247gdJGx27zcphqf2TL1osG0R7RGkxTHSzh4-lft24QnniHly6_Gyyw7smW1a-DGo3NVY4D7eJU4ATgyXx4Je2n6oq3l-Gv1QzGBuypOUFOGBDCpmLA8M4099mg7Vpx3Imr0w-B4NGZr8IttJV9Sx7BILkepNaDJu--SE9q5Ps6s4npbahlD3umiEhrVEFrIoqACp5nCG4eAcLk_taQfbF0Y_rSR1mZFHIC06o46lizAaafqO2p7zyaXJBmsHeL2QZdo9gApO0BSdE0baLojxPSUX30tNJCSC4LSMtF0CqRsLyOzVWkKnxuDV0g1D4kSSjjCH2jyy80L_QR0YR07YvPUEHgAGLdqoJkMVyfEbALRUDFamtQWfzEBHCF_ee-0FDTFYh5ESlnyawOqFiX3z1g02woOtFPUmbbVcsoOPAyfzKuXgPbnK4A4jlJ8F1EbRPto7-bQhJTVUdQe1vk5P-0KPmp2uokkeLo6kHnyZMNE9enR0YkKQnwEF4LDG2sBrkTS30WkDNCKjxNwMcD9EWM3PGNlAbn6X75mZ2DWGzfYEeCfRiUCQQdXY4DMIb_NNdPuhz-JtD1YVMz26BTT1GFkOW_iHOcLXj0isWm6pEC1nYhxEQnIOI-xPXBiz5k_P0bvDh6cgjc_x4pHLomDQhgopFnX6Qa6vLGF1S99yNo6Pei_VJG07hE7U5-HDKtkJOoUF9m9OLIpaxljW02sumLFieHzkaJqwxS7gyEUj7hgs5S30-kazXnVjQRm0-DvSvZMrpt93r2zWeLIQ6u35VGQ2L2a1M69XZSOzg623Q2H8Il-sDaLXYc3eg_aaQvcvOv-b22fWtdOnJVSPuEfvbb4qm8oe_Cs-ISBSaRaA4pb_1Y22hj5pImmdHcp4Uws3_uad0bKxNQVMriI9ZeD3VNblB8QxaPJaEaQrgDiNg-yiK1G";
		public static string SessionState => "2685ed45-a767-4fc0-9517-e760752ad4d0";
		#endregion

		public static Guid ApplicationId => Guid.Parse("f7f273d2-680a-45d1-b6e5-fbe2e55698aa");

		public static IOAuthApplicationModel ApplicationSettings => new OAuthApplicationModel
		{
			AuthorizeUrl = new Uri("https://login.microsoftonline.com"),
			LogoutUrl = new Uri("https://login.microsoftonline.com"),
			RevokeTokenUrl = new Uri("https://login.microsoftonline.com"),
			TokenUrl = new Uri("https://login.microsoftonline.com"),
			ClientId = ClientId,
			ClientSecret = SecretKey,
			RedirectUrl = $"{BaseUrl}/rest/OAuthHandlerService/OAuthCallBack",
			Scopes = new List<string>
			{
				{"Calendars.ReadWrite"},
				{"offline_access"},
				{"OnlineMeetingArtifact.Read.All"},
				{"OnlineMeetings.ReadWrite"},
				{"User.Read"}
			}
		};


		public static Dto.Token Token => new Dto.Token
		{
			TokenType = "Bearer",
			Scope = "",
			ExpiresIn = 3600,
			AccessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ik5HVEZ2ZEstZnl0aEV1Q...",
			RefreshToken = "AwABAAAAvPM1KaPlrEqdFSBzjqfTGAMxZGUTdM0t4B4..."
		};
	}
}
