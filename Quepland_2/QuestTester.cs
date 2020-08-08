using System;
using System.Collections.Generic;

public class QuestTester
{
	public Quest CurrentQuest;
    public List<Node> DialogTree;
    public List<Dialog> UsedDialogs;

	public void TestQuests()
    {
        foreach(Quest quest in QuestManager.Instance.Quests)
        {
            IsCompletable(quest);
        }
    }



    public void BuildTree(Quest quest)
    {
        DialogTree = new List<Node>();
        UsedDialogs = new List<Dialog>();
        CurrentQuest = quest;
        if (CurrentQuest != null)
        {          
            for (int i = 0; i < CurrentQuest.ProgressToComplete; i++)
            {
                UsedDialogs = new List<Dialog>();
                foreach (NPC npc in NPCManager.Instance.NPCs)
                {
                    foreach (Dialog d in npc.Dialogs)
                    {
                        if (d.Quest == CurrentQuest.Name)
                        {
                            foreach (Requirement req in d.Requirements)
                            {
                                if (req.Quest == CurrentQuest.Name)
                                {
                                    if (i >= req.MinimumQuestProgress && i <= req.MaximumQuestProgress)
                                    {
                                        if (UsedDialogs.Contains(d) == false)
                                        {
                                            DialogTree.Add(new Node(d, CurrentQuest));
                                            UsedDialogs.Add(d);
                                            continue;
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }

        }
        /*
        foreach(Node n in DialogTree)
        {
            Console.WriteLine("Parent:" + n.Parent.NewQuestProgressValue);
            Console.Write("Children:");
            foreach(Dialog d in n.Children)
            {             
                Console.Write(d.NewQuestProgressValue + ", ");
            }
            Console.WriteLine();
        }*/
    }
    public bool IsCompletable(Quest quest)
    {
        int progress = 0;
        BuildTree(quest);
        foreach(Node n in DialogTree)
        {
            progress = Math.Max(progress, n.Parent.NewQuestProgressValue);
        }
        if(progress == quest.ProgressToComplete)
        {
            return true;
        }
        Console.WriteLine("Quest:" + quest.Name + " soft locks at progress value:" + progress + ".");
        foreach(NPC npc in NPCManager.Instance.NPCs)
        {
            foreach(Dialog d in npc.Dialogs)
            {
                if(d.Quest == quest.Name && d.NewQuestProgressValue == progress)
                {
                    Console.WriteLine("NPC:" + npc.Name + ", Button Text:" + d.ButtonText);
                }
            }
        }
        return false;
    }

    public class Node
    {
        public Node(Dialog parent, Quest CurrentQuest)
        {
            Parent = parent;
            foreach(NPC npc in NPCManager.Instance.NPCs)
            {
                foreach(Dialog d in npc.Dialogs)
                {
                    if (d.Quest == CurrentQuest.Name)
                    {
                        foreach (Requirement req in d.Requirements)
                        {
                            if (req.Quest == CurrentQuest.Name)
                            {
                                if (req.MinimumQuestProgress >= Parent.NewQuestProgressValue && req.MaximumQuestProgress <= Parent.NewQuestProgressValue)
                                {
                                    Children.Add(d);
                                    //Console.WriteLine("Adding child:" + d.ButtonText + " to parent:" + Parent.ButtonText);
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
        }
        public Dialog Parent { get; set; }
        public List<Dialog> Children { get; set; } = new List<Dialog>();
    }
}
