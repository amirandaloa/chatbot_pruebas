using System.Collections.Generic;

namespace EnlaceVA.Models
{
    public class Conversation
    {
        public int ConversationId { get; set; }
        public int ConversationScore { get; set; }

        public string AnalyticScore { get; set; }

        public List<Question> Questions { get; set; }
    }
}
