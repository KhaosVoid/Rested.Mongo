using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rested.Core.Data;
using Rested.Core.MediatR.Commands;
using Rested.Mongo.MediatR.Commands;

namespace Rested.Mongo.MediatR.MSTest.Commands
{
    public abstract class MultiMongoDtoCommandTest<TData, TMultiMongoDtoCommand, TMultiMongoDtoCommandValidator, TMultiMongoDtoCommandHandler> :
        MultiMongoCommandTest<TData, TMultiMongoDtoCommand, TMultiMongoDtoCommandValidator, TMultiMongoDtoCommandHandler>
        where TData : IData
        where TMultiMongoDtoCommand : MultiMongoDtoCommand<TData>
        where TMultiMongoDtoCommandValidator : MultiMongoDtoCommandValidator<TData, TMultiMongoDtoCommand>
        where TMultiMongoDtoCommandHandler : MultiMongoDtoCommandHandler<TData, TMultiMongoDtoCommand>
    {
        #region Properties

        protected List<Dto<TData>> TestDtos { get; set; }

        #endregion Properties

        #region Initialization

        protected override void OnInitializeTestDocuments()
        {
            TestContext.WriteLine("Initializing Test Dtos...");
            OnInitializeTestDtos();

            TestDocuments = TestDtos.Select(
                dto =>
                {
                    var document = CreateDocument(dto.Data);

                    document.Id = dto.Id;
                    document.ETag = dto.ETag;

                    return document;
                }).ToList();
        }

        protected virtual void OnInitializeTestDtos()
        {
            TestDtos = InitializeTestData().Select(CreateDto).ToList();
        }

        #endregion Initialization

        #region Methods

        protected Dto<TData> CreateDto(TData data = default) => CreateDto<TData>(data);

        protected Dto<T> CreateDto<T>(T data = default) where T : IData
        {
            return new Dto<T>()
            {
                Id = Guid.NewGuid(),
                ETag = BitConverter.GetBytes(0UL),
                Data = data
            };
        }

        protected override TMultiMongoDtoCommand CreateCommand(CommandActions action)
        {
            return (TMultiMongoDtoCommand)Activator.CreateInstance(
                type: typeof(TMultiMongoDtoCommand),
                args: new object[] { TestDtos, action });
        }

        #endregion Methods

        #region Command Validation Rule Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_COMMAND_VALIDATION_RULE_TESTS)]
        public void DtoCollectionMustNotBeEmptyValidation()
        {
            TestDtos = null;

            TestCommandValidationRule(
                action: CommandActions.Insert,
                serviceErrorCode: CreateCommandValidator().ServiceErrorCodes.CommonErrorCodes.CollectionMustNotBeEmpty);
        }

        #endregion Command Validation Rule Tests

        #region Insert Validation Rule Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_INSERT_VALIDATION_RULE_TESTS)]
        public void DataIsRequiredInsertValidation()
        {
            TestDtos.ForEach(dto =>
            {
                dto.Id = Guid.Empty;
                dto.ETag = null;
                dto.Data = default;
            });

            TestCommandValidationRule(
                action: CommandActions.Insert,
                serviceErrorCode: CreateCommandValidator().ServiceErrorCodes.CommonErrorCodes.DataIsRequired,
                duplicateRules: true);
        }

        #endregion Insert Validation Rule Tests

        #region Update Validation Rule Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_UPDATE_VALIDATION_RULE_TESTS)]
        public void IdIsRequiredUpdateValidation()
        {
            TestDtos.ForEach(dto => dto.Id = Guid.Empty);

            TestCommandValidationRule(
                action: CommandActions.Update,
                serviceErrorCode: CreateCommandValidator().ServiceErrorCodes.CommonErrorCodes.IDIsRequired,
                duplicateRules: true);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_UPDATE_VALIDATION_RULE_TESTS)]
        public void ETagIsRequiredUpdateValidation()
        {
            TestDtos.ForEach(dto => dto.ETag = null);

            TestCommandValidationRule(
                action: CommandActions.Update,
                serviceErrorCode: CreateCommandValidator().ServiceErrorCodes.CommonErrorCodes.ETagIsRequired,
                duplicateRules: true);
        }

        public void DataIsRequiredUpdateValidation()
        {
            TestDtos.ForEach(dto => dto.Data = default);

            TestCommandValidationRule(
                action: CommandActions.Update,
                serviceErrorCode: CreateCommandValidator().ServiceErrorCodes.CommonErrorCodes.DataIsRequired,
                duplicateRules: true);
        }

        #endregion Update Validation Rule Tests

        #region Patch Validation Rule Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_PATCH_VALIDATION_RULE_TESTS)]
        public void IdIsRequiredPatchValidation()
        {
            TestDtos.ForEach(dto => dto.Id = Guid.Empty);

            TestCommandValidationRule(
                action: CommandActions.Patch,
                serviceErrorCode: CreateCommandValidator().ServiceErrorCodes.CommonErrorCodes.IDIsRequired,
                duplicateRules: true);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_PATCH_VALIDATION_RULE_TESTS)]
        public void ETagIsRequiredPatchValidation()
        {
            TestDtos.ForEach(dto => dto.ETag = null);

            TestCommandValidationRule(
                action: CommandActions.Patch,
                serviceErrorCode: CreateCommandValidator().ServiceErrorCodes.CommonErrorCodes.ETagIsRequired,
                duplicateRules: true);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_PATCH_VALIDATION_RULE_TESTS)]
        public void DataIsRequiredPatchValidation()
        {
            TestDtos.ForEach(dto => dto.Data = default);

            TestCommandValidationRule(
                action: CommandActions.Patch,
                serviceErrorCode: CreateCommandValidator().ServiceErrorCodes.CommonErrorCodes.DataIsRequired,
                duplicateRules: true);
        }

        #endregion Patch Validation Rule Tests

        #region Delete Validation Rule Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_DELETE_VALIDATION_RULE_TESTS)]
        public void IdIsRequiredDeleteValidation()
        {
            TestDtos.ForEach(dto => dto.Id = Guid.Empty);

            TestCommandValidationRule(
                action: CommandActions.Delete,
                serviceErrorCode: CreateCommandValidator().ServiceErrorCodes.CommonErrorCodes.IDIsRequired,
                duplicateRules: true);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_DELETE_VALIDATION_RULE_TESTS)]
        public void ETagIsRequiredDeleteValidation()
        {
            TestDtos.ForEach(dto => dto.ETag = null);

            TestCommandValidationRule(
                action: CommandActions.Delete,
                serviceErrorCode: CreateCommandValidator().ServiceErrorCodes.CommonErrorCodes.ETagIsRequired,
                duplicateRules: true);
        }

        #endregion Delete Validation Rule Tests

        #region Insert Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_INSERT_TESTS)]
        public override void Insert()
        {
            TestDtos.ForEach(dto =>
            {
                dto.Id = Guid.Empty;
                dto.ETag = null;
            });

            base.Insert();
        }

        #endregion Insert Tests
    }
}
