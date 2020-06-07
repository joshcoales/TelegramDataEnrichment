﻿using System.Collections.Generic;
using DreadBot;
using TelegramDataEnrichment.Sessions;

namespace TelegramDataEnrichment
{
    public abstract class Menu
    {
        protected abstract string Text();
        protected abstract InlineKeyboardMarkup Keyboard();

        public void SendReply(long chatId, long messageId)
        {
            Methods.sendReply(chatId, messageId, Text(), keyboard: Keyboard());
        }

        public void EditMessage(long chatId, long messageId)
        {
            Methods.editMessageText(chatId, messageId, Text(), keyboard: Keyboard());
        }
    }

    internal class UnknownMenu : Menu
    {
        private readonly string _callbackData;

        public UnknownMenu(string callbackData)
        {
            _callbackData = callbackData;
        }

        protected override string Text()
        {
            return $"I do not understand this callback data yet: {_callbackData}";
        }

        protected override InlineKeyboardMarkup Keyboard()
        {
            var keyboard = new InlineKeyboardMarkup();
            keyboard.addCallbackButton("🔙", RootMenu.CallbackName, 0);
            return keyboard;
        }
    }

    internal class RootMenu : Menu
    {
        private readonly List<EnrichmentSession> _sessions;
        private readonly bool _creationInProgress;
        public const string CallbackName = "menu";

        public RootMenu(List<EnrichmentSession> sessions, bool creationInProgress)
        {
            _sessions = sessions;
            _creationInProgress = creationInProgress;
        }

        protected override string Text()
        {
            var activeSessions = _sessions.FindAll(s => s.IsActive).Count;
            var text = "Welcome to the enrichment system menu.\n";
            if (_creationInProgress)
            {
                text += "Cancelled session creation.\n";
            }

            text += $"There are {_sessions.Count} configured sessions, and {activeSessions} are active.";
            return text;
        }

        protected override InlineKeyboardMarkup Keyboard()
        {
            var keyboard = new InlineKeyboardMarkup();
            keyboard.addCallbackButton("Create new session", CreateSessionMenu.CallbackName, 0);
            keyboard.addCallbackButton("Start session", StartSessionMenu.CallbackName, 1);
            keyboard.addCallbackButton("End session", StopSessionMenu.CallbackName, 2);
            keyboard.addCallbackButton("Delete session", DeleteSessionMenu.CallBackName, 3);
            return keyboard;
        }
    }

    internal class StartSessionMenu : Menu
    {
        private readonly List<EnrichmentSession> _sessions;
        public const string CallbackName = "session_start";

        public StartSessionMenu(List<EnrichmentSession> sessions)
        {
            _sessions = sessions;
        }

        protected override string Text()
        {
            return "Which enrichment session would you like to start?";
        }

        protected override InlineKeyboardMarkup Keyboard()
        {
            var inactiveSessions = _sessions.FindAll(s => !s.IsActive);
            var keyboard = new InlineKeyboardMarkup();
            var row = 0;
            foreach (var session in inactiveSessions)
            {
                keyboard.addCallbackButton(session.Name, $"{CallbackName}:{session.Id}", row++);
            }

            keyboard.addCallbackButton("🔙", RootMenu.CallbackName, row);
            return keyboard;
        }
    }

    internal class CreateSessionMenu : Menu
    {
        public const string CallbackName = "session_create";

        protected override string Text()
        {
            return "Creating a new session.\nWhat would you like to name the session?";
        }

        protected override InlineKeyboardMarkup Keyboard()
        {
            return null;
        }
    }

    internal class CreateSessionBatchSizeMenu : Menu
    {
        public const string CallbackName = "session_c_batch";

        protected override string Text()
        {
            return "How many data points should it post at once?";
        }

        protected override InlineKeyboardMarkup Keyboard()
        {
            var keyboard = new InlineKeyboardMarkup();
            keyboard.addCallbackButton("1", $"{CallbackName}:1", 0);
            keyboard.addCallbackButton("3", $"{CallbackName}:3", 0);
            keyboard.addCallbackButton("5", $"{CallbackName}:5", 0);
            keyboard.addCallbackButton("10", $"{CallbackName}:10", 0);
            return keyboard;
        }
    }

    internal class SessionCreatedMenu : Menu
    {
        private readonly EnrichmentSession _newSession;

        public SessionCreatedMenu(EnrichmentSession newSession)
        {
            _newSession = newSession;
        }

        protected override string Text()
        {
            return $"New session has been created: {_newSession.Name}";
        }

        protected override InlineKeyboardMarkup Keyboard()
        {
            var keyboard = new InlineKeyboardMarkup();
            keyboard.addCallbackButton("🔙 to menu", RootMenu.CallbackName, 0);
            return keyboard;
        }
    }

    internal class SessionStartedMenu : Menu
    {
        protected override string Text()
        {
            return "Session started.";
        }

        protected override InlineKeyboardMarkup Keyboard()
        {
            return null;
        }
    }

    internal class NoMatchingSessionMenu : Menu
    {
        private readonly string _sessionId;

        public NoMatchingSessionMenu(string sessionId)
        {
            _sessionId = sessionId;
        }

        protected override string Text()
        {
            return $"No session matching the id: {_sessionId}";
        }

        protected override InlineKeyboardMarkup Keyboard()
        {
            var keyboard = new InlineKeyboardMarkup();
            keyboard.addCallbackButton("🔙 to menu", RootMenu.CallbackName, 0);
            return keyboard;
        }
    }

    internal class StopSessionMenu : Menu
    {
        private readonly List<EnrichmentSession> _sessions;
        public const string CallbackName = "stop_session";

        public StopSessionMenu(List<EnrichmentSession> sessions)
        {
            _sessions = sessions;
        }

        protected override string Text()
        {
            return "Which enrichment session would you like to stop?";
        }

        protected override InlineKeyboardMarkup Keyboard()
        {
            var activeSessions = _sessions.FindAll(s => s.IsActive);
            var keyboard = new InlineKeyboardMarkup();
            var row = 0;
            foreach (var session in activeSessions)
            {
                keyboard.addCallbackButton(session.Name, $"{CallbackName}:{session.Id}", row++);
            }

            keyboard.addCallbackButton("🔙", RootMenu.CallbackName, row);
            return keyboard;
        }
    }

    internal class SessionStoppedMenu : Menu
    {
        protected override string Text()
        {
            return "Session ended.";
        }

        protected override InlineKeyboardMarkup Keyboard()
        {
            var keyboard = new InlineKeyboardMarkup();
            keyboard.addCallbackButton("🔙 to menu", RootMenu.CallbackName, 0);
            return keyboard;
        }
    }

    internal class DeleteSessionMenu : Menu
    {
        private readonly List<EnrichmentSession> _sessions;
        public const string CallBackName = "session_delete";

        public DeleteSessionMenu(List<EnrichmentSession> sessions)
        {
            _sessions = sessions;
        }

        protected override string Text()
        {
            return "Which session did you want to delete?";
        }

        protected override InlineKeyboardMarkup Keyboard()
        {
            var keyboard = new InlineKeyboardMarkup();
            var row = 0;
            foreach (var session in _sessions)
            {
                keyboard.addCallbackButton(session.Name, $"{CallBackName}:{session.Id}", row++);
            }

            keyboard.addCallbackButton("🔙 to menu", RootMenu.CallbackName, row);
            return keyboard;
        }
    }

    internal class DeleteSessionConfirmMenu : Menu
    {
        private readonly EnrichmentSession _session;

        public DeleteSessionConfirmMenu(EnrichmentSession session)
        {
            _session = session;
        }

        protected override string Text()
        {
            return $"Are you sure you want to delete the session: {_session.Name}?";
        }

        protected override InlineKeyboardMarkup Keyboard()
        {
            var keyboard = new InlineKeyboardMarkup();
            keyboard.addCallbackButton(
                "Yes, delete the session.",
                $"{DeleteSessionConfirmedMenu.CallbackName}:{_session.Id}",
                0
            );
            keyboard.addCallbackButton("No, go back to menu 🔙", RootMenu.CallbackName, 1);
            return keyboard;
        }
    }

    internal class DeleteSessionConfirmedMenu : Menu
    {
        public const string CallbackName = "session_delete_conf";

        protected override string Text()
        {
            return "Deleted session.";
        }

        protected override InlineKeyboardMarkup Keyboard()
        {
            var keyboard = new InlineKeyboardMarkup();
            keyboard.addCallbackButton("🔙 to menu", RootMenu.CallbackName, 0);
            return keyboard;
        }
    }
}