using System;
using System.Collections.Generic;

namespace EnlaceVA.Models
{
    public class Question
    {
        public string Text { get; set; }

        public List<string> Answers { get; set; }

        public string DispatchIntent { get; set; }

        public double DispatchScore { get; set; }

        public DateTime DateTime { get; set; }

        public string Topic { get; set; }

        public int QuestionId { get; set; }

        public string CompanyId { get; set; }

        public string SubTopic { get; set; }

        public string CompanyName { get; set; }

        public string QuestionType { get; set; }

        public bool WasFromMenu { get; set; } = false;

        public string OptionSubMenu { get; set; }

        public string OptionMenu { get; set; }

        public string DataTreatmentGas { get; set; }

        public string DataTreatmentBrilla { get; set; }

        public GeneralInformation generalInformation { get; set; }

    }
}
