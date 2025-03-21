using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using KumanoKodo.Services;
using KumanoKodo.Models;

namespace KumanoKodo.Tests
{
    public class PdfExtractionServiceTests
    {
        private readonly Mock<IDataAccess> _mockDataAccess;
        private readonly Mock<ILogger<PdfExtractionService>> _mockLogger;
        private readonly string _testPdfDirectory;

        public PdfExtractionServiceTests()
        {
            _mockDataAccess = new Mock<IDataAccess>();
            _mockLogger = new Mock<ILogger<PdfExtractionService>>();
            _testPdfDirectory = Path.Combine(
                Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.FullName,
                "Marugoto",
                "A1 Level"
            );
        }

        [Fact]
        public async Task ExtractVocabularyAsync_WithValidPdf_ExtractsCorrectData()
        {
            // Arrange
            var service = new PdfExtractionService(_testPdfDirectory, _mockDataAccess.Object, _mockLogger.Object);
            var extractedVocabulary = new List<VocabularyData>();
            var insertedLessonIds = new HashSet<int>();

            _mockDataAccess.Setup(x => x.InsertLessonAsync(It.IsAny<int>(), null, null))
                .Callback<int, string?, string?>((id, _, _) => insertedLessonIds.Add(id))
                .Returns(Task.CompletedTask);

            _mockDataAccess.Setup(x => x.InsertVocabularyAsync(It.IsAny<List<VocabularyData>>()))
                .Callback<List<VocabularyData>>(vocabulary => extractedVocabulary = vocabulary)
                .Returns(Task.CompletedTask);

            // Act
            await service.ExtractVocabularyAsync();

            // Assert
            Assert.NotEmpty(extractedVocabulary);
            Assert.All(extractedVocabulary, v => Assert.NotNull(v.Pronunciation));
            Assert.All(extractedVocabulary, v => Assert.NotNull(v.Romaaji));
            Assert.All(extractedVocabulary, v => Assert.NotNull(v.Meaning));
            Assert.All(extractedVocabulary, v => Assert.True(v.LessonId > 0));
            Assert.All(extractedVocabulary, v => Assert.Contains(v.LessonId, insertedLessonIds));
        }

        [Fact]
        public async Task ExtractVocabularyAsync_WithInvalidPdf_ThrowsFileNotFoundException()
        {
            // Arrange
            var invalidDirectory = Path.Combine(_testPdfDirectory, "Invalid");
            var service = new PdfExtractionService(invalidDirectory, _mockDataAccess.Object, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => service.ExtractVocabularyAsync());
        }
    }
} 