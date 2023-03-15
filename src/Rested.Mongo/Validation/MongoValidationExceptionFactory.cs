using FluentValidation;
using FluentValidation.Results;
using MongoDB.Driver;
using Rested.Core.Validation;
using Rested.Mongo.Data;

namespace Rested.Mongo.Validation
{
    public static class MongoValidationExceptionFactory
    {
        public static void Throw(MongoMultipleException mongoMultipleException, ServiceErrorCodes serviceErrorCodes)
        {
            var validationFailures = new List<ValidationFailure>();
            var databaseIndexViolationError = serviceErrorCodes.CommonErrorCodes.DatabaseIndexViolationError;
            var databaseError = serviceErrorCodes.CommonErrorCodes.DatabaseError;

            foreach (var mongoMultipleExceptionDetail in mongoMultipleException.MongoExceptions)
            {
                if (mongoMultipleExceptionDetail.Exception is MongoWriteException mongoWriteException)
                {
                    var isMongoIndexViolationError = MongoIndexViolationError.TryParse(
                        writeError: mongoWriteException.WriteError,
                        mongoIndexViolationError: out var mongoIndexViolationError);

                    if (isMongoIndexViolationError)
                    {
                        var validationFailure = new ValidationFailure(
                            propertyName: mongoIndexViolationError.IndexName,
                            errorMessage:
                                string.Format(
                                    databaseIndexViolationError.Message,
                                    mongoIndexViolationError.IndexName,
                                    mongoIndexViolationError.IndexValue))
                        {
                            ErrorCode = databaseIndexViolationError.ExtendedStatusCode,
                            AttemptedValue = mongoIndexViolationError.IndexValue
                        };

                        validationFailures.Add(validationFailure);
                    }

                    else
                    {
                        validationFailures.Add(new ValidationFailure()
                        {
                            ErrorCode = databaseError.ExtendedStatusCode,
                            ErrorMessage = string.Format(databaseError.Message, mongoWriteException.WriteError.Message)
                        });
                    }
                }

                else
                {
                    validationFailures.Add(new ValidationFailure()
                    {
                        ErrorCode = databaseError.ExtendedStatusCode,
                        ErrorMessage = string.Format(databaseError.Message, mongoMultipleExceptionDetail.Exception.Message)
                    });
                }
            }

            throw new ValidationException(
                message: string.Format(databaseError.Message, mongoMultipleException.Message),
                errors: validationFailures);
        }

        public static void ThrowIfMongoIndexViolation(MongoWriteException mongoWriteException, ServiceErrorCodes serviceErrorCodes)
        {
            var result = MongoIndexViolationError.TryParse(
                writeError: mongoWriteException.WriteError,
                mongoIndexViolationError: out var mongoIndexViolationError);

            if (result)
            {
                var databaseIndexViolationError = serviceErrorCodes.CommonErrorCodes.DatabaseIndexViolationError;

                var validationFailures = new List<ValidationFailure>()
                {
                    new ValidationFailure(
                        propertyName: mongoIndexViolationError.IndexName,
                        errorMessage: string.Format(
                            databaseIndexViolationError.Message,
                            mongoIndexViolationError.IndexName,
                            mongoIndexViolationError.IndexValue))
                    {
                        ErrorCode = databaseIndexViolationError.ExtendedStatusCode,
                        AttemptedValue = mongoIndexViolationError.IndexValue
                    }
                };

                throw new ValidationException(validationFailures);
            }
        }
    }
}
