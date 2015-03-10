using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codehard.CodeSharp
{
    internal class ActionType
    {
        public ActionType() { }
        public ActionType(Action<object> Callback, bool BindOnce)
        {
            this.Callback = Callback;
            this.BindOnce = BindOnce;
        }

        public Action<object> Callback { get; set; }
        public bool BindOnce { get; set; }
    }

    internal class EventCollection
    {
        public EventCollection() { }
        public EventCollection(Guid Id, string Event, List<ActionType> Callbacks)
        {
            this.Id = Id;
            this.Event = Event;
            this.Callbacks = Callbacks;
        }

        public Guid Id { get; set; }
        public string Event { get; set; }
        public List<ActionType> Callbacks { get; set; }
    }

    public class Emitter
    {
        private static List<EventCollection> EventCollection { get; set; }
        private Guid ID { get; set; }

        static Emitter()
        {
            EventCollection = new List<EventCollection>();
        }

        public Emitter()
        {
            ID = Guid.NewGuid();
        }

        private void _Create(string Event, ActionType Callback)
        {
            var EventList = GetEventList(this.ID, Event);

            if (EventList != null)
            {
                EventList.Add(Callback);
            }
            else
            {
                EventCollection.Add(new EventCollection(this.ID, Event, new List<ActionType> { Callback }));
            }
        }

        private void _Destroy(List<ActionType> EventList, Action<object> Callback)
        {
            EventList.RemoveAll(x => x.Callback == Callback);
        }

        private static void Invoke(List<ActionType> EventList, object Value)
        {
            if (EventList != null)
            {
                Parallel.Invoke(EventList.Select(x => (Action)(() => x.Callback(Value))).ToArray());
                EventList.RemoveAll(x => x.BindOnce);
            }
        }

        private static List<ActionType> GetEventList(string Event)
        {
            return EventCollection.Where(x => x.Event == Event).SelectMany(x => x.Callbacks).ToList();
        }

        private static List<ActionType> GetEventList(Guid Id, string Event)
        {
            return EventCollection.Where(x => x.Id == Id && x.Event == Event).Select(x => x.Callbacks).FirstOrDefault();
        }

        public void Emit(string Event, object Value)
        {
            Invoke(GetEventList(this.ID, Event), Value);
        }

        public void Boardcast(string Event, object Value)
        {
            Invoke(GetEventList(Event), Value);
        }

        public void Once(string Event, Action<object> Callback)
        {
            _Create(Event, new ActionType(Callback, true));
        }

        public void On(string Event, Action<object> Callback)
        {
            _Create(Event, new ActionType(Callback, false));
        }

        public void UnBind(string Event, Action<object> Callback)
        {
            _Destroy(GetEventList(this.ID, Event), Callback);
        }
    }

    public class DataStream : Emitter, IDisposable
    {
        private StreamWriter Stream { get; set; }
        private bool Disposed = false;

        protected virtual void Dispose(bool Disposing)
        {
            if (!Disposed)
            {
                if (Disposing)
                {
                    if (Stream != null)
                    {
                        Stream.Dispose();
                    }
                }

                Disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    class Program : Emitter
    {
        static void Main(string[] args)
        {
            Program Event1 = new Program();
            Action<object> b = (a) => { Console.WriteLine("1" + a); };
            Event1.On("Hello", b);
            Event1.Emit("Hello", "Emit1");
            Event1.UnBind("Hello", b);
            Event1.Emit("Hello", "Emit2");

            Program Event2 = new Program();
            Event2.On("Hello", (a) => { Console.WriteLine("2" + a); });
            Event2.Emit("Hello", "Emit3");

            Event1.Boardcast("Hello", "Boardcast");

            Console.Read();
        }
    }
}
