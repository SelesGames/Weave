﻿using Common.Microsoft.OneNote.Response;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Common.Microsoft.OneNote
{
    public class OneNoteServiceClient
    {
        // v1.0 Endpoints        
        const string PAGESENDPOINT = "https://www.onenote.com/api/v1.0/pages";
        
        const string PRESENTATION = "Presentation";
        string accessToken;

        public OneNoteServiceClient(string accessToken)
        {
            this.accessToken = accessToken;
        }

        public async Task<BaseResponse> CreateSimple(string html)
        {
            // Create the request message, which is a multipart/form-data request
            var createMessage = new HttpRequestMessage(HttpMethod.Post, PAGESENDPOINT)
            {
                Content = CreateHtmlContent(html)
            };

            var response = await CreateClient().SendAsync(createMessage);
            return await TranslateResponse(response);
        }

        public async Task<BaseResponse> CreateWithImage(string html, string imageName, Stream imageStream)
        {
            using (var imageContent = new StreamContent(imageStream))
            {
                imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                
                var createMessage = new HttpRequestMessage(HttpMethod.Post, PAGESENDPOINT)
                {
                    Content = 
                        new MultipartFormDataContent
                        {
                            { CreateHtmlContent(html), PRESENTATION },
                            { imageContent, imageName }
                        }
                };

                var response = await CreateClient().SendAsync(createMessage);
                return await TranslateResponse(response);
            }
        }

        public async Task<BaseResponse> CreateWithHtml(string html, string embeddedPartName, string embeddedHtml)
        {
            var createMessage = new HttpRequestMessage(HttpMethod.Post, PAGESENDPOINT)
            {
                Content = 
                    new MultipartFormDataContent
                    {
                        { CreateHtmlContent(html), PRESENTATION },
                        { CreateHtmlContent(embeddedHtml), embeddedPartName },
                    }
            };

            var response = await CreateClient().SendAsync(createMessage);
            return await TranslateResponse(response);
        }




        #region Private helper functions

        HttpClient CreateClient()
        {
            var client = new HttpClient();
            var headers = client.DefaultRequestHeaders;

            // Note: API only supports JSON return type.
            headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // This allows you to see what happens when an unauthenticated call is made.
            if (accessToken != null)
            {
                headers.Authorization = CreateAuthHeader();
            }

            return client;
        }

        AuthenticationHeaderValue CreateAuthHeader()
        {
            return new AuthenticationHeaderValue("Bearer", accessToken);
        }

        StringContent CreateHtmlContent(string html)
        {
            return new StringContent(html, Encoding.UTF8, "text/html");
        }

        async static Task<BaseResponse> TranslateResponse(HttpResponseMessage response)
        {
            BaseResponse standardResponse;
            if (response.StatusCode == HttpStatusCode.Created)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseObject = JsonConvert.DeserializeObject(responseString);
                standardResponse = new CreateSuccessResponse
                {
                    StatusCode = response.StatusCode,
                    OneNoteClientUrl = responseObject.links.oneNoteClientUrl.href,
                    OneNoteWebUrl = responseObject.links.oneNoteWebUrl.href
                };
            }
            else
            {
                standardResponse = new StandardErrorResponse
                {
                    StatusCode = response.StatusCode,
                    Message = await response.Content.ReadAsStringAsync()
                };
            }

            // Extract the correlation id.  Apps should log this if they want to collect data to diagnose failures with Microsoft support 
            IEnumerable<string> correlationValues;
            if (response.Headers.TryGetValues("X-CorrelationId", out correlationValues))
            {
                standardResponse.CorrelationId = correlationValues.FirstOrDefault();
            }

            return standardResponse;
        }

        #endregion
    }
}
