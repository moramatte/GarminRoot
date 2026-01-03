using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Infrastructure.Extensions;

namespace Infrastructure.EventAggregator {
    /// <summary>
    ///   A marker interface for classes that subscribe to messages.
    /// </summary>
    public interface IHandle { }

    /// <summary>
    /// Tests might want to receive all published events
    /// </summary>
    public interface IEventReceiver
    {
        void ReceiveEvent(object payload);
    }

    /// <summary>
    ///   Denotes a class which can handle a particular type of message.
    /// </summary>
    /// <typeparam name = "TMessage">The type of message to handle.</typeparam>
    public interface IHandle<TMessage> : IHandle {//don't use contravariance here
        /// <summary>
        ///   Handles the message.
        /// </summary>
        /// <param name = "message">The message.</param>
        void Handle(TMessage message);
    }

    /// <summary>
    ///   Enables loosely-coupled publication of and subscription to events.
    /// </summary>
    public interface IPocoEventAggregator {
        /// <summary>
        ///   Gets or sets the default publication thread marshaller.
        /// </summary>
        /// <value>
        ///   The default publication thread marshaller.
        /// </value>
        Action<System.Action> PublicationThreadMarshaller { get; set; }

        /// <summary>
        /// Searches the subscribed handlers to check if we have a handler for
        /// the message type supplied.
        /// </summary>
        /// <param name="messageType">The message type to check with</param>
        /// <returns>True if any handler is found, false if not.</returns>
        bool HandlerExistsFor(Type messageType);

        void Subscribe<T>(Action<T> eventReceiver);

        void Subscribe<T>(Func<T, Task> eventType);

		/// <summary>
		///   Unsubscribes the instance from all events.
		/// </summary>
		/// <param name = "subscriber">The instance to unsubscribe.</param>
		void Unsubscribe(object subscriber);

        /// <summary>
        ///   Publishes a message.
        /// </summary>
        /// <param name = "message">The message instance.</param>
        /// <remarks>
        ///   Uses the default thread marshaller during publication.
        /// </remarks>
        void Publish(object message);

		/// <summary>
		///   Publishes a message. Awaitable
		/// </summary>
		/// <param name = "message">The message instance.</param>
		Task PublishAsync(object message);

		/// <summary>
		///   Publishes a message.
		/// </summary>
		/// <param name = "message">The message instance.</param>
		/// <param name = "marshal">Allows the publisher to provide a custom thread marshaller for the message publication.</param>
		void Publish(object message, Action<System.Action> marshal);
    }

    /// <summary>
    ///   Enables loosely-coupled publication of and subscription to events.
    /// </summary>
    public class PocoEventAggregator : IPocoEventAggregator
    {
        /// <summary>
        /// The default thread marshaller used for publication;
        /// </summary>
        public static Action<System.Action> DefaultPublicationThreadMarshaller = action => action();

        /// <summary>
        /// Processing of handler results on publication thread.
        /// </summary>
        public static Action<object, object> HandlerResultProcessing = (target, result) => { };

        /// <summary>
        /// Constructor
        /// </summary>
        public PocoEventAggregator() {
            PublicationThreadMarshaller = DefaultPublicationThreadMarshaller;
        }

        /// <summary>
        /// Gets or sets the default publication thread marshaller.
        /// </summary>
        /// <value>
        /// The default publication thread marshaller.
        /// </value>
        public Action<System.Action> PublicationThreadMarshaller { get; set; }

        readonly List<WeakReferenceMethodHandler> handlers = new List<WeakReferenceMethodHandler>();

        /// <summary>
        /// Searches the subscribed handlers to check if we have a handler for
        /// the message type supplied.
        /// </summary>
        /// <param name="messageType">The message type to check with</param>
        /// <returns>True if any handler is found, false if not.</returns>
        public bool HandlerExistsFor(Type messageType) {
                return handlers.Any(handler => handler.Handles(messageType) & !handler.IsDead);
        }

        /// <summary>
        /// Unsubscribes the instance from all events.
        /// </summary>
        /// <param name = "subscriber">The instance to unsubscribe.</param>
        public virtual void Unsubscribe(object subscriber) {
            if (subscriber == null) {
                throw new ArgumentNullException("subscriber");
            }

            lock (handlers) {
                var found = handlers.Where(x => x.Matches(subscriber)).ToList();

                foreach (var weakReferenceMethodHandler in found)
                {
                    handlers.Remove(weakReferenceMethodHandler);
                }             
            }
        }

        /// <summary>
        /// Publishes a message.
        /// </summary>
        /// <param name = "message">The message instance.</param>
        /// <remarks>
        ///  Does not marshall the the publication to any special thread by default.
        /// </remarks>
        public virtual void Publish(object message) {
            if (message == null) {
                throw new ArgumentNullException("message");
            }

            Publish(message, PublicationThreadMarshaller);
        }

		public virtual Task PublishAsync(object message)
		{
			if (message == null)
			{
				throw new ArgumentNullException("message");
			}

			return PublishAsync(message, PublicationThreadMarshaller);
		}

		/// <summary>
		/// Publishes a message.
		/// </summary>
		/// <param name = "message">The message instance.</param>
		/// <param name = "marshal">Allows the publisher to provide a custom thread marshaller for the message publication.</param>
		[DebuggerStepThrough]
		public virtual void Publish(object message, Action<System.Action> marshal) {
            if (message == null) {
                throw new ArgumentNullException("message");
            }

            if (marshal == null) {
                throw new ArgumentNullException("marshal");
            }

            WeakReferenceMethodHandler[] toNotify;
            lock (handlers) {
                toNotify = handlers.ToArray();
            }

            marshal(() => {
                var messageType = message.GetType();

                List<WeakReferenceMethodHandler> dead = [];
                List<Exception> exceptions = [];
                foreach (var handler in toNotify)
                {
                    try
                    {
                        if (!handler.Handle(messageType, message))
                            dead.Add(handler);
                    }
                    catch (Exception e)
                    {
                        exceptions.Add(e);
                    }
                }

                if (dead.Any()) {
                    lock (handlers) {
                        dead.Apply(x => handlers.Remove(x));
                    }
                }

                if (exceptions.Any())
                    throw new AggregateException(exceptions);
            });
        }

		public virtual async Task PublishAsync(object message, Action<System.Action> marshal)
		{
			if (message == null)
			{
				throw new ArgumentNullException("message");
			}

			if (marshal == null)
			{
				throw new ArgumentNullException("marshal");
			}

			WeakReferenceMethodHandler[] toNotify;
			lock (handlers)
			{
				toNotify = handlers.ToArray();
			}

			var messageType = message.GetType();
			List<Exception> exceptions = [];
			foreach (var handler in toNotify)
            {
                try
                {
                    await handler.HandleAsync(messageType, message);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

			foreach (var dead in toNotify.Where(handler => handler.IsDead))
			{
				lock (handlers)
				{
                    handlers.Remove(dead);
				}
			}

            if (exceptions.Any())
                throw new AggregateException(exceptions);
		}

		public void Subscribe<T>(Action<T> eventReceiver)
		{
			if (eventReceiver == null)
			{
				throw new ArgumentNullException("eventReceiver");
			}

            Subscribe(eventReceiver.Target, eventReceiver.Method);
		}

		public void Subscribe<T>(Func<T, Task> eventReceiver)
		{
			if (eventReceiver == null)
			{
				throw new ArgumentNullException("eventReceiver");
			}

			Subscribe(eventReceiver.Target, eventReceiver.Method);
		}

        private void Subscribe(object target, MethodInfo method)
        {
			lock (handlers)
			{
				if (handlers.Any(x => x.Handles(target, method)))
				{
					return;
				}

				if (handlers.TryFind(x => x.Matches(target), out var handler))
					handler.Handle(method);
				else
					handlers.Add(new WeakReferenceMethodHandler(target, method));
			}
		}

        [DebuggerDisplay("{reference.Target.GetType()}, SupportedHandlers={Handlers}")]
        public class WeakReferenceMethodHandler
        {
            readonly WeakReference reference;
            readonly Dictionary<Type, List<MethodInfo>> supportedHandlers = new();

            public string Handlers =>
                $"{supportedHandlers.Select(kvp => $"{kvp.Key}-[{kvp.Value.Select(mi => mi.Name).ToCommaSeparatedString()}]")}";

        public bool IsDead {
                get { return reference.Target == null; }
            }

            public WeakReferenceMethodHandler(object handler, MethodInfo method) {
                reference = new WeakReference(handler);
                Handle(method);
            }

            public void Handle(MethodInfo handleMethod)
            {
                if (handleMethod == null)
                {
                    throw new ArgumentNullException(nameof(handleMethod));
                }

				foreach (var handleMethodArgument in handleMethod.GetParameters())
				{
					if (!supportedHandlers.ContainsKey(handleMethodArgument.ParameterType))
						supportedHandlers[handleMethodArgument.ParameterType] = [];
					supportedHandlers[handleMethodArgument.ParameterType].Add(handleMethod);
				}
			}

			public bool Matches(object instance)
			{
				return reference.Target == instance;
			}

			public bool Handles(object target, MethodInfo method)
			{
				return reference.Target == target && supportedHandlers.Any(x => x.Value.Any(y => y == method));
			}

			public bool Handle(Type messageType, object message) {
                var target = reference.Target;
                if (target == null) {
                    return false;
                }

                foreach (var pair in supportedHandlers) {
                    if (pair.Key.IsAssignableFrom(messageType)) {
                        foreach (var handler in pair.Value)
                        {
                            var result = handler.Invoke(target, PackParameters(handler, message));
                            if (result != null) {
                                HandlerResultProcessing(target, result);
                            }
                        }
                    }
                }

                return true;
            }

			public async Task HandleAsync(Type messageType, object message)
			{
				var target = reference.Target;
				if (target == null)
				{
					return;
				}

				foreach (var pair in supportedHandlers)
				{
					if (pair.Key.IsAssignableFrom(messageType))
					{
                        foreach (var handler in pair.Value)
                        {
						    var result = handler.Invoke(target, PackParameters(handler, message));
						    if (result is Task task)
						    {
                                await task;
						    }
                        }
					}
				}
			}

			private object[] PackParameters(MethodInfo methodInfo, object payload)
            {
                var parameters = methodInfo.GetParameters();
                if (parameters.Length == 1)
                {
                    return [payload];
                }

                var result = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    var arg = parameters[i];
                    result[i] = arg.ParameterType == payload.GetType() ? payload : null;
                }

                return result;
            }

            public bool Handles(Type messageType) {
                return supportedHandlers.Any(pair => pair.Key.IsAssignableFrom(messageType));
            }
        }
    }
}
