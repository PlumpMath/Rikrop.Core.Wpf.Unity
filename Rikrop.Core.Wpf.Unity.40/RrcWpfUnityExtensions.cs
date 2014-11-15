using System;
using System.Collections.Generic;
using System.Linq;
using Rikrop.Core.Exceptions;
using Rikrop.Core.Framework.Exceptions;
using Rikrop.Core.Wpf.Async;
using Rikrop.Core.Wpf.Exceptions;
using Rikrop.Core.Wpf.MessageRouting;
using Rikrop.Core.Wpf.Mvvm.Visualizer;
using Rikrop.Core.Wpf.Workspace;
using Microsoft.Practices.Unity;

namespace Rikrop.Core.Wpf.Unity
{
    public static class RrcWpfUnityExtensions
    {
        /// <summary>
        ///   Регистрирует для IExceptionHandler'a класс AggregatedExceptionHandler.
        /// </summary>
        public static IUnityContainer RegisterExceptionHandlers(this IUnityContainer container, Action<ExceptionHandlersRegistrator> registrator)
        {
            var eRegistrator = new ExceptionHandlersRegistrator(container);
            registrator(eRegistrator);


            var resolvedParameters = eRegistrator.GetRegisteredTypes().Select(o => (object) new ResolvedParameter(o)).ToArray();

            container.RegisterType<IExceptionHandler, AggregatedExceptionHandler>(new ContainerControlledLifetimeManager(),
                                                                                  new InjectionConstructor(new ResolvedArrayParameter<IExceptionHandler>(resolvedParameters)));

            return container;
        }

        /// <summary>
        ///   Регистрирует:
        ///   IWorkspaceVisualizator -> WorkspaceVisualizator.
        ///   IPopupVisualizer -> PopupVisualizer.
        ///   IPopupSource -> PopupVisualizer.
        /// </summary>
        public static IUnityContainer RegisterVisualizers(this IUnityContainer container)
        {
            container.RegisterType<IWorkspaceVisualizator, WorkspaceVisualizator>(new ContainerControlledLifetimeManager());
            
            container.RegisterType<IPopupVisualizer, PopupVisualizer>(new ContainerControlledLifetimeManager());
            container.RegisterType<IPopupSource, PopupVisualizer>(new ContainerControlledLifetimeManager());

            return container;
        }

        /// <summary>
        ///   Регистрирует:
        ///   IServiceExecutorFactory -> ServiceExecutorFactory.
        /// </summary>
        public static IUnityContainer RegisterWpfAsyncs(this IUnityContainer container)
        {
            container.RegisterType(typeof (IServiceExecutorFactory<>), typeof (ServiceExecutorFactory<>));

            return container;
        }

        /// <summary>
        ///   Регистрирует:
        ///   IDialogShower -> DialogShower.
        /// </summary>
        public static IUnityContainer RegisterDialogs(this IUnityContainer container, string dialogsTitle)
        {
            container.RegisterType<IDialogShower, DialogShower>(new ContainerControlledLifetimeManager(), new InjectionConstructor(dialogsTitle));

            return container;
        }

        /// <summary>
        ///   Регистрирует 
        ///   IMessageListener - MessageListener
        ///   IMessageSender - MessageListener
        /// </summary>
        public static IUnityContainer RegisterMessageRouting(this IUnityContainer container)
        {
            container.RegisterType(typeof (MessageListener<>), new ContainerControlledLifetimeManager());
            container.RegisterType(typeof (IMessageListener<>), typeof (MessageListener<>));
            container.RegisterType(typeof (IMessageSender<>), typeof (MessageListener<>));

            return container;
        }
    }

    public class ExceptionHandlersRegistrator
    {
        private readonly IUnityContainer _container;

        private readonly List<Type> _registeredTypes = new List<Type>();

        internal ExceptionHandlersRegistrator(IUnityContainer container)
        {
            _container = container;
        }

        internal IEnumerable<Type> GetRegisteredTypes()
        {
            return _registeredTypes;

            /*
             *                                  typeof (CatchedExceptionHandler),
                                 typeof (OperationCanceledExceptionHandler),
                                 typeof (BusinessExceptionHandler),
                                 typeof (SecurityAccessDeniedExceptionHandler),
                                 typeof (CommunicationExceptionHandler),
                                 typeof (ErrorWorkspaceExceptionHandler),

             * */
        }

        public ExceptionHandlersRegistrator CatchedExceptionHandler()
        {
            _registeredTypes.Add(typeof(CatchedExceptionHandler));
            
            return this;
        }

        public ExceptionHandlersRegistrator OperationCanceledExceptionHandler()
        {
            _registeredTypes.Add(typeof(OperationCanceledExceptionHandler));

            return this;
        }

        public ExceptionHandlersRegistrator BusinessExceptionHandler(TypedInjectionValue dialogShower, TypedInjectionValue businessExceptionDetailsConverter)
        {
            _registeredTypes.Add(typeof(BusinessExceptionHandler));
            _container.RegisterType<BusinessExceptionHandler>(new InjectionConstructor(dialogShower, businessExceptionDetailsConverter));

            return this;
        }

        public ExceptionHandlersRegistrator BusinessExceptionHandler(TypedInjectionValue dialogShower)
        {
           return BusinessExceptionHandler(dialogShower, new InjectionParameter(new EnumBusinessExceptionDetailsConverter()));
        }

        public ExceptionHandlersRegistrator BusinessExceptionHandler(IDialogShower dialogShower, IBusinessExceptionDetailsConverter businessExceptionDetailsConverter)
        {
            return BusinessExceptionHandler(new InjectionParameter(dialogShower), new InjectionParameter(businessExceptionDetailsConverter));
        }
        
        public ExceptionHandlersRegistrator BusinessExceptionHandler(IDialogShower dialogShower)
        {
            return BusinessExceptionHandler(new InjectionParameter(dialogShower));
        }

        public ExceptionHandlersRegistrator ErrorWorkspaceExceptionHandler()
        {
            _registeredTypes.Add(typeof(ErrorWorkspaceExceptionHandler));

            return this;
        }

        public ExceptionHandlersRegistrator Custom(Type iExceptionHandler, string name = null, LifetimeManager lifetimeManager = null, params InjectionMember[] injectionMembers)
        {
            _registeredTypes.Add(iExceptionHandler);
            _container.RegisterType(iExceptionHandler, name, lifetimeManager, injectionMembers);

            return this;
        }

        public ExceptionHandlersRegistrator Custom(Type iExceptionHandler, LifetimeManager lifetimeManager = null, params InjectionMember[] injectionMembers)
        {
            return Custom(iExceptionHandler, null, lifetimeManager, injectionMembers);
        }
        
        public ExceptionHandlersRegistrator Custom(Type iExceptionHandler, params InjectionMember[] injectionMembers)
        {
            return Custom(iExceptionHandler, null, null, injectionMembers);
        }

        public ExceptionHandlersRegistrator Custom<TExceptionHandler>(string name = null, LifetimeManager lifetimeManager = null, params InjectionMember[] injectionMembers)
            where TExceptionHandler : IExceptionHandler
        {
            return Custom(typeof (TExceptionHandler), name, lifetimeManager, injectionMembers);
        }

        public ExceptionHandlersRegistrator Custom<TExceptionHandler>(LifetimeManager lifetimeManager = null, params InjectionMember[] injectionMembers)
            where TExceptionHandler : IExceptionHandler
        {
            return Custom(typeof(TExceptionHandler), lifetimeManager, injectionMembers);
        }

        public ExceptionHandlersRegistrator Custom<TExceptionHandler>(params InjectionMember[] injectionMembers)
            where TExceptionHandler : IExceptionHandler
        {
            return Custom(typeof(TExceptionHandler), injectionMembers);
        }
    }
}