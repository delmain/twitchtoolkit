using MoonSharp.Interpreter;
using System;
using System.Text;
using System.Text.RegularExpressions;
using ToolkitCore.Models;
using Verse;

namespace TwitchToolkit
{
    public class Command : Def
    {
        public void RunCommand(MessageDetails message)
        {
            if (command == null)
            {
                throw new InvalidOperationException("Command is null");
            }

            CommandDriver driver = (CommandDriver)Activator.CreateInstance(commandDriver);
            driver.command = this;
            driver.RunCommand(message);
        }

        public string Label
        {
            get
            {
                if (label != null && label != "")
                {
                    return label;
                }

                return defName;
            }
        }

        public string command = null;

        public bool enabled = true;

        public bool shouldBeInSeparateRoom = false;

        public Type commandDriver = typeof(CommandDriver);

        public bool requiresMod = false;

        public bool requiresAdmin = false;

        public string outputMessage = "";

        public bool isCustomMessage = false;
    }

    public class Functions
    {
        public ViewerState GetViewer(string username)
        {
            return ViewerStates.GetViewer(username);
        }

        public string ReturnString()
        {
            return "Hello World!";
        }
    }

    public class CommandDriver
    {
        public Command command = null;

        public virtual void RunCommand(MessageDetails message)
        {
            Helper.DebugLog("filtering command");

            string output = FilterTags(message, command.outputMessage);

            Helper.DebugLog("command filtered");

            if (!UserData.IsTypeRegistered<Functions>())
            {
                UserData.RegisterType<Functions>();
                UserData.RegisterType<ViewerState>();
            }
            
            Helper.DebugLog("creating script");

            Script script = new Script();
            script.DebuggerEnabled = true;
            DynValue functions = UserData.Create(new Functions());
            script.Globals.Set("functions", functions);

            Helper.DebugLog("Parsing Script " + output);

            DynValue res = script.DoString(output);
            message.Reply(res.CastToString());

            Log.Message(res.CastToString());            
        }

        public string FilterTags(MessageDetails message, string input)
        {
            Helper.DebugLog("starting filter");

            ViewerState viewer = ViewerStates.GetViewer(message.Viewer.Username);

            StringBuilder output = new StringBuilder(input);
            output.Replace("{username}", message.Viewer.Username);
            output.Replace("{balance}", viewer.Coins.ToString());
            output.Replace("{karma}", viewer.Karma.ToString());
            output.Replace("{purchaselist}", ToolkitSettings.CustomPricingSheetLink);
            output.Replace("{coin-reward}", ToolkitSettings.CoinAmount.ToString());

            output.Replace("\n", "");

            Helper.DebugLog("starting regex");

            Regex regex = new Regex(@"\[(.*?)\]");

            MatchCollection matches = regex.Matches(output.ToString());

            foreach (Match match in matches)
            {
                Helper.DebugLog("found match " + match.Value);
                string code = match.Value;
                code = code.Replace("[", "");
                code = code.Replace("]", "");

                output.Replace(match.Value, MoonSharpString(code));
            }

            return output.ToString();
        }

        public string MoonSharpString(string function)
        {
            string script = @function;

            DynValue res = Script.RunString(script);
            return res.String;
        }

        public double MoonSharpDouble(string function)
        {
            string script = @function;

            DynValue res = Script.RunString(script);
            return res.Number;
        }
    }
}
