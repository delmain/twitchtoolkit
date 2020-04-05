using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using TwitchToolkit.Utilities;
using TwitchToolkit.Store;
using ToolkitCore.Models;

namespace TwitchToolkit
{
    public static class ViewerStates
    {
        public static string jsonallviewers;
        public static List<ViewerState> All = new List<ViewerState>();

        public static void AwardViewersCoins(int setamount = 0)
        {
            List<string> usernames = ParseViewersFromJsonAndFindActiveViewers();
            if (usernames != null)
            {
                foreach (string username in usernames)
                {
                    ViewerState viewer = GetViewer(username);

                    if (viewer.IsBanned)
                    {
                        continue;
                    }

                    if (setamount > 0)
                    {
                        viewer.GiveViewerCoins(setamount);
                    }
                    else
                    {
                        int baseCoins = ToolkitSettings.CoinAmount;
                        float baseMultiplier = (float)viewer.Karma / 100f;

                        if (viewer.IsSub)
                        {
                            baseCoins += ToolkitSettings.SubscriberExtraCoins;
                            baseMultiplier *= ToolkitSettings.SubscriberCoinMultiplier;
                        }
                        
                        if (viewer.IsVIP)
                        {
                            baseCoins += ToolkitSettings.VIPExtraCoins;
                            baseMultiplier *= ToolkitSettings.VIPCoinMultiplier;
                        }
                        
                        if (viewer.IsModerator)
                        {
                            baseCoins += ToolkitSettings.ModExtraCoins;
                            baseMultiplier *= ToolkitSettings.ModCoinMultiplier;
                        }

                        // check if viewer is active in chat
                        int minutesSinceViewerWasActive = TimeHelper.MinutesElapsed(viewer.last_seen);

                        if (ToolkitSettings.ChatReqsForCoins)
                        {
                            if (minutesSinceViewerWasActive > ToolkitSettings.TimeBeforeHalfCoins)
                            {
                                baseMultiplier *= 0.5f;
                            }

                            if (minutesSinceViewerWasActive > ToolkitSettings.TimeBeforeNoCoins)
                            {
                                baseMultiplier *= 0f;
                            }
                        }

                        double coinsToReward = (double)baseCoins * baseMultiplier;

                        Store_Logger.LogString($"{viewer.username} gets {baseCoins} * {baseMultiplier} coins, total {(int)Math.Ceiling(coinsToReward)}");
                        
                        viewer.GiveViewerCoins((int)Math.Ceiling(coinsToReward));
                    }
                }
            }
        }

        public static void GiveAllViewersCoins(int amount, List<ViewerState> viewers = null)
        {
            if (viewers != null)
            {
                foreach (ViewerState viewer in viewers)
                {
                    viewer.GiveViewerCoins(amount);
                }

                return;
            }

            List<string> usernames = ParseViewersFromJsonAndFindActiveViewers();
            if (usernames != null)
            {
                foreach (string username in usernames)
                {
                    ViewerState viewer = ViewerStates.GetViewer(username);
                    if (viewer != null && viewer.Karma > 1)
                    {
                        viewer.GiveViewerCoins(amount);
                    }
                }
            }
        }

        public static void SetAllViewersCoins(int amount, List<ViewerState> viewers = null)
        {
            if (viewers != null)
            {
                foreach (ViewerState viewer in viewers)
                {
                    viewer.SetViewerCoins(amount);
                }

                return;
            }

            if (All != null)
            {
                foreach (ViewerState viewer in All)
                {
                    if (viewer != null)
                    {
                        viewer.SetViewerCoins(amount);
                    }
                }
            }
        }

        public static void GiveAllViewersKarma(int amount, List<ViewerState> viewers = null)
        {
            if (viewers != null)
            {
                foreach (ViewerState viewer in viewers)
                {
                    viewer.SetViewerKarma(Math.Min(ToolkitSettings.KarmaCap, viewer.Karma + amount));
                }

                return;
            }

            List<string> usernames = ParseViewersFromJsonAndFindActiveViewers();
            if (usernames != null)
            {
                foreach (string username in usernames)
                {
                    ViewerState viewer = ViewerStates.GetViewer(username);
                    if (viewer != null && viewer.Karma > 1)
                    {
                        viewer.SetViewerKarma( Math.Min(ToolkitSettings.KarmaCap, viewer.Karma + amount) );
                    }
                }
            }
        }

        public static void TakeAllViewersKarma(int amount, List<ViewerState> viewers = null)
        {
            if (viewers != null)
            {
                foreach (ViewerState viewer in viewers)
                {
                    viewer.SetViewerKarma(Math.Max(0, viewer.Karma - amount));
                }

                return;
            }

            if (All != null)
            {
                foreach (ViewerState viewer in All)
                {
                    if (viewer != null)
                    {
                        viewer.SetViewerKarma( Math.Max(0, viewer.Karma - amount) );
                    }
                }
            }
        }

        public static void SetAllViewersKarma(int amount, List<ViewerState> viewers = null)
        {
            if (viewers != null)
            {
                foreach (ViewerState viewer in viewers)
                {
                    viewer.SetViewerKarma(amount);
                }

                return;
            }

            if (All != null)
            {
                foreach (ViewerState viewer in All)
                {
                    if (viewer != null)
                    {
                        viewer.SetViewerKarma( amount );
                    }
                }
            }
        }

        public static List<string> ParseViewersFromJsonAndFindActiveViewers()
        {
            List<string> usernames = new List<string>();

            string json = jsonallviewers;

            if (json.NullOrEmpty())
            {
                return null;
            }

            var parsed = JSON.Parse(json);
            var chatters = parsed["chatters"];
            List<JSONArray> groups = new List<JSONArray>();
            groups.Add(chatters["moderators"].AsArray);
            groups.Add(chatters["staff"].AsArray);
            groups.Add(chatters["admins"].AsArray);
            groups.Add(chatters["global_mods"].AsArray);
            groups.Add(chatters["viewers"].AsArray);
            groups.Add(chatters["vips"].AsArray);
            groups.Add(chatters["broadcaster"].AsArray);
            foreach (JSONArray group in groups)
            {
                foreach (JSONNode username in group)
                {
                    // TODO either fix this or more realistically move it to ToolkitCore since it's user management
                    string usernameconvert = username.ToString();
                    usernameconvert = usernameconvert.Remove(0, 1);
                    usernameconvert = usernameconvert.Remove(usernameconvert.Length - 1, 1);
                    usernames.Add(usernameconvert);
                }
            }

            // for bigger streams, the chatter api can get buggy. Therefore we add viewers active in chat within last 30 minutes just in case.

            foreach (ViewerState viewer in All.Where(s => s.last_seen != null && TimeHelper.MinutesElapsed(s.last_seen) <= ToolkitSettings.TimeBeforeHalfCoins))
            {
                if (!usernames.Contains(viewer.username))
                {
                    Helper.Log("Viewer " + viewer.username + " added to active viewers through chat participation but not in chatter list.");
                    usernames.Add(viewer.username);
                }
                // TODO this doesn't do anything except put logs in chat
            }

            return usernames;
        }

        public static bool SaveUsernamesFromJsonResponse(TwitchToolkitDev.RequestState request)
        {
            jsonallviewers = request.jsonString;
            return true;
        }

        public static void ResetViewers()
        {
            All = new List<ViewerState>();
        }

        public static ViewerState GetState(Viewer viewer)
        {
            return All.FirstOrDefault(v => v.username == viewer.Username) 
                ?? new ViewerState(viewer.Username);
        }

        public static ViewerState GetViewer(string user)
        {
            return All.Find(x => x.username == user.ToLower())
                ?? new ViewerState(user);
        }

        public static ViewerState GetViewerById(int id)
        {
            return All.Find(s => s.id == id);
        }

        public static void RefreshViewers()
        {
            TwitchToolkitDev.WebRequest_BeginGetResponse.Main(
                $"https://tmi.twitch.tv/group/user/{ToolkitSettings.Channel.ToLower()}/chatters", 
                SaveUsernamesFromJsonResponse
            );
        }

        public static void ResetViewersCoins()
        {
            foreach(ViewerState viewer in All) 
                viewer.SetViewerCoins(ToolkitSettings.StartingBalance);
        }

        public static void ResetViewersKarma()
        {
            foreach (ViewerState viewer in All) 
                viewer.SetViewerKarma(ToolkitSettings.StartingKarma);
        }
    }
}
