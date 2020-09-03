using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

public static class SwampMaze
{
	public static List<List<string>> Squares = new List<List<string>>();
    public static List<string> OpenMessages = new List<string>()
    {
        "You climb over sunken tree trunks and overgrown root systems to arrive even deeper in the swamp.",
        "As you walk you hear the howls of some strange creature, before they suddenly disappear into the bog.",
        "You cross a number of massive lilypads sitting on the water, being sure to avoid the one that seems to be breathing.",
        "The only way forward is through the water. You wade through slowly, occasionally you feel something rub against your leg in the water.",
        "The swamp is light here and travel is easy, for a moment.",
        "You carefully step over tree roots and mossy slime as you continue into the swamp.",
        "The swamp water bubbles ominously here, you decide to take a slightly longer path around.",
        "There's a small patch of welcome dry ground here. Well, drier than most of the bog.",
        "You approach a group of impassable trees. Strange birds screech as you get near. Despite this, you manage to work your way around.",
        "The swamp here grows very deep. You have to stand on your toes to avoid going completely under. If only you knew how to swim...",
        "You wade through the waters of the swamp. A strange smell fills your nostrils as you pass by an undulating tree.",
        "You can barely make out your way forward as the swamp grows very dark. Regardless, you continue forward.",
        "Something grabs your foot as you wade through the water. It pulls you down but you manage to slip away just in time.",
        "Here and there you notice a kappa sitting in the swamp, watching you as you pass by.",
        "You hear the sound of two creatures fighting up in a tree near you, followed by a splash as they tumble into the water below. The struggle abruptly ends in silence.",
        "Every time you turn around you can't help but feel the swamp has changed behind you. Those vines weren't like that before, were they?",
        "A swarm of bugs swirls around a rather large tree in the way in front of you. You stay low to the swamp water and pass by unharmed.",
        "You struggle over branches and through bushes to continue deeper into the swamp.",
        "An extremely old bridge sits in the path ahead. It leads nowhere but it's a welcome reprieve from walking through swamp water.",
        "You pass by a number of trees with odd markings on them. It almost looks like a human did this.",
        "The swamp grows light as you approach a clearing. There's only swamp in every direction, but the sunlight is nice.",
        "You pass by a tree and do a double take. Didn't you just pass by the same tree a minute ago?",
        "The ground is so soft it sucks your feet right in. You grab onto vines from the nearby tree and drag yourself along.",
        "The trees are so thick that you need to crawl between gaps in the roots. It's a tight fit but you manage to make it through.",
        "You hear screeching in the distance as you continue your way through the swamp.",
        "You can't help but feel a sense of deja vu as you reach a small mound in the swamp.",
        "As you continue forward it seems like the trees ahead of you are moving. But when you get close, it appears it was just an illusion.",
        "Some strange plants line the roots of the trees here. Once in a while they shoot a puff of smelly brown powder into the air.",
        "You stumble across a swamp hand, out of the water, consuming something. You take a long way around.",
        "A jumble of swamp snakes cover the roots of the trees, here. It's impossible to tell what's tree and what's snake."

    };
    public static List<string> TeleportMessages = new List<string>()
    {
        "The vines entangle you and by the time you struggle free you find yourself in a wildly different place than you thought.",
        "As you walk you hear the howls of some strange creature and turn around. When you turn back you feel like you're somewhere different.",
        "You cross a number of massive lilypads sitting on the water, but fall in on the last one. When you surface your surroundings have completely changed.",
        "The swamp is light here and travel is easy, for a moment, or so you think. When you continue forward you find you're back where you've already been.",
        "You carefully step over tree roots and mossy slime as you continue into the swamp. Or you try to, but slip and hit your head. When you get back up, the swamp has changed.",
        "The swamp water bubbles ominously here, you decide to take a slightly longer path around, but you get lost. A few minutes later, you find yourself somewhere completey different.",
        "You approach a group of impassable trees. Strange birds screech as you get near. You try to work your way around but lose your way. You have no idea where you are now.",
        "You can barely make out your way forward as the swamp grows very dark. Regardless, you continue forward, only to find you have lost any sense of direction and end up somwhere else.",
        "Every time you turn around you can't help but feel the swamp has changed behind you. Those vines weren't like that before, were they? You turn ahead to see the swamp has changed ahead of you as well.",
        "A swarm of bugs swirls around a rather large tree in the way in front of you. You try to stay low to the swamp water and pass by but you trip and fall. When you stand back up the bugs are gone, and so is the tree. Where are you?",
        "An old bridge sits in the path ahead. It's a welcome reprieve from walking through swamp water. You cross and step down you lose sight of the bridge. Did you imagine it? The swamp ahead looks completely different.",
        "You pass by a tree and do a double take. This is tree is from a far different place than you thought you were.",
        "The trees are so thick that you need to crawl between gaps in the roots. It's a tight fit but you manage to make it through. On the other side you turn around to see no sign of the trees you crawled through.",
        "You can't help but feel a sense of deja vu as you reach a small mound in the swamp. You're not where you thought you were.",
        "You stumble across a swamp hand, out of the water, consuming something. You take a long way around, but get lost. You end up somewhere else, entirely disoriented."

    };
    private static int currentMessage;
    public static int CurrentMessage { get
        {
            return currentMessage;
        }
        set
        {
            currentMessage = value;
            if(currentMessage >= OpenMessages.Count)
            {
                currentMessage = 0;
            }
        }
    }
    private static int currentTeleportMessage;
    public static int CurrentTeleportMessage
    {
        get
        {
            return currentTeleportMessage;
        }
        set
        {
            currentTeleportMessage = value;
            if (currentTeleportMessage >= TeleportMessages.Count)
            {
                currentTeleportMessage = 0;
            }
        }
    }
    public static bool MazeLoaded = false;
	public static void LoadMaze()
    {
        if(MazeLoaded == false)
        {
            OpenMessages.Shuffle();
            TeleportMessages.Shuffle();
            Squares.AddRange(new[]
            {
                new List<string>
                {
                    "Open", "Open", "Open", "Blocked", "Blocked", "Exit", "Teleport:6,2", "Open", "Open", "Open"
                },
                new List<string>
                {
                    "Blocked", "Teleport:1,4", "Open", "Blocked", "Open", "Open", "Open", "Teleport:4,1", "Open", "Open"
                },                
                new List<string>
                {
                    "Blocked", "Open", "Open", "Teleport:3,4", "Open", "Teleport:2,1", "Open", "Blocked", "Open", "Open"
                },                
                new List<string>
                {
                    "Exit", "Open", "Exit", "Teleport:3,4", "Open", "Exit", "Open", "Blocked", "Open", "Open"
                },
                new List<string>
                {
                    "Blocked", "Open", "Open", "Open", "Open", "Blocked", "Open", "Exit", "Open", "Open"
                },
                new List<string>
                {
                    "Open", "Teleport:1,2", "Blocked", "Blocked", "Exit", "Blocked", "Open", "Blocked", "Blocked", "Blocked"
                },
                new List<string>
                {
                    "Blocked", "Open", "Open", "Open", "Open", "Open", "Open", "Teleport:6,1", "Open", "Sawoka"
                },
                new List<string>
                {
                    "Exit", "Open", "Blocked", "Exit", "Teleport:3,4", "Exit", "Blocked", "Teleport:1,8", "Open", "Blocked"
                },
                new List<string>
                {
                    "Blocked", "Open", "Open", "Open", "Open", "Open", "Open", "Open", "Open", "Teleport:6,1"
                },
                new List<string>
                {
                    "Open", "Blocked", "Blocked", "Blocked", "Teleport:3,8", "Blocked", "Blocked", "Exit", "Blocked", "Open"
                },
            });
            MazeLoaded = true;
        }
    }

    private static Random rng = new Random();

    private static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

}
