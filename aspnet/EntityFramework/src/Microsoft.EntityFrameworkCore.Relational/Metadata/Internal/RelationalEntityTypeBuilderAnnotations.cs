// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class RelationalEntityTypeBuilderAnnotations : RelationalEntityTypeAnnotations
    {
        protected readonly string DefaultDiscriminatorName = "Discriminator";

        public RelationalEntityTypeBuilderAnnotations(
            [NotNull] InternalEntityTypeBuilder internalBuilder,
            ConfigurationSource configurationSource,
            [CanBeNull] RelationalFullAnnotationNames providerFullAnnotationNames)
            : base(new RelationalAnnotationsBuilder(internalBuilder, configurationSource), providerFullAnnotationNames)
        {
        }

        protected new virtual RelationalAnnotationsBuilder Annotations => (RelationalAnnotationsBuilder)base.Annotations;
        protected virtual InternalEntityTypeBuilder EntityTypeBuilder => (InternalEntityTypeBuilder)Annotations.MetadataBuilder;

        protected override RelationalModelAnnotations GetAnnotations(IModel model)
            => new RelationalModelBuilderAnnotations(
                ((Model)model).Builder,
                Annotations.ConfigurationSource,
                ProviderFullAnnotationNames);

        protected override RelationalEntityTypeAnnotations GetAnnotations(IEntityType entityType)
            => new RelationalEntityTypeBuilderAnnotations(
                ((EntityType)entityType).Builder,
                Annotations.ConfigurationSource,
                ProviderFullAnnotationNames);

        public virtual bool ToTable([CanBeNull] string name)
        {
            Check.NullButNotEmpty(name, nameof(name));

            return SetTableName(name);
        }

        public virtual bool ToSchema([CanBeNull] string name)
        {
            Check.NullButNotEmpty(name, nameof(name));

            return SetSchema(name);
        }

        public virtual bool ToTable([CanBeNull] string name, [CanBeNull] string schema)
        {
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var originalTable = TableName;
            if (!SetTableName(name))
            {
                return false;
            }

            if (!SetSchema(schema))
            {
                SetTableName(originalTable);
                return false;
            }

            return true;
        }

        public virtual DiscriminatorBuilder HasDiscriminator() => DiscriminatorBuilder(null, null);

        public virtual DiscriminatorBuilder HasDiscriminator([CanBeNull] Type discriminatorType)
        {
            if (discriminatorType == null)
            {
                return RemoveDiscriminator();
            }

            return DiscriminatorProperty != null
                   && DiscriminatorProperty.ClrType == discriminatorType
                ? DiscriminatorBuilder(null, null)
                : DiscriminatorBuilder(null, discriminatorType);
        }

        public virtual DiscriminatorBuilder HasDiscriminator([NotNull] string name, [NotNull] Type discriminatorType)
            => DiscriminatorProperty != null
               && DiscriminatorProperty.Name == name
               && DiscriminatorProperty.ClrType == discriminatorType
                ? DiscriminatorBuilder(null, null)
                : DiscriminatorBuilder(b => b.Property(name, discriminatorType, Annotations.ConfigurationSource), null);

        public virtual DiscriminatorBuilder HasDiscriminator([CanBeNull] PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                return RemoveDiscriminator();
            }

            return (DiscriminatorProperty != null)
                   && (DiscriminatorProperty.Name == propertyInfo.Name)
                   && (DiscriminatorProperty.ClrType == propertyInfo.PropertyType)
                ? DiscriminatorBuilder(null, null)
                : DiscriminatorBuilder(b => b.Property(propertyInfo, Annotations.ConfigurationSource), null);
        }

        private DiscriminatorBuilder RemoveDiscriminator()
        {
            var discriminatorProperty = (Property)GetNonRootDiscriminatorProperty();
            if (discriminatorProperty != null)
            {
                if (!SetDiscriminatorProperty(null))
                {
                    return null;
                }

                if (discriminatorProperty.DeclaringEntityType == EntityTypeBuilder.Metadata)
                {
                    EntityTypeBuilder.RemoveShadowPropertiesIfUnused(new[] { discriminatorProperty });
                }
            }

            return new DiscriminatorBuilder(Annotations, entityBuilder
                => new RelationalEntityTypeBuilderAnnotations(entityBuilder, Annotations.ConfigurationSource, ProviderFullAnnotationNames));
        }

        private DiscriminatorBuilder DiscriminatorBuilder(
            [CanBeNull] Func<InternalEntityTypeBuilder, InternalPropertyBuilder> createProperty,
            [CanBeNull] Type propertyType)
        {
            var discriminatorProperty = DiscriminatorProperty;
            if (discriminatorProperty != null
                && (createProperty != null
                    || propertyType != null))
            {
                if (!SetDiscriminatorProperty(null))
                {
                    return null;
                }
            }

            var rootType = EntityTypeBuilder.Metadata.RootType();
            var rootTypeBuilder = EntityTypeBuilder.Metadata == rootType
                ? EntityTypeBuilder
                : rootType.Builder;

            var configurationSource = Annotations.ConfigurationSource;
            InternalPropertyBuilder propertyBuilder;
            if (createProperty != null)
            {
                propertyBuilder = createProperty(rootTypeBuilder);
            }
            else if (propertyType != null)
            {
                propertyBuilder = rootTypeBuilder.Property(DefaultDiscriminatorName, propertyType, configurationSource);
            }
            else if (discriminatorProperty == null)
            {
                propertyBuilder = rootTypeBuilder.Property(DefaultDiscriminatorName, typeof(string), ConfigurationSource.Convention);
            }
            else
            {
                propertyBuilder = rootTypeBuilder.Property(discriminatorProperty.Name, ConfigurationSource.Convention);
            }

            if (propertyBuilder == null)
            {
                if (discriminatorProperty != null
                    && (createProperty != null
                        || propertyType != null))
                {
                    SetDiscriminatorProperty(discriminatorProperty);
                }
                return null;
            }

            if (discriminatorProperty != null
                && (createProperty != null || propertyType != null)
                && propertyBuilder.Metadata != discriminatorProperty)
            {
                if (discriminatorProperty.DeclaringEntityType == EntityTypeBuilder.Metadata)
                {
                    EntityTypeBuilder.RemoveShadowPropertiesIfUnused(new[] { (Property)discriminatorProperty });
                }
            }

            if (discriminatorProperty == null
                || createProperty != null
                || propertyType != null)
            {
                var discriminatorSet = SetDiscriminatorProperty(propertyBuilder.Metadata);
                Debug.Assert(discriminatorSet);
            }

            propertyBuilder.IsRequired(true, configurationSource);
            // TODO: #2132
            //propertyBuilder.ReadOnlyBeforeSave(true, configurationSource);
            propertyBuilder.ReadOnlyAfterSave(true, configurationSource);
            propertyBuilder.RequiresValueGenerator(true, configurationSource);

            return new DiscriminatorBuilder(Annotations, entityBuilder
                => new RelationalEntityTypeBuilderAnnotations(entityBuilder, Annotations.ConfigurationSource, ProviderFullAnnotationNames));
        }

        public virtual bool HasDiscriminatorValue([CanBeNull] object value) => SetDiscriminatorValue(value);
    }
}
