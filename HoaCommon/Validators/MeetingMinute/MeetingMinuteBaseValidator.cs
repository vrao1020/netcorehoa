using FluentValidation;
using HoaEntities.Models.ManipulationModels;

namespace HoaCommon.Validators
{
    public class MeetingMinuteBaseValidator<T> : AbstractValidator<T> where T: MeetingMinuteManipulationDto
    {
        public MeetingMinuteBaseValidator()
        {
            RuleFor(minute => minute.FileName).NotNull().WithMessage("File Name cannot be null")
                .Length(1, 50)
                .Matches(@"[a-zA-Z0-9]+.(pdf|doc|docx|txt)")
                .WithMessage("The file should be a text file, pdf, or word document");
        }
    }
}
