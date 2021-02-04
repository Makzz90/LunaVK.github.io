using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LunaVK.Core.Utils;
using LunaVK.Core.Framework;

namespace LunaVK.Core.Framework
{
    public class EventAggregator1
    {
        public static Action<Action> DefaultPublicationThreadMarshaller = (action => Execute.ExecuteOnUIThread(action));
        private readonly List<EventAggregator1.Handler> handlers = new List<EventAggregator1.Handler>();

        private static EventAggregator1 _current;
        public static EventAggregator1 Instance
        {
            get
            {
                if (EventAggregator1._current == null)
                    EventAggregator1._current = new EventAggregator1();
                return EventAggregator1._current;
            }
        }

        public Action<Action> PublicationThreadMarshaller { get; set; }

        public EventAggregator1()
        {
            this.PublicationThreadMarshaller = EventAggregator1.DefaultPublicationThreadMarshaller;
        }

        //was public virtual
        public void SubsribeEvent(object instance)
        {
            lock (this.handlers)
            {
                if (this.handlers.Any<EventAggregator1.Handler>((x => x.Matches(instance))))
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("[BUG]: SubsribeEvent" + instance.ToString());
#endif
                    return;
                }
#if DEBUG
                //System.Diagnostics.Debug.WriteLine("SubsribeEvent " + instance.ToString());
#endif
                this.handlers.Add(new EventAggregator1.Handler(instance));
            }
        }

        //was public virtual
        public void UnSubsribeEvent(object instance)
        {
            lock (this.handlers)
            {
                EventAggregator1.Handler handler = this.handlers.FirstOrDefault<EventAggregator1.Handler>((x => x.Matches(instance)));
                if (handler == null)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("[BUG]: UnSubsribeEvent" + instance.ToString());
#endif
                    return;
                }
#if DEBUG
                //System.Diagnostics.Debug.WriteLine("UnSubsribeEvent " + instance.ToString());
#endif
                this.handlers.Remove(handler);
            }
        }

        //was public virtual
        public void PublishEvent(object message)
        {
            this.Publish(message, this.PublicationThreadMarshaller);
        }

        //was public virtual
        private void Publish(object message, Action<Action> marshal)
        {
            EventAggregator1.Handler[] toNotify;
            lock (this.handlers)
                toNotify = this.handlers.ToArray();
            marshal(() =>
            {
                Type messageType = message.GetType();
                List<EventAggregator1.Handler> list = toNotify.Where<EventAggregator1.Handler>((handler => !handler.Handle(messageType, message))).ToList<EventAggregator1.Handler>();
                if (!list.Any<EventAggregator1.Handler>())
                    return;
                lock (this.handlers)
                    list.Apply<EventAggregator1.Handler>((x => this.handlers.Remove(x)));
            });
        }

        //public void PublishEvent<TEventType>(TEventType message)
        //{
        //    this.Publish(message, this.PublicationThreadMarshaller);
        //}
        
        protected class Handler
        {
            private readonly Dictionary<Type, MethodInfo> supportedHandlers = new Dictionary<Type, MethodInfo>();
            private readonly WeakReference reference;

            public Handler(object handler)
            {
                this.reference = new WeakReference(handler);
                
                Type type = handler.GetType();
                
                List<MethodInfo> methods = type.GetRuntimeMethods().Where((m) => { return m.Name.StartsWith("OnEventHandler"); }).ToList();
                
                for (int i = 0; i < methods.Count;i++ )
                {
                    MethodInfo m = methods[i];
                    ParameterInfo[] temp3 = m.GetParameters();


                    Type interf = temp3[0].ParameterType;//interfaces[i];
                    this.supportedHandlers[interf] = m;
                }
            }

            public bool Matches(object instance)
            {
                return this.reference.Target == instance;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="messageType">Это тип события, например events.counter changed</param>
            /// <param name="message">Собственно сами данные</param>
            /// <returns></returns>
            public bool Handle(Type messageType, object message)
            {
                object target = this.reference.Target;
                if (target == null)
                    return false;
                foreach (KeyValuePair<Type, MethodInfo> supportedHandler in this.supportedHandlers)
                {
                    if (supportedHandler.Key == messageType)
                    {
                        supportedHandler.Value.Invoke(target, new object[1] { message });
                        return true;
                    }
                }
                return true;
            }
        }

        
    }

    //public interface ISubscriber
    //{
    //}
    public interface ISubscriber<T>
    {
        void OnEventHandler(T e);
    }
}
