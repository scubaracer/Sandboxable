﻿// -----------------------------------------------------------------------------------------
// <copyright file="TableOperators.cs" company="Microsoft">
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

namespace Sandboxable.Microsoft.WindowsAzure.Storage.Table
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Defines the set of Boolean operators for constructing queries.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "Due to Javascript projection limitations.")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1303:ConstFieldNamesMustBeginWithUpperCaseLetter", Justification = "Due to Javascript projection limitations..")]
    public static class TableOperators
    {
#if WINDOWS_RT
        internal const string and = "and";
        internal const string not = "not";
        internal const string or = "or";

        public static string And
        {
            get { return and; }
        }

        public static string Not
        {
            get { return not; }
        }

        public static string Or
        {
            get { return or; }
        }
#else
        /// <summary>
        /// Represents the And operator.
        /// </summary>
        public const string And = "and";

        /// <summary>
        /// Represents the Not operator.
        /// </summary>
        public const string Not = "not";

        /// <summary>
        /// Represents the Or operator.
        /// </summary>
        public const string Or = "or";
#endif
    }
}
