using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using KumanoKodo.Models;
using Microsoft.Extensions.Logging;

namespace KumanoKodo.Services
{
    public class PdfExtractionService
    {
        private readonly string _pdfDirectory;
        private readonly IDataAccess _dataAccess;
        private readonly ILogger<PdfExtractionService> _logger;

        public PdfExtractionService(string pdfDirectory, IDataAccess dataAccess, ILogger<PdfExtractionService> logger)
        {
            _pdfDirectory = pdfDirectory;
            _dataAccess = dataAccess;
            _logger = logger;
        }

        public async Task ExtractVocabularyAsync()
        {
            var vocabularyPath = Path.Combine(_pdfDirectory, "MarugotoStarterActivitiesVocabularyIndex2_EN.pdf");
            if (!File.Exists(vocabularyPath))
            {
                _logger.LogError("Vocabulary PDF not found at: {Path}", vocabularyPath);
                throw new FileNotFoundException("Vocabulary PDF not found", vocabularyPath);
            }

            _logger.LogInformation("Starting vocabulary extraction from: {Path}", vocabularyPath);
            var vocabulary = new List<VocabularyData>();
            var currentLessonId = 0;
            var lineCount = 0;
            var skippedLines = 0;
            var processedLines = 0;

            using var pdfReader = new PdfReader(vocabularyPath);
            using var pdfDocument = new PdfDocument(pdfReader);

            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                _logger.LogDebug("Processing page {PageNumber}", i);
                var page = pdfDocument.GetPage(i);
                var text = PdfTextExtractor.GetTextFromPage(page);
                var lines = text.Split('\n');

                foreach (var line in lines)
                {
                    lineCount++;
                    // Skip empty lines and headers/footers
                    if (string.IsNullOrWhiteSpace(line) || 
                        line.Contains("Marugoto") || 
                        line.Contains("Vocabulary") ||
                        line.Contains("Page"))
                    {
                        skippedLines++;
                        continue;
                    }

                    // Check for lesson ID pattern (e.g., "Lesson 1", "Lesson 2", etc.)
                    var lessonMatch = Regex.Match(line, @"Lesson\s+(\d+)");
                    if (lessonMatch.Success)
                    {
                        currentLessonId = int.Parse(lessonMatch.Groups[1].Value);
                        _logger.LogDebug("Found new lesson ID: {LessonId}", currentLessonId);
                        continue;
                    }

                    // Match vocabulary entries (columns: LessonId, Pronunciation, Romaaji, Meaning)
                    var match = Regex.Match(line, @"(\d+)\s+([^\s]+)\s+([^\s]+)\s+(.+)");
                    if (match.Success)
                    {
                        var lessonId = int.Parse(match.Groups[1].Value);
                        if (lessonId != currentLessonId)
                        {
                            _logger.LogWarning("Lesson ID mismatch: Expected {Expected}, Found {Found}", currentLessonId, lessonId);
                        }

                        vocabulary.Add(new VocabularyData
                        {
                            Kanji = null, // This PDF doesn't provide Kanji
                            Pronunciation = match.Groups[2].Value.Trim(),
                            Romaaji = match.Groups[3].Value.Trim(),
                            Meaning = match.Groups[4].Value.Trim(),
                            LessonId = lessonId
                        });
                        processedLines++;
                    }
                }
            }

            _logger.LogInformation("Extraction complete. Processed {Processed} lines, skipped {Skipped} lines", 
                processedLines, skippedLines);

            // Insert lessons first
            var uniqueLessonIds = vocabulary.Select(v => v.LessonId).Distinct();
            _logger.LogInformation("Found {Count} unique lesson IDs", uniqueLessonIds.Count());

            foreach (var lessonId in uniqueLessonIds)
            {
                await _dataAccess.InsertLessonAsync(lessonId, null, null);
            }

            // Then insert vocabulary
            _logger.LogInformation("Inserting {Count} vocabulary entries", vocabulary.Count);
            await _dataAccess.InsertVocabularyAsync(vocabulary);

            _logger.LogInformation("Vocabulary extraction and insertion completed successfully");
        }

        public async Task ExtractSentencesAsync()
        {
            var sentencesPath = Path.Combine(_pdfDirectory, "MarugotoStarterActivitiesPhraseIndex.pdf");
            if (!File.Exists(sentencesPath))
                throw new FileNotFoundException("Sentences PDF not found", sentencesPath);

            var sentences = new List<SentenceData>();
            using var pdfReader = new PdfReader(sentencesPath);
            using var pdfDocument = new PdfDocument(pdfReader);

            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                var page = pdfDocument.GetPage(i);
                var text = PdfTextExtractor.GetTextFromPage(page);
                var lines = text.Split('\n');

                foreach (var line in lines)
                {
                    var match = Regex.Match(line, @"(.*?)\s+\((.*?)\)\s+\[(.*?)\]\s+(.*?)\s+(\d+)");
                    if (match.Success)
                    {
                        sentences.Add(new SentenceData
                        {
                            Kanji = match.Groups[1].Value.Trim(),
                            Pronunciation = match.Groups[2].Value.Trim(),
                            Romaaji = match.Groups[3].Value.Trim(),
                            Translation = match.Groups[4].Value.Trim(),
                            LessonId = int.Parse(match.Groups[5].Value)
                        });
                    }
                }
            }

            await _dataAccess.InsertSentencesAsync(sentences);
        }

        public async Task ExtractGrammarTopicsAsync()
        {
            var grammarPath = Path.Combine(_pdfDirectory, "MarugotoStarterActivitiesGrammarIndex.pdf");
            if (!File.Exists(grammarPath))
                throw new FileNotFoundException("Grammar PDF not found", grammarPath);

            var grammarTopics = new List<GrammarTopicData>();
            using var pdfReader = new PdfReader(grammarPath);
            using var pdfDocument = new PdfDocument(pdfReader);

            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                var page = pdfDocument.GetPage(i);
                var text = PdfTextExtractor.GetTextFromPage(page);
                var lines = text.Split('\n');

                foreach (var line in lines)
                {
                    var match = Regex.Match(line, @"(.*?)\s+(\d+)\s+(.*)");
                    if (match.Success)
                    {
                        grammarTopics.Add(new GrammarTopicData
                        {
                            Title = match.Groups[1].Value.Trim(),
                            LessonId = int.Parse(match.Groups[2].Value),
                            PDF_Link = match.Groups[3].Value.Trim()
                        });
                    }
                }
            }

            await _dataAccess.InsertGrammarTopicsAsync(grammarTopics);
        }

        private int ExtractLessonIdFromPage(int pageNumber)
        {
            // Map page numbers to lesson IDs based on the PDF structure
            // This is a simplified example - adjust based on actual PDF layout
            return (pageNumber - 1) / 2 + 1;
        }
    }
} 