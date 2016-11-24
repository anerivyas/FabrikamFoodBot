using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Collections.Generic;
using FabrikamFoodBot.DataModels;
using FabrikamFoodBot.DataModels;
namespace FabrikamFoodBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                StateClient stateClient = activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

                var userMessage = activity.Text;

                string endOutput = "Hello";



                bool isWeatherRequest = true;

                if (userMessage.ToLower().Contains("clear"))
                {
                    endOutput = "User data cleared";
                    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                    isWeatherRequest = false;
                }

                if (userMessage.ToLower().Equals("view reviews"))
                {
                    List<Timeline> timelines = await AzureManager.AzureManagerInstance.GetTimelines();
                    endOutput = "";
                    foreach (Timeline t in timelines)
                    {
                        endOutput += "[" + t.ID + "] : " + t.Firstname + " said, " + t.Comments + "\n\n";
                    }
                    isWeatherRequest = false;

                }

                if (activity.Text.Length > 16)
                {

                    if (activity.Text.ToLower().Substring(0, 10).Equals("add review"))
                    {

                        string subs = activity.Text.Substring(11);
                        string[] split = subs.Split(' ');
                        userData.SetProperty<string>("Id", split[0]);
                        userData.SetProperty<string>("firstName", split[1]);
                        userData.SetProperty<string>("Comments", split[2]);



                        Timeline Timeline = new Timeline()
                        {
                            ID = userData.GetProperty<string>("Id"),
                            Firstname = userData.GetProperty<string>("firstName"),
                            Comments = userData.GetProperty<string>("Comments")

                        };

                        await AzureManager.AzureManagerInstance.AddTimeline(Timeline);

                        isWeatherRequest = false;

                        endOutput = "New timeline added [" + Timeline.Firstname + "]";
                    }
                    isWeatherRequest = false;
                }

                if (userMessage.ToLower().Contains("update review"))
                {
                    var review = userMessage.Split(' ');
                    List<Timeline> timelines = await AzureManager.AzureManagerInstance.GetTimelines();
                    string firstname = activity.From.Name;
                    bool isUpdated = false;
                    foreach (Timeline t in timelines)
                    {

                        if (t.Firstname.Equals(firstname))
                        {
                            t.Firstname = "Bob";
                            await AzureManager.AzureManagerInstance.UpdateTimeline(t);
                            endOutput = "Your review has succesfully been added" + activity.From.Name;
                            isUpdated = true;
                            break;
                        }
                    }
                


                if (isUpdated == false)
                    {
                        isWeatherRequest = false;
                        endOutput = "Sorry we cannot update your review. Try again.";
                    }


                }

                if (!isWeatherRequest)
                    {
                        // return our reply to the user
                        Activity infoReply = activity.CreateReply(endOutput);

                        await connector.Conversations.ReplyToActivityAsync(infoReply);

                    }
                    else
                    {

                        Console.WriteLine("Error");

                    }
                }
                else
                {
                    HandleSystemMessage(activity);
                }
                var response = Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
     

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}