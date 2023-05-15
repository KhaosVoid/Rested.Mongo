using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rested.Core.CQRS.Commands;
using Rested.Core.CQRS.Data;
using Rested.Mongo.CQRS.Commands;

namespace Rested.Mongo.CQRS.MSTest.Commands
{
    public abstract class MongoDtoCommandTest<TData, TMongoDtoCommand, TMongoDtoCommandValidator, TMongoDtoCommandHandler> :
        MongoCommandTest<TData, TMongoDtoCommand, TMongoDtoCommandValidator, TMongoDtoCommandHandler>
        where TData : IData
        where TMongoDtoCommand : MongoDtoCommand<TData>
        where TMongoDtoCommandValidator : MongoDtoCommandValidator<TData, TMongoDtoCommand>
        where TMongoDtoCommandHandler : MongoDtoCommandHandler<TData, TMongoDtoCommand>
    {
        #region Properties

        protected Dto<TData> TestDto { get; set; }

        #endregion Properties

        #region Initialization

        protected override void OnInitializeTestDocument()
        {
            TestContext.WriteLine("Initializing Test Dto...");
            OnInitializeTestDto();

            TestDocument = CreateDocument(data: TestDto.Data);
        }

        protected virtual void OnInitializeTestDto()
        {
            TestDto = CreateDto(data: InitializeTestData());
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

        protected override TMongoDtoCommand CreateCommand(CommandActions action)
        {
            return (TMongoDtoCommand)Activator.CreateInstance(
                type: typeof(TMongoDtoCommand),
                args: new object[] { TestDto, action });
        }

        #endregion Methods

        #region Insert Validation Rule Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_INSERT_VALIDATION_RULE_TESTS)]
        public void DataIsRequiredInsertValidation()
        {
            TestDto.Id = Guid.Empty;
            TestDto.ETag = null;
            TestDto.Data = default;

            TestCommandValidationRule(
                action: CommandActions.Insert,
                serviceErrorCode: CreateCommandValidator().ServiceErrorCodes.CommonErrorCodes.DataIsRequired);
        }

        #endregion Insert Validation Rule Tests

        #region Update Validation Rule Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_UPDATE_VALIDATION_RULE_TESTS)]
        public void IdIsRequiredUpdateValidation()
        {
            TestDto.Id = Guid.Empty;

            TestCommandValidationRule(
                action: CommandActions.Update,
                serviceErrorCode: CreateCommandValidator().ServiceErrorCodes.CommonErrorCodes.IDIsRequired);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_UPDATE_VALIDATION_RULE_TESTS)]
        public void ETagIsRequiredUpdateValidation()
        {
            TestDto.ETag = null;

            TestCommandValidationRule(
                action: CommandActions.Update,
                serviceErrorCode: CreateCommandValidator().ServiceErrorCodes.CommonErrorCodes.ETagIsRequired);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_UPDATE_VALIDATION_RULE_TESTS)]
        public void DataIsRequiredUpdateValidation()
        {
            TestDto.Data = default;

            TestCommandValidationRule(
                action: CommandActions.Update,
                serviceErrorCode: CreateCommandValidator().ServiceErrorCodes.CommonErrorCodes.DataIsRequired);
        }

        #endregion Update Validation Rule Tests

        #region Patch Validation Rule Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_PATCH_VALIDATION_RULE_TESTS)]
        public void IdIsRequiredPatchValidation()
        {
            TestDto.Id = Guid.Empty;

            TestCommandValidationRule(
                action: CommandActions.Patch,
                serviceErrorCode: CreateCommandValidator().ServiceErrorCodes.CommonErrorCodes.IDIsRequired);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_PATCH_VALIDATION_RULE_TESTS)]
        public void ETagIsRequiredPatchValidation()
        {
            TestDto.ETag = null;

            TestCommandValidationRule(
                action: CommandActions.Patch,
                serviceErrorCode: CreateCommandValidator().ServiceErrorCodes.CommonErrorCodes.ETagIsRequired);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_PATCH_VALIDATION_RULE_TESTS)]
        public void DataIsRequiredPatchValidation()
        {
            TestDto.Data = default;

            TestCommandValidationRule(
                action: CommandActions.Patch,
                serviceErrorCode: CreateCommandValidator().ServiceErrorCodes.CommonErrorCodes.DataIsRequired);
        }

        #endregion Patch Validation Rule Tests

        #region Prune Validation Rule Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_PRUNE_VALIDATION_RULE_TESTS)]
        public void IdIsRequiredPruneValidation()
        {
            TestDto.Id = Guid.Empty;

            TestCommandValidationRule(
                action: CommandActions.Prune,
                serviceErrorCode: CreateCommandValidator().ServiceErrorCodes.CommonErrorCodes.IDIsRequired);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_PRUNE_VALIDATION_RULE_TESTS)]
        public void ETagIsRequiredPruneValidation()
        {
            TestDto.ETag = null;

            TestCommandValidationRule(
                action: CommandActions.Prune,
                serviceErrorCode: CreateCommandValidator().ServiceErrorCodes.CommonErrorCodes.ETagIsRequired);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_PRUNE_VALIDATION_RULE_TESTS)]
        public void DataIsRequiredPruneValidation()
        {
            TestDto.Data = default;

            TestCommandValidationRule(
                action: CommandActions.Prune,
                serviceErrorCode: CreateCommandValidator().ServiceErrorCodes.CommonErrorCodes.DataIsRequired);
        }

        #endregion Prune Validation Rule Tests

        #region Delete Validation Rule Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_DELETE_VALIDATION_RULE_TESTS)]
        public void IdIsRequiredDeleteValidation()
        {
            TestDto.Id = Guid.Empty;

            TestCommandValidationRule(
                action: CommandActions.Delete,
                serviceErrorCode: CreateCommandValidator().ServiceErrorCodes.CommonErrorCodes.IDIsRequired);
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_DELETE_VALIDATION_RULE_TESTS)]
        public void ETagIsRequiredDeleteValidation()
        {
            TestDto.ETag = null;

            TestCommandValidationRule(
                action: CommandActions.Delete,
                serviceErrorCode: CreateCommandValidator().ServiceErrorCodes.CommonErrorCodes.ETagIsRequired);
        }

        #endregion Delete Validation Rule Tests

        #region Insert Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_INSERT_TESTS)]
        public override void Insert()
        {
            TestDto.Id = Guid.Empty;
            TestDto.ETag = null;

            base.Insert();
        }

        #endregion Insert Tests
    }
}
