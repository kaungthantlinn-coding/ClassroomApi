using Classroom.Dtos.Submission;
using Classroom.Services.Interface;
using FluentValidation;
using System;
using System.Threading.Tasks;

namespace Classroom.Examples
{
    // This is an example class demonstrating how to use FluentValidation in service methods
    // It is not meant to be used in production, just for reference
    public class ValidationExample
    {
        private readonly IValidationService _validationService;
        private readonly IValidator<CreateSubmissionDto> _createSubmissionValidator;

        public ValidationExample(
            IValidationService validationService,
            IValidator<CreateSubmissionDto> createSubmissionValidator)
        {
            _validationService = validationService;
            _createSubmissionValidator = createSubmissionValidator;
        }

        // Example 1: Using ValidationService to validate and get validation result
        public async Task<bool> ExampleValidateWithResult(CreateSubmissionDto dto)
        {
            // Get validation result
            var validationResult = _validationService.ValidateCreateSubmission(dto);

            if (!validationResult.IsValid)
            {
                // Handle validation errors
                foreach (var error in validationResult.Errors)
                {
                    Console.WriteLine($"Validation error: {error.PropertyName} - {error.ErrorMessage}");
                }
                return false;
            }

            // Continue with business logic if validation passes
            return true;
        }

        // Example 2: Using ValidationService to validate and throw exception
        public async Task ExampleValidateAndThrow(CreateSubmissionDto dto)
        {
            try
            {
                // This will throw ValidationException if validation fails
                _validationService.ValidateAndThrow(dto, _createSubmissionValidator);

                // Continue with business logic if validation passes
            }
            catch (ValidationException ex)
            {
                // Handle validation exception
                foreach (var error in ex.Errors)
                {
                    Console.WriteLine($"Validation error: {error.PropertyName} - {error.ErrorMessage}");
                }
                throw; // Re-throw or handle as needed
            }
        }
    }
}