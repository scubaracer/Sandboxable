﻿//-----------------------------------------------------------------------
// <copyright file="TableUtilities.cs" company="Microsoft">
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

namespace Sandboxable.Microsoft.WindowsAzure.Storage.Table.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.IO;
    using System.Linq.Expressions;
    using System.Net;
    using System.Text;

    internal static class TableUtilities
    {
        /// <summary>
        /// Translates the data service exception.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="reqResult">The request result.</param>
        /// <param name="parseError">The delegate used to parse the error to get extended error information.</param>
        /// <returns>
        /// The translated exception.
        /// </returns>
        internal static StorageException TranslateDataServiceException(Exception e, RequestResult reqResult, Func<Stream, IDictionary<string, string>, StorageExtendedErrorInformation> parseError)
        {
            try
            {
                // The exception thrown is based on whether it is a change/query operation.
                DataServiceRequestException dsre = FindInnerExceptionOfType<DataServiceRequestException>(e);

                DataServiceQueryException dsqe = FindInnerExceptionOfType<DataServiceQueryException>(e);

                if (dsre == null && dsqe == null)
                {
                    InvalidOperationException ioe = TableUtilities.FindInnerExceptionOfType<InvalidOperationException>(e);

                    if (ioe != null && !(ioe is WebException) && string.CompareOrdinal(ioe.Source, "Microsoft.Data.Services.Client") == 0 && ioe.Message.Contains("type is not compatible with the expected"))
                    {
                        return new StorageException(reqResult, e.Message, e) { IsRetryable = false };
                    }

                    return null;
                }
                else if (dsre != null)
                {
                    DataServiceResponse response = dsre.Response;

                    // Get the batch status code first in case batch does not contain any responses.
                    reqResult.HttpStatusCode = response.BatchStatusCode;

                    IDictionary<string, string> headers;
                    foreach (OperationResponse operationResponse in response)
                    {
                        reqResult.HttpStatusCode = operationResponse.StatusCode;

                        // The exception thrown will contain the first error in the group of requests.
                        if (reqResult.HttpStatusCode >= 300)
                        {
                            headers = operationResponse.Headers;

                            // Strip off the extra exception type at the beginning.
                            string innerException = dsre.InnerException.ToString().Replace("System.Data.Services.Client.DataServiceClientException: ", string.Empty);

                            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(innerException)))
                            {
                                reqResult.ExtendedErrorInformation = parseError(stream, headers);
                            }

                            break;
                        }
                    }

                    return new StorageException(
                                reqResult,
                                reqResult.ExtendedErrorInformation != null ? reqResult.ExtendedErrorInformation.ErrorCode : dsre.Message,
                                dsre);
                }
                else
                {
                    QueryOperationResponse response = dsqe.Response;

                    reqResult.HttpStatusCode = response.StatusCode;

                    string innerException = dsqe.InnerException.Message;

                    using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(innerException)))
                    {
                        reqResult.ExtendedErrorInformation = parseError(stream, response.Headers);
                    }

                    return new StorageException(
                        reqResult,
                        reqResult.ExtendedErrorInformation != null ? reqResult.ExtendedErrorInformation.ErrorCode : dsqe.Message,
                        dsqe);
                }
            }
            catch (Exception)
            {
                return new StorageException(reqResult, e.Message, e);
            }
        }

        /// <summary>
        /// Look for an inner exception of type T. 
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>The found exception or <c>null</c>.</returns>
        internal static T FindInnerExceptionOfType<T>(Exception exception) where T : Exception
        {
            T dsce = null;

            while (exception != null)
            {
                dsce = exception as T;

                if (dsce != null)
                {
                    break;
                }

                exception = exception.InnerException;
            }

            return dsce;
        }

        /// <summary>
        /// Applies the continuation to query.
        /// </summary>
        /// <param name="continuationToken">The continuation token.</param>
        /// <param name="localQuery">The local query.</param>
        /// <returns>The modified query.</returns>
        public static DataServiceQuery<TElement> ApplyContinuationToQuery<TElement>(
            TableContinuationToken continuationToken, DataServiceQuery<TElement> localQuery)
        {
            if (continuationToken != null)
            {
                if (continuationToken.NextPartitionKey != null)
                {
                    localQuery = localQuery.AddQueryOption(TableConstants.TableServiceNextPartitionKey, continuationToken.NextPartitionKey);
                }

                if (continuationToken.NextRowKey != null)
                {
                    localQuery = localQuery.AddQueryOption(TableConstants.TableServiceNextRowKey, continuationToken.NextRowKey);
                }

                if (continuationToken.NextTableName != null)
                {
                    localQuery = localQuery.AddQueryOption(TableConstants.TableServiceNextTableName, continuationToken.NextTableName);
                }
            }

            return localQuery;
        }

        /// <summary>
        /// Gets the query take count.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        /// The take count of the query, if any.
        /// </returns>
        public static long GetQueryTakeCount<TElement>(DataServiceQuery<TElement> query, long defaultValue)
        {
            MethodCallExpression expression = query.Expression as MethodCallExpression;

            if (expression != null && expression.Method.Name == "Take")
            {
                ConstantExpression argument = expression.Arguments[1] as ConstantExpression;

                if (argument != null)
                {
                    return (long)((int)argument.Value);
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets the table continuation from response.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>The continuation.</returns>
        public static TableContinuationToken ContinuationFromResponse(QueryOperationResponse response)
        {
            string nextPartitionKey;
            string nextRowKey;
            string nextTableName;

            response.Headers.TryGetValue(
                TableConstants.TableServicePrefixForTableContinuation + TableConstants.TableServiceNextPartitionKey,
                out nextPartitionKey);
            response.Headers.TryGetValue(
                TableConstants.TableServicePrefixForTableContinuation + TableConstants.TableServiceNextRowKey,
                out nextRowKey);
            response.Headers.TryGetValue(
                TableConstants.TableServicePrefixForTableContinuation + TableConstants.TableServiceNextTableName,
                out nextTableName);

            if (string.IsNullOrEmpty(nextPartitionKey) && string.IsNullOrEmpty(nextRowKey) && string.IsNullOrEmpty(nextTableName))
            {
                return null;
            }

            TableContinuationToken newContinuationToken = new TableContinuationToken()
                                                              {
                                                                  NextPartitionKey = nextPartitionKey,
                                                                  NextRowKey = nextRowKey,
                                                                  NextTableName = nextTableName
                                                              };

            return newContinuationToken;
        }

        /// <summary>
        /// Copies the headers and properties from a request into a different request.
        /// </summary>
        /// <param name="destinationRequest">The request to copy into.</param>
        /// <param name="sourceRequest">The request to copy from.</param>
        internal static void CopyRequestData(HttpWebRequest destinationRequest, HttpWebRequest sourceRequest)
        {
            // Copy the request properties
            destinationRequest.AllowAutoRedirect = sourceRequest.AllowAutoRedirect;
            destinationRequest.AllowWriteStreamBuffering = sourceRequest.AllowWriteStreamBuffering;
            destinationRequest.AuthenticationLevel = sourceRequest.AuthenticationLevel;
            destinationRequest.AutomaticDecompression = sourceRequest.AutomaticDecompression;
            destinationRequest.CachePolicy = sourceRequest.CachePolicy;
            destinationRequest.ClientCertificates = sourceRequest.ClientCertificates;
            destinationRequest.ConnectionGroupName = sourceRequest.ConnectionGroupName;
            destinationRequest.ContinueDelegate = sourceRequest.ContinueDelegate;
            destinationRequest.CookieContainer = sourceRequest.CookieContainer;
            destinationRequest.Credentials = sourceRequest.Credentials;
            destinationRequest.ImpersonationLevel = sourceRequest.ImpersonationLevel;
            destinationRequest.KeepAlive = sourceRequest.KeepAlive;
            destinationRequest.MaximumAutomaticRedirections = sourceRequest.MaximumAutomaticRedirections;
            destinationRequest.MaximumResponseHeadersLength = sourceRequest.MaximumResponseHeadersLength;
            destinationRequest.MediaType = sourceRequest.MediaType;
            destinationRequest.Method = sourceRequest.Method;
            destinationRequest.Pipelined = sourceRequest.Pipelined;
            destinationRequest.PreAuthenticate = sourceRequest.PreAuthenticate;
            destinationRequest.ProtocolVersion = sourceRequest.ProtocolVersion;
            destinationRequest.Proxy = sourceRequest.Proxy;
            destinationRequest.ReadWriteTimeout = sourceRequest.ReadWriteTimeout;
            destinationRequest.SendChunked = sourceRequest.SendChunked;
            destinationRequest.Timeout = sourceRequest.Timeout;
            destinationRequest.UnsafeAuthenticatedConnectionSharing = sourceRequest.UnsafeAuthenticatedConnectionSharing;
            destinationRequest.UseDefaultCredentials = sourceRequest.UseDefaultCredentials;

            // Copy the headers.
            // Some headers can't be copied over. We check for these headers.
            foreach (string headerName in sourceRequest.Headers)
            {
                switch (headerName)
                {
                    case "Accept":
                        destinationRequest.Accept = sourceRequest.Accept;
                        break;

                    case "Connection":
                        destinationRequest.Connection = sourceRequest.Connection;
                        break;

                    case "Content-Length":
                        destinationRequest.ContentLength = sourceRequest.ContentLength;
                        break;

                    case "Content-Type":
                        destinationRequest.ContentType = sourceRequest.ContentType;
                        break;

                    case "Expect":
                        destinationRequest.Expect = sourceRequest.Expect;
                        break;

                    case "If-Modified-Since":
                        destinationRequest.IfModifiedSince = sourceRequest.IfModifiedSince;
                        break;

                    case "Referer":
                        destinationRequest.Referer = sourceRequest.Referer;
                        break;

                    case "Transfer-Encoding":
                        destinationRequest.TransferEncoding = sourceRequest.TransferEncoding;
                        break;

                    case "User-Agent":
                        destinationRequest.UserAgent = sourceRequest.UserAgent;
                        break;

                    default:
                        destinationRequest.Headers.Add(headerName, sourceRequest.Headers[headerName]);
                        break;
                }
            }
        }
    }
}