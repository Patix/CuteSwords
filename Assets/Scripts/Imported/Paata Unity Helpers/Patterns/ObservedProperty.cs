using System;
using System.Collections.Generic;

namespace Patik.CodeArchitecture.Patterns
{
    /// <summary>
    /// Observed Property which Raises event on Change
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct ObservedProperty<T>
    {
        /// <summary>
        /// Event Notifying Value Change (<param name="newValue">New value </param>)
        /// </summary>
        public event Action<T> OnValueChange;
        private T value;

        public T Value
        {
            get => value;
            set
            {
                if (!value.Equals(this.value))
                {
                    this.value = value;
                    OnValueChange?.Invoke(this.value);
                }
            }
        }
        /// <summary>
        /// Same as Subscription to event , but additionally catching up to events which were raised before creation of subscriber
        /// </summary>
        /// <param name="action"></param>
        public void ExecuteAndSubscribe(Action<T> action)
        {
            action?.Invoke(Value);
            OnValueChange += action;
        }

        public ObservedProperty(T value) : this()
        {
            Value = value;
        }

        public static implicit operator T(ObservedProperty<T> _this)
        {
            return _this.Value;
        }

        public static bool operator ==(ObservedProperty<T> property1, ObservedProperty<T> property2)
        {
            return property1.Equals(property2);
        }


        public static bool operator !=(ObservedProperty<T> property1, ObservedProperty<T> property2)
        {
            return !property1.Equals(property2);
        }


        public static bool operator ==(ObservedProperty<T> property1, T property2)
        {
            return property1.value.Equals(property2);
        }

        public static bool operator !=(ObservedProperty<T> property1, T property2)
        {
            return !property1.value.Equals(property2);
        }


        public bool Equals(ObservedProperty<T> other)
        {
            return EqualityComparer<T>.Default.Equals(value, other.value);
        }

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare with the current instance. </param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, <see langword="false" />. </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ObservedProperty<T> other && Equals(other);
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}