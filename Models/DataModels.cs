namespace KumanoKodo.Models
{
    public class VocabularyData
    {
        public string? Kanji { get; set; }
        public required string Pronunciation { get; set; }
        public required string Romaaji { get; set; }
        public required string Meaning { get; set; }
        public required int LessonId { get; set; }
    }

    public class SentenceData
    {
        public required string Kanji { get; set; }
        public required string Pronunciation { get; set; }
        public required string Romaaji { get; set; }
        public required string Translation { get; set; }
        public required int LessonId { get; set; }
    }

    public class GrammarTopicData
    {
        public required string Title { get; set; }
        public required int LessonId { get; set; }
        public required string PDF_Link { get; set; }
    }
} 