using System;
using System.Net.Http;

namespace SelesGames.Rest
{
    public class ResponseException : Exception
    {
        public HttpResponseMessage Response { get; private set; }
        
        public ResponseException(HttpResponseMessage response) 
            : base()
        {
            Response = response;
        }        
        
        public ResponseException(HttpResponseMessage response) : base()
        {
            Response = response;
        }        
        
        public ResponseException(Exception innerException, HttpResponseMessage response) 
            : base(null, innerException)
        {
            Response = response;
        }
    }
}