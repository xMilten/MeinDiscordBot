using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeinDiscordBot;
public class SaveInfos {
    public List<MyChannel> Channels { get; set; } = new List<MyChannel>();
    public ulong Info { get; set; }
    public ulong Error { get; set; }
}
