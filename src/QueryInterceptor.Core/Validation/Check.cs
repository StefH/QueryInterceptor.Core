﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using System.Diagnostics;
using System.Reflection;

// Copied from https://github.com/aspnet/EntityFramework/blob/dev/src/Shared/Check.cs
namespace QueryInterceptor.Core.Validation {
    [DebuggerStepThrough]
    internal static class Check {
        public static T Condition<T>([NoEnumeration] T value, [NotNull] Predicate<T> condition, [InvokerParameterName][NotNull] string parameterName) {
            NotNull(condition, nameof(condition));
            NotNull(value, nameof(value));

            if (!condition(value)) {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentOutOfRangeException(parameterName);
            }

            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static T NotNull<T>([NoEnumeration] T value, [InvokerParameterName][NotNull] string parameterName) {
            if (value is null) {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static T NotNull<T>([NoEnumeration] T value, [InvokerParameterName][NotNull] string parameterName, [NotNull] string propertyName) {
            if (value is null) {
                NotEmpty(parameterName, nameof(parameterName));
                NotEmpty(propertyName, nameof(propertyName));

                throw new ArgumentException(CoreStrings.ArgumentPropertyNull(propertyName, parameterName));
            }

            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static IList<T> NotEmpty<T>(IList<T> value, [InvokerParameterName][NotNull] string parameterName) {
            NotNull(value, parameterName);

            if (value.Count == 0) {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(CoreStrings.CollectionArgumentIsEmpty(parameterName));
            }

            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static string? NotEmpty(string value, [InvokerParameterName][NotNull] string parameterName) {
            Exception? e = null;
            if (value is null) {
                e = new ArgumentNullException(parameterName);
            } else if (value.Trim().Length == 0) {
                e = new ArgumentException(CoreStrings.ArgumentIsEmpty(parameterName));
            }

            if (e != null) {
                NotEmpty(parameterName, nameof(parameterName));

                throw e;
            }

            return value;
        }

        public static string? NullButNotEmpty(string value, [InvokerParameterName][NotNull] string parameterName) {
            if (value is not null && (value.Length == 0)) {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(CoreStrings.ArgumentIsEmpty(parameterName));
            }

            return value;
        }

        public static IList<T> HasNoNulls<T>(IList<T> value, [InvokerParameterName][NotNull] string parameterName)
            where T : class {
            NotNull(value, parameterName);

            if (value.Any(e => e == null)) {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(parameterName);
            }

            return value;
        }

        public static Type ValidEntityType(Type value, [InvokerParameterName][NotNull] string parameterName) {
            if (!value.GetTypeInfo().IsClass) {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(CoreStrings.InvalidEntityType(value, parameterName));
            }

            return value;
        }
    }
}