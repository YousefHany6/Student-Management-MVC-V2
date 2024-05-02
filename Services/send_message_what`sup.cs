using RestSharp;
using System.Configuration;
using System.Reflection;

namespace FStudentManagement.Services
{
	public   class send_message_what_sup
	{
		public static async Task Send_Message(string phone_number, string name, DateTime date, string course)
		{

			string instid = "instance78446";
			string token = "civab9iko3ksfd01";
			string phone = phone_number;
			string message = $"الطالب {name} قد تغيب يوم {date} عن مادة {course}";
			var url = "https://api.ultramsg.com/" + instid + "/messages/chat";

			var client = new RestClient(url);
			var request = new RestRequest(url, Method.Post);
			request.AddHeader("Absence","application/x-www-form-urlencoded");
			request.AddParameter("token", token);
			request.AddParameter("to", phone);
			request.AddParameter("body", message);


			RestResponse response = await client.ExecuteAsync(request);

		}
	}
}
