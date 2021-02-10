using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace API_Tests_GitHub
{
    public class Tests
    {
        const string GitHubAPIUsername = "testnakov";
        const string GitHubAPIPass = "put_your_api_token_key_here";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test_GitHubAPI_GetIssuesByRepo()
        {
            var client = new RestClient("https://api.github.com/repos/testnakov/test-nakov-repo/issues");
            client.Timeout = 3000;
            var request = new RestRequest(Method.GET);
            var response = client.Execute(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            Assert.IsTrue(response.ContentType.StartsWith("application/json"));

            var issues = new JsonDeserializer().Deserialize<List<IssueResponse>>(response);

            Assert.Pass();
        }

        [Test]
        public void Test_GitHubAPI_CreateNewIssue()
        {
            var client = new RestClient("https://api.github.com/repos/testnakov/test-nakov-repo/issues");
            client.Timeout = 3000;
            var request = new RestRequest(Method.POST);
            client.Authenticator = new HttpBasicAuthenticator(GitHubAPIUsername, GitHubAPIPass);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(new { 
                title = "some title",
                body = "some body",
                labels = new string[] { "bug", "importance:high", "type:UI" }
            });
            var response = client.Execute(request);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            Assert.IsTrue(response.ContentType.StartsWith("application/json"));

            var issue = new JsonDeserializer().Deserialize<IssueResponse>(response);

            Assert.IsTrue(issue.id > 0);
            Assert.IsTrue(issue.number > 0);
            Assert.IsTrue(!String.IsNullOrEmpty(issue.title));
            Assert.IsTrue(!String.IsNullOrEmpty(issue.body));
        }

        [Test]
        public void Test_GitHubAPI_CreateNewIssue_Unauthorized()
        {
            var client = new RestClient("https://api.github.com/repos/testnakov/test-nakov-repo/issues");
            client.Timeout = 3000;
            var request = new RestRequest(Method.POST);
            //client.Authenticator = new HttpBasicAuthenticator(GitHubAPIUsername, GitHubAPIPass);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(new
            {
                title = "some title",
                body = "some body",
                labels = new string[] { "bug", "importance:high", "type:UI" }
            });
            var response = client.Execute(request);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public void Test_GitHubAPI_DeleteComment()
        {
            // Create a new comment for issue #6
            var client = new RestClient("https://api.github.com/repos/testnakov/test-nakov-repo/issues/6/comments");
            client.Timeout = 3000;
            var request = new RestRequest(Method.POST);
            client.Authenticator = new HttpBasicAuthenticator(GitHubAPIUsername, GitHubAPIPass);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(new
            {
                body = "comment body"
            });
            var response = client.Execute(request);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var newComment = new JsonDeserializer().Deserialize<CommentResponse>(response);

            // Delete the newly created comment
            var clientDel = new RestClient("https://api.github.com/repos/testnakov/test-nakov-repo/issues/comments/" + newComment.id);
            clientDel.Timeout = 3000;
            var delRequest = new RestRequest(Method.DELETE);
            clientDel.Authenticator = new HttpBasicAuthenticator(GitHubAPIUsername, GitHubAPIPass);
            var delResponse = clientDel.Execute(delRequest);

            Assert.AreEqual(HttpStatusCode.NoContent, delResponse.StatusCode);
        }

        [Test]
        public void Test_GitHubAPI_CreateNewIssue_MissingTitle()
        {
            var client = new RestClient("https://api.github.com/repos/testnakov/test-nakov-repo/issues");
            client.Timeout = 3000;
            var request = new RestRequest(Method.POST);
            client.Authenticator = new HttpBasicAuthenticator(GitHubAPIUsername, GitHubAPIPass);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(new
            {
                body = "some body",
                labels = new string[] { "bug", "importance:high", "type:UI" }
            });
            var response = client.Execute(request);

            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }
    }
}

namespace API_Tests_GitHub
{
    public class IssueResponse
    {
        public IssueResponse() { }
        public long id { get; set; }
        public long number { get; set; }
        public string title { get; set; }
        public string body { get; set; }
    }
}

namespace API_Tests_GitHub
{
    internal class CommentResponse
    {
        public long id { get; set; }
        public string body { get; set; }
    }
}
