# FluentValidation Implementation

This directory contains validation classes implemented using FluentValidation for the Classroom API project.

## Overview

FluentValidation is a popular validation library for .NET that uses a fluent interface to construct strongly-typed validation rules. This implementation adds validation to the DTOs used in the Classroom API, ensuring data integrity before it reaches the service layer.

## Validators

### Submission Validators

- **CreateSubmissionValidator**: Validates the `CreateSubmissionDto` ensuring submission content is provided and doesn't exceed maximum length.
- **GradeSubmissionValidator**: Validates the `GradeSubmissionDto` ensuring grade values are within acceptable range (0-100).
- **FeedbackSubmissionValidator**: Validates the `FeedbackSubmissionDto` ensuring feedback is provided and doesn't exceed maximum length.

## Usage

### Controller Level Validation

Validation happens automatically at the controller level due to the `AddFluentValidationAutoValidation()` registration in `Program.cs`. When a request with invalid data is received, the API will return a 400 Bad Request with validation errors.

### Service Level Validation

For additional validation at the service level, inject the `IValidationService` into your service class:

```csharp
private readonly IValidationService _validationService;

public YourService(IValidationService validationService)
{
    _validationService = validationService;
}
```

Then use it to validate DTOs:

```csharp
// Method 1: Get validation result
var validationResult = _validationService.ValidateCreateSubmission(createSubmissionDto);
if (!validationResult.IsValid)
{
    // Handle validation errors
}

// Method 2: Throw exception on validation failure
_validationService.ValidateAndThrow(createSubmissionDto, _createSubmissionValidator);
```

## Adding New Validators

To add validation for a new DTO:

1. Create a new validator class that inherits from `AbstractValidator<YourDto>`
2. Define validation rules in the constructor
3. Register the validator in `Program.cs`
4. Update `ValidationService` and `IValidationService` if service-level validation is needed
