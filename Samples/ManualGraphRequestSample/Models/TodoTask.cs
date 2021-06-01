// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace ManualGraphRequestSample.Models
{
    /// <summary>
    /// The type Todo Task.
    /// The complete type definition can refer to here: https://github.com/microsoftgraph/msgraph-sdk-dotnet/blob/dev/src/Microsoft.Graph/Generated/model/TodoTask.cs
    /// </summary>
    [DataContract]
    public class TodoTask
    {
        /// <summary>
        /// Gets or sets @odata.type.
        /// </summary>
        [JsonProperty("@odata.type")]
        public string ODataType { get; set; }

        /// <summary>
        /// Gets or sets id.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets title.
        /// A brief description of the task.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TodoTask"/> class.
        /// </summary>
        public TodoTask()
        {
            this.ODataType = "microsoft.graph.todoTask";
        }
    }
}
