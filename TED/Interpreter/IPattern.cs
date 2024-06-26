﻿namespace TED.Interpreter
{
    /// <summary>
    /// Interface common to all the different generic Pattern types and their instantiations
    /// Functions as a base class for the patterns (can't use a real base class because they're structs)
    /// </summary>
    public interface IPattern
    {
        /// <summary>
        /// True if all the arguments in the pattern are instantiated.
        /// That is, they are either constants or variables that have already been given values by some previous call in the rule.
        /// </summary>
        bool IsInstantiated { get; }

        /// <summary>
        /// True if the index'th input is read mode
        /// </summary>
        bool IsReadModeAt(int index);

        /// <summary>
        /// The cell backing the specified argument
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        ValueCell ArgumentCell(int index);

        /// <summary>
        /// The MatchOperation[T] operations for each of the arguments in the pattern.
        /// </summary>
        public IMatchOperation[] Arguments { get; }
    }
}
