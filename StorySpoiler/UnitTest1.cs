using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using StorySpoiler.Models;
using System.Net;
using System.Text.Json;


namespace StorySpoiler
{
    [TestFixture]
    public class StorySpoilerTest
    {
        private RestClient client;
        private static string storySpoilerId;
        //your link here
        private const string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";
        private const string username = "Testuser12345";
        private const string password = "Testuser!234";


        [OneTimeSetUp]
        public void Setup()
        {
            // your credentials
            string token = GetJwtToken("string", "string");

            var options = new RestClientOptions(baseUrl)
            {
               Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);

            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;

        }

        [Test, Order(1)]
        public void CreateNewStory_WithRequiredFields_ShouldSucceed_And_ReturnCreated() 
        {
           var storySpoiler = new StoryDTO
           {
               Title = "New StorySpoiler",
               Description = "Description of the StorySpoiler",
               Url = ""
           };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(storySpoiler);
          
            var response = client.Execute(request);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.Content, Does.Contain("Successfully created!"));
            Assert.That(String.IsNullOrEmpty(response.Content), Is.False);

            var responseDto = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(responseDto.StoryId, Is.Not.Null);
            storySpoilerId = responseDto.StoryId;        
        }

        [Test, Order(2)]

        public void EditCreatedStory_ShouldSucceed()
        {
            var editedStory = new StoryDTO()
            {
                Title = "Edited StorySpoiler",
                Description = "Edited description of the StorySpoiler",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{storySpoilerId}", Method.Put);
            request.AddJsonBody(editedStory);

            var response = client.Execute(request);
            var json = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(json.Msg, Is.EqualTo("Successfully edited"));
        }

        [Test, Order(3)]

        public void GetAllStories_ShouldReturnAllStories()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = client.Execute(request);

            var json = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));            
            Assert.That(response.Content, Is.Not.Null);
        }

        [Test, Order(4)]

        public void DeleteCreatedStory_ShouldSucceed()
        {
           var request = new RestRequest($"/api/Story/Delete/{storySpoilerId}", Method.Delete);
           var response = client.Execute(request);

           //Assert
           Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
           Assert.That(response.Content, Does.Contain("Deleted successfully!"));
        }

        [Test, Order(5)]

        public void CreateStory_WithoutRequiredFields_ShouldFail()
        {
            var storySpoiler = new StoryDTO
            {
                Title = "",
                Description = "",
               
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(storySpoiler);

            var response = client.Execute(request);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }


        [Test, Order(6)]

        public void EditNonExistingStory_ShouldFail()
        {
            string nonExistingStoryId = "1234";
            var editedStory = new StoryDTO()
            {
                Title = "Edit non-existing StorySpoiler",
                Description = "Edited description of the non-existing StorySpoiler",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{nonExistingStoryId}", Method.Put);
            request.AddQueryParameter("id", nonExistingStoryId);
            request.AddJsonBody(editedStory);

            var response = client.Execute(request);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No spoilers..."));                        
        }

        [Test, Order(7)]

        public void DeleteNonExistingStory_ShouldFail()
        {
          string nonExistingStoryId = "1234";

          var request = new RestRequest($"/api/Story/Delete/{nonExistingStoryId}", Method.Delete);
          var response = client.Execute(request);

          //Assert
          Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
          Assert.That(response.Content, Does.Contain("Unable to delete this story spoiler!"));
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}