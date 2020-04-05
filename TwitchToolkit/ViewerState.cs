using System;
using System.Collections.Generic;
using ToolkitCore.Models;
using TwitchToolkit.Store;

namespace TwitchToolkit
{
    public class ViewerState
    {
        private readonly Lazy<Viewer> viewer = null;
        private Viewer Viewer => viewer?.Value;

        public int id;
        
        [Obsolete] // TODO figure out the linking between Viewers and ViewerStates better.
        public string username;

        public int Coins { get; protected set; }
        public int Karma { get; protected set; }

        public DateTime last_seen;

        public ViewerState(string username)
        {
            this.username = username;
            viewer = new Lazy<Viewer>(() => Viewers.FindByUsername(this.username) ?? Viewers.CreateAndAddViewer(this.username));

            Coins = ToolkitSettings.StartingBalance;
            Karma = ToolkitSettings.StartingKarma;
            
            id = ViewerStates.All.Count;
            ViewerStates.All.Add(this);
        }

        public bool IsSub => Viewer.IsSubscriber; 

        public bool IsModerator
        {
            get
            {
                return Viewer.IsModerator || (ToolkitSettings.ViewerModerators.ContainsKey(Viewer.Username) && ToolkitSettings.ViewerModerators[Viewer.Username]);
            }
        }

        public bool IsVIP => Viewer.IsVIP;

        public bool IsBroadcaster => Viewer.IsBroadcaster;

        public void SetAsModerator()
        {
            if (!IsModerator)
            {
                ToolkitSettings.ViewerModerators[Viewer.Username] = true;
            }
        }

        public void RemoveAsModerator()
        {
            if (IsModerator && !Viewer.IsModerator)
            {
                ToolkitSettings.ViewerModerators[Viewer.Username] = false;
            }
        }

        public void SetViewerKarma(int karma)
        {
            // do not let karma go below zero
            if (karma < 0)
                karma = 0;

            Karma = karma;
        }

        public int GiveViewerKarma(int karma)
        {
            SetViewerKarma(Karma + karma);
            return Karma;
        }

        public int TakeViewerKarma(int karma)
        {
            return GiveViewerKarma(0 - karma);
        }

        public void CalculateNewKarma(KarmaType karmaType, int price)
        {
            int old = Karma;
            int newKarma = Store.Karma.CalculateNewKarma(old, karmaType, price);
            
            SetViewerKarma(newKarma);
            Store_Logger.LogKarmaChange(Viewer.Username, old, newKarma);
        }

        public void SetViewerCoins(int coins)
        {
            // do not let coins go below zero
            if (coins < 0)
                coins = 0;
            Coins = coins;
        }

        public void GiveViewerCoins(int coins)
        {
            SetViewerCoins(Coins + coins);
        }

        public void TakeViewerCoins(int coins)
        {
            GiveViewerCoins(0 - coins);
        }

        public bool IsBanned => ToolkitSettings.BannedViewers.Contains(Viewer.Username);

        public void BanViewer()
        {
            if (!IsBanned)
            {
                ToolkitSettings.BannedViewers.Add(Viewer.Username);
            }
        }

        public void UnBanViewer()
        {
            if (IsBanned)
            {
                ToolkitSettings.BannedViewers.Remove(Viewer.Username);
            }
        }

        public static string GetViewerColorCode(string username)
        {
            if (!ToolkitSettings.ViewerColorCodes.ContainsKey(username))
            {
                SetViewerColorCode(Helper.GetRandomColorCode(), username);
            }
            
            return ToolkitSettings.ViewerColorCodes[username];
        }

        public static void SetViewerColorCode(string colorcode, string username)
        {
            ToolkitSettings.ViewerColorCodes[username] = colorcode;
        }

        public override string ToString()
        {
            return $"{username} (#{id})-C:{Coins}|K:{Karma}";
        }
    }
}