// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class RelationalModelValidator : LoggingModelValidator
    {
        private readonly IRelationalAnnotationProvider _relationalExtensions;
        private readonly IRelationalTypeMapper _typeMapper;

        public RelationalModelValidator(
            [NotNull] ILogger<RelationalModelValidator> loggerFactory,
            [NotNull] IRelationalAnnotationProvider relationalExtensions,
            [NotNull] IRelationalTypeMapper typeMapper)
            : base(loggerFactory)
        {
            _relationalExtensions = relationalExtensions;
            _typeMapper = typeMapper;
        }

        public override void Validate(IModel model)
        {
            base.Validate(model);

            EnsureDistinctTableNames(model);
            EnsureSharedColumnsCompatibility(model);
            ValidateInheritanceMapping(model);
            EnsureDataTypes(model);
        }

        protected virtual void EnsureDataTypes([NotNull] IModel model)
        {
            foreach (var entityType in model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    var dataType = _relationalExtensions.For(property).ColumnType;
                    if (dataType != null)
                    {
                        _typeMapper.ValidateTypeName(dataType);
                    }
                }
            }
        }

        protected virtual void EnsureDistinctTableNames([NotNull] IModel model)
        {
            var tables = new HashSet<string>();
            foreach (var entityType in model.GetEntityTypes().Where(et => et.BaseType == null))
            {
                var annotations = _relationalExtensions.For(entityType);

                var name = annotations.Schema + "." + annotations.TableName;

                if (!tables.Add(name))
                {
                    ShowError(RelationalStrings.DuplicateTableName(annotations.TableName, annotations.Schema, entityType.DisplayName()));
                }
            }
        }

        protected virtual void EnsureSharedColumnsCompatibility([NotNull] IModel model)
        {
            var groupedEntityTypes = new Dictionary<string, List<IEntityType>>();
            foreach (var entityType in model.GetEntityTypes())
            {
                var annotations = _relationalExtensions.For(entityType);
                var tableName = annotations.Schema + "." + annotations.TableName;
                if (!groupedEntityTypes.ContainsKey(tableName))
                {
                    groupedEntityTypes[tableName] = new List<IEntityType>();
                }
                groupedEntityTypes[tableName].Add(entityType);
            }

            foreach (var table in groupedEntityTypes.Keys)
            {
                var properties = groupedEntityTypes[table].SelectMany(et => et.GetDeclaredProperties());
                var propertyTypeMappings = new Dictionary<string, IProperty>();

                foreach (var property in properties)
                {
                    var propertyAnnotations = _relationalExtensions.For(property);
                    var columnName = propertyAnnotations.ColumnName;
                    IProperty duplicateProperty;
                    if (propertyTypeMappings.TryGetValue(columnName, out duplicateProperty))
                    {
                        var previousAnnotations = _relationalExtensions.For(duplicateProperty);
                        var currentTypeString = propertyAnnotations.ColumnType
                                                ?? _typeMapper.GetMapping(property).DefaultTypeName;
                        var previousTypeString = previousAnnotations.ColumnType
                                                 ?? _typeMapper.GetMapping(duplicateProperty).DefaultTypeName;
                        if (!currentTypeString.Equals(previousTypeString, StringComparison.OrdinalIgnoreCase))
                        {
                            ShowError(RelationalStrings.DuplicateColumnName(
                                duplicateProperty.DeclaringEntityType.DisplayName(),
                                duplicateProperty.Name,
                                property.DeclaringEntityType.DisplayName(),
                                property.Name,
                                columnName,
                                table,
                                previousTypeString,
                                currentTypeString));
                        }

                        if (property.IsColumnNullable() != duplicateProperty.IsColumnNullable())
                        {
                            ShowError(RelationalStrings.DuplicateColumnNameNullabilityMismatch(
                                duplicateProperty.DeclaringEntityType.DisplayName(),
                                duplicateProperty.Name,
                                property.DeclaringEntityType.DisplayName(),
                                property.Name,
                                columnName,
                                table));
                        }

                        var currentComputedValueSql = propertyAnnotations.ComputedValueSql ?? "";
                        var previousComputedValueSql = previousAnnotations.ComputedValueSql ?? "";
                        if (!currentComputedValueSql.Equals(previousComputedValueSql, StringComparison.OrdinalIgnoreCase))
                        {
                            ShowError(RelationalStrings.DuplicateColumnNameComputedSqlMismatch(
                                duplicateProperty.DeclaringEntityType.DisplayName(),
                                duplicateProperty.Name,
                                property.DeclaringEntityType.DisplayName(),
                                property.Name,
                                columnName,
                                table,
                                previousComputedValueSql,
                                currentComputedValueSql));
                        }

                        var currentDefaultValue = propertyAnnotations.DefaultValue;
                        var previousDefaultValue = previousAnnotations.DefaultValue;
                        if (currentDefaultValue != previousDefaultValue)
                        {
                            ShowError(RelationalStrings.DuplicateColumnNameDefaultSqlMismatch(
                                duplicateProperty.DeclaringEntityType.DisplayName(),
                                duplicateProperty.Name,
                                property.DeclaringEntityType.DisplayName(),
                                property.Name,
                                columnName,
                                table,
                                previousDefaultValue ?? "NULL",
                                currentDefaultValue ?? "NULL"));
                        }

                        var currentDefaultValueSql = propertyAnnotations.DefaultValueSql ?? "";
                        var previousDefaultValueSql = previousAnnotations.DefaultValueSql ?? "";
                        if (!currentDefaultValueSql.Equals(previousDefaultValueSql, StringComparison.OrdinalIgnoreCase))
                        {
                            ShowError(RelationalStrings.DuplicateColumnNameDefaultSqlMismatch(
                                duplicateProperty.DeclaringEntityType.DisplayName(),
                                duplicateProperty.Name,
                                property.DeclaringEntityType.DisplayName(),
                                property.Name,
                                columnName,
                                table,
                                previousDefaultValueSql,
                                currentDefaultValueSql));
                        }
                    }
                    else
                    {
                        propertyTypeMappings[columnName] = property;
                    }
                }
            }
        }

        protected virtual void ValidateInheritanceMapping([NotNull] IModel model)
        {
            var hierarchies = new Dictionary<IEntityType, List<IEntityType>>();
            foreach (var entityType in model.GetEntityTypes().Where(et => et.BaseType != null))
            {
                var root = entityType.RootType();
                if (root != entityType)
                {
                    List<IEntityType> derivedTypes;
                    if (!hierarchies.TryGetValue(root, out derivedTypes))
                    {
                        derivedTypes = new List<IEntityType>();
                        hierarchies[root] = derivedTypes;
                    }
                    derivedTypes.Add(entityType);
                }
            }

            foreach (var rootEntityType in hierarchies.Keys)
            {
                ValidateDiscriminatorValues(rootEntityType, hierarchies[rootEntityType]);
            }
        }

        private void ValidateDiscriminator(IEntityType entityType)
        {
            var annotations = _relationalExtensions.For(entityType);
            if (annotations.DiscriminatorProperty == null)
            {
                ShowError(RelationalStrings.NoDiscriminatorProperty(entityType.DisplayName()));
            }
            if (annotations.DiscriminatorValue == null)
            {
                ShowError(RelationalStrings.NoDiscriminatorValue(entityType.DisplayName()));
            }
        }

        private void ValidateDiscriminatorValues(IEntityType rootEntityType, IReadOnlyList<IEntityType> derivedTypes)
        {
            var discriminatorValues = new Dictionary<object, IEntityType>();
            if (rootEntityType.ClrType?.IsInstantiable() == true)
            {
                ValidateDiscriminator(rootEntityType);
                discriminatorValues[_relationalExtensions.For(rootEntityType).DiscriminatorValue] = rootEntityType;
            }

            foreach (var derivedType in derivedTypes)
            {
                if (derivedType.ClrType?.IsInstantiable() != true)
                {
                    continue;
                }

                ValidateDiscriminator(derivedType);

                var discriminatorValue = _relationalExtensions.For(derivedType).DiscriminatorValue;
                IEntityType duplicateEntityType;
                if (discriminatorValues.TryGetValue(discriminatorValue, out duplicateEntityType))
                {
                    ShowError(RelationalStrings.DuplicateDiscriminatorValue(
                        derivedType.DisplayName(), discriminatorValue, duplicateEntityType.DisplayName()));
                }
                discriminatorValues[discriminatorValue] = derivedType;
            }
        }
    }
}
