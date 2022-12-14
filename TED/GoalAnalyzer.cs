﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace TED
{
    /// <summary>
    /// Keeps the state involved in scanning the body of a rule:
    /// - What TablePredicates does this rule depend on (reference)
    /// - What variables appear in the rule?
    /// - What ValueCells do they correspond to?
    /// - Have they been bound yet?
    /// </summary>
    internal class GoalAnalyzer
    {
        private GoalAnalyzer(Dictionary<AnyTerm, object> variableCells, HashSet<TablePredicate> dependencies)
        {
            variableValueCells = variableCells;
            tables = dependencies;
        }

        public GoalAnalyzer() : this(new Dictionary<AnyTerm, object>(), new HashSet<TablePredicate>())
        { }

        /// <summary>
        /// Makes a goal analyzer identical to this one, except that any subsequent value cells won't be added to this analyzer
        /// Dependencies will be added, however.
        /// </summary>
        public GoalAnalyzer MakeChild()
            => new GoalAnalyzer(new Dictionary<AnyTerm, object>(variableValueCells), tables);
        

        private readonly Dictionary<AnyTerm, object> variableValueCells;
        private readonly HashSet<TablePredicate> tables;

        public void AddDependency(TablePredicate p) => tables.Add(p);

        public TablePredicate[] Dependencies => tables.ToArray();

        public MatchOperation<T> Emit<T>(Term<T> term)
        {
            if (term is Constant<T> c)
                return MatchOperation<T>.Constant(c.Value);
            // it's a variable
            if (!(term is Var<T> v))
                throw new Exception($"{term} cannot be used as an argument to a predicate");
            if (variableValueCells.TryGetValue(v, out var cell))
                return MatchOperation<T>.Read((ValueCell<T>)cell);
            var vc = ValueCell<T>.MakeVariable(v.Name);
            variableValueCells[v] = vc;
            return MatchOperation<T>.Write(vc);
        }
    }
}
