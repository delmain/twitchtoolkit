using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TwitchToolkit.PawnQueue
{
    class QueueWindow : Window
    {
        GameComponentPawns pawnComponent;

        public QueueWindow()
        {
            doCloseButton = true;
            pawnComponent = Current.Game.GetComponent<GameComponentPawns>();
            if (pawnComponent == null)
            {
                Log.Error("component null");
                Close();
            }
            GetPawn(PawnQueueSelector.FirstDefault);
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect unnamedCounter = new Rect(inRect.x + 10, 0,  300, 52);
            Widgets.Label(unnamedCounter, "Unnamed Colonists: " + unnamedColonists.Count);

            Rect colonistPortrait = new Rect(inRect.x + 10, 60, 100, 140);
            DrawColonist(colonistPortrait, selectedPawn);

            Rect rightSide = new Rect(130, 60, 300, 26);
            selectedUsername = Widgets.TextEntryLabeled(rightSide, "Username:", selectedUsername);

            rightSide.width = 120;
            rightSide.y += 26;
            rightSide.x += 150;
            if (Widgets.ButtonText(rightSide, "Assign", active: !string.IsNullOrWhiteSpace(selectedUsername)))
            {
                NameColonist(selectedUsername, selectedPawn);
            }         
            
            Rect pawnSelectors = new Rect(26, 210, 40, 26);

            if (Widgets.ButtonText(pawnSelectors, "<"))
            {
                GetPawn(PawnQueueSelector.Back);
            }

            pawnSelectors.x += 42;
            if (Widgets.ButtonText(pawnSelectors, ">"))
            {
                GetPawn(PawnQueueSelector.Next);
            }

            Rect namedStatus = new Rect(0, 236, 300, 26);
            bool viewerNamed = pawnComponent.HasPawnBeenNamed(selectedPawn);

            Widgets.Label(namedStatus, "Name: " + selectedPawn.Name);

            namedStatus.y += 26;
            Widgets.Label(
                namedStatus,
                "Colonist " + (
                        viewerNamed ? 
                        "named after <color=#4BB543>" + pawnComponent.UserAssignedToPawn(selectedPawn) + "</color>" : 
                        "<color=#B2180E>unnamed</color>"
                    )
                );

            Rect queueButtons = new Rect(0, 300, 200, 26);

            Widgets.Label(queueButtons, "Viewers in Queue: " + pawnComponent.ViewersInQueue());

            queueButtons.y += 26;
            if (Widgets.ButtonText(queueButtons, "Next Viewer from Queue"))
            {
                selectedUsername = pawnComponent.GetNextViewerFromQueue();
            }

            queueButtons.y += 26;
            if (Widgets.ButtonText(queueButtons, "Random Viewer from Queue"))
            {
                selectedUsername = pawnComponent.GetRandomViewerFromQueue();
            }

            queueButtons.y += 26;
            if (Widgets.ButtonText(queueButtons, "Ban Viewer from Queue"))
            {
                Viewer viewer = Viewers.GetViewer(selectedUsername);
                viewer.BanViewer();
            }
        }

        public void GetPawn(PawnQueueSelector method)
        {
            switch (method)
            {
                case PawnQueueSelector.Next:
                    if (pawnIndex + 1 == allColonists.Count)
                    {
                        selectedPawn = allColonists[0];
                        pawnIndex = 0;
                    }
                    else
                    {
                        pawnIndex++;
                        selectedPawn = allColonists[pawnIndex];
                    }
                    break;
                case PawnQueueSelector.Back:
                    if (pawnIndex - 1 < 0)
                    {
                        selectedPawn = allColonists[allColonists.Count - 1];
                        pawnIndex = allColonists.FindIndex(s => s == selectedPawn);
                    }
                    else
                    {
                        pawnIndex--;
                        selectedPawn = allColonists[pawnIndex];
                    }
                    break;
                case PawnQueueSelector.FirstDefault:
                    allColonists = Find.ColonistBar.GetColonistsInOrder();
                    unnamedColonists = GetUnamedColonists();
                    selectedUsername = "";
                    GetPawn(PawnQueueSelector.New);
                    break;
                case PawnQueueSelector.New:
                    if (unnamedColonists.Count > 0)
                    {
                        selectedPawn = unnamedColonists[0];
                    }
                    else
                    {
                        selectedPawn = allColonists[0];
                    }
                    pawnIndex = allColonists.FindIndex(s => s == selectedPawn);
                    break;
            }
        }

        public void NextColonist()
        {
            List<Pawn> colonistsUnnamed = GetUnamedColonists();
            if (colonistsUnnamed.NullOrEmpty())
                    return;

        }

        public List<Pawn> GetUnamedColonists()
        {
            List<Pawn> allColonists = Find.ColonistBar.GetColonistsInOrder();
            List<Pawn> colonistsUnnamed = new List<Pawn>();
            foreach (Pawn pawn in allColonists)
            {
                if (!pawnComponent.pawnHistory.ContainsValue(pawn))
                    colonistsUnnamed.Add(pawn);
            }
            return colonistsUnnamed;
        }

        public void DrawColonist(Rect rect, Pawn colonist)
        {
            GUI.DrawTexture(
                rect, 
                PortraitsCache.Get(
                    colonist, 
                    ColonistBarColonistDrawer.PawnTextureSize, 
                    ColonistBarColonistDrawer.PawnTextureCameraOffset, 
                    1.28205f
                    )
                );
        }

        public void NameColonist(string username, Pawn pawn)
        {
            if (pawnComponent.HasUserBeenNamed(username))
            {
                var pawnRemoved = pawnComponent.pawnHistory[username];
                Name newName;
                if(pawnRemoved.Name is NameTriple nameTri)
                {
                    // Default the name back to the colonist's first name.
                    // We can't know if the colonist had a nickname originally, but this is good enough.
                    newName = new NameTriple(nameTri.First, nameTri.First, nameTri.Last);
                }
                else if (pawnRemoved.Name is NameSingle nameSin)
                {
                    // Shouldn't really ever come up, but not having an option seems like a bad idea.
                    // If the pawn is already registered, it should have been created with a NameTriple.
                    newName = new NameSingle(nameSin.NameWithoutNumber + (nameSin.Number + 1), true);
                }
                else
                {
                    // Dibs (this literally can only happen if someone adds a new Name type)
                    newName = new NameSingle("Delmain");
                }

                pawnRemoved.Name = newName;
                pawnComponent.pawnHistory.Remove(username);
            }

            if (pawnComponent.HasPawnBeenNamed(pawn))
            {
                if (pawnComponent.pawnHistory.ContainsValue(pawn))
                {
                    var usernameRemoved = pawnComponent.pawnHistory.FirstOrDefault(kvp => kvp.Value == pawn).Key;
                    if (usernameRemoved != null)
                        pawnComponent.pawnHistory.Remove(usernameRemoved);
                }
            }

            if(pawn.Name is NameTriple currentName)
                pawn.Name = new NameTriple(currentName.First, username, currentName.Last);
            else
                pawn.Name = new NameSingle(username);
            pawnComponent.AssignUserToPawn(selectedUsername.ToLower(), selectedPawn);
            GetPawn(PawnQueueSelector.FirstDefault);
        }

        public string selectedUsername = "";
        public Pawn selectedPawn = null;
        public int pawnIndex = -1;
        public List<Pawn> allColonists = new List<Pawn>();
        public List<Pawn> unnamedColonists = new List<Pawn>();
        public override Vector2 InitialSize => new Vector2(500f, 500f);
    }

    public enum PawnQueueSelector
    {
        FirstDefault,
        Next,
        Back,
        New
    }
}
