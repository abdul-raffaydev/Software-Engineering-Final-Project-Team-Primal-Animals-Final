using System;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.ViewModels
{
    public class CommentVM
    {
        public string Author { get; set; }      // e.g. "Clinician"
        public string Content { get; set; }
        public DateTime CommentTime { get; set; }
    }
}
