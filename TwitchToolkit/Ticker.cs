﻿using System.Threading;
using System.Collections.Generic;
using Verse;
using System;
using RimWorld;
using System.Linq;
using UnityEngine;
using TwitchToolkit.Votes;
using TwitchToolkit.Store;

namespace TwitchToolkit
{
    public class Ticker : Thing
    {
        public Timer timer = null;

        public static long LastIRCPong = 0;

        public static Queue<FiringIncident> FiringIncidents = new Queue<FiringIncident>();
        public static Queue<VoteEvent> VoteEvents = new Queue<VoteEvent>();
        public static Queue<IncidentWorker> Incidents = new Queue<IncidentWorker>();
        public static Queue<IncidentHelper> IncidentHelpers = new Queue<IncidentHelper>();
        public static Queue<IncidentHelperVariables> IncidentHelperVariables = new Queue<IncidentHelperVariables>();

        public bool CreatedByController { get; internal set; }

        static Thread _registerThread;
        static Game _game;
        static TwitchToolkit _mod = Toolkit.Mod;

        static Ticker _instance;
        public static Ticker Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Ticker();
                }
                return _instance;
            }
        }

        public Ticker()
        {
            def = new ThingDef { tickerType = TickerType.Normal, isSaveable = false };
            _registerThread = new Thread(Register);
            _registerThread.Start();
            lastEvent = DateTime.Now;
            LastIRCPong = DateTime.Now.ToFileTime();
        }

        void Register()
        {
            while (true)
            {
                try
                {
                    if (_game != Current.Game)
                    {
                        if (_game != null)
                        {
                            _game.tickManager.DeRegisterAllTickabilityFor(this);
                            _game = null;
                        }

                        _game = Current.Game;
                        if (_game != null)
                        {
                            _game = Current.Game;
                            _game.tickManager.RegisterAllTickabilityFor(this);
                            Toolkit.Mod.RegisterTicker();
                        }   
                    }
                }
                catch (Exception ex)
                {
                    Helper.Log("Exception: " + ex.Message + "\n" + ex.StackTrace);
                }
                finally
                {
                    Thread.Sleep(1000);
                }
            }
        }

        int[] _baseTimes = { 20, 60, 120, 180, 999999 };
        private int _lastMinute = -1;
        private int _lastCoinReward = -1;

        public override void Tick()
        {
            try
            {
                if (_game == null || _mod == null)
                {
                    return;
                }

                _mod.Tick();
                var minutes = (int)(_game.Info.RealPlayTimeInteracting / 60f);
                double getTime = (double)Time.time / 60f;
                int time = Convert.ToInt32(Math.Truncate(getTime));

                if (IncidentHelpers.Count > 0)
                {
                    while (IncidentHelpers.Count > 0)
                    {
                        var incidentHelper = IncidentHelpers.Dequeue();
                        if (!(incidentHelper is VotingHelper))
                        {
                            Purchase_Handler.QueuePlayerMessage(incidentHelper.Viewer, incidentHelper.message);
                        }
                        incidentHelper.TryExecute();
                    }

                    Helper.playerMessages = new List<string>();
                }

                if (IncidentHelperVariables.Count > 0)
                {
                    while (IncidentHelperVariables.Count > 0)
                    {
                        var incidentHelper = IncidentHelperVariables.Dequeue();
                        Purchase_Handler.QueuePlayerMessage(incidentHelper.Viewer, incidentHelper.message, incidentHelper.storeIncident.variables);
                        incidentHelper.TryExecute();
                        if (Purchase_Handler.viewerNamesDoingVariableCommands.Contains(incidentHelper.Viewer.username))
                            Purchase_Handler.viewerNamesDoingVariableCommands.Remove(incidentHelper.Viewer.username);
                    }

                    Helper.playerMessages = new List<string>();
                }

                if (Incidents.Count > 0)
                {
                    var incident = Incidents.Dequeue();
			        IncidentParms incidentParms = new IncidentParms();
			        incidentParms.target = Helper.AnyPlayerMap;
                    incident.TryExecute(incidentParms);
                }

                if (FiringIncidents.Count > 0)
                {
                    Helper.Log("Firing " + FiringIncidents.First().def.defName);
                    var incident = FiringIncidents.Dequeue();
                    incident.def.Worker.TryExecute(incident.parms);
                }

                VoteHandler.CheckForQueuedVotes();

                if (_lastCoinReward < 0)
                {
                    _lastCoinReward = time;
                }
                else if (ToolkitSettings.EarningCoins && ((time - _lastCoinReward) >= ToolkitSettings.CoinInterval) && ViewerStates.jsonallviewers != null)
                {
                    _lastCoinReward = time;
                    ViewerStates.AwardViewersCoins();
                }
                if (_lastMinute < 0)
                {
                    _lastMinute = time;
                }
                else if (_lastMinute < time)
                {
                    _lastMinute = time;
                    Toolkit.JobManager.CheckAllJobs();
                    ViewerStates.RefreshViewers();
                }   
            }
            catch (Exception ex)
            {
                Helper.Log("Exception: " + ex.Message + ex.StackTrace);
            }
        }

        public static DateTime lastEvent;
    }
}
