namespace SearchTool_ServerSide.Models
{
    public class FeedbackFormEntry
    {
        public int Id { get; set; }
        public string FormTitle { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public List<SectionEntry> Sections { get; set; }
    }

    public class SectionEntry
    {
        public int Id { get; set; }
        public string SectionTitle { get; set; }
        public int FeedbackFormEntryId { get; set; }
        public FeedbackFormEntry FeedbackForm { get; set; }

        public List<QuestionEntry> Questions { get; set; }
    }

    public class QuestionEntry
    {
        public int Id { get; set; }
        public string QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string Type { get; set; }
        public string TextAnswer { get; set; }
        public string SelectedAnswersJson { get; set; } // stored as JSON array

        public int SectionEntryId { get; set; }
        public SectionEntry Section { get; set; }
    }
    public class FeedbackFormResponse
    {
        public string FormTitle { get; set; }
        public List<SectionResponse> Sections { get; set; }
    }

    public class SectionResponse
    {
        public string SectionTitle { get; set; }
        public List<QuestionResponse> Questions { get; set; }
    }

    public class QuestionResponse
    {
        public string QuestionId { get; set; }
        public string QuestionText { get; set; }
        public QuestionType Type { get; set; }
        public List<string> SelectedAnswers { get; set; }
        public string TextAnswer { get; set; }
    }

    public enum QuestionType
    {
        SingleChoice,
        MultipleChoice,
        ShortAnswer,
        Paragraph
    }
}