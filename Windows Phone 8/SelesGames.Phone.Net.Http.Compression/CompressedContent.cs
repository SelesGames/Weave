﻿using ICSharpCode.SharpZipLib.GZip;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SelesGames.Phone.Net.Http.Compression
{
    public class CompressedContent : HttpContent
    {
        private HttpContent originalContent;
        private string encodingType;

        public CompressedContent(HttpContent content, string encodingType)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (encodingType == null) throw new ArgumentNullException("encodingType");

            originalContent = content;
            this.encodingType = encodingType.ToLowerInvariant();

            if (this.encodingType != "gzip" && this.encodingType != "deflate")
            {
                throw new InvalidOperationException(string.Format("Encoding '{0}' is not supported. Only supports gzip or deflate encoding.", this.encodingType));
            }

            // copy the headers from the original content
            foreach (var header in originalContent.Headers)
            {
                this.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            this.Headers.ContentEncoding.Add(encodingType);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;

            return false;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            Stream compressedStream = null;

            if (encodingType == "gzip")
            {
                compressedStream = new GZipOutputStream(stream);
            }
            else if (encodingType == "deflate")
            {
                throw new NotSupportedException(
                    "deflate is not supported in the phone version of SelesGames.Phone.Net.Http.Compression.CompressedContent");
            }

            return originalContent.CopyToAsync(compressedStream).ContinueWith(tsk =>
            {
                if (compressedStream != null)
                {
                    compressedStream.Flush();
                    compressedStream.Close();
                    compressedStream.Dispose();
                }
            });
        }
    }
}
