using System;
using System.Diagnostics;
using Firepuma.WebPush.FunctionApp.Commands;
using Firepuma.WebPush.FunctionApp.Infrastructure.Helpers;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace Firepuma.WebPush.FunctionApp.Models.TableModels
{
    [DebuggerDisplay("{ToString()}")]
    public class CommandExecutionEvent : TableEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CommandId { get; set; }
        public string TypeName { get; set; }
        public string TypeNamespace { get; set; }
        public string Payload { get; set; }
        public DateTime CreatedOn { get; set; }

        public bool? Successful { get; set; }
        public string Result { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorStackTrack { get; set; }
        public double ExecutionTimeInSeconds { get; set; }
        public double TotalTimeInSeconds { get; set; }
        public DateTime? Updated { get; set; }


        public CommandExecutionEvent(BaseCommand baseCommand)
        {
            PartitionKey = "";
            RowKey = Id;

            CommandId = baseCommand.CommandId;
            TypeName = baseCommand.GetType().GetTypeNameExcludingNamespace();
            TypeNamespace = baseCommand.GetType().GetTypeNamespace();
            Payload = JsonConvert.SerializeObject(baseCommand, GetCommandPayloadSerializerSettings());
            CreatedOn = baseCommand.CreatedOn;
        }

        private static JsonSerializerSettings GetCommandPayloadSerializerSettings()
        {
            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            return jsonSerializerSettings;
        }

        public override string ToString()
        {
            return $"{Id}/{CommandId}/{TypeNamespace}.{TypeName}";
        }
    }
}