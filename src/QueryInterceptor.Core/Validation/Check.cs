using System.Diagnostics.CodeAnalysis;

namespace QueryInterceptor.Core.Validation {
    internal static class Check {
        public static T NotNull<T>([NotNull] T? value, string parameterName) {
            if (value is null) {
                NotEmpty(parameterName, nameof(parameterName));
                throw new ArgumentNullException(parameterName);
            }
            return value;
        }

        public static string NotEmpty(string? value, string parameterName) {
            if (!string.IsNullOrWhiteSpace(value)) return value;
            NotEmpty(parameterName, nameof(parameterName));
            throw new ArgumentNullException(parameterName);
        }
    }
}