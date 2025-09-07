namespace Pawfect_Messenger.Data.Entities.Types.Authorisation
{
    public static class Permission
    {
        // Users
        public const String CanViewUsers = "CanViewUsers";
        public const String CreateUsers = "CreateUsers";
        public const String BrowseUsers = "BrowseUsers";
        public const String EditUsers = "EditUsers";
        public const String DeleteUsers = "DeleteUsers";

        // Shelters
        public const String CanViewShelters = "CanViewShelters";
        public const String BrowseShelters = "BrowseShelters";

        // Conversations
        public const String CanViewConversations = "CanViewConversations";
        public const String CreateConversations = "CreateConversations";
        public const String BrowseConversations = "BrowseConversations";
        public const String EditConversations = "EditConversations";
        public const String DeleteConversations = "DeleteConversations";

        // Messages
        public const String CanViewMessages = "CanViewMessages";
        public const String CreateMessages = "CreateMessages";
        public const String BrowseMessages = "BrowseMessages";
        public const String EditMessages = "EditMessages";
        public const String DeleteMessages = "DeleteMessages";

        // Files
        public const String CanViewFiles = "CanViewFiles";
        public const String CreateFiles = "CreateFiles";
        public const String BrowseFiles = "BrowseFiles";
        public const String EditFiles = "EditFiles";
        public const String DeleteFiles = "DeleteFiles";
    }
}