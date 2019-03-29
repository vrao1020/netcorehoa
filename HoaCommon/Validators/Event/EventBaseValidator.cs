using FluentValidation;
using HoaEntities.Models.ManipulationModels;
using System;

namespace HoaCommon.Validators
{
    public class EventBaseValidator<T> : AbstractValidator<T> where T : EventManipulationDto
    {
        public EventBaseValidator()
        {
            RuleFor(manipulationDto => manipulationDto.Title).NotNull().WithMessage("Title cannot be null")
              .Length(1, 50);

            RuleFor(manipulationDto => manipulationDto.Message).NotNull().WithMessage("Message cannot be null")
                  .Length(1, 1000);

            RuleFor(manipulationDto => manipulationDto.ScheduledTime).NotNull().WithMessage("Scheduled Time cannot be null")
                  .Must(date => date.Value >= DateTime.Now).WithMessage("Scheduled Time cannot be in the past");            
        }
    }
}
