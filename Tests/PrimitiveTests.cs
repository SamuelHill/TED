﻿using TED;
using TED.Interpreter;
using static TED.Language;

namespace Tests
{
    [TestClass]
    public class PrimitiveTests
    {
        [TestMethod]
        public void LessThanTest()
        {
            var x = (Var<int>)"x";
            var y = (Var<int>)"y";

            var t = new TablePredicate<int, int>("t", x,y);
            for (var i = 0; i < 10; i++)
            for (var j = 0; j < 10; j++)
                t.AddRow(i, j);

            var s = new TablePredicate<int, int>("s", x, y);

            s[x, y].If(t[x, y], x < y);

            var hits = s.ToArray();
            for (var j = 0; j < 10; j++)
            {
                Assert.AreEqual(45, hits.Length);
                foreach (var (a, b) in hits)
                    Assert.IsTrue(a < b);
            }
        }

        readonly struct Vector2Int
        {
            public readonly int X;
            public readonly int Y;

            public static Vector2Int Zero = new Vector2Int(0, 0);
            public static Vector2Int UnitX = new Vector2Int(1, 0);
            public static Vector2Int UnitY = new Vector2Int(0, 1);

            public Vector2Int(int x, int y)
            {
                X = x;
                Y = y;
            }

            public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new Vector2Int(a.X + b.X, a.Y+b.Y);
        }

        [TestMethod]
        public void SamTest()
        {
            var neighborhood = new[] { new Vector2Int(0, 1), new Vector2Int(1, 1),
                new Vector2Int(1, 0), new Vector2Int(1, -1),
                new Vector2Int(0, -1), new Vector2Int(-1, -1),
                new Vector2Int(-1, 0), new Vector2Int(-1, 1) };
            var cell =     (Var<Vector2Int>)"cell";
            var neighbor = (Var<Vector2Int>)"neighbor";
            var temp =     (Var<Vector2Int>)"temp";
            //var where = (Var<Vector2Int>)"where";
            var count = (Var<int>)"count";
            var b = (Var<bool>)"b";
            //var Neighbor = Definition("Neighbor", cell, neighbor).IfAndOnlyIf(In(temp, neighborhood), neighbor == cell + temp);

            var tileTable = Predicate("tileTable", cell, b);
            tileTable.IndexByKey(cell);
            var index = tileTable.KeyIndex(cell);
            tileTable.AddRow(Vector2Int.Zero, true);

            var NeighborCount = Predicate("NeighborCount", cell, count);
            NeighborCount[cell, count].If(tileTable[cell, b], count == Count(And[In(temp, neighborhood), neighbor == cell + temp, tileTable[neighbor, true]]));
            //NeighborCount[where, count].If(tileTable[cell, b], count == Count(And[Neighbor[where, neighbor], tileTable[neighbor, true]]));
            //NeighborCount[where, Count(And[Neighbor[where, neighbor], tileTable[neighbor, true]])].If(tileTable[where, b]);

            var x = (Var<Vector2Int>)"x";
            var s = new TablePredicate<Vector2Int>("s", x);
            s[x].If(tileTable[x, true], Constant(1) <= Constant(2));

            s[x].If(tileTable[x, true], NeighborCount[x, count], count < 2);
            Assert.IsTrue(s.Any());
        }

        [TestMethod]
        public void NegationTest()
        {
            var x = (Var<int>)"x";
            var y = (Var<int>)"y";
            var t = new TablePredicate<int, int>("t", x, y);
            for (var i = 0; i < 10; i++)
            for (var j = 0; j < 10; j++)
                t.AddRow(i, j);

            var s = new TablePredicate<int, int>("s", x, y);


            s[x, y].If(t[x, y], x < y);

            var u = new TablePredicate<int, int>("u", x, y);
            var v = new TablePredicate<int, int>("v", x, y);

            u[x,y].If(t[x,y], !s[x,y]);
            v[x,y].If(t[x,y], !(x < y));

            var hits = u.ToArray();
            for (var j = 0; j < 10; j++)
            {
                Assert.AreEqual(55, hits.Length);
                foreach (var (a, b) in hits)
                    Assert.IsTrue(a >= b);
            }
        }

        [TestMethod]
        public void OnceTest()
        {
            var p = Predicate("p", new[] { 1, 2, 3 });
            var x = (Var<int>)"x";
            var q = Predicate("q", x).If(Once[p[x]]);
            CollectionAssert.AreEqual(new[] {1 }, q.ToArray());
        }

        [TestMethod]
        public void LimitSolutionsTest()
        {
            var p = Predicate("p", new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            var x = (Var<int>)"x";
            var q = Predicate("q", x).If(LimitSolutions[5, p[x]]);
            CollectionAssert.AreEqual(new[] {1, 2, 3, 4, 5 }, q.ToArray());
        }

        [TestMethod]
        public void AndTest()
        {
            var x = (Var<int>)"x";
            var y = (Var<int>)"y";

            var t = new TablePredicate<int, int>("t", x, y);
            for (var i = 0; i < 10; i++)
            for (var j = 0; j < 10; j++)
                t.AddRow(i, j);

            var u = new TablePredicate<int>("u", x);
            u.AddRow(2);
            u.AddRow(4);
            u.AddRow(6);

            var s = new TablePredicate<int>("s", x);

            s[x].If(And[t[x,x], And[u[x]]]);

            var hits = s.ToArray();
            Assert.AreEqual(3, hits.Length);
            Assert.AreEqual(2, hits[0]);
            Assert.AreEqual(4, hits[1]);
            Assert.AreEqual(6, hits[2]);
        }

        [TestMethod]
        public void AndFlattening()
        {
            var n = (Var<int>)"n";
            var p = Predicate("p", n);
            var g = p[0] & p[1] & p[2];
            var conjuncts = g.Arguments.Select(c => ((Constant<Goal>)c).Value).ToArray();
            Assert.AreEqual(3, conjuncts.Length);
            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(p, conjuncts[i].Predicate);
                Assert.AreEqual(i, ((Constant<int>)conjuncts[i].Arguments[0]).Value);
            }
        }

        [TestMethod]
        public void AndSimplification()
        {
            var n = (Var<int>)"n";
            var p = Predicate("p", n);
            // p & true = p
            Assert.AreEqual(p, (p[0] & True).Predicate);
            // p & false = false
            Assert.AreEqual(False, (p[0] & False).Predicate);
        }

        [TestMethod]
        public void OrInstantiated()
        {
            var x = (Var<int>)"x";
            var y = (Var<int>)"y";

            var t = new TablePredicate<int, int>("t", x, y);
            for (var i = 0; i < 10; i++)
            for (var j = 0; j < 10; j++)
                t.AddRow(i, j);

            var u = new TablePredicate<int>("u", x);
            u.AddRow(2);
            u.AddRow(4);
            u.AddRow(6);

            var s = new TablePredicate<int>("s", x);

            s[x].If(And[t[x,x], And[u[x]]]);

            var v = Predicate("v", new[] { 8, 10 });

            var w = Predicate("w", x).If(Or[s[x], v[x]]);

            CollectionAssert.AreEqual(new[] { 2, 4, 6, 8, 10 }, w.ToArray());
        }

        [TestMethod, ExpectedException(typeof(InstantiationException))]
        public void OrUninstantiated()
        {
            var x = (Var<int>)"x";
            var y = (Var<int>)"y";

            var t = new TablePredicate<int, int>("t", x, y);
            for (var i = 0; i < 10; i++)
            for (var j = 0; j < 10; j++)
                t.AddRow(i, j);

            var u = new TablePredicate<int>("u", x);
            u.AddRow(2);
            u.AddRow(4);
            u.AddRow(6);

            var s = new TablePredicate<int>("s", x);

            s[x].If(And[t[x,x], And[u[x]]]);

            var v = Predicate("v", new[] { 8, 10 });

            var w = Predicate("w", x).If(Or[s[x], v[1]]);

            CollectionAssert.AreEqual(new[] { 2, 4, 6, 8, 10 }, w.ToArray());
        }

        [TestMethod]
        public void FirstOfTest()
        {
            var n = (Var<int>)"n";
            var tag = (Var<string>)"tag";

            var num = Predicate("num", new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            var odd = Predicate("odd", new[] { 1, 3, 5, 7, 9 });
            var div3 = Predicate("div3", new[] { 0, 3, 6, 9 });

            var tagOf = Predicate("tagOf", n, tag)
                .If(num[n],
                    FirstOf[And[div3[n], tag=="div3"],
                            And[odd[n], tag == "odd"],
                            tag == "even"]);
            CollectionAssert.AreEqual(
                new (int,string)[]
                {
                    (0, "div3"),
                    (1, "odd"),
                    (2, "even"),
                    (3, "div3"),
                    (4, "even"),
                    (5, "odd"),
                    (6, "div3"),
                    (7, "odd"),
                    (8, "even"),
                    (9, "div3")
                },
                tagOf.ToArray());
        }

        [TestMethod]
        public void NegatedDefinitionTest()
        {
            var n = (Var<int>)"n";
            var A = Predicate("A", new[] { 1, 2, 3, 4, 5, 6 }, n);
            var B = Predicate("B", new[] { 1, 2, 3, 4 }, n);
            var C = Predicate("C", new[] { 3, 4, 5, 6 }, n);
            var D = Definition("D", n).Is(B[n], C[n]);
            var E = Predicate("E", n).If(A[n], !D[n]);
            var results = E.ToArray();
            Assert.AreEqual(4, results.Length);
            Assert.AreEqual(1, results[0]);
            Assert.AreEqual(2, results[1]);
            Assert.AreEqual(5, results[2]);
            Assert.AreEqual(6, results[3]);
        }

        [TestMethod]
        public void InTestModeTest()
        {
            var c = new[] { 1, 2, 3, 4, 5 };
            var n = (Var<int>)"n";
            var Test = Predicate("Test", n);
            Test[0].If(In<int>(0, c));
            Test[1].If(In<int>(4, c));
            CollectionAssert.AreEqual(new[]{1}, Test.ToArray());
        }

        [TestMethod]
        public void InGenerateModeTest()
        {
            var c = new[] { 1, 2, 3, 4, 5 };
            var n = (Var<int>)"n";
            var Test = Predicate("Test", n);
            Test[n].If(In<int>(n, c));  // Should just copy c into Test
            CollectionAssert.AreEqual(c, Test.ToArray());
        }

        [TestMethod]
        public void MaximalTest()
        {
            var name = (Var<string>)"name";
            var age = (Var<int>)"age";

            var t = TablePredicate<string, int>.FromCsv("test", "../../../TestTable.csv", name, age);

            var floatAge = (Var<float>)"floatAge";
            var M = Predicate("M", name, floatAge).If(Maximal(name, floatAge, And[t[name, age], floatAge == Float[age]]));
            var rows = M.ToArray();
            Assert.AreEqual(1,rows.Length);
            Assert.AreEqual(("Jenny", 12.0f), rows[0]);
        }

        [TestMethod]
        public void MinimalTest()
        {
            var name = (Var<string>)"name";
            var age = (Var<int>)"age";

            var t = TablePredicate<string, int>.FromCsv("test", "../../../TestTable.csv", name, age);
            var floatAge = (Var<float>)"floatAge";
            var M = Predicate("M", name, floatAge).If(Minimal(name, floatAge, And[t[name, age], floatAge == Float[age]]));
            var rows = M.ToArray();
            Assert.AreEqual(1,rows.Length);
            Assert.AreEqual(("Elroy", 9.0f), rows[0]);
        }

        [TestMethod]
        public void TwoArgMaximalTest()
        {
            var name = (Var<string>)"name";
            var family = (Var<string>)"family";
            var age = (Var<float>)"age";
            var t = Predicate("t",
                new (string, string, float)[]
                {
                    ("billie", "george", 36),
                    ("george", "billie", 37),
                    ("sandra", "mitchell", 24)
                },
                name,
                family,
                age);

            var M = Predicate("M", name, family, age).If(Maximal((name, family), age, t[name, family, age]));
            var rows = M.ToArray();
            Assert.AreEqual(1,rows.Length);
            Assert.AreEqual(("george", "billie", 37.0f), rows[0]);
        }

        [TestMethod]
        public void TwoArgMinimalTest()
        {
            var name = (Var<string>)"name";
            var family = (Var<string>)"family";
            var age = (Var<float>)"age";
            var t = Predicate("t",
                new (string, string, float)[]
                {
                    ("billie", "george", 36),
                    ("sandra", "mitchell", 24),
                    ("george", "billie", 37)
                },
                name,
                family,
                age);

            var M = Predicate("M", name, family, age).If(Minimal((name, family), age, t[name, family, age]));
            var rows = M.ToArray();
            Assert.AreEqual(1,rows.Length);
            Assert.AreEqual(("sandra", "mitchell", 24.0f), rows[0]);
        }

        [TestMethod]
        public void ThreeArgMaximalTest()
        {
            var name = (Var<string>)"name";
            var family = (Var<string>)"family";
            var gender = (Var<string>)"gender";
            var age = (Var<float>)"age";

            var t = Predicate("t",
                new (string, string, string, float)[]
                {
                    ("billie", "george", "m", 36),
                    ("sandra", "mitchell", "f", 44),
                    ("george", "billie", "m", 37)
                },
                name,
                family,
                gender,
                age);

            var M = Predicate("M", name, family, gender, age).If(Maximal((name, family, gender), age, t[name, family, gender, age]));
            var rows = M.ToArray();
            Assert.AreEqual(1,rows.Length);
            Assert.AreEqual(("sandra", "mitchell", "f", 44.0f), rows[0]);
        }

        [TestMethod]
        public void AssertInIn()
        {
            var n = (Var<int>)"n";
            Predicate("test", n).If(n == 1, AssertIn(n, "Should be in"));
        }

        [TestMethod, ExpectedException(typeof(InstantiationException))]
        public void AssertInOut()
        {
            var n = (Var<int>)"n";
            Predicate("test", n).If(AssertIn(n, "Should be in"), n==1);
        }

        [TestMethod, ExpectedException(typeof(InstantiationException))]
        public void AssertOuIn()
        {
            var n = (Var<int>)"n";
            Predicate("test", n).If(n == 1, AssertOut(n, "Should be out"));
        }

        [TestMethod]
        public void AssertOutOut()
        {
            var n = (Var<int>)"n";
            Predicate("test", n).If(AssertOut(n, "Should be out"), n==1);
        }

        [TestMethod]
        public void CaptureState()
        {
            var n = (Var<int>)"n";
            var m = (Var<int>)"m";
            var test = Predicate("test", n, m, TED.Primitives.CaptureDebugStatePrimitive.DebugState)
                .If(n == 1, m == n + 1);
            Assert.AreEqual(1u, test.Length);
            var dict = test.First().Item3;
            Assert.AreEqual(2, dict.Count);
            foreach (var pair in dict)
                if (ReferenceEquals(pair.Key, n))
                    Assert.AreEqual(1, pair.Value);
                else if (ReferenceEquals(pair.Key, m))
                    Assert.AreEqual(2, pair.Value);
                else
                    throw new Exception($"Unexpected variable in captured state: {pair.Key}");
        }
    }
}
