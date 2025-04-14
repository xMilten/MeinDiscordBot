using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeinDiscordBot;
public class MyChannel {
    public ulong Id { get; set; }
    public string Name { get; set; }
    public Dictionary<string, ulong> Messages { get; set; } = new Dictionary<string, ulong>();

    public MyChannel(string name, ulong id) {
        Name = name;
        Id = id;
    }
}