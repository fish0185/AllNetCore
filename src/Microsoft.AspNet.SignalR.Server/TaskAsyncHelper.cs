// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.SignalR
{
    internal static class TaskAsyncHelper
    {
        private static readonly Task _emptyTask = MakeTask<object>(null);
        private static readonly Task<bool> _trueTask = MakeTask<bool>(true);
        private static readonly Task<bool> _falseTask = MakeTask<bool>(false);

        private static Task<T> MakeTask<T>(T value)
        {
            return FromResult<T>(value);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task Empty
        {
            get
            {
                return _emptyTask;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task<bool> True
        {
            get
            {
                return _trueTask;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task<bool> False
        {
            get
            {
                return _falseTask;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task OrEmpty(this Task task)
        {
            return task ?? Empty;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task<T> OrEmpty<T>(this Task<T> task)
        {
            return task ?? TaskCache<T>.Empty;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are set in a tcs")]
        public static Task FromAsync(Func<AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod, object state)
        {
            try
            {
                return Task.Factory.FromAsync(beginMethod, endMethod, state);
            }
            catch (Exception ex)
            {
                return TaskAsyncHelper.FromError(ex);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are set in a tcs")]
        public static Task<T> FromAsync<T>(Func<AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, T> endMethod, object state)
        {
            try
            {
                return Task.Factory.FromAsync<T>(beginMethod, endMethod, state);
            }
            catch (Exception ex)
            {
                return TaskAsyncHelper.FromError<T>(ex);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static TTask Catch<TTask>(this TTask task, ILogger logger = null) where TTask : Task
        {
            return Catch(task, ex => { }, logger);
        }

        public static TTask Catch<TTask>(this TTask task, ILogger logger, params IPerformanceCounter[] counters) where TTask : Task
        {
            return Catch(task, _ =>
                {
                    if (counters == null)
                    {
                        return;
                    }
                    for (var i = 0; i < counters.Length; i++)
                    {
                        counters[i].Increment();
                    }
                },
                logger);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static TTask Catch<TTask>(this TTask task, Action<AggregateException, object> handler, object state, ILogger logger = null) where TTask : Task
        {
            if (task != null && task.Status != TaskStatus.RanToCompletion)
            {
                if (task.Status == TaskStatus.Faulted)
                {
                    ExecuteOnFaulted(handler, state, task.Exception, logger);
                }
                else
                {
                    AttachFaultedContinuation<TTask>(task, handler, state, logger);
                }
            }

            return task;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        private static void AttachFaultedContinuation<TTask>(TTask task, Action<AggregateException, object> handler, object state, ILogger logger) where TTask : Task
        {
            task.ContinueWithPreservedCulture(innerTask =>
            {
                ExecuteOnFaulted(handler, state, innerTask.Exception, logger);
            },
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        private static void ExecuteOnFaulted(Action<AggregateException, object> handler, object state, AggregateException exception, ILogger logger)
        {
            // Observe Exception
            if (logger != null)
            {
                logger.WriteWarning("Exception thrown by Task", exception);
            }

            handler(exception, state);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static TTask Catch<TTask>(this TTask task, Action<AggregateException> handler, ILogger logger = null) where TTask : Task
        {
            return task.Catch((ex, state) => ((Action<AggregateException>)state).Invoke(ex), handler, logger);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are set in a tcs")]
        public static Task ContinueWithNotComplete(this Task task, Action action)
        {
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                case TaskStatus.Canceled:
                    try
                    {
                        action();
                        return task;
                    }
                    catch (Exception e)
                    {
                        return FromError(e);
                    }
                case TaskStatus.RanToCompletion:
                    return task;
                default:
                    var tcs = new TaskCompletionSource<object>();

                    task.ContinueWithPreservedCulture(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            try
                            {
                                action();

                                if (t.IsFaulted)
                                {
                                    tcs.TrySetUnwrappedException(t.Exception);
                                }
                                else
                                {
                                    tcs.TrySetCanceled();
                                }
                            }
                            catch (Exception e)
                            {
                                tcs.TrySetException(e);
                            }
                        }
                        else
                        {
                            tcs.TrySetResult(null);
                        }
                    },
                    TaskContinuationOptions.ExecuteSynchronously);

                    return tcs.Task;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static void ContinueWithNotComplete(this Task task, TaskCompletionSource<object> tcs)
        {
            task.ContinueWithPreservedCulture(t =>
            {
                if (t.IsFaulted)
                {
                    tcs.SetUnwrappedException(t.Exception);
                }
                else if (t.IsCanceled)
                {
                    tcs.SetCanceled();
                }
            },
            TaskContinuationOptions.NotOnRanToCompletion);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task ContinueWith(this Task task, TaskCompletionSource<object> tcs)
        {
            task.ContinueWithPreservedCulture(t =>
            {
                if (t.IsFaulted)
                {
                    tcs.TrySetUnwrappedException(t.Exception);
                }
                else if (t.IsCanceled)
                {
                    tcs.TrySetCanceled();
                }
                else
                {
                    tcs.TrySetResult(null);
                }
            },
            TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static void ContinueWith<T>(this Task<T> task, TaskCompletionSource<T> tcs)
        {
            task.ContinueWithPreservedCulture(t =>
            {
                if (t.IsFaulted)
                {
                    tcs.TrySetUnwrappedException(t.Exception);
                }
                else if (t.IsCanceled)
                {
                    tcs.TrySetCanceled();
                }
                else
                {
                    tcs.TrySetResult(t.Result);
                }
            });
        }

        // Then extesions
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task Then(this Task task, Action successor)
        {
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                case TaskStatus.Canceled:
                    return task;

                case TaskStatus.RanToCompletion:
                    return FromMethod(successor);

                default:
                    return RunTask(task, successor);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task<TResult> Then<TResult>(this Task task, Func<TResult> successor)
        {
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                    return FromError<TResult>(task.Exception);

                case TaskStatus.Canceled:
                    return Canceled<TResult>();

                case TaskStatus.RanToCompletion:
                    return FromMethod(successor);

                default:
                    return TaskRunners<object, TResult>.RunTask(task, successor);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task Then<T1>(this Task task, Action<T1> successor, T1 arg1)
        {
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                case TaskStatus.Canceled:
                    return task;

                case TaskStatus.RanToCompletion:
                    return FromMethod(successor, arg1);

                default:
                    return GenericDelegates<object, object, T1, object, object>.ThenWithArgs(task, successor, arg1);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task Then<T1, T2>(this Task task, Action<T1, T2> successor, T1 arg1, T2 arg2)
        {
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                case TaskStatus.Canceled:
                    return task;

                case TaskStatus.RanToCompletion:
                    return FromMethod(successor, arg1, arg2);

                default:
                    return GenericDelegates<object, object, T1, T2, object>.ThenWithArgs(task, successor, arg1, arg2);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task Then<T1>(this Task task, Func<T1, Task> successor, T1 arg1)
        {
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                case TaskStatus.Canceled:
                    return task;

                case TaskStatus.RanToCompletion:
                    return FromMethod(successor, arg1);

                default:
                    return GenericDelegates<object, Task, T1, object, object>.ThenWithArgs(task, successor, arg1)
                                                                     .FastUnwrap();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task Then<T1, T2>(this Task task, Func<T1, T2, Task> successor, T1 arg1, T2 arg2)
        {
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                case TaskStatus.Canceled:
                    return task;

                case TaskStatus.RanToCompletion:
                    return FromMethod(successor, arg1, arg2);

                default:
                    return GenericDelegates<object, Task, T1, T2, object>.ThenWithArgs(task, successor, arg1, arg2)
                                                                 .FastUnwrap();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task Then<T1, T2, T3>(this Task task, Func<T1, T2, T3, Task> successor, T1 arg1, T2 arg2, T3 arg3)
        {
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                case TaskStatus.Canceled:
                    return task;

                case TaskStatus.RanToCompletion:
                    return FromMethod(successor, arg1, arg2, arg3);

                default:
                    return GenericDelegates<object, Task, T1, T2, T3>.ThenWithArgs(task, successor, arg1, arg2, arg3)
                                                                 .FastUnwrap();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task<TResult> Then<T, TResult>(this Task<T> task, Func<T, Task<TResult>> successor)
        {
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                    return FromError<TResult>(task.Exception);

                case TaskStatus.Canceled:
                    return Canceled<TResult>();

                case TaskStatus.RanToCompletion:
                    return FromMethod(successor, task.Result);

                default:
                    return TaskRunners<T, Task<TResult>>.RunTask(task, t => successor(t.Result))
                                                        .FastUnwrap();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task<TResult> Then<T, TResult>(this Task<T> task, Func<T, TResult> successor)
        {
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                    return FromError<TResult>(task.Exception);

                case TaskStatus.Canceled:
                    return Canceled<TResult>();

                case TaskStatus.RanToCompletion:
                    return FromMethod(successor, task.Result);

                default:
                    return TaskRunners<T, TResult>.RunTask(task, t => successor(t.Result));
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task<TResult> Then<T, T1, TResult>(this Task<T> task, Func<T, T1, TResult> successor, T1 arg1)
        {
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                    return FromError<TResult>(task.Exception);

                case TaskStatus.Canceled:
                    return Canceled<TResult>();

                case TaskStatus.RanToCompletion:
                    return FromMethod(successor, task.Result, arg1);

                default:
                    return GenericDelegates<T, TResult, T1, object, object>.ThenWithArgs(task, successor, arg1);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task Then(this Task task, Func<Task> successor)
        {
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                case TaskStatus.Canceled:
                    return task;

                case TaskStatus.RanToCompletion:
                    return FromMethod(successor);

                default:
                    return TaskRunners<object, Task>.RunTask(task, successor)
                                                    .FastUnwrap();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task<TResult> Then<TResult>(this Task task, Func<Task<TResult>> successor)
        {
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                    return FromError<TResult>(task.Exception);

                case TaskStatus.Canceled:
                    return Canceled<TResult>();

                case TaskStatus.RanToCompletion:
                    return FromMethod(successor);

                default:
                    return TaskRunners<object, Task<TResult>>.RunTask(task, successor)
                                                             .FastUnwrap();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task Then<TResult>(this Task<TResult> task, Action<TResult> successor)
        {
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                case TaskStatus.Canceled:
                    return task;

                case TaskStatus.RanToCompletion:
                    return FromMethod(successor, task.Result);

                default:
                    return TaskRunners<TResult, object>.RunTask(task, successor);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task Then<TResult>(this Task<TResult> task, Func<TResult, Task> successor)
        {
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                case TaskStatus.Canceled:
                    return task;

                case TaskStatus.RanToCompletion:
                    return FromMethod(successor, task.Result);

                default:
                    return TaskRunners<TResult, Task>.RunTask(task, t => successor(t.Result))
                                                     .FastUnwrap();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task<TResult> Then<TResult, T1>(this Task<TResult> task, Func<Task<TResult>, T1, Task<TResult>> successor, T1 arg1)
        {
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                case TaskStatus.Canceled:
                    return task;

                case TaskStatus.RanToCompletion:
                    return FromMethod(successor, task, arg1);

                default:
                    return GenericDelegates<TResult, Task<TResult>, T1, object, object>.ThenWithArgs(task, successor, arg1)
                                                                               .FastUnwrap();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are flowed to the caller")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task Finally(this Task task, Action<object> next, object state)
        {
            try
            {
                switch (task.Status)
                {
                    case TaskStatus.Faulted:
                    case TaskStatus.Canceled:
                        next(state);
                        return task;
                    case TaskStatus.RanToCompletion:
                        return FromMethod(next, state);

                    default:
                        return RunTaskSynchronously(task, next, state, onlyOnSuccess: false);
                }
            }
            catch (Exception ex)
            {
                return FromError(ex);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task RunSynchronously(this Task task, Action successor)
        {
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                case TaskStatus.Canceled:
                    return task;

                case TaskStatus.RanToCompletion:
                    return FromMethod(successor);

                default:
                    return RunTaskSynchronously(task, state => ((Action)state).Invoke(), successor);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task FastUnwrap(this Task<Task> task)
        {
            var innerTask = (task.Status == TaskStatus.RanToCompletion) ? task.Result : null;
            return innerTask ?? task.Unwrap();
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task<T> FastUnwrap<T>(this Task<Task<T>> task)
        {
            var innerTask = (task.Status == TaskStatus.RanToCompletion) ? task.Result : null;
            return innerTask ?? task.Unwrap();
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task Delay(TimeSpan timeOut)
        {
            return Task.Delay(timeOut);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are set in a tcs")]
        public static Task FromMethod(Action func)
        {
            try
            {
                func();
                return Empty;
            }
            catch (Exception ex)
            {
                return FromError(ex);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are set in a tcs")]
        public static Task FromMethod<T1>(Action<T1> func, T1 arg)
        {
            try
            {
                func(arg);
                return Empty;
            }
            catch (Exception ex)
            {
                return FromError(ex);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are set in a tcs")]
        public static Task FromMethod<T1, T2>(Action<T1, T2> func, T1 arg1, T2 arg2)
        {
            try
            {
                func(arg1, arg2);
                return Empty;
            }
            catch (Exception ex)
            {
                return FromError(ex);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are set in a tcs")]
        public static Task FromMethod(Func<Task> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                return FromError(ex);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are set in a tcs")]
        public static Task<TResult> FromMethod<TResult>(Func<Task<TResult>> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                return FromError<TResult>(ex);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are set in a tcs")]
        public static Task<TResult> FromMethod<TResult>(Func<TResult> func)
        {
            try
            {
                return FromResult<TResult>(func());
            }
            catch (Exception ex)
            {
                return FromError<TResult>(ex);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are set in a tcs")]
        public static Task FromMethod<T1>(Func<T1, Task> func, T1 arg)
        {
            try
            {
                return func(arg);
            }
            catch (Exception ex)
            {
                return FromError(ex);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are set in a tcs")]
        public static Task FromMethod<T1, T2>(Func<T1, T2, Task> func, T1 arg1, T2 arg2)
        {
            try
            {
                return func(arg1, arg2);
            }
            catch (Exception ex)
            {
                return FromError(ex);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are set in a tcs")]
        public static Task FromMethod<T1, T2, T3>(Func<T1, T2, T3, Task> func, T1 arg1, T2 arg2, T3 arg3)
        {
            try
            {
                return func(arg1, arg2, arg3);
            }
            catch (Exception ex)
            {
                return FromError(ex);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are set in a tcs")]
        public static Task<TResult> FromMethod<T1, TResult>(Func<T1, Task<TResult>> func, T1 arg)
        {
            try
            {
                return func(arg);
            }
            catch (Exception ex)
            {
                return FromError<TResult>(ex);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are set in a tcs")]
        public static Task<TResult> FromMethod<T1, TResult>(Func<T1, TResult> func, T1 arg)
        {
            try
            {
                return FromResult<TResult>(func(arg));
            }
            catch (Exception ex)
            {
                return FromError<TResult>(ex);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are set in a tcs")]
        public static Task<TResult> FromMethod<T1, T2, TResult>(Func<T1, T2, Task<TResult>> func, T1 arg1, T2 arg2)
        {
            try
            {
                return func(arg1, arg2);
            }
            catch (Exception ex)
            {
                return FromError<TResult>(ex);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are set in a tcs")]
        public static Task<TResult> FromMethod<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 arg1, T2 arg2)
        {
            try
            {
                return FromResult<TResult>(func(arg1, arg2));
            }
            catch (Exception ex)
            {
                return FromError<TResult>(ex);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task<T> FromResult<T>(T value)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(value);
            return tcs.Task;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        internal static Task FromError(Exception e)
        {
            return FromError<object>(e);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        internal static Task<T> FromError<T>(Exception e)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetUnwrappedException<T>(e);
            return tcs.Task;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        internal static void SetUnwrappedException<T>(this TaskCompletionSource<T> tcs, Exception e)
        {
            var aggregateException = e as AggregateException;
            if (aggregateException != null)
            {
                tcs.SetException(aggregateException.InnerExceptions);
            }
            else
            {
                tcs.SetException(e);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        internal static bool TrySetUnwrappedException<T>(this TaskCompletionSource<T> tcs, Exception e)
        {
            var aggregateException = e as AggregateException;
            if (aggregateException != null)
            {
                return tcs.TrySetException(aggregateException.InnerExceptions);
            }
            else
            {
                return tcs.TrySetException(e);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        private static Task Canceled()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetCanceled();
            return tcs.Task;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        private static Task<T> Canceled<T>()
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetCanceled();
            return tcs.Task;
        }


#if !K10
        internal struct CulturePair
        {
            public CultureInfo Culture;
            public CultureInfo UICulture;
        }

        internal static CulturePair SaveCulture()
        {
            return new CulturePair
            {
                Culture = Thread.CurrentThread.CurrentCulture,
                UICulture = Thread.CurrentThread.CurrentUICulture
            };
        }

        internal static TResult RunWithPreservedCulture<T1, T2, TResult>(CulturePair preservedCulture, Func<T1, T2, TResult> func, T1 arg1, T2 arg2)
        {
            var replacedCulture = SaveCulture();
            try
            {
                Thread.CurrentThread.CurrentCulture = preservedCulture.Culture;
                Thread.CurrentThread.CurrentUICulture = preservedCulture.UICulture;
                return func(arg1, arg2);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = replacedCulture.Culture;
                Thread.CurrentThread.CurrentUICulture = replacedCulture.UICulture;
            }
        }

        internal static TResult RunWithPreservedCulture<T, TResult>(CulturePair preservedCulture, Func<T, TResult> func, T arg)
        {
            return RunWithPreservedCulture(preservedCulture, (f, state) => f(state), func, arg);
        }

        internal static void RunWithPreservedCulture<T>(CulturePair preservedCulture, Action<T> action, T arg)
        {
            RunWithPreservedCulture(preservedCulture, (f, state)  =>
            {
                f(state);
                return (object)null;
            },
            action, arg);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        internal static void RunWithPreservedCulture(CulturePair preservedCulture, Action action)
        {
            RunWithPreservedCulture(preservedCulture, f => f(), action);
        }
#endif

        internal static Task ContinueWithPreservedCulture(this Task task, Action<Task> continuationAction, TaskContinuationOptions continuationOptions)
        {
            // TODO
#if ASPNETCORE50
            // The Thread class is not available on WinRT
            return task.ContinueWith(continuationAction, continuationOptions);
#else
            var preservedCulture = SaveCulture();
            return task.ContinueWith(t => RunWithPreservedCulture(preservedCulture, continuationAction, t), continuationOptions);
#endif
        }

        internal static Task ContinueWithPreservedCulture<T>(this Task<T> task, Action<Task<T>> continuationAction, TaskContinuationOptions continuationOptions)
        {
#if ASPNETCORE50
            // The Thread class is not available on WinRT
            return task.ContinueWith(continuationAction, continuationOptions);
#else
            var preservedCulture = SaveCulture();
            return task.ContinueWith(t => RunWithPreservedCulture(preservedCulture, continuationAction, t), continuationOptions);
#endif
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        internal static Task<TResult> ContinueWithPreservedCulture<T, TResult>(this Task<T> task, Func<Task<T>, TResult> continuationAction, TaskContinuationOptions continuationOptions)
        {
#if ASPNETCORE50
            // The Thread class is not available on WinRT
            return task.ContinueWith(continuationAction, continuationOptions);
#else
            var preservedCulture = SaveCulture();
            return task.ContinueWith(t => RunWithPreservedCulture(preservedCulture, continuationAction, t), continuationOptions);
#endif
        }

        internal static Task ContinueWithPreservedCulture(this Task task, Action<Task> continuationAction)
        {
            return task.ContinueWithPreservedCulture(continuationAction, TaskContinuationOptions.None);
        }

        internal static Task ContinueWithPreservedCulture<T>(this Task<T> task, Action<Task<T>> continuationAction)
        {
            return task.ContinueWithPreservedCulture(continuationAction, TaskContinuationOptions.None);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        internal static Task<TResult> ContinueWithPreservedCulture<T, TResult>(this Task<T> task, Func<Task<T>, TResult> continuationAction)
        {
            return task.ContinueWithPreservedCulture(continuationAction, TaskContinuationOptions.None);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are set in a tcs")]
        private static Task RunTask(Task task, Action successor)
        {
            var tcs = new TaskCompletionSource<object>();
            task.ContinueWithPreservedCulture(t =>
            {
                if (t.IsFaulted)
                {
                    tcs.SetUnwrappedException(t.Exception);
                }
                else if (t.IsCanceled)
                {
                    tcs.SetCanceled();
                }
                else
                {
                    try
                    {
                        successor();
                        tcs.SetResult(null);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetUnwrappedException(ex);
                    }
                }
            });

            return tcs.Task;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are set in a tcs")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        private static Task RunTaskSynchronously(Task task, Action<object> next, object state, bool onlyOnSuccess = true)
        {
            var tcs = new TaskCompletionSource<object>();
            task.ContinueWithPreservedCulture(t =>
            {
                try
                {
                    if (t.IsFaulted)
                    {
                        if (!onlyOnSuccess)
                        {
                            next(state);
                        }

                        tcs.SetUnwrappedException(t.Exception);
                    }
                    else if (t.IsCanceled)
                    {
                        if (!onlyOnSuccess)
                        {
                            next(state);
                        }

                        tcs.SetCanceled();
                    }
                    else
                    {
                        next(state);
                        tcs.SetResult(null);
                    }
                }
                catch (Exception ex)
                {
                    tcs.SetUnwrappedException(ex);
                }
            },
            TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }

        private static class TaskRunners<T, TResult>
        {
            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are set in a tcs")]
            internal static Task RunTask(Task<T> task, Action<T> successor)
            {
                var tcs = new TaskCompletionSource<object>();
                task.ContinueWithPreservedCulture(t =>
                {
                    if (t.IsFaulted)
                    {
                        tcs.SetUnwrappedException(t.Exception);
                    }
                    else if (t.IsCanceled)
                    {
                        tcs.SetCanceled();
                    }
                    else
                    {
                        try
                        {
                            successor(t.Result);
                            tcs.SetResult(null);
                        }
                        catch (Exception ex)
                        {
                            tcs.SetUnwrappedException(ex);
                        }
                    }
                });

                return tcs.Task;
            }

            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are set in a tcs")]
            internal static Task<TResult> RunTask(Task task, Func<TResult> successor)
            {
                var tcs = new TaskCompletionSource<TResult>();
                task.ContinueWithPreservedCulture(t =>
                {
                    if (t.IsFaulted)
                    {
                        tcs.SetUnwrappedException(t.Exception);
                    }
                    else if (t.IsCanceled)
                    {
                        tcs.SetCanceled();
                    }
                    else
                    {
                        try
                        {
                            tcs.SetResult(successor());
                        }
                        catch (Exception ex)
                        {
                            tcs.SetUnwrappedException(ex);
                        }
                    }
                });

                return tcs.Task;
            }

            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are set in a tcs")]
            internal static Task<TResult> RunTask(Task<T> task, Func<Task<T>, TResult> successor)
            {
                var tcs = new TaskCompletionSource<TResult>();
                task.ContinueWithPreservedCulture(t =>
                {
                    if (task.IsFaulted)
                    {
                        tcs.SetUnwrappedException(t.Exception);
                    }
                    else if (task.IsCanceled)
                    {
                        tcs.SetCanceled();
                    }
                    else
                    {
                        try
                        {
                            tcs.SetResult(successor(t));
                        }
                        catch (Exception ex)
                        {
                            tcs.SetUnwrappedException(ex);
                        }
                    }
                });

                return tcs.Task;
            }
        }

        private static class GenericDelegates<T, TResult, T1, T2, T3>
        {
            internal static Task ThenWithArgs(Task task, Action<T1> successor, T1 arg1)
            {
                return RunTask(task, () => successor(arg1));
            }

            internal static Task ThenWithArgs(Task task, Action<T1, T2> successor, T1 arg1, T2 arg2)
            {
                return RunTask(task, () => successor(arg1, arg2));
            }

            internal static Task<TResult> ThenWithArgs(Task task, Func<T1, TResult> successor, T1 arg1)
            {
                return TaskRunners<object, TResult>.RunTask(task, () => successor(arg1));
            }

            internal static Task<TResult> ThenWithArgs(Task task, Func<T1, T2, TResult> successor, T1 arg1, T2 arg2)
            {
                return TaskRunners<object, TResult>.RunTask(task, () => successor(arg1, arg2));
            }

            internal static Task<TResult> ThenWithArgs(Task<T> task, Func<T, T1, TResult> successor, T1 arg1)
            {
                return TaskRunners<T, TResult>.RunTask(task, t => successor(t.Result, arg1));
            }

            internal static Task<Task> ThenWithArgs(Task task, Func<T1, Task> successor, T1 arg1)
            {
                return TaskRunners<object, Task>.RunTask(task, () => successor(arg1));
            }

            internal static Task<Task> ThenWithArgs(Task task, Func<T1, T2, Task> successor, T1 arg1, T2 arg2)
            {
                return TaskRunners<object, Task>.RunTask(task, () => successor(arg1, arg2));
            }

            internal static Task<Task> ThenWithArgs(Task task, Func<T1, T2, T3, Task> successor, T1 arg1, T2 arg2, T3 arg3)
            {
                return TaskRunners<object, Task>.RunTask(task, () => successor(arg1, arg2, arg3));
            }

            internal static Task<Task<TResult>> ThenWithArgs(Task<T> task, Func<T, T1, Task<TResult>> successor, T1 arg1)
            {
                return TaskRunners<T, Task<TResult>>.RunTask(task, t => successor(t.Result, arg1));
            }

            internal static Task<Task<T>> ThenWithArgs(Task<T> task, Func<Task<T>, T1, Task<T>> successor, T1 arg1)
            {
                return TaskRunners<T, Task<T>>.RunTask(task, t => successor(t, arg1));
            }
        }

        private static class TaskCache<T>
        {
            public static Task<T> Empty = MakeTask<T>(default(T));
        }
    }
}
