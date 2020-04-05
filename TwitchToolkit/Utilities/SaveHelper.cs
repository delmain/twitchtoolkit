using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine;

namespace TwitchToolkit.Utilities
{
    public static class SaveHelper
    {
        public static string modFolder = "TwitchToolkit";
        public static string dataPath = Application.persistentDataPath + $"/{modFolder}/";
        public static string viewerDataPath = Path.Combine(dataPath, "ViewerData.json");
        public static string itemDataPath = Path.Combine(dataPath, "ItemData.json");
        public static string incItemsDataPath = Path.Combine(dataPath, "IncItemsData.json");
        public static string storePricesDataPath = Path.Combine(dataPath, "StorePrices.csv");

        private static void SaveJsonToDataPath(string json, string savePath)
        {
            bool dataPathExists = Directory.Exists(dataPath);
            
            if(!dataPathExists)
                Directory.CreateDirectory(dataPath);

            using (StreamWriter streamWriter = File.CreateText (savePath))
            {
                streamWriter.Write (json.ToString());
            }
        }

        public static void SaveAllModData()
        {
            Helper.Log("Saving data");

            SaveListOfViewersAsJson();
        }

        public static void SaveListOfViewersAsJson()
        {
            var newViewers = new List<ViewerSaveable>();
            if (ViewerStates.All == null)
                return;

            foreach (ViewerState vwr in ViewerStates.All)
            {
                newViewers.Add(new ViewerSaveable
                {
                    id = vwr.id,
                    username = vwr.username,
                    coins = vwr.Coins,
                    karma = vwr.Karma
                });
            }

            if (newViewers.Count <= 0)
                return;

            // TODO fix all this
            var viewerslisttemplate = JSON.Parse("{\"viewers\":[],\"total\":0}");
            string viewertemplate = "{\"id\":0,\"username\":\"string\",\"karma\":0,\"coins\":0}";
            foreach (ViewerSaveable vwr in newViewers)
            {
                var v = JSON.Parse(viewertemplate);
                v["id"] = vwr.id;
                v["username"] = vwr.username;
                v["karma"] = vwr.karma;
                v["coins"] = vwr.coins;
                viewerslisttemplate["viewers"].Add(vwr.id.ToString(), v);
            }
            viewerslisttemplate["total"] = newViewers.Count;

            Helper.Log("Saving viewers file");

            SaveJsonToDataPath(viewerslisttemplate.ToString(), viewerDataPath);
        }

        public static void LoadListOfViewers()
        {
            try
            {
                if (!File.Exists(viewerDataPath))
                    return;

                using (StreamReader streamReader = File.OpenText (viewerDataPath))
                {
                    string jsonString = streamReader.ReadToEnd();
                    var node = JSON.Parse(jsonString);
                    Helper.Log(node.ToString());
                    var listOfViewers = new List<ViewerState>();
                    for (int i = 0; i < node["total"]; i++)
                    {
                        var viewer = new ViewerState(node["viewers"][i]["username"]);
                        viewer.SetViewerCoins(node["viewers"][i]["coins"].AsInt);
                        viewer.SetViewerKarma(node["viewers"][i]["karma"].AsInt);
                        listOfViewers.Add(viewer);
                    }

                    ViewerStates.All = listOfViewers;
                }
            }
            catch (InvalidDataException e)
            {
                Helper.Log("Invalid " + e.Message);
            }

        }
    }

    public class ViewerSaveable
    {
        public int id;
        public string username;
        public int karma;
        public int coins;
    }
}