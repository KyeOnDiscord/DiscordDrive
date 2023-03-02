namespace DiscordDrive;

internal class MessageModal
{
    public class Root
    {
        public string id { get; set; }
        public int type { get; set; }
        public string content { get; set; }
        public string channel_id { get; set; }
        public Author author { get; set; }
        public Attachment[] attachments { get; set; }
        public object[] embeds { get; set; }
        public object[] mentions { get; set; }
        public object[] mention_roles { get; set; }
        public bool pinned { get; set; }
        public bool mention_everyone { get; set; }
        public bool tts { get; set; }
        public DateTime timestamp { get; set; }
        public object edited_timestamp { get; set; }
        public int flags { get; set; }
        public object[] components { get; set; }
        public string webhook_id { get; set; }
    }

    public class Author
    {
        public bool bot { get; set; }
        public string id { get; set; }
        public string username { get; set; }
        public object avatar { get; set; }
        public string discriminator { get; set; }
    }

    public class Attachment
    {
        public string id { get; set; }
        public string filename { get; set; }
        public int size { get; set; }
        public string url { get; set; }
        public string proxy_url { get; set; }
    }

}
