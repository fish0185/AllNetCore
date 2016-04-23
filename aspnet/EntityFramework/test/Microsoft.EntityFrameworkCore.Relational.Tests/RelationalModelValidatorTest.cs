// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore.Tests.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Relational.Tests
{
    public class RelationalModelValidatorTest : LoggingModelValidatorTest
    {
        [Fact]
        public virtual void Detects_duplicate_table_names()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            var entityB = model.AddEntityType(typeof(B));
            SetPrimaryKey(entityB);
            entityA.Relational().TableName = "Table";
            entityB.Relational().TableName = "Table";

            VerifyError(RelationalStrings.DuplicateTableName("Table", null, entityB.DisplayName()), model);
        }

        [Fact]
        public virtual void Detects_duplicate_table_names_with_schema()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            var entityB = model.AddEntityType(typeof(B));
            SetPrimaryKey(entityB);
            entityA.Relational().TableName = "Table";
            entityA.Relational().Schema = "Schema";
            entityB.Relational().TableName = "Table";
            entityB.Relational().Schema = "Schema";

            VerifyError(RelationalStrings.DuplicateTableName("Table", "Schema", entityB.DisplayName()), model);
        }

        [Fact]
        public virtual void Does_not_detect_duplicate_table_names_in_different_schema()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            var entityB = model.AddEntityType(typeof(B));
            SetPrimaryKey(entityB);
            entityA.Relational().TableName = "Table";
            entityA.Relational().Schema = "SchemaA";
            entityB.Relational().TableName = "Table";
            entityB.Relational().Schema = "SchemaB";

            Validate(model);
        }

        [Fact]
        public virtual void Does_not_detect_duplicate_table_names_for_inherited_entities()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            var entityC = model.AddEntityType(typeof(C));
            SetBaseType(entityC, entityA);

            Validate(model);
        }

        [Fact]
        public virtual void Detects_duplicate_column_names()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            modelBuilder.Entity<Product>();
            modelBuilder.Entity<Product>().Property(b => b.Name).HasColumnName("Id");

            VerifyError(RelationalStrings.DuplicateColumnName(
                nameof(Product), "Id", nameof(Product), "Name", "Id", ".Product", "default_int_mapping", "just_string(2000)"), modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_duplicate_columns_in_derived_types_with_different_types()
        {
            var modelBuilder = new ModelBuilder(TestConventionalSetBuilder.Build());
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>();
            modelBuilder.Entity<Dog>();

            VerifyError(RelationalStrings.DuplicateColumnName(
                nameof(Cat), nameof(Cat.Type), nameof(Dog), nameof(Dog.Type), nameof(Cat.Type), ".Animal", "just_string(2000)", "default_int_mapping"), modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_MaxLength()
        {
            var modelBuilder = new ModelBuilder(TestConventionalSetBuilder.Build());
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().Ignore(e => e.Type).Property(c => c.Breed).HasMaxLength(30);
            modelBuilder.Entity<Dog>().Ignore(e => e.Type).Property(d => d.Breed).HasMaxLength(15);

            VerifyError(RelationalStrings.DuplicateColumnName(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), ".Animal", "just_string(30)", "just_string(15)"), modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_ComputedColumnSql()
        {
            var modelBuilder = new ModelBuilder(TestConventionalSetBuilder.Build());
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().Ignore(e => e.Type).Property(c => c.Breed).HasComputedColumnSql("1");
            modelBuilder.Entity<Dog>().Ignore(e => e.Type);

            VerifyError(RelationalStrings.DuplicateColumnNameComputedSqlMismatch(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), ".Animal", "1", ""), modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_DefaultValue()
        {
            var modelBuilder = new ModelBuilder(TestConventionalSetBuilder.Build());
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().Ignore(e => e.Type).Property(c => c.Breed).HasDefaultValueSql("1");
            modelBuilder.Entity<Dog>().Ignore(e => e.Type).Property(c => c.Breed).HasDefaultValue("1");

            VerifyError(RelationalStrings.DuplicateColumnNameDefaultSqlMismatch(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), ".Animal", "NULL", "1"), modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_DefaultValueSql()
        {
            var modelBuilder = new ModelBuilder(TestConventionalSetBuilder.Build());
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().Ignore(e => e.Type).Property(c => c.Breed).HasDefaultValueSql("1");
            modelBuilder.Entity<Dog>().Ignore(e => e.Type);

            VerifyError(RelationalStrings.DuplicateColumnNameDefaultSqlMismatch(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), ".Animal", "1", ""), modelBuilder.Model);
        }

        [Fact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_nullability()
        {
            var modelBuilder = new ModelBuilder(TestConventionalSetBuilder.Build());
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().Ignore(e => e.Type);
            modelBuilder.Entity<Dog>().Ignore(e => e.Type).Property(d => d.Type).HasColumnName("Id");

            VerifyError(RelationalStrings.DuplicateColumnNameNullabilityMismatch(
                nameof(Animal), nameof(Animal.Id), nameof(Dog), nameof(Dog.Type), nameof(Animal.Id), ".Animal"), modelBuilder.Model);
        }

        [Fact]
        public virtual void Does_not_detect_duplicate_column_names_within_hierarchy()
        {
            var modelBuilder = new ModelBuilder(TestConventionalSetBuilder.Build());
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>(eb =>
                {
                    eb.Ignore(e => e.Type);
                    eb.Property(c => c.Breed).HasMaxLength(25);
                    eb.Property(c => c.Breed).HasColumnName("BreedName");
                    eb.Property(c => c.Breed).HasDefaultValue("None");
                });
            modelBuilder.Entity<Dog>(eb =>
                {
                    eb.Ignore(e => e.Type);
                    eb.Property(c => c.Breed).HasMaxLength(25);
                    eb.Property(c => c.Breed).HasColumnName("BreedName");
                    eb.Property(c => c.Breed).HasDefaultValue("None");
                });

            Validate(modelBuilder.Model);
        }

        [Fact]
        public virtual void Passes_for_non_hierarchical_model()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);

            Validate(model);
        }

        [Fact]
        public virtual void Detects_missing_discriminator_property()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            var entityC = model.AddEntityType(typeof(C));
            entityC.HasBaseType(entityA);

            VerifyError(RelationalStrings.NoDiscriminatorProperty(entityA.DisplayName()), model);
        }

        [Fact]
        public virtual void Detects_missing_discriminator_value_on_base()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            var entityC = model.AddEntityType(typeof(C));
            entityC.HasBaseType(entityA);

            var discriminatorProperty = entityA.AddProperty("D", typeof(int));
            entityA.Relational().DiscriminatorProperty = discriminatorProperty;
            entityC.Relational().DiscriminatorValue = 1;

            VerifyError(RelationalStrings.NoDiscriminatorValue(entityA.DisplayName()), model);
        }

        [Fact]
        public virtual void Detects_missing_discriminator_value_on_leaf()
        {
            var model = new Model();
            var entityAbstract = model.AddEntityType(typeof(Abstract));
            SetPrimaryKey(entityAbstract);
            var entityGeneric = model.AddEntityType(typeof(Generic<string>));
            entityGeneric.HasBaseType(entityAbstract);

            var discriminatorProperty = entityAbstract.AddProperty("D", typeof(int));
            entityAbstract.Relational().DiscriminatorProperty = discriminatorProperty;
            entityAbstract.Relational().DiscriminatorValue = 0;

            VerifyError(RelationalStrings.NoDiscriminatorValue(entityGeneric.DisplayName()), model);
        }

        [Fact]
        public virtual void Detects_missing_non_string_discriminator_values()
        {
            var modelBuilder = new ModelBuilder(TestConventionalSetBuilder.Build());
            modelBuilder.Entity<C>();
            modelBuilder.Entity<A>().HasDiscriminator<byte>("ClassType")
                .HasValue<A>(0)
                .HasValue<D>(1);

            var model = modelBuilder.Model;
            VerifyError(RelationalStrings.NoDiscriminatorValue(typeof(C).Name), model);
        }

        [Fact]
        public virtual void Detects_duplicate_discriminator_values()
        {
            var modelBuilder = new ModelBuilder(TestConventionalSetBuilder.Build());
            modelBuilder.Entity<A>().HasDiscriminator<byte>("ClassType")
                .HasValue<A>(1)
                .HasValue<C>(1)
                .HasValue<D>(2);

            var model = modelBuilder.Model;
            VerifyError(RelationalStrings.DuplicateDiscriminatorValue(typeof(C).Name, 1, typeof(A).Name), model);
        }

        [Fact]
        public virtual void Does_not_detect_missing_discriminator_value_for_abstract_class()
        {
            var modelBuilder = new ModelBuilder(TestConventionalSetBuilder.Build());
            modelBuilder.Entity<Abstract>();
            modelBuilder.Entity<A>().HasDiscriminator<byte>("ClassType")
                .HasValue<A>(0)
                .HasValue<C>(1)
                .HasValue<D>(2)
                .HasValue<Generic<string>>(3);

            Validate(modelBuilder.Model);
        }

        protected override void SetBaseType(EntityType entityType, EntityType baseEntityType)
        {
            base.SetBaseType(entityType, baseEntityType);

            var discriminatorProperty = baseEntityType.GetOrAddProperty("Discriminator", typeof(string), shadow: true);
            baseEntityType.Relational().DiscriminatorProperty = discriminatorProperty;
            baseEntityType.Relational().DiscriminatorValue = baseEntityType.Name;
            entityType.Relational().DiscriminatorValue = entityType.Name;
        }

        protected class C : A
        {
        }

        protected class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        protected class Animal
        {
            public int Id { get; set; }
        }

        protected class Cat : Animal
        {
            public string Breed { get; set; }
            public string Type { get; set; }
        }

        protected class Dog : Animal
        {
            public string Breed { get; set; }
            public int Type { get; set; }
        }

        protected override ModelValidator CreateModelValidator()
            => new RelationalModelValidator(
                new Logger<RelationalModelValidator>(
                    new ListLoggerFactory(Log, l => l == typeof(RelationalModelValidator).FullName)),
                new TestAnnotationProvider(),
                new TestRelationalTypeMapper());
    }
}
