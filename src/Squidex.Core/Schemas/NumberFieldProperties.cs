﻿// ==========================================================================
//  NumberFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Collections.Immutable;
using Squidex.Infrastructure;

namespace Squidex.Core.Schemas
{
    [TypeName("NumberField")]
    public sealed class NumberFieldProperties : FieldProperties
    {
        private double? maxValue;
        private double? minValue;
        private double? defaultValue;
        private ImmutableList<double> allowedValues;

        public double? MaxValue
        {
            get { return maxValue; }
            set
            {
                ThrowIfFrozen();

                maxValue = value;
            }
        }

        public double? MinValue
        {
            get { return minValue; }
            set
            {
                ThrowIfFrozen();

                minValue = value;
            }
        }

        public double? DefaultValue
        {
            get { return defaultValue; }
            set
            {
                ThrowIfFrozen();

                defaultValue = value;
            }
        }

        public ImmutableList<double> AllowedValues
        {
            get { return allowedValues; }
            set
            {
                ThrowIfFrozen();

                allowedValues = value;
            }
        }

        protected override IEnumerable<ValidationError> ValidateCore()
        {
            if (MaxValue.HasValue && MinValue.HasValue && MinValue.Value >= MaxValue.Value)
            {
                yield return new ValidationError("Max value must be greater than min value", nameof(MinValue), nameof(MaxValue));
            }

            if (AllowedValues != null && (MinValue.HasValue || MaxValue.HasValue))
            {
                yield return new ValidationError("Either or allowed values or range can be defined",
                    nameof(AllowedValues),
                    nameof(MinValue),
                    nameof(MaxValue));
            }

            if (!DefaultValue.HasValue)
            {
                yield break;
            }

            if (MinValue.HasValue && DefaultValue.Value < MinValue.Value)
            {
                yield return new ValidationError("Default value must be greater than min value", nameof(DefaultValue));
            }

            if (MaxValue.HasValue && DefaultValue.Value > MaxValue.Value)
            {
                yield return new ValidationError("Default value must be less than max value", nameof(DefaultValue));
            }
        }
    }
}