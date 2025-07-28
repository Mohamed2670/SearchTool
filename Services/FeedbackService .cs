using System.Globalization;
using System.Text.Json;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using SearchTool_ServerSide.Data;
using SearchTool_ServerSide.Models;
public class FeedbackResponseDto
{
    public string FormTitle { get; set; }
    public string? UserEmail { get; set; } // Optional, if you want to track who submitted the feedback
    public DateTime SubmittedAt { get; set; }
    public List<SectionDto> Sections { get; set; }
}

public class SectionDto
{
    public string SectionTitle { get; set; }
    public List<QuestionDto> Questions { get; set; }
}

public class QuestionDto
{
    public string QuestionId { get; set; }
    public string QuestionText { get; set; }
    public string Type { get; set; }
    public string TextAnswer { get; set; }
    public List<string> SelectedAnswers { get; set; }
}

public class FeedbackService
{
    private readonly SearchToolDBContext _context;

    public FeedbackService(SearchToolDBContext context)
    {
        _context = context;
    }

    public async Task SaveFeedbackAsync(FeedbackFormResponse model)
    {
        var entry = new FeedbackFormEntry
        {
            FormTitle = model.FormTitle,
            Sections = model.Sections.Select(section => new SectionEntry
            {
                SectionTitle = section.SectionTitle,

                Questions = section.Questions.Select(q => new QuestionEntry
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    Type = q.Type.ToString(),
                    TextAnswer = q.TextAnswer,
                    SelectedAnswersJson = JsonSerializer.Serialize(q.SelectedAnswers ?? new List<string>())
                }).ToList()
            }).ToList(),
            UserEmail = model.UserEmail // Set the email from the model
        };

        _context.FeedbackForms.Add(entry);
        await _context.SaveChangesAsync();
    }
    public async Task<IEnumerable<FeedbackResponseDto>> GetAllFeedbackAsync()
    {
        var entries = await _context.FeedbackForms
            .Include(f => f.Sections)
                .ThenInclude(s => s.Questions)
            .OrderByDescending(f => f.SubmittedAt)
            .ToListAsync();

        var result = entries.Select(f => new FeedbackResponseDto
        {
            FormTitle = f.FormTitle,
            SubmittedAt = f.SubmittedAt,
            UserEmail = f.UserEmail,
            Sections = f.Sections.Select(s => new SectionDto
            {
                SectionTitle = s.SectionTitle,
                Questions = s.Questions.Select(q => new QuestionDto
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    Type = q.Type,
                    TextAnswer = q.TextAnswer,
                    SelectedAnswers = string.IsNullOrEmpty(q.SelectedAnswersJson)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(q.SelectedAnswersJson)
                }).ToList()
            }).ToList()
        });

        return result;
    }
    public async Task<byte[]> ExportFeedbackAsCsvAsync()
    {
        var feedbacks = await _context.FeedbackForms
            .Include(f => f.Sections)
                .ThenInclude(s => s.Questions)
            .OrderByDescending(f => f.SubmittedAt)
            .ToListAsync();

        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteField("SubmittedAt");
        csv.WriteField("FormTitle");
        csv.WriteField("User Email");
        csv.WriteField("Section");
        csv.WriteField("Question");
        csv.WriteField("Type");
        csv.WriteField("TextAnswer");
        csv.WriteField("SelectedAnswers");
        csv.NextRecord();

        foreach (var form in feedbacks)
        {
            foreach (var section in form.Sections)
            {
                foreach (var question in section.Questions)
                {
                    var selectedAnswers = string.IsNullOrEmpty(question.SelectedAnswersJson)
                        ? ""
                        : string.Join(" | ", JsonSerializer.Deserialize<List<string>>(question.SelectedAnswersJson));

                    csv.WriteField(form.SubmittedAt.ToString("s"));
                    csv.WriteField(form.FormTitle);
                    csv.WriteField(form.UserEmail);
                    csv.WriteField(section.SectionTitle);
                    csv.WriteField(question.QuestionText);
                    csv.WriteField(question.Type);
                    csv.WriteField(question.TextAnswer ?? "");
                    csv.WriteField(selectedAnswers);
                    csv.NextRecord();
                }
            }
        }

        writer.Flush();
        return ms.ToArray();
    }
}
