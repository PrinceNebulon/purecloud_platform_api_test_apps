using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ININ.PureCloud.OAuthControl;
using ININ.PureCloudApi.Api;
using ININ.PureCloudApi.Client;
using ININ.PureCloudApi.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SDK_Test
{
    internal class Program
    {
        private static UserMe _me = null;

        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                #region Auth
                if (!GetSavedAuthToken())
                {
                    // Create form
                    var form = new OAuthWebBrowserForm();

                    // Set settings
                    form.oAuthWebBrowser1.ClientId = "babbc081-0761-4f16-8f56-071aa402ebcb";
                    form.oAuthWebBrowser1.RedirectUriIsFake = true;
                    form.oAuthWebBrowser1.RedirectUri = "http://localhost:8080";

                    // Show form
                    var result = form.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        SaveToken(form.oAuthWebBrowser1.AccessToken);
                        if (!GetSavedAuthToken())
                            throw new Exception("Failed to authorize!");
                    }
                    else
                        throw new Exception("Failed to authorize!");
                }
                #endregion

                #region TEST METHODS

                TestWebSockets();

                #endregion

                Console.WriteLine("\nDone. Press any key...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("**** ERROR ****");
                Console.WriteLine(ex);
                Console.ReadKey();
            }
        }

        #region Helper methods

        private static bool GetSavedAuthToken()
        {
            var token = Properties.Settings.Default.AccessToken;
            if (string.IsNullOrEmpty(token)) return false;

            try
            {
                Configuration.Default.AccessToken = token;
                var api = new UsersApi();
                _me = api.GetMe();
                Console.WriteLine($"Hello {_me.Name}");
                SaveToken(token);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void SaveToken(string token)
        {
            Properties.Settings.Default.AccessToken = token;
            Properties.Settings.Default.Save();
        }

        #endregion

        #region Test methods

        private static void TestWebSockets()
        {
            var handler = new NotificationHandler();
            handler.AddSubscription($"v2.users.{_me.Id}.presence", typeof(UserPresenceNotification));
            handler.AddSubscription($"v2.users.{_me.Id}.conversations", typeof(ConversationNotification));
            handler.NotificationReceived += (data) =>
            {
                Console.WriteLine(JsonConvert.SerializeObject(data, Formatting.Indented));

                if (data.GetType() == typeof (NotificationData<UserPresenceNotification>))
                {
                    var presence = (NotificationData<UserPresenceNotification>) data;
                    Console.WriteLine($"New presence: {presence.EventBody.PresenceDefinition.SystemPresence}");
                }
                else if (data.GetType() == typeof (NotificationData<ConversationNotification>))
                {
                    var conversation = (NotificationData<ConversationNotification>) data;
                    Console.WriteLine($"Conversation: {conversation.EventBody.Id}");
                }
            };

            Console.WriteLine("Websocket connected, awaiting messages...");
            Console.WriteLine("Press any key to remove conversations subscription.");
            Console.ReadKey(true);

            handler.RemoveSubscription($"v2.users.{_me.Id}.conversations");

            Console.WriteLine("Conversations subscription removed, awaiting messages...");
            Console.ReadKey(true);
        }

        private static NotificationData<T> DeserializeNotification<T>(string data)
        {
            var notification = JsonConvert.DeserializeObject<NotificationData<JObject>>(data);
            if (!notification.TopicName.Equals("", StringComparison.InvariantCultureIgnoreCase))
                return null;
            return JsonConvert.DeserializeObject<NotificationData<T>>(data);
        }

        private static void TestMakeCallback()
        {
            var d = DateTime.Parse("2009-06-15T13:45:30.123456-7");
            var f = "yyyy-MM-ddThh:mm:ss.FFFK";
            Console.WriteLine(d.ToString(f, CultureInfo.InvariantCulture));
            Console.WriteLine(d.ToUniversalTime().ToString(f, CultureInfo.InvariantCulture));
            return;
            var conversationsApi = new ConversationsApi();
            var callback = new CreateCallbackCommand(
                QueueId: "636f60d4-04d9-4715-9350-7125b9b553db",
                CallbackNumbers: new List<string> {"3179876873"},
                CallbackScheduledTime: DateTime.Now.AddMinutes(10));
            Console.WriteLine(callback.ToJson());
            var response = conversationsApi.PostCallbacks(callback);
            Console.WriteLine(response);
        }

        private static void TestMakeConversation()
        {
            var usersApi = new UsersApi();
            Console.WriteLine($"Effective Station ID {usersApi.GetUserIdStation(_me.Id).EffectiveStation.Id}");

            var conversationsApi = new ConversationsApi();
            var call = conversationsApi.PostCalls(new CreateCallRequest(PhoneNumber: "3172222222"));
            Console.WriteLine($"call: {call.ToJson()}");

            Thread.Sleep(7000);

            var calls = conversationsApi.GetCalls();
            Console.WriteLine($"calls: {calls.ToJson()}");
        }

        private static void TestCreateStation()
        {
            var suffix = "4";

            var telephonyApi = new TelephonyProvidersEdgeApi();

            // Get edges
            var edgeGroups = telephonyApi.GetProvidersEdgesEdgegroups();

            // Get phone base settings
            var phoneBaseSettings = telephonyApi.GetProvidersEdgesPhonebasesettings();

            // Get sites
            var sites = telephonyApi.GetProvidersEdgesSites();

            // Get line base settings
            var lineBaseSettings = telephonyApi.GetProvidersEdgesLinebasesettings();

            // Create request
            var phone = new Phone(
                Name: $"Test phone {suffix}",
                Site: new UriReference(sites.Entities[0].Id),
                PhoneBaseSettings: new UriReference(phoneBaseSettings.Entities[0].Id),
                Lines: new List<Line>
                {
                    new Line
                    {
                        Name = $"line{suffix}",
                        LineBaseSettings = new UriReference(lineBaseSettings.Entities[0].Id),
                        EdgeGroup = new UriReference(edgeGroups.Entities[0].Id),
                        Properties = new Dictionary<string, object>
                        {
                            {"station_label", new ValueString("line" + suffix)},
                            {"station_remote_address", new ValueString("3172222222")},
                            {"station_lineLabel", new ValueString("line" + suffix)},
                            {"station_lineKeyPosition", new ValueInt(1)}
                        }
                    }
                },
                Properties: new Dictionary<string, object>
                {
                    {"phone_hardwareId", new ValueString("0004f000000" + suffix)}
                });
            var request = phone.ToJson();
            /*
            phone.Name = $"Test phone {suffix}";
            phone.Site = new UriReference(sites.Entities[0].Id);
            phone.PhoneBaseSettings = new UriReference(phoneBaseSettings.Entities[0].Id);
            phone.Lines = new List<Line>
            {
                new Line
                {
                    Name = $"line{suffix}",
                    LineBaseSettings = new UriReference(lineBaseSettings.Entities[0].Id),
                    EdgeGroup = new UriReference(edgeGroups.Entities[0].Id),
                    Properties = new Dictionary<string, object>
                    {
                        {"station_label", "{\"value\": {\"instance\": \"line" + suffix + "\"}"},
                        {"station_remote_address", "{\"value\": {\"instance\": \"3172222222\"}"},
                        {"station_lineLabel", "{\"value\": {\"instance\": \"line" + suffix + "\"}"},
                        {"station_lineKeyPosition", "{\"value\": {\"instance\": 0}"}
                    }
                }
            };
            phone.Properties = new Dictionary<string, object>
            {
                {"phone_hardwareId", "{\"value\": {\"instance\": \"0004f000000" + suffix + "\"}"}
            };
            */

            // Create phone
            telephonyApi.PostProvidersEdgesPhones(phone);
        }

        private static void TestRoutingApi()
        {
            var routingApi = new RoutingApi();
            var queues = routingApi.GetQueues();
            var users = routingApi.GetQueuesQueueIdUsers(queues.Entities[0].Id);
            Console.WriteLine(users);
        }

        private static void TestOutboundApi()
        {
            var outboundApi = new OutboundApi();
            var listId = "f19465cf-5bc6-4871-b59f-5307575ddddf";
            var contacts = new List<DialerContact>();
            var contact = new DialerContact(null, listId, new Dictionary<string, object>
            {
                {"phone", "3175551212"},
                {"name", "A contact"},
                {"address", "123 Street Ave., Cityville, ST 00000"}
            }, null, true, null);
            contacts.Add(contact);

            var response = outboundApi.PostContactlistsContactlistIdContacts(listId, contacts, false);
            Console.WriteLine(response);
        }

        private static void TestUserSearch()
        {
            var body = new UserSearchRequest();
            body.PageSize = 2;
            body.Query = new List<UserSearchCriteria>();
            body.Query.Add((new UserSearchCriteria(Type: UserSearchCriteria.TypeEnum.RequiredFields)
            {
                Fields = new List<string> {"name"}
            }));

            var usersApi = new UsersApi();
            var result1 = usersApi.PostSearch(body);
            var query = result1.CurrentPage.Split('=').Last();

            while (true)
            {
                query = query.Replace("%3D", "=");
                var result = usersApi.GetSearch(query);
                if (result.Results != null)
                {
                    result.Results.ForEach(user => Console.WriteLine(user.Name));
                    var nextQuery = result.NextPage.Split('=').Last();
                    if (!string.Equals(query, nextQuery))
                    {
                        query = nextQuery;
                        continue;
                    }
                }
                Console.WriteLine("Processed all results");
                break;
            }
        }

        private static void TestOutOfOffice()
        {
            var usersApi = new UsersApi();
            var ooo = new OutOfOffice(Active: true, StartDate: DateTime.Now, EndDate: DateTime.Now.AddMonths(1));
            var response = usersApi.PutUserIdOutofoffice(_me.Id, ooo);
            Console.WriteLine(response);
        }

        private static void TestUserAggregatesQuery()
        {
            var usersApi = new UsersApi();
            var body = new AggregationQuery();
            body.Interval = DateTime.Today.AddDays(-7).ToUniversalTime().ToString("s", CultureInfo.InvariantCulture) +
                            "/" +
                            DateTime.Today.AddDays(1).ToUniversalTime().ToString("s", CultureInfo.InvariantCulture);
            body.Filter = new AnalyticsQueryFilter(Type: AnalyticsQueryFilter.TypeEnum.Or,
                Predicates:
                    new List<AnalyticsQueryPredicate>
                    {
                        new AnalyticsQueryPredicate(Dimension: AnalyticsQueryPredicate.DimensionEnum.Userid,
                            Value: _me.Id)
                    });
            body.GroupBy = new List<AggregationQuery.GroupByEnum> {AggregationQuery.GroupByEnum.Userid};

            var result = usersApi.PostUsersAggregatesQuery(body);
            Console.WriteLine(result);
        }

        #endregion
    }
}
