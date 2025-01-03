using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class EventLog : MonoBehaviour
{
    public TMP_Text logText; // Assign your Text or TextMeshPro component
    private Queue<LogEntry> logQueue = new Queue<LogEntry>();
    private const int maxLogEntries = 8; // Number of events to display
    private const float logExpirationTime = 10f; // Time in seconds before a log expires

    // Define the LogEntry structure
    private class LogEntry
    {
        public string message;
        public float timestamp;

        public LogEntry(string message)
        {
            this.message = message;
            this.timestamp = Time.time;
        }
    }

    public void Update()
    {
        // Remove expired logs
        RemoveExpiredLogs();

        // Update the displayed text
        logText.text = string.Join("\n", GetLogMessages());
    }

    public void AddLog(string message, Color color)
    {
        // Convert the color to a hex string
        string colorHex = ColorUtility.ToHtmlStringRGB(color);

        // Format the message with the specified color
        string coloredMessage = $"<color=#{colorHex}>{message}</color>";

        // Create a new LogEntry
        LogEntry newLog = new LogEntry(coloredMessage);

        // Add the new message to the queue
        logQueue.Enqueue(newLog);

        // If the queue exceeds the max entries, remove the oldest
        if (logQueue.Count > maxLogEntries)
        {
            logQueue.Dequeue();
        }

        // Remove expired logs
        RemoveExpiredLogs();

        // Update the displayed text
        logText.text = string.Join("\n", GetLogMessages());
    }

    private void RemoveExpiredLogs()
    {
        // Remove logs that have exceeded the expiration time
        while (logQueue.Count > 0 && Time.time - logQueue.Peek().timestamp > logExpirationTime)
        {
            logQueue.Dequeue();
        }
    }

    private IEnumerable<string> GetLogMessages()
    {
        // Return the messages from the log queue
        foreach (var log in logQueue)
        {
            yield return log.message;
        }
    }
}
