﻿//-----------------------------------------------------------------------
// <copyright file="CopyState.cs" company="Microsoft">
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
//-----------------------------------------------------------------------

namespace Sandboxable.Microsoft.WindowsAzure.Storage.Blob
{
    using System;

    /// <summary>
    /// Represents the attributes of a copy operation.
    /// </summary>
    public sealed class CopyState
    {
        /// <summary>
        /// Gets the ID of the copy operation.
        /// </summary>
        /// <value>A copy ID string.</value>
        public string CopyId
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the time the copy operation completed, and indicates whether completion was due to a successful copy, the cancelling of the operation, or a failure.
        /// </summary>
        /// <value>A <see cref="DateTimeOffset"/> containing the completion time, or <c>null</c> if the operation has not completed.</value>
        public DateTimeOffset? CompletionTime
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the status of the copy operation.
        /// </summary>
        /// <value>A <see cref="CopyStatus"/> enumeration indicating the status of the operation.</value>
        public CopyStatus Status
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the source URI of a copy operation.
        /// </summary>
        /// <value>A <see cref="System.Uri"/> indicating the source of a copy operation, or <c>null</c>.</value>
        public Uri Source
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the number of bytes copied in the operation so far.
        /// </summary>
        /// <value>The number of bytes copied in the operation so far, or <c>null</c>.</value>
        public long? BytesCopied
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the total number of bytes in the source of the copy.
        /// </summary>
        /// <value>The number of bytes in the source, or <c>null</c>.</value>
        public long? TotalBytes
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the description of the current status, if any.
        /// </summary>
        /// <value>A status description string, or <c>null</c>.</value>
        public string StatusDescription
        {
            get;
            internal set;
        }
    }
}
