// Imported from Lokad.Shared, 2011-02-08

using System;

namespace Lokad.Cloud.Storage
{
    /// <summary>
    /// Helper class that allows to pass out method call results without using exceptions
    /// </summary>
    /// <typeparam name="T">type of the associated data</typeparam>
    public class Result<T> : IEquatable<Result<T>>
    {
        readonly bool _isSuccess;
        readonly T _value;
        readonly string _error;

        Result(bool isSuccess, T value, string error)
        {
            _isSuccess = isSuccess;
            _value = value;
            _error = error;
        }

        /// <summary>
        /// Creates the success result.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>result encapsulating the success value</returns>
        /// <exception cref="ArgumentNullException">if value is a null reference type</exception>
        public static Result<T> CreateSuccess(T value)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull
            if (null == value) throw new ArgumentNullException("value");
            // ReSharper restore CompareNonConstrainedGenericWithNull

            return new Result<T>(true, value, default(string));
        }

        /// <summary>
        /// Creates the error result.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns>result encapsulating the error value</returns>
        /// <exception cref="ArgumentNullException">if error is null</exception>
        public static Result<T> CreateError(string error)
        {
            if (null == error) throw new ArgumentNullException("error");

            return new Result<T>(false, default(T), error);
        }


        /// <summary>
        /// Performs an implicit conversion from <typeparamref name="T"/> to <see cref="Result{T}"/>.
        /// </summary>
        /// <param name="value">The item.</param>
        /// <returns>The result of the conversion.</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="value"/> is a reference type that is null</exception>
        public static implicit operator Result<T>(T value)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull
            if (null == value) throw new ArgumentNullException("value");
            // ReSharper restore CompareNonConstrainedGenericWithNull
            return new Result<T>(true, value, null);
        }


        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Result<T>)) return false;
            return Equals((Result<T>) obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = _isSuccess.GetHashCode();
                result = (result*397) ^ _value.GetHashCode();
                result = (result*397) ^ (_error != null ? _error.GetHashCode() : 0);
                return result;
            }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(Result<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other._isSuccess.Equals(_isSuccess) && Equals(other._value, _value) && Equals(other._error, _error);
        }

        /// <summary>
        /// Gets a value indicating whether this result is valid.
        /// </summary>
        /// <value><c>true</c> if this result is valid; otherwise, <c>false</c>.</value>
        public bool IsSuccess
        {
            get { return _isSuccess; }
        }

        /// <summary>
        /// item associated with this result
        /// </summary>
        public T Value
        {
            get
            {
                if (!_isSuccess)
                    throw new InvalidOperationException("Dont access result on error. " + _error);

                return _value;
            }
        }

        /// <summary>
        /// Error message associated with this failure
        /// </summary>
        public string Error
        {
            get
            {
                if (_isSuccess)
                    throw new InvalidOperationException("Dont access error on valid result.");

                return _error;
            }
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="Result{T}"/>.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns>The result of the conversion.</returns>
        /// <exception cref="ArgumentNullException">If value is a null reference type</exception>
        public static implicit operator Result<T>(string error)
        {
            if (null == error) throw new ArgumentNullException("error");
            return CreateError(error);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (!_isSuccess)
                return "<Error: '" + _error + "'>";

            return "<Value: '" + _value + "'>";
        }
    }
}