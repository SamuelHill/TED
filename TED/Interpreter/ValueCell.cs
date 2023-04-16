﻿using System.Collections.Generic;

namespace TED
{
    /// <summary>
    /// Run-time representation of a variable or a constant
    /// A typed object whose only purpose is to hold a value.  For constants, the values is
    /// pre-stored in the cell. For variables, it's stored during write-mode matching.
    /// </summary>
    public abstract class ValueCell
    {
        /// <summary>
        /// Name to use for cells representing constants
        /// </summary>
        protected const string ConstantName = " const ";
        
        /// <summary>
        /// Name, for debugging purposes
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Make a cell with the specified name (name used only for debugging)
        /// </summary>
        protected ValueCell(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Is this the value cell for a variable or for a constant?
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public bool IsVariable => Name != ConstantName;

        /// <summary>
        /// Value stored in the cell, typed as object
        /// </summary>
        public abstract object? BoxedValue { get; }
    }

    /// <summary>
    /// A container for a run-time value manipulated by Calls in a Rule
    /// These are used to hold:
    /// - The run-time values of variables (Vars) within a Rule.
    /// - Constants
    /// These get read and written through MatchInstructions, which get run when a Pattern is Matched
    /// or written to a row in a table using its Write method.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ValueCell<T> : ValueCell
    {
        /// <summary>
        /// Data stored in the cell.
        /// </summary>
        public T Value;

        /// <inheritdoc />
        public override object? BoxedValue => Value;

        /// <summary>
        /// Object used to test equality of two objects of type T
        /// We need this because C# generics don't allow you to use ==
        /// </summary>
        private static readonly EqualityComparer<T> Equal = EqualityComparer<T>.Default;

        /// <summary>
        /// Table of ValueCells used for different constants of type T
        /// </summary>
        private static readonly Dictionary<T, ValueCell<T>> ConstantTable = new Dictionary<T, ValueCell<T>>();
        
        private ValueCell(T value, string name) : base(name)
        {
            Value = value;
        }
        
        /// <summary>
        /// Make a ValueCell to hold the run-time value of a variable (a Var[T]).
        /// </summary>
        public static ValueCell<T> MakeVariable(string name) => new ValueCell<T>(default(T)!, name);

        /// <summary>
        /// Return a ValueCell holding the specified constant
        /// There will only be one such cell per constant.
        /// </summary>
        public static ValueCell<T> Constant(T value)
        {
            if (!ConstantTable.TryGetValue(value, out var c))
            {
                c = new ValueCell<T>(value, ConstantName);
                ConstantTable[value] = c;
            }

            return c;
        }

        /// <summary>
        /// Test of the cell holds the specified value
        /// </summary>
        public bool Match(in T value) => Equal.Equals(Value, value);
    }
}