using FluentValidation;
using HoaEntities.Models.ManipulationModels;
using System;

namespace HoaCommon.Validators
{
    public class BoardMeetingBaseValidator<T> : AbstractValidator<T> where T : BoardMeetingManipulationDto
    {
        public BoardMeetingBaseValidator()
        {
            RuleFor(manipulationDto => manipulationDto.Title).NotNull().WithMessage("Title cannot be null")
              .Length(1, 50);

            RuleFor(manipulationDto => manipulationDto.Description).NotNull().WithMessage("Description cannot be null")
                  .Length(1, 1000);

            RuleFor(manipulationDto => manipulationDto.ScheduledTime).NotNull().WithMessage("Scheduled Time cannot be null")
                  .Must(date => date >= DateTime.Now).WithMessage("Scheduled Time cannot be in the past");

            RuleFor(manipulationDto => manipulationDto.ScheduledLocation).NotNull().WithMessage("Scheduled Location cannot be null")
                  .Length(1, 300);
        }
    }
}
