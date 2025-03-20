using System;

namespace KumanoKodo.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string AudioUrl { get; set; }
        public bool IsCompleted { get; set; }
    }
} 