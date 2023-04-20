using MongoDB.Driver;
using Rested.Core.CQRS.Data;
using Rested.Mongo.CQRS.Data;

namespace Rested.Mongo.CQRS.Queries
{
    public static class SearchMongoQueryFactory
    {
        private static readonly string MONGO_ID_PROPERTY_NAME = "_id";

        public static async Task<SearchDocumentsResults<TData, MongoDocument<TData>>> SearchMongoDocuments<TData>(
            IMongoContext mongoContext,
            SearchRequest searchRequest,
            List<FieldFilterInfo> implicitFieldFilters = null)
            where TData : IData
        {
            ReplaceAnyMongoIdFieldFilterNames(searchRequest.FieldFilters, out var convertedFieldFilters);
            ReplaceAnyMongoIdSortingFieldNames(searchRequest.SortingFields, out var convertedSortingFields);

            var filterDefinition = CreateFilterDefinition<TData>(convertedFieldFilters, implicitFieldFilters);

            var searchResults = await mongoContext
                .RepositoryFactory
                .Create<TData>()
                .Collection
                .FindAsync(
                    filter: filterDefinition,
                    options: new FindOptions<MongoDocument<TData>>()
                    {
                        Sort = CreateSortDefinition<TData>(convertedSortingFields),
                        Skip = (searchRequest.Page - 1) * searchRequest.PageSize,
                        Limit = searchRequest.PageSize
                    })
                .Result
                .ToListAsync();

            var totalQueriedRecords = await GetTotalQueriedRecords(mongoContext, filterDefinition);

            var totalPages = totalQueriedRecords > searchRequest.PageSize ?
                Math.Ceiling((decimal)totalQueriedRecords / searchRequest.PageSize) : 1;

            return new SearchDocumentsResults<TData, MongoDocument<TData>>(searchRequest)
            {
                TotalPages = (int)totalPages,
                TotalQueriedRecords = totalQueriedRecords,
                TotalRecords = await GetTotalRecords<TData>(mongoContext, implicitFieldFilters),
                Items = searchResults
            };
        }

        public static async Task<SearchProjectionsResults<TData, TProjection>> SearchMongoProjections<TData, TProjection>(
            IMongoContext mongoContext,
            SearchRequest searchRequest,
            List<FieldFilterInfo> implicitFieldFilters = null)
            where TData : IData
            where TProjection : Projection
        {
            var projectionDefinition = Builders<MongoDocument<TData>>
                .Projection
                .Expression(Projection.GetProjectionExpression<TProjection, MongoDocument<TData>>());

            ConvertFieldFilterFieldNamesToMongoDocumentFieldNames<TData, TProjection>(
                fieldFilters: searchRequest.FieldFilters,
                convertedFieldFilters: out var convertedFieldFilters);

            ConvertSortingFieldNamesToMongoDocumentFieldNames<TData, TProjection>(
                sortingFields: searchRequest.SortingFields,
                convertedSortingFields: out var convertedSortingFields);

            ReplaceAnyMongoIdFieldFilterNames(convertedFieldFilters, out convertedFieldFilters);
            ReplaceAnyMongoIdSortingFieldNames(convertedSortingFields, out convertedSortingFields);

            var filterDefinition = CreateFilterDefinition<TData>(convertedFieldFilters, implicitFieldFilters);

            var searchResults = await mongoContext
                .RepositoryFactory
                .Create<TData>()
                .Collection
                .FindAsync(
                    filter: filterDefinition,
                    options: new FindOptions<MongoDocument<TData>, TProjection>()
                    {
                        Sort = CreateSortDefinition<TData>(convertedSortingFields),
                        Skip = (searchRequest.Page - 1) * searchRequest.PageSize,
                        Limit = searchRequest.PageSize,
                        Projection = projectionDefinition
                    })
                .Result
                .ToListAsync();

            var totalQueriedRecords = await GetTotalQueriedRecords(mongoContext, filterDefinition);

            var totalPages = totalQueriedRecords > searchRequest.PageSize ?
                Math.Ceiling((decimal)totalQueriedRecords / searchRequest.PageSize) : 1;

            return new SearchProjectionsResults<TData, TProjection>(searchRequest)
            {
                TotalPages = (int)totalPages,
                TotalQueriedRecords = totalQueriedRecords,
                TotalRecords = await GetTotalRecords<TData>(mongoContext, implicitFieldFilters),
                Items = searchResults
            };
        }

        private static void ConvertSortingFieldNamesToMongoDocumentFieldNames<TData, TProjection>(
            List<FieldSortInfo> sortingFields,
            out List<FieldSortInfo> convertedSortingFields)
            where TData : IData
            where TProjection : Projection
        {
            var _convertedSortingFields = new List<FieldSortInfo>();

            for (int i = 0; i < sortingFields?.Count; i++)
            {
                ProjectionMappings.TryGetMapping<TProjection>(
                    projectionPropertyPath: sortingFields[i].FieldName,
                    projectionMapping: out var projectionMapping,
                    isCamelCase: true);

                _convertedSortingFields.Add(new FieldSortInfo()
                {
                    FieldName = projectionMapping.DocumentPropertyPath.ToCamelCase(),
                    SortDirection = sortingFields[i].SortDirection
                });
            }

            convertedSortingFields = _convertedSortingFields;
        }

        private static void ConvertFieldFilterFieldNamesToMongoDocumentFieldNames<TData, TProjection>(
            List<FieldFilterInfo> fieldFilters,
            out List<FieldFilterInfo> convertedFieldFilters)
            where TData : IData
            where TProjection : Projection
        {
            var _convertedFieldFilters = new List<FieldFilterInfo>();

            for (int i = 0; i < fieldFilters?.Count; i++)
                _convertedFieldFilters.Add(MapFieldFilterInfoFieldNames<TData, TProjection>(fieldFilters[i]));

            convertedFieldFilters = _convertedFieldFilters;
        }

        private static FieldFilterInfo MapFieldFilterInfoFieldNames<TData, TProjection>(FieldFilterInfo fieldFilterInfo)
            where TData : IData
            where TProjection : Projection
        {
            ProjectionMappings.TryGetMapping<TProjection>(
                    projectionPropertyPath: fieldFilterInfo.FieldName,
                    projectionMapping: out var projectionMapping,
                    isCamelCase: true);

            var convertedFieldFilter = new FieldFilterInfo()
            {
                FieldName = projectionMapping.DocumentPropertyPath.ToCamelCase(),
                FilterType = fieldFilterInfo.FilterType,
                FilterOperation = fieldFilterInfo.FilterOperation,
                FilterValue = fieldFilterInfo.FilterValue,
                FilterToValue = fieldFilterInfo.FilterToValue,
                FilterCondition1 = fieldFilterInfo.FilterCondition1,
                FilterCondition2 = fieldFilterInfo.FilterCondition2
            };

            if (convertedFieldFilter.FilterType is FieldFilterTypes.Combined)
            {
                if (convertedFieldFilter.FilterCondition1 is not null)
                    convertedFieldFilter.FilterCondition1 = MapFieldFilterInfoFieldNames<TData, TProjection>(convertedFieldFilter.FilterCondition1);

                if (convertedFieldFilter.FilterCondition2 is not null)
                    convertedFieldFilter.FilterCondition2 = MapFieldFilterInfoFieldNames<TData, TProjection>(convertedFieldFilter.FilterCondition2);
            }

            return convertedFieldFilter;
        }

        private static void ReplaceAnyMongoIdFieldFilterNames(List<FieldFilterInfo> fieldFilters, out List<FieldFilterInfo> convertedFieldFilters)
        {
            var _convertedFieldFilters = new List<FieldFilterInfo>();

            for (int i = 0; i < fieldFilters?.Count; i++)
                _convertedFieldFilters.Add(ReplaceMongoIdFieldFilterName(fieldFilters[i]));

            convertedFieldFilters = _convertedFieldFilters;
        }

        private static FieldFilterInfo ReplaceMongoIdFieldFilterName(FieldFilterInfo fieldFilter)
        {
            var convertedFieldFilter = new FieldFilterInfo()
            {
                FieldName = fieldFilter.FieldName,
                FilterType = fieldFilter.FilterType,
                FilterOperation = fieldFilter.FilterOperation,
                FilterValue = fieldFilter.FilterValue,
                FilterToValue = fieldFilter.FilterToValue,
                FilterCondition1 = fieldFilter.FilterCondition1,
                FilterCondition2 = fieldFilter.FilterCondition2
            };

            if (convertedFieldFilter.FieldName == nameof(IIdentifiable.Id).ToCamelCase())
                convertedFieldFilter.FieldName = MONGO_ID_PROPERTY_NAME;

            if (convertedFieldFilter.FilterType is FieldFilterTypes.Combined)
            {
                if (convertedFieldFilter.FilterCondition1 is not null)
                    convertedFieldFilter.FilterCondition1 = ReplaceMongoIdFieldFilterName(fieldFilter.FilterCondition1);

                if (convertedFieldFilter.FilterCondition2 is not null)
                    convertedFieldFilter.FilterCondition2 = ReplaceMongoIdFieldFilterName(fieldFilter.FilterCondition2);
            }

            return convertedFieldFilter;
        }

        private static void ReplaceAnyMongoIdSortingFieldNames(List<FieldSortInfo> sortingFields, out List<FieldSortInfo> convertedSortingFields)
        {
            var _convertedSortingFields = new List<FieldSortInfo>();

            for (int i = 0; i < sortingFields?.Count; i++)
            {
                var convertedSortingField = new FieldSortInfo()
                {
                    FieldName = sortingFields[i].FieldName,
                    SortDirection = sortingFields[i].SortDirection
                };

                if (convertedSortingField.FieldName == nameof(IIdentifiable.Id).ToCamelCase())
                    convertedSortingField.FieldName = MONGO_ID_PROPERTY_NAME;

                _convertedSortingFields.Add(convertedSortingField);
            }

            convertedSortingFields = _convertedSortingFields;
        }

        private static SortDefinition<MongoDocument<TData>> CreateSortDefinition<TData>(List<FieldSortInfo> sortingFields)
            where TData : IData
        {
            SortDefinition<MongoDocument<TData>> sortDefinition = null;

            sortingFields?.ForEach(fieldSortInfo =>
            {
                switch (fieldSortInfo.SortDirection)
                {
                    default:
                    case FieldSortDirection.Ascending:
                        sortDefinition = sortDefinition?.Ascending(fieldSortInfo.FieldName) ??
                            Builders<MongoDocument<TData>>.Sort.Ascending(fieldSortInfo.FieldName);
                        break;

                    case FieldSortDirection.Descending:
                        sortDefinition = sortDefinition?.Descending(fieldSortInfo.FieldName) ??
                            Builders<MongoDocument<TData>>.Sort.Descending(fieldSortInfo.FieldName);
                        break;
                }
            });

            return sortDefinition;
        }

        private static FilterDefinition<MongoDocument<TData>> CreateFilterDefinition<TData>(
            List<FieldFilterInfo> fieldFilters,
            List<FieldFilterInfo> implicitFieldFilters = null)
            where TData : IData
        {
            var filterDefinition = CreateFilterDefinition<TData>(
                fieldFilters: implicitFieldFilters,
                filterDefinition: null);

            return CreateFilterDefinition(fieldFilters, filterDefinition);
        }

        private static FilterDefinition<MongoDocument<TData>> CreateFilterDefinition<TData>(
            List<FieldFilterInfo> fieldFilters,
            FilterDefinition<MongoDocument<TData>> filterDefinition = null)
            where TData : IData
        {
            if (filterDefinition is null)
                filterDefinition = Builders<MongoDocument<TData>>.Filter.Empty;

            fieldFilters?.ForEach(fieldFilterInfo =>
            {
                filterDefinition = CreateFieldFilterDefinition(fieldFilterInfo, filterDefinition);
            });

            return filterDefinition;
        }

        private static FilterDefinition<MongoDocument<TData>> CreateFieldFilterDefinition<TData>(
            FieldFilterInfo fieldFilterInfo,
            FilterDefinition<MongoDocument<TData>> filterDefinition)
            where TData : IData
        {
            return fieldFilterInfo.FilterType switch
            {
                FieldFilterTypes.Number => CreateFilterDefinitionFromNumberField(fieldFilterInfo, filterDefinition),
                FieldFilterTypes.Date => CreateFilterDefinitionFromDateField(fieldFilterInfo, filterDefinition),
                FieldFilterTypes.DateTime => CreateFilterDefinitionFromDateTimeField(fieldFilterInfo, filterDefinition),
                FieldFilterTypes.Combined => CreateFilterDefinitionFromCombinedField(fieldFilterInfo, filterDefinition),
                FieldFilterTypes.Text or _ => CreateFilterDefinitionFromTextField(fieldFilterInfo, filterDefinition)
            };
        }

        private static FilterDefinition<MongoDocument<TData>> CreateFilterDefinitionFromTextField<TData>(
            FieldFilterInfo fieldFilterInfo,
            FilterDefinition<MongoDocument<TData>> filterDefinition)
            where TData : IData
        {
            var filterDefinitionBuilder = Builders<MongoDocument<TData>>.Filter;

            return (TextFieldFilterOperations)fieldFilterInfo.FilterOperation switch
            {
                TextFieldFilterOperations.Equals =>
                    filterDefinition &= filterDefinitionBuilder.Eq(
                        field: fieldFilterInfo.FieldName,
                        value: fieldFilterInfo.FilterValue),

                TextFieldFilterOperations.NotEquals =>
                    filterDefinition &= filterDefinitionBuilder.Not(
                        filterDefinitionBuilder.Eq(
                            field: fieldFilterInfo.FieldName,
                            value: fieldFilterInfo.FilterValue)),

                TextFieldFilterOperations.Contains =>
                    filterDefinition &= filterDefinitionBuilder.Contains(
                        fieldDefinition: fieldFilterInfo.FieldName,
                        value: fieldFilterInfo.FilterValue),

                TextFieldFilterOperations.NotContains =>
                    filterDefinition &= filterDefinitionBuilder.Not(
                        filterDefinitionBuilder.Contains(
                            fieldDefinition: fieldFilterInfo.FieldName,
                            value: fieldFilterInfo.FilterValue)),

                TextFieldFilterOperations.StartsWith =>
                    filterDefinition &= filterDefinitionBuilder.StartsWith(
                        fieldDefinition: fieldFilterInfo.FieldName,
                        value: fieldFilterInfo.FilterValue),

                TextFieldFilterOperations.EndsWith =>
                    filterDefinition &= filterDefinitionBuilder.EndsWith(
                        fieldDefinition: fieldFilterInfo.FieldName,
                        value: fieldFilterInfo.FilterValue),

                TextFieldFilterOperations.Blank =>
                    filterDefinition &= filterDefinitionBuilder.Not(
                        filterDefinitionBuilder.Nin(
                            field: fieldFilterInfo.FieldName,
                            values: new[] { null, "" })),

                TextFieldFilterOperations.NotBlank =>
                    filterDefinition &= filterDefinitionBuilder.Nin(
                        field: fieldFilterInfo.FieldName,
                        values: new[] { null, "" }),

                TextFieldFilterOperations.Empty or _ =>
                    filterDefinition &= Builders<MongoDocument<TData>>.Filter.Empty
            };
        }

        private static FilterDefinition<MongoDocument<TData>> CreateFilterDefinitionFromNumberField<TData>(
            FieldFilterInfo fieldFilterInfo,
            FilterDefinition<MongoDocument<TData>> filterDefinition = null)
            where TData : IData
        {
            var filterDefinitionBuilder = Builders<MongoDocument<TData>>.Filter;

            int.TryParse(fieldFilterInfo.FilterValue, out var numberFilterValue);
            int.TryParse(fieldFilterInfo.FilterToValue, out var numberFilterToValue);

            return (NumberFieldFilterOperations)fieldFilterInfo.FilterOperation switch
            {
                NumberFieldFilterOperations.Equals =>
                    filterDefinition &= filterDefinitionBuilder.Eq(
                        field: fieldFilterInfo.FieldName,
                        value: numberFilterValue),

                NumberFieldFilterOperations.NotEquals =>
                    filterDefinition &= filterDefinitionBuilder.Not(
                        filterDefinitionBuilder.Eq(
                            field: fieldFilterInfo.FieldName,
                            value: numberFilterValue)),

                NumberFieldFilterOperations.LessThan =>
                    filterDefinition &= filterDefinitionBuilder.Lt(
                        field: fieldFilterInfo.FieldName,
                        value: numberFilterValue),

                NumberFieldFilterOperations.LessThanOrEqual =>
                    filterDefinition &= filterDefinitionBuilder.Lte(
                        field: fieldFilterInfo.FieldName,
                        value: numberFilterValue),

                NumberFieldFilterOperations.GreaterThan =>
                    filterDefinition &= filterDefinitionBuilder.Gt(
                        field: fieldFilterInfo.FieldName,
                        value: numberFilterValue),

                NumberFieldFilterOperations.GreaterThanOrEqual =>
                    filterDefinition &= filterDefinitionBuilder.Gte(
                        field: fieldFilterInfo.FieldName,
                        value: numberFilterValue),

                NumberFieldFilterOperations.InRange =>
                    filterDefinition &= filterDefinitionBuilder.And(
                        new[]
                        {
                            filterDefinitionBuilder.Gte(
                                field: fieldFilterInfo.FieldName,
                                value: numberFilterValue),
                            filterDefinitionBuilder.Lte(
                                field: fieldFilterInfo.FieldName,
                                value: numberFilterToValue)
                        }),

                NumberFieldFilterOperations.Blank =>
                    filterDefinition &= filterDefinitionBuilder.Not(
                        filterDefinitionBuilder.Nin(
                            field: fieldFilterInfo.FieldName,
                            values: new[] { null, "" })),

                NumberFieldFilterOperations.NotBlank =>
                    filterDefinition &= filterDefinitionBuilder.Nin(
                        field: fieldFilterInfo.FieldName,
                        values: new[] { null, "" }),

                NumberFieldFilterOperations.Empty or _ =>
                    filterDefinition &= Builders<MongoDocument<TData>>.Filter.Empty
            };
        }

        private static FilterDefinition<MongoDocument<TData>> CreateFilterDefinitionFromDateField<TData>(
            FieldFilterInfo fieldFilterInfo,
            FilterDefinition<MongoDocument<TData>> filterDefinition = null)
            where TData : IData
        {
            var filterDefinitionBuilder = Builders<MongoDocument<TData>>.Filter;

            DateOnly.TryParse(fieldFilterInfo.FilterValue, out var dateFilterValue);
            DateOnly.TryParse(fieldFilterInfo.FilterToValue, out var dateFilterToValue);

            return (DateOnlyFieldFilterOperations)fieldFilterInfo.FilterOperation switch
            {
                DateOnlyFieldFilterOperations.Equals =>
                    filterDefinition &= filterDefinitionBuilder.Eq(
                        field: fieldFilterInfo.FieldName,
                        value: dateFilterValue),

                DateOnlyFieldFilterOperations.NotEquals =>
                    filterDefinition &= filterDefinitionBuilder.Not(
                        filterDefinitionBuilder.Eq(
                            field: fieldFilterInfo.FieldName,
                            value: dateFilterValue)),

                DateOnlyFieldFilterOperations.LessThan =>
                    filterDefinition &= filterDefinitionBuilder.Lt(
                        field: fieldFilterInfo.FieldName,
                        value: dateFilterValue),

                DateOnlyFieldFilterOperations.GreaterThan =>
                    filterDefinition &= filterDefinitionBuilder.Gt(
                        field: fieldFilterInfo.FieldName,
                        value: dateFilterValue),

                DateOnlyFieldFilterOperations.InRange =>
                    filterDefinition &= filterDefinitionBuilder.And(
                        new[]
                        {
                            filterDefinitionBuilder.Gte(
                                field: fieldFilterInfo.FieldName,
                                value: dateFilterValue),
                            filterDefinitionBuilder.Lte(
                                field: fieldFilterInfo.FieldName,
                                value: dateFilterToValue)
                        }),

                DateOnlyFieldFilterOperations.Blank =>
                    filterDefinition &= filterDefinitionBuilder.Not(
                        filterDefinitionBuilder.Nin(
                            field: fieldFilterInfo.FieldName,
                            values: new[] { null, "" })),

                DateOnlyFieldFilterOperations.NotBlank =>
                    filterDefinition &= filterDefinitionBuilder.Nin(
                        field: fieldFilterInfo.FieldName,
                        values: new[] { null, "" }),

                DateOnlyFieldFilterOperations.Empty or _ =>
                    filterDefinition &= Builders<MongoDocument<TData>>.Filter.Empty
            };
        }

        private static FilterDefinition<MongoDocument<TData>> CreateFilterDefinitionFromDateTimeField<TData>(
            FieldFilterInfo fieldFilterInfo,
            FilterDefinition<MongoDocument<TData>> filterDefinition = null)
            where TData : IData
        {
            var filterDefinitionBuilder = Builders<MongoDocument<TData>>.Filter;

            DateTimeOffset.TryParse(fieldFilterInfo.FilterValue, out var dateTimeFilterValue);
            DateTimeOffset.TryParse(fieldFilterInfo.FilterToValue, out var dateTimeFilterToValue);

            return (DateTimeFieldFilterOperations)fieldFilterInfo.FilterOperation switch
            {
                DateTimeFieldFilterOperations.Equals =>
                    filterDefinition &= filterDefinitionBuilder.Eq(
                        field: fieldFilterInfo.FieldName,
                        value: dateTimeFilterValue.UtcDateTime),

                DateTimeFieldFilterOperations.NotEquals =>
                    filterDefinition &= filterDefinitionBuilder.Not(
                        filterDefinitionBuilder.Eq(
                            field: fieldFilterInfo.FieldName,
                            value: dateTimeFilterValue.UtcDateTime)),

                DateTimeFieldFilterOperations.LessThan =>
                    filterDefinition &= filterDefinitionBuilder.Lt(
                        field: fieldFilterInfo.FieldName,
                        value: dateTimeFilterValue.UtcDateTime),

                DateTimeFieldFilterOperations.GreaterThan =>
                    filterDefinition &= filterDefinitionBuilder.Gt(
                        field: fieldFilterInfo.FieldName,
                        value: dateTimeFilterValue.UtcDateTime),

                DateTimeFieldFilterOperations.InRange =>
                    filterDefinition &= filterDefinitionBuilder.And(
                        new[]
                        {
                            filterDefinitionBuilder.Gte(
                                field: fieldFilterInfo.FieldName,
                                value: dateTimeFilterValue.UtcDateTime),
                            filterDefinitionBuilder.Lte(
                                field: fieldFilterInfo.FieldName,
                                value: dateTimeFilterToValue.UtcDateTime)
                        }),

                DateTimeFieldFilterOperations.Blank =>
                    filterDefinition &= filterDefinitionBuilder.Not(
                        filterDefinitionBuilder.Nin(
                            field: fieldFilterInfo.FieldName,
                            values: new[] { null, "" })),

                DateTimeFieldFilterOperations.NotBlank =>
                    filterDefinition &= filterDefinitionBuilder.Nin(
                        field: fieldFilterInfo.FieldName,
                        values: new[] { null, "" }),

                DateTimeFieldFilterOperations.Empty or _ =>
                    filterDefinition &= Builders<MongoDocument<TData>>.Filter.Empty
            };
        }

        private static FilterDefinition<MongoDocument<TData>> CreateFilterDefinitionFromCombinedField<TData>(
            FieldFilterInfo fieldFilterInfo,
            FilterDefinition<MongoDocument<TData>> filterDefinition = null)
            where TData : IData
        {
            var filterDefinitionBuilder = Builders<MongoDocument<TData>>.Filter;

            return (CombinedFieldFilterOperations)fieldFilterInfo.FilterOperation switch
            {
                CombinedFieldFilterOperations.And =>
                    filterDefinition &= filterDefinitionBuilder.And(
                        new[]
                        {
                            CreateFieldFilterDefinition(fieldFilterInfo.FilterCondition1, filterDefinition),
                            CreateFieldFilterDefinition(fieldFilterInfo.FilterCondition2, filterDefinition)
                        }),

                CombinedFieldFilterOperations.Or or _ =>
                    filterDefinition &= filterDefinitionBuilder.Or(
                        new[]
                        {
                            CreateFieldFilterDefinition(fieldFilterInfo.FilterCondition1, filterDefinition),
                            CreateFieldFilterDefinition(fieldFilterInfo.FilterCondition2, filterDefinition)
                        })
            };
        }

        private static async Task<long> GetTotalQueriedRecords<TData>(
            IMongoContext mongoContext,
            FilterDefinition<MongoDocument<TData>> filterDefinition)
            where TData : IData
        {
            return await mongoContext
                .RepositoryFactory
                .Create<TData>()
                .Collection
                .CountDocumentsAsync(filter: filterDefinition);
        }

        private static async Task<long> GetTotalRecords<TData>(
            IMongoContext mongoContext,
            List<FieldFilterInfo> implicitFieldFilters = null)
            where TData : IData
        {
            var implicitFilterDefinition = CreateFilterDefinition<TData>(
                fieldFilters: implicitFieldFilters,
                filterDefinition: null);

            return await mongoContext
                .RepositoryFactory
                .Create<TData>()
                .Collection
                .CountDocumentsAsync(implicitFilterDefinition);
        }
    }
}
