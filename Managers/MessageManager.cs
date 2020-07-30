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

    public static void AddMessage(string message)
    {
        AddMessage(message, "white");
    }
    public static void AddMessage(string newMessageString, string color)
    {
        Message newMessage = new Message(newMessageString, color);
        if(Messages.Count > 0)
        {
            Messages.Last().Style = "";
        }
        
        newMessage.Style = "font-weight:bold;";
        
        if (lastMessage == newMessageString)
        {
            repeatMessageCount++;
            Messages.Last().Text = lastMessage + "(" + repeatMessageCount + ")";
            Messages.Last().Style = "font-weight:bold;";
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
        return Messages;
    }
    public static List<Message> GetReversedMessages()
    {
        return Messages.Reverse<Message>().ToList();
    }
}

