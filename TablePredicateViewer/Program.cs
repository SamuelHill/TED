using TED;
using static TED.Language;

namespace TablePredicateViewer
{
    internal static class Program
    {
        public static Simulation Simulation = null!;

        enum Status
        {
            Alive,
            Dead
        };

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread] 
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            Simulation = new Simulation("Test");
            Simulation.BeginPredicates();
            
            // Variables for rules
            var person = (Var<string>)"person";
            var name = (Var<string>)"name";
            var sex = (Var<string>)"sex";
            var age = (Var<int>)"age";
            var woman = (Var<string>)"woman";
            var man = (Var<string>)"man";
            var status = (Var<Status>)"status";

            // ReSharper disable InconsistentNaming

            // Predicates loaded from disk
            var Person = TablePredicate<string, string, int, Status>.FromCsv("../../../Population.csv", person.Key, sex.Indexed, age, status.Indexed);
            var MaleName = TablePredicate<string>.FromCsv("../../../male_name.csv", name);
            var FemaleName = TablePredicate<string>.FromCsv("../../../female_name.csv", name);

            // Death
            var Dead = Definition("Dead", person).Is(Person[person, sex, age, Status.Dead]);
            var Alive = Definition("Alive", person).Is(Person[person, sex, age, Status.Alive]);

            //var Died = Predicate("Died", person).If(Person, Prob[0.01f], Alive[person]);
            Person.Set(person, status, Status.Dead).If(Alive[person], Prob[0.01f]);

            // Birth
            var Man = Predicate("Man", person).If(Person[person, "m", age, Status.Alive], age > 18);
            var Woman = Predicate("Woman", person).If(Person[person, "f", age, Status.Alive], age > 18);
            var BirthTo = Predicate("BirthTo", woman, man, sex)
                    .If(Woman[woman], Prob[0.1f], RandomElement(Man, man), PickRandomly(sex, "f", "m"));

            // Naming of newborns
            var NewBorn = Predicate("NewBorn", person, sex, age, status);
            NewBorn[person, "f", 0, Status.Alive].If(BirthTo[man, woman, "f"], RandomElement(FemaleName, person));
            NewBorn[person, "m", 0, Status.Alive].If(BirthTo[man, woman, "m"], RandomElement(MaleName, person));

            // Add births to the population
            Person.Accumulates(NewBorn);
            // ReSharper restore InconsistentNaming
            Simulation.EndPredicates();

            var timer = new System.Windows.Forms.Timer();
            timer.Tick += (_, _) => { UpdateCycle(Person); };
            timer.Interval = 100;
            timer.Start();
            PredicateViewer.ShowPredicates(Person, BirthTo, NewBorn, Woman);
            Application.Run(PredicateViewer.Of(Person));
        }

        private static void UpdateCycle(TablePredicate<string, string, int, Status> person)
        {
            person.UpdateRows((ref (string name, string sex, int age, Status status) row) => row.age++);
            Simulation.Update();
            PredicateViewer.UpdateAll();
        }
    }
}