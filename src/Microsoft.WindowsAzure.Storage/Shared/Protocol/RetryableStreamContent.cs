﻿// -----------------------------------------------------------------------------------------
// <copyright file="RetryableStreamContent.cs" company="Microsoft">
//    Copyright 2013 Microsoft Corporation
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>
// -----------------------------------------------------------------------------------------

namespace Sandboxable.Microsoft.WindowsAzure.Storage.Shared.Protocol
{
    using System.IO;
    using System.Net.Http;

    internal class RetryableStreamContent : StreamContent
    {
        /// <summary>
        /// Creates a new instance of the RetryableStreamContent class.
        /// </summary>
        /// <param name="content">The content used to initialize the RetryableStreamContent.</param>
        public RetryableStreamContent(Stream content)
            : base(content)
        {
        }

        /// <summary>
        /// Creates a new instance of the RetryableStreamContent class.
        /// </summary>
        /// <param name="content">The content used to initialize the RetryableStreamContent.</param>
        /// <param name="bufferSize">The size, in bytes, of the buffer for the RetryableStreamContent.</param>
        public RetryableStreamContent(Stream content, int bufferSize)
            : base(content, bufferSize)
        {
        }

        /// <summary>
        /// Ignores the request and does not call base.Dispose.
        /// </summary>
        /// <param name="disposing">Not used.</param>
        protected override void Dispose(bool disposing)
        {
            // This method does not call base.Dispose, which will go ahead
            // and dispose the underlying stream as well. However, that would
            // mean each retry by Executor needing a new stream to be re-created
            // and also disposing client's stream without their knowledge.
        }
    }
}
