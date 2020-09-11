using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public static class MessageManager
{
    private static readonly int maxMessages = 100;
    private static List<Message> Messages = new List<Message>();
    private static string lastMessage;
    private static int repeatMessageCount = 1;
    public static string CurrentTag = "All";

    public static void AddMessage(string message)
    {
        AddMessage(message, "white", "All");
    }
    public static void AddMessage(string message, string color)
    {
        AddMessage(message, color, "All");
    }
    public static void AddMessage(string newMessageString, string color, string tag)
    {
        Message newMessage = new Message(newMessageString, color, tag);
        if(Messages.Count > 0)
        {
            Messages.Last().Style = "";
            Messages.Last().Style += "opacity:0.9;";
        }
        
        newMessage.Style = "font-weight:bold;text-decoration:underline;";
        
        if (lastMessage == newMessageString)
        {
            repeatMessageCount++;
            Messages.Last().Text = lastMessage + "(" + repeatMessageCount + ")";
            Messages.Last().Style = "font-weight:bold;text-decoration:underline;";
        }
        else
        {
            repeatMessageCount = 1;
            Messages.Add(newMessage);
            if (Messages.Count >= maxMessages)
            {
                Messages.Remove(Messages[0]);
            }
        }

        lastMessage = newMessage.Text;
    }
    public static List<Message> GetMessages()
    {
        if(CurrentTag == "All")
        {
            return Messages;
        }
        return Messages.Where(x => x.Tag == CurrentTag).ToList();
    }
    public static List<Message> GetReversedMessages()
    {
        if (CurrentTag == "All")
        {
            return Messages.Reverse<Message>().ToList();
        }
        return Messages.Where(x => x.Tag == CurrentTag).Reverse<Message>().ToList();
    }
}

