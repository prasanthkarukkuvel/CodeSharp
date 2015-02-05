using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActionType = System.Action<object>;

namespace Codehard.CodeSharp
{
    public class Emitter
    {
        private static List<Tuple<Guid, string, List<ActionType>>> EventCollection { get; set; }
        private Guid ID { get; set; }

        static Emitter()
        {
            EventCollection = new List<Tuple<Guid, string, List<ActionType>>>();
        }

        public Emitter()
        {
            ID = Guid.NewGuid();
        }

        private static void Invoke(List<ActionType> EventList, object Value)
        {
            if (EventList != null)
            {
                Parallel.Invoke(EventList.Select(x => (Action)(() => x(Value))).ToArray());
            }
        }

        private static List<ActionType> GetEventList(string Event)
        {
            return EventCollection.Where(x => x.Item2 == Event).SelectMany(x => x.Item3).ToList();
        }

        private static List<ActionType> GetEventList(Guid ID, string Event)
        {
            return EventCollection.Where(x => x.Item1 == ID && x.Item2 == Event).Select(x => x.Item3).FirstOrDefault();
        }        

        public void Emit(string Event, object Value)
        {
            Invoke(GetEventList(this.ID, Event), Value);
        }

        public void Boardcast(string Event, object Value)
        {
            Invoke(GetEventList(Event), Value);
        }

        public void On(string Event, ActionType Callback)
        {
            var EventList = GetEventList(this.ID, Event);

            if (EventList != null)
            {
                EventList.Add(Callback);
            }
            else
            {
                EventCollection.Add(new Tuple<Guid, string, List<ActionType>>(this.ID, Event, new List<ActionType> { Callback }));
            }
        }
    }

    class Program : Emitter
    {
        static void Main(string[] args)
        {
            Program Event1 = new Program();
            Event1.On("Hello", (a) => { Console.WriteLine("1" + a); });
            Event1.Emit("Hello", "Emit");

            Program Event2 = new Program();
            Event2.On("Hello", (a) => { Console.WriteLine("2" + a); });
            Event2.Emit("Hello", "Emit");

            Event1.Boardcast("Hello", "Boardcast");

            Console.Read();
        }
    }
}
