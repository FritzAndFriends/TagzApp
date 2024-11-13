using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TagzApp.Common.Models;

[JsonSerializable(typeof(YouTubeBroadcast))]
public record YouTubeBroadcast(string Id, string Title, DateTimeOffset? BroadcastTime, string LiveChatId);

