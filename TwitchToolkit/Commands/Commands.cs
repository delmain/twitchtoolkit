using ToolkitCore.Models;
using System;
using System.Linq;
using Verse;

namespace TwitchToolkit
{
    public static class CommandsHandler
    {
        public static void CheckCommand(MessageDetails msg)
        {
            if (msg == null)
                return;

            if (msg.Message == null)
                return;

            string message = msg.Message;
            string user = msg.Viewer.Username;
            if (message.Split(' ')[0] == "/w")
            {
                message = message.Substring(2); // remove /w at the beginning
                Helper.DebugLog($"Cleaned whisper: {message}");
            }

            ViewerState viewer = ViewerStates.GetViewer(user);
            viewer.last_seen = DateTime.Now;

            if (viewer.IsBanned)
            {
                return;
            }

            Command commandDef = DefDatabase<Command>.AllDefs.ToList().Find(s => msg.Message.StartsWith("!" + s.command));
            if (commandDef != null)
            {
                bool runCommand = true;

                if (commandDef.requiresMod && !(viewer.IsModerator || viewer.IsBroadcaster))
                    runCommand = false;

                if (commandDef.requiresAdmin && !viewer.IsBroadcaster)
                    runCommand = false;

                if (!commandDef.enabled)
                    runCommand = false;

                if (commandDef.shouldBeInSeparateRoom && !AllowCommand(msg))
                    runCommand = false;

                if (runCommand)
                    commandDef.RunCommand(msg);
            }
        }

        [Obsolete]
        public static bool AllowCommand(MessageDetails msg)
        {
            return true;
        }
    }
}
