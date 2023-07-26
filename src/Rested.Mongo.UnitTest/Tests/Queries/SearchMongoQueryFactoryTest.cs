using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Rested.Core.Data;
using Rested.Mongo.Data;
using Rested.Mongo.MediatR.Queries;
using Rested.Mongo.UnitTest.Data;
using System.Reflection;

namespace Rested.Mongo.UnitTest.Tests.Queries
{
    [TestClass]
    public class SearchMongoQueryFactoryTest
    {
        #region Constants

        protected const string TESTCATEGORY_QUERY_SORT_TESTS = "Query Sort Tests";
        protected const string TESTCATEGORY_QUERY_FILTER_TESTS = "Query Filter Tests";
        protected const string TESTCATEGORY_QUERY_TEXT_FILTER_TESTS = "Query Text Filter Tests";
        protected const string TESTCATEGORY_QUERY_NUMBER_FILTER_TESTS = "Query Number Filter Tests";
        protected const string TESTCATEGORY_QUERY_DATE_FILTER_TESTS = "Query Date Filter Tests";
        protected const string TESTCATEGORY_QUERY_DATETIME_FILTER_TESTS = "Query DateTime Filter Tests";
        protected const string TESTCATEGORY_QUERY_COMBINED_FILTER_TESTS = "Query Combined Filter Tests";

        #endregion Constants

        #region Properties

        public TestContext TestContext { get; set; }
        protected List<FieldSortInfo> MockSortingFields { get; set; } = new List<FieldSortInfo>();
        protected List<FieldFilterInfo> MockFieldFilters { get; set; } = new List<FieldFilterInfo>();
        protected List<FieldFilterInfo> MockImplicitFieldFilters { get; set; } = new List<FieldFilterInfo>();

        #endregion Properties

        #region Members

        protected readonly string TESTCONTEXTMSG_TEST_STATUS = "Test {0}: {1}";

        #endregion Members

        #region Initialization

        [TestInitialize]
        public void Initialize()
        {
            TestContext.WriteLine(
                format: TESTCONTEXTMSG_TEST_STATUS,
                args: new[] { TestContext.TestName, TestContext.CurrentTestOutcome.ToString() });
            TestContext.WriteLine(string.Empty);

            OnInitialize();
        }

        protected virtual void OnInitialize()
        {
            TestContext.WriteLine("Initializing Projection Registration...");
            ProjectionRegistration.Initialize();
        }

        #endregion Initialization

        #region Test Cleanup

        [TestCleanup]
        public void TestCleanup()
        {
            TestContext.WriteLine(
                format: TESTCONTEXTMSG_TEST_STATUS,
                args: new[] { TestContext.TestName, TestContext.CurrentTestOutcome.ToString() });
        }

        #endregion Test Cleanup

        #region Methods

        protected void TestFieldFilter()
        {
            var filterDefinition = SearchMongoQueryFactory_CreateFilterDefinition();

            var filterBsonDocument = filterDefinition.Render(
                documentSerializer: BsonSerializer.SerializerRegistry.GetSerializer<MongoDocument<Employee>>(),
                serializerRegistry: BsonSerializer.SerializerRegistry);

            filterBsonDocument.TryGetElement(
                name: MockFieldFilters[0].FieldName,
                value: out var filterBsonElement);

            filterBsonDocument.ElementCount.Should().Be(1);
            filterBsonElement.Should().NotBeNull();
        }

        protected void TestEmptyFieldFilter()
        {
            var filterDefinition = SearchMongoQueryFactory_CreateFilterDefinition();

            var filterBsonDocument = filterDefinition.Render(
                documentSerializer: BsonSerializer.SerializerRegistry.GetSerializer<MongoDocument<Employee>>(),
                serializerRegistry: BsonSerializer.SerializerRegistry);

            filterBsonDocument.ElementCount.Should().Be(0);
        }

        protected void TestCombinedFieldFilter()
        {
            var filterDefinition = SearchMongoQueryFactory_CreateFilterDefinition();

            var filterBsonDocument = filterDefinition.Render(
                documentSerializer: BsonSerializer.SerializerRegistry.GetSerializer<MongoDocument<Employee>>(),
                serializerRegistry: BsonSerializer.SerializerRegistry);

            filterBsonDocument.TryGetElement(
                name: $"${MockFieldFilters[0].FilterOperation.ToString().ToLowerInvariant()}",
                value: out var filterBsonElement);

            filterBsonDocument.ElementCount.Should().Be(1);
            filterBsonElement.Should().NotBeNull();

            var filterConditionsBsonArray = filterBsonElement.Value.AsBsonArray;

            filterConditionsBsonArray.Should().NotBeNull();
            filterConditionsBsonArray.Count.Should().Be(2);

            filterConditionsBsonArray[0].AsBsonDocument.TryGetElement(
                name: MockFieldFilters[0].FilterCondition1.FieldName,
                value: out var filterCondition1BsonElement);

            filterConditionsBsonArray[1].AsBsonDocument.TryGetElement(
                name: MockFieldFilters[0].FilterCondition2.FieldName,
                value: out var filterCondition2BsonElement);

            filterCondition1BsonElement.Should().NotBeNull();
            filterCondition2BsonElement.Should().NotBeNull();
        }

        protected List<FieldSortInfo> SearchMongoQueryFactory_ConvertSortingFieldNamesToMongoDocumentFieldNames()
        {
            var invokeParameters = new object[] { MockSortingFields, new List<FieldSortInfo>() };

            typeof(SearchMongoQueryFactory)
                .GetMethod(
                    name: "ConvertSortingFieldNamesToMongoDocumentFieldNames",
                    bindingAttr: BindingFlags.Static | BindingFlags.NonPublic,
                    types: new[] { typeof(List<FieldSortInfo>), typeof(List<FieldSortInfo>).MakeByRefType() })
                .MakeGenericMethod(typeof(Employee), typeof(EmployeeProjection))
                .Invoke(
                    obj: null,
                    parameters: invokeParameters);

            return invokeParameters[1] as List<FieldSortInfo>;
        }

        protected List<FieldFilterInfo> SearchMongoQueryFactory_ConvertFieldFilterFieldNamesToMongoDocumentFieldNames()
        {
            var invokeParameters = new object[] { MockFieldFilters, new List<FieldFilterInfo>() };

            typeof(SearchMongoQueryFactory)
                .GetMethod(
                    name: "ConvertFieldFilterFieldNamesToMongoDocumentFieldNames",
                    bindingAttr: BindingFlags.Static | BindingFlags.NonPublic,
                    types: new[] { typeof(List<FieldFilterInfo>), typeof(List<FieldFilterInfo>).MakeByRefType() })
                .MakeGenericMethod(typeof(Employee), typeof(EmployeeProjection))
                .Invoke(
                    obj: null,
                    parameters: invokeParameters);

            return invokeParameters[1] as List<FieldFilterInfo>;
        }

        protected SortDefinition<MongoDocument<Employee>> SearchMongoQueryFactory_CreateSortDefinition()
        {
            return typeof(SearchMongoQueryFactory)
                .GetMethod(
                    name: "CreateSortDefinition",
                    bindingAttr: BindingFlags.Static | BindingFlags.NonPublic,
                    types: new[] { typeof(List<FieldSortInfo>) })
                .MakeGenericMethod(typeof(Employee))
                .Invoke(
                    obj: null,
                    parameters: new object[] { MockSortingFields }) as SortDefinition<MongoDocument<Employee>>;
        }

        protected FilterDefinition<MongoDocument<Employee>> SearchMongoQueryFactory_CreateFilterDefinition()
        {
            return typeof(SearchMongoQueryFactory)
                .GetMethod(
                    name: "CreateFilterDefinition",
                    bindingAttr: BindingFlags.Static | BindingFlags.NonPublic,
                    types: new[] { typeof(List<FieldFilterInfo>), typeof(List<FieldFilterInfo>) })
                .MakeGenericMethod(typeof(Employee))
                .Invoke(
                    obj: null,
                    parameters: new object[] { MockFieldFilters, MockImplicitFieldFilters }) as FilterDefinition<MongoDocument<Employee>>;
        }

        #endregion Methods

        #region Query Sort Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_SORT_TESTS)]
        public void CanCreateSortingFields()
        {
            MockSortingFields = new List<FieldSortInfo>()
            {
                new FieldSortInfo()
                {
                    FieldName = "testField1",
                    SortDirection = FieldSortDirection.Ascending
                },
                new FieldSortInfo()
                {
                    FieldName = "testField2",
                    SortDirection = FieldSortDirection.Descending
                }
            };

            var sortBsonDocument = SearchMongoQueryFactory_CreateSortDefinition()
                .Render(
                    documentSerializer: BsonSerializer.SerializerRegistry.GetSerializer<MongoDocument<Employee>>(),
                    serializerRegistry: BsonSerializer.SerializerRegistry);

            sortBsonDocument.ElementCount.Should().Be(MockSortingFields.Count);

            foreach (var fieldSortInfo in MockSortingFields)
            {
                sortBsonDocument.TryGetElement(
                    name: fieldSortInfo.FieldName,
                    value: out var filterBsonElement);

                filterBsonElement.Should().NotBeNull();
            }
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_SORT_TESTS)]
        public void CanConvertProjectionSortingFieldNamesToMongoEntityFieldNames()
        {
            MockSortingFields = new List<FieldSortInfo>()
            {
                new FieldSortInfo()
                {
                    FieldName = nameof(EmployeeProjection.FullName).ToCamelCase(),
                    SortDirection = FieldSortDirection.Ascending
                },
                new FieldSortInfo()
                {
                    FieldName = nameof(EmployeeProjection.Age).ToCamelCase(),
                    SortDirection = FieldSortDirection.Ascending
                },
                new FieldSortInfo()
                {
                    FieldName = nameof(EmployeeProjection.DOB).ToCamelCase(),
                    SortDirection = FieldSortDirection.Ascending
                },
                new FieldSortInfo()
                {
                    FieldName = nameof(EmployeeProjection.StartDate).ToCamelCase(),
                    SortDirection = FieldSortDirection.Ascending
                },
                new FieldSortInfo()
                {
                    FieldName = nameof(EmployeeProjection.EmploymentType).ToCamelCase(),
                    SortDirection = FieldSortDirection.Ascending
                }
            };

            var projectionMappings = ProjectionMappings.GetProjectionMappings<EmployeeProjection>();
            var convertedSortingFields = SearchMongoQueryFactory_ConvertSortingFieldNamesToMongoDocumentFieldNames();

            convertedSortingFields.Should().NotBeNullOrEmpty();

            foreach (var convertedSortingField in convertedSortingFields)
                projectionMappings.Should().Contain(pm => pm.DocumentPropertyPath.ToCamelCase().Equals(convertedSortingField.FieldName));
        }

        #endregion Query Sort Tests

        #region Query Text Filter Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TEXT_FILTER_TESTS)]
        public void CanCreateTextFieldEqualsFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "textField",
                FilterType = FieldFilterTypes.Text,
                FilterOperation = (FieldFilterOperations)TextFieldFilterOperations.Equals,
                FilterValue = "testValue"
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TEXT_FILTER_TESTS)]
        public void CanCreateTextFieldNotEqualsFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "textField",
                FilterType = FieldFilterTypes.Text,
                FilterOperation = (FieldFilterOperations)TextFieldFilterOperations.NotEquals,
                FilterValue = "testValue"
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TEXT_FILTER_TESTS)]
        public void CanCreateTextFieldContainsFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "textField",
                FilterType = FieldFilterTypes.Text,
                FilterOperation = (FieldFilterOperations)TextFieldFilterOperations.Contains,
                FilterValue = "testValue"
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TEXT_FILTER_TESTS)]
        public void CanCreateTextFieldNotContainsFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "textField",
                FilterType = FieldFilterTypes.Text,
                FilterOperation = (FieldFilterOperations)TextFieldFilterOperations.NotContains,
                FilterValue = "testValue"
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TEXT_FILTER_TESTS)]
        public void CanCreateTextFieldStartsWithFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "textField",
                FilterType = FieldFilterTypes.Text,
                FilterOperation = (FieldFilterOperations)TextFieldFilterOperations.StartsWith,
                FilterValue = "testValue"
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TEXT_FILTER_TESTS)]
        public void CanCreateTextFieldEndsWithFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "textField",
                FilterType = FieldFilterTypes.Text,
                FilterOperation = (FieldFilterOperations)TextFieldFilterOperations.EndsWith,
                FilterValue = "testValue"
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TEXT_FILTER_TESTS)]
        public void CanCreateTextFieldBlankFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "textField",
                FilterType = FieldFilterTypes.Text,
                FilterOperation = (FieldFilterOperations)TextFieldFilterOperations.Blank,
                FilterValue = "testValue"
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TEXT_FILTER_TESTS)]
        public void CanCreateTextFieldNotBlankFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "textField",
                FilterType = FieldFilterTypes.Text,
                FilterOperation = (FieldFilterOperations)TextFieldFilterOperations.NotBlank,
                FilterValue = "testValue"
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_TEXT_FILTER_TESTS)]
        public void CanCreateTextFieldEmptyFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "textField",
                FilterType = FieldFilterTypes.Text,
                FilterOperation = (FieldFilterOperations)TextFieldFilterOperations.Empty,
                FilterValue = "testValue"
            });

            TestEmptyFieldFilter();
        }

        #endregion Query Text Filter Tests

        #region Query Number Filter Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_NUMBER_FILTER_TESTS)]
        public void CanCreateNumberFieldEqualsFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "numberField",
                FilterType = FieldFilterTypes.Number,
                FilterOperation = (FieldFilterOperations)NumberFieldFilterOperations.Equals,
                FilterValue = "0"
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_NUMBER_FILTER_TESTS)]
        public void CanCreateNumberFieldNotEqualsFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "numberField",
                FilterType = FieldFilterTypes.Number,
                FilterOperation = (FieldFilterOperations)NumberFieldFilterOperations.NotEquals,
                FilterValue = "0"
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_NUMBER_FILTER_TESTS)]
        public void CanCreateNumberFieldLessThanFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "numberField",
                FilterType = FieldFilterTypes.Number,
                FilterOperation = (FieldFilterOperations)NumberFieldFilterOperations.LessThan,
                FilterValue = "0"
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_NUMBER_FILTER_TESTS)]
        public void CanCreateNumberFieldLessThanOrEqualFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "numberField",
                FilterType = FieldFilterTypes.Number,
                FilterOperation = (FieldFilterOperations)NumberFieldFilterOperations.LessThanOrEqual,
                FilterValue = "0"
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_NUMBER_FILTER_TESTS)]
        public void CanCreateNumberFieldGreaterThanFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "numberField",
                FilterType = FieldFilterTypes.Number,
                FilterOperation = (FieldFilterOperations)NumberFieldFilterOperations.GreaterThan,
                FilterValue = "0"
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_NUMBER_FILTER_TESTS)]
        public void CanCreateNumberFieldGreaterThanOrEqualFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "numberField",
                FilterType = FieldFilterTypes.Number,
                FilterOperation = (FieldFilterOperations)NumberFieldFilterOperations.GreaterThanOrEqual,
                FilterValue = "0"
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_NUMBER_FILTER_TESTS)]
        public void CanCreateNumberFieldInRangeFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "numberField",
                FilterType = FieldFilterTypes.Number,
                FilterOperation = (FieldFilterOperations)NumberFieldFilterOperations.InRange,
                FilterValue = "0",
                FilterToValue = "1"
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_NUMBER_FILTER_TESTS)]
        public void CanCreateNumberFieldBlankFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "numberField",
                FilterType = FieldFilterTypes.Number,
                FilterOperation = (FieldFilterOperations)NumberFieldFilterOperations.Blank,
                FilterValue = "0"
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_NUMBER_FILTER_TESTS)]
        public void CanCreateNumberFieldNotBlankFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "numberField",
                FilterType = FieldFilterTypes.Number,
                FilterOperation = (FieldFilterOperations)NumberFieldFilterOperations.NotBlank,
                FilterValue = "0"
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_NUMBER_FILTER_TESTS)]
        public void CanCreateNumberFieldEmptyFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "numberField",
                FilterType = FieldFilterTypes.Number,
                FilterOperation = (FieldFilterOperations)NumberFieldFilterOperations.Empty,
                FilterValue = "0"
            });

            TestEmptyFieldFilter();
        }

        #endregion Query Number Filter Tests

        #region Query Date Filter Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_DATE_FILTER_TESTS)]
        public void CanCreateDateFieldEqualsFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "dateField",
                FilterType = FieldFilterTypes.Date,
                FilterOperation = (FieldFilterOperations)DateOnlyFieldFilterOperations.Equals,
                FilterValue = DateOnly.FromDateTime(DateTime.Now).ToString()
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_DATE_FILTER_TESTS)]
        public void CanCreateDateFieldNotEqualsFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "dateField",
                FilterType = FieldFilterTypes.Date,
                FilterOperation = (FieldFilterOperations)DateOnlyFieldFilterOperations.NotEquals,
                FilterValue = DateOnly.FromDateTime(DateTime.Now).ToString()
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_DATE_FILTER_TESTS)]
        public void CanCreateDateFieldLessThanFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "dateField",
                FilterType = FieldFilterTypes.Date,
                FilterOperation = (FieldFilterOperations)DateOnlyFieldFilterOperations.LessThan,
                FilterValue = DateOnly.FromDateTime(DateTime.Now).ToString()
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_DATE_FILTER_TESTS)]
        public void CanCreateDateFieldGreaterThanFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "dateField",
                FilterType = FieldFilterTypes.Date,
                FilterOperation = (FieldFilterOperations)DateOnlyFieldFilterOperations.GreaterThan,
                FilterValue = DateOnly.FromDateTime(DateTime.Now).ToString()
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_DATE_FILTER_TESTS)]
        public void CanCreateDateFieldInRangeFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "dateField",
                FilterType = FieldFilterTypes.Date,
                FilterOperation = (FieldFilterOperations)DateOnlyFieldFilterOperations.InRange,
                FilterValue = DateOnly.FromDateTime(DateTime.Now).ToString(),
                FilterToValue = DateOnly.FromDateTime(DateTime.Now).ToString()
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_DATE_FILTER_TESTS)]
        public void CanCreateDateFieldBlankFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "dateField",
                FilterType = FieldFilterTypes.Date,
                FilterOperation = (FieldFilterOperations)DateOnlyFieldFilterOperations.Blank,
                FilterValue = DateOnly.FromDateTime(DateTime.Now).ToString()
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_DATE_FILTER_TESTS)]
        public void CanCreateDateFieldNotBlankFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "dateField",
                FilterType = FieldFilterTypes.Date,
                FilterOperation = (FieldFilterOperations)DateOnlyFieldFilterOperations.NotBlank,
                FilterValue = DateOnly.FromDateTime(DateTime.Now).ToString()
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_DATE_FILTER_TESTS)]
        public void CanCreateDateFieldEmptyFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "dateField",
                FilterType = FieldFilterTypes.Date,
                FilterOperation = (FieldFilterOperations)DateOnlyFieldFilterOperations.Empty,
                FilterValue = DateOnly.FromDateTime(DateTime.Now).ToString()
            });

            TestEmptyFieldFilter();
        }

        #endregion Query Date Filter Tests

        #region Query DateTime Filter Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_DATETIME_FILTER_TESTS)]
        public void CanCreateDateTimeFieldEqualsFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "dateTimeField",
                FilterType = FieldFilterTypes.DateTime,
                FilterOperation = (FieldFilterOperations)DateTimeFieldFilterOperations.Equals,
                FilterValue = DateTime.Now.ToString()
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_DATETIME_FILTER_TESTS)]
        public void CanCreateDateTimeFieldNotEqualsFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "dateTimeField",
                FilterType = FieldFilterTypes.DateTime,
                FilterOperation = (FieldFilterOperations)DateTimeFieldFilterOperations.NotEquals,
                FilterValue = DateTime.Now.ToString()
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_DATETIME_FILTER_TESTS)]
        public void CanCreateDateTimeFieldLessThanFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "dateTimeField",
                FilterType = FieldFilterTypes.DateTime,
                FilterOperation = (FieldFilterOperations)DateTimeFieldFilterOperations.LessThan,
                FilterValue = DateTime.Now.ToString()
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_DATETIME_FILTER_TESTS)]
        public void CanCreateDateTimeFieldGreaterThanFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "dateTimeField",
                FilterType = FieldFilterTypes.DateTime,
                FilterOperation = (FieldFilterOperations)DateTimeFieldFilterOperations.GreaterThan,
                FilterValue = DateTime.Now.ToString()
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_DATETIME_FILTER_TESTS)]
        public void CanCreateDateTimeFieldInRangeFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "dateTimeField",
                FilterType = FieldFilterTypes.DateTime,
                FilterOperation = (FieldFilterOperations)DateTimeFieldFilterOperations.InRange,
                FilterValue = DateTime.Now.ToString(),
                FilterToValue = DateTime.Now.ToString()
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_DATETIME_FILTER_TESTS)]
        public void CanCreateDateTimeFieldBlankFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "dateTimeField",
                FilterType = FieldFilterTypes.DateTime,
                FilterOperation = (FieldFilterOperations)DateTimeFieldFilterOperations.Blank,
                FilterValue = DateTime.Now.ToString()
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_DATETIME_FILTER_TESTS)]
        public void CanCreateDateTimeFieldNotBlankFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "dateTimeField",
                FilterType = FieldFilterTypes.DateTime,
                FilterOperation = (FieldFilterOperations)DateTimeFieldFilterOperations.NotBlank,
                FilterValue = DateTime.Now.ToString()
            });

            TestFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_DATETIME_FILTER_TESTS)]
        public void CanCreateDateTimeFieldEmptyFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "dateTimeField",
                FilterType = FieldFilterTypes.DateTime,
                FilterOperation = (FieldFilterOperations)DateTimeFieldFilterOperations.Empty,
                FilterValue = DateTime.Now.ToString()
            });

            TestEmptyFieldFilter();
        }

        #endregion Query DateTime Filter Tests

        #region Query Combined Filter Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_COMBINED_FILTER_TESTS)]
        [Ignore("Test Skipped: Need to reassess how to determine if a combined field filter was properly created.")]
        public void CanCreateCombinedFieldAndFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "textField",
                FilterType = FieldFilterTypes.Combined,
                FilterOperation = (FieldFilterOperations)CombinedFieldFilterOperations.And,
                FilterCondition1 = new FieldFilterInfo()
                {
                    FieldName = "textField",
                    FilterType = FieldFilterTypes.Text,
                    FilterOperation = (FieldFilterOperations)TextFieldFilterOperations.Contains,
                    FilterValue = "testValue1"
                },
                FilterCondition2 = new FieldFilterInfo()
                {
                    FieldName = "textField",
                    FilterType = FieldFilterTypes.Text,
                    FilterOperation = (FieldFilterOperations)TextFieldFilterOperations.Contains,
                    FilterValue = "testValue2"
                },
            });

            TestCombinedFieldFilter();
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_COMBINED_FILTER_TESTS)]
        [Ignore("Test Skipped: Need to reassess how to determine if a combined field filter was properly created.")]
        public void CanCreateCombinedFieldOrFilter()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "textField",
                FilterType = FieldFilterTypes.Combined,
                FilterOperation = (FieldFilterOperations)CombinedFieldFilterOperations.Or,
                FilterCondition1 = new FieldFilterInfo()
                {
                    FieldName = "textField",
                    FilterType = FieldFilterTypes.Text,
                    FilterOperation = (FieldFilterOperations)TextFieldFilterOperations.Contains,
                    FilterValue = "testValue1"
                },
                FilterCondition2 = new FieldFilterInfo()
                {
                    FieldName = "textField",
                    FilterType = FieldFilterTypes.Text,
                    FilterOperation = (FieldFilterOperations)TextFieldFilterOperations.Contains,
                    FilterValue = "testValue2"
                },
            });

            TestCombinedFieldFilter();
        }

        #endregion Query Combined Filter Tests

        #region Query Filter Tests

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_FILTER_TESTS)]
        public void CanCreateImplicitFieldFilters()
        {
            MockFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "textField",
                FilterType = FieldFilterTypes.Text,
                FilterOperation = (FieldFilterOperations)TextFieldFilterOperations.Equals,
                FilterValue = "testValue"
            });

            MockImplicitFieldFilters.Add(new FieldFilterInfo()
            {
                FieldName = "textField2",
                FilterType = FieldFilterTypes.Text,
                FilterOperation = (FieldFilterOperations)TextFieldFilterOperations.Equals,
                FilterValue = "testValue"
            });

            var filterBsonDocument = SearchMongoQueryFactory_CreateFilterDefinition()
                .Render(
                    documentSerializer: BsonSerializer.SerializerRegistry.GetSerializer<MongoDocument<Employee>>(),
                    serializerRegistry: BsonSerializer.SerializerRegistry);

            filterBsonDocument.ElementCount.Should().Be(MockFieldFilters.Count + MockImplicitFieldFilters.Count);

            foreach (var fieldFilterInfo in MockFieldFilters)
            {
                filterBsonDocument.TryGetElement(
                    name: fieldFilterInfo.FieldName,
                    value: out var filterBsonElement);

                filterBsonElement.Should().NotBeNull();
            }

            foreach (var fieldFilterInfo in MockImplicitFieldFilters)
            {
                filterBsonDocument.TryGetElement(
                    name: fieldFilterInfo.FieldName,
                    value: out var filterBsonElement);

                filterBsonElement.Should().NotBeNull();
            }
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_FILTER_TESTS)]
        public void CanCreateMultipleFieldFilters()
        {
            MockFieldFilters = new List<FieldFilterInfo>()
            {
                new FieldFilterInfo()
                {
                    FieldName = "textField",
                    FilterType = FieldFilterTypes.Text,
                    FilterOperation = (FieldFilterOperations)TextFieldFilterOperations.Contains,
                    FilterValue = "testValue"
                },
                new FieldFilterInfo()
                {
                    FieldName = "numberField",
                    FilterType = FieldFilterTypes.Number,
                    FilterOperation = (FieldFilterOperations)NumberFieldFilterOperations.Equals,
                    FilterValue = "0"
                },
                new FieldFilterInfo()
                {
                    FieldName = "dateTimeField",
                    FilterType = FieldFilterTypes.DateTime,
                    FilterOperation = (FieldFilterOperations)DateTimeFieldFilterOperations.Equals,
                    FilterValue = DateTime.Now.ToString()
                }
            };

            var filterBsonDocument = SearchMongoQueryFactory_CreateFilterDefinition()
                .Render(
                    documentSerializer: BsonSerializer.SerializerRegistry.GetSerializer<MongoDocument<Employee>>(),
                    serializerRegistry: BsonSerializer.SerializerRegistry);

            foreach (var fieldFilterInfo in MockFieldFilters)
            {
                filterBsonDocument.TryGetElement(
                    name: fieldFilterInfo.FieldName,
                    value: out var filterBsonElement);

                filterBsonElement.Should().NotBeNull();
            }
        }

        [TestMethod]
        [TestCategory(TESTCATEGORY_QUERY_FILTER_TESTS)]
        public void CanConvertFieldFilterFieldNamesToMongoEntityFieldNames()
        {
            MockFieldFilters = new List<FieldFilterInfo>()
            {
                new FieldFilterInfo()
                {
                    FieldName = nameof(EmployeeProjection.FullName).ToCamelCase(),
                    FilterType = FieldFilterTypes.Text,
                    FilterOperation = (FieldFilterOperations)TextFieldFilterOperations.Contains,
                    FilterValue = "test"
                },
                new FieldFilterInfo()
                {
                    FieldName = nameof(EmployeeProjection.Age).ToCamelCase(),
                    FilterType = FieldFilterTypes.Number,
                    FilterOperation = (FieldFilterOperations)NumberFieldFilterOperations.Equals,
                    FilterValue = "42"
                },
                new FieldFilterInfo()
                {
                    FieldName = nameof(EmployeeProjection.DOB).ToCamelCase(),
                    FilterType = FieldFilterTypes.DateTime,
                    FilterOperation = (FieldFilterOperations)DateTimeFieldFilterOperations.Equals,
                    FilterValue = DateTime.Now.ToString()
                },
                new FieldFilterInfo()
                {
                    FieldName = nameof(EmployeeProjection.StartDate).ToCamelCase(),
                    FilterType = FieldFilterTypes.Date,
                    FilterOperation = (FieldFilterOperations)DateOnlyFieldFilterOperations.Equals,
                    FilterValue = DateOnly.FromDateTime(DateTime.Now).ToString()
                },
                new FieldFilterInfo()
                {
                    FieldName = nameof(EmployeeProjection.EmploymentType).ToCamelCase(),
                    FilterType = FieldFilterTypes.Combined,
                    FilterOperation = (FieldFilterOperations)CombinedFieldFilterOperations.Or,
                    FilterCondition1 = new FieldFilterInfo()
                    {
                        FieldName = nameof(EmployeeProjection.EmploymentType).ToCamelCase(),
                        FilterType = FieldFilterTypes.Text,
                        FilterOperation = (FieldFilterOperations)TextFieldFilterOperations.Equals,
                        FilterValue = "test"
                    },
                    FilterCondition2 = new FieldFilterInfo()
                    {
                        FieldName = nameof(EmployeeProjection.EmploymentType).ToCamelCase(),
                        FilterType = FieldFilterTypes.Text,
                        FilterOperation = (FieldFilterOperations)TextFieldFilterOperations.Equals,
                        FilterValue = "test"
                    }
                }
            };

            var projectionMappings = ProjectionMappings.GetProjectionMappings<EmployeeProjection>();
            var convertedFieldFilters = SearchMongoQueryFactory_ConvertFieldFilterFieldNamesToMongoDocumentFieldNames();

            convertedFieldFilters.Should().NotBeNullOrEmpty();

            foreach (var convertedFieldFilter in convertedFieldFilters)
                TestFieldFilterFieldNameMapping(convertedFieldFilter, projectionMappings);
        }

        private void TestFieldFilterFieldNameMapping(FieldFilterInfo fieldFilterInfo, IEnumerable<ProjectionMapping> projectionMappings)
        {
            projectionMappings.Should().Contain(pm => pm.DocumentPropertyPath.ToCamelCase().Equals(fieldFilterInfo.FieldName));

            if (fieldFilterInfo.FilterType is FieldFilterTypes.Combined)
            {
                if (fieldFilterInfo.FilterCondition1 is not null)
                    TestFieldFilterFieldNameMapping(fieldFilterInfo.FilterCondition1, projectionMappings);

                if (fieldFilterInfo.FilterCondition2 is not null)
                    TestFieldFilterFieldNameMapping(fieldFilterInfo.FilterCondition2, projectionMappings);
            }
        }

        #endregion Query Filter Tests
    }
}
