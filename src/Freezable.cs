using System;
using System.Diagnostics;
using System.Reflection;

namespace DevZest.Licensing
{
    /// <summary>Represents object that can be frozen for modification.</summary>
    /// <typeparam name="T">Type of the freezer.</typeparam>
    public abstract class Freezable<T>
    {
        internal abstract void Freeze(T freezer);

        /// <summary>Gets a value indicates whether this object is currently modifiable.</summary>
        /// <value><see langword="true" /> if the object is frozen and cannot be modified; otherwise <see langword="false"/>.</value>
        /// <remarks>Attempting to modify this object when its <see cref="IsFrozen" /> property is set to <see langword="true" /> throws an
        /// <see cref="InvalidOperationException" />. All derived classes should respect this property by calling
        /// <see cref="VerifyFrozenAccess">VerifyFrozenAccess</see> in all public methods and property setters that might modify this object.
        /// </remarks>
        public abstract bool IsFrozen { get; }

        /// <summary>Enforces that this object is modifiable.</summary>
        /// <exception cref="InvalidOperationException">This object is not modifiable.</exception>
        protected internal void VerifyFrozenAccess()
        {
            if (IsFrozen)
                throw new InvalidOperationException(ExceptionMessages.FrozenAccess);
        }
    }
}
