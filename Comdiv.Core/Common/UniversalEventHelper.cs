// Copyright 2007-2010 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// Supported by Media Technology LTD 
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
namespace Comdiv.Common{
    public static class UniversalEventHelper{
        public static UniversalEventArgs<T, int, object> Execute<T>(UniversalEventHandler<T, int, object> handler,
                                                                    T data, int eventType, object sender,
                                                                    bool cancallAble, bool handleAble, bool acceptReturn){
            return Execute<T, int, object>(handler, data, eventType, sender, cancallAble, handleAble, acceptReturn);
        }

        public static UniversalEventArgs<T, C, R> Execute<T, C, R>(UniversalEventHandler<T, C, R> handler, T data,
                                                                   C eventType, object sender, bool
                                                                                                   cancallAble,
                                                                   bool handleAble, bool acceptReturn)
            where C : struct{
            if (handler == null || handler.GetInvocationList().Length == 0) return null;
            var args = new UniversalEventArgs<T, C, R>(eventType, data, cancallAble, acceptReturn, handleAble);
            if (handleAble || cancallAble){
                foreach (UniversalEventHandler<T, C, R> h in handler.GetInvocationList()){
                    h(sender, args);
                    if (cancallAble && args.Cancel) return args;
                    if (handleAble && args.Handled) return args;
                }
            }
            else
                handler(sender, args);
            return args;
        }

        public static void Notify(UniversalEventHandler<object, int, object> handler){
            Notify(handler, null, 0, null);
        }

        public static void Notify<T>(UniversalEventHandler<T, int, object> handler, object sender, T data){
            Notify(handler, sender, 0, data);
        }

        public static void Notify<T>(UniversalEventHandler<T, int, object> handler, object sender, int eventType, T data){
            Notify<T, int, object>(handler, sender, eventType, data);
        }

        public static void Notify<T, C, R>(UniversalEventHandler<T, C, R> handler, object sender, C eventType, T data)
            where C : struct{
            Execute(handler, data, eventType, sender, false, false, false);
        }


        public static UniversalEventArgs<object, int, object> ExecuteForResult(
            UniversalEventHandler<object, int, object> handler, object sender){
            return ExecuteForResult(handler, sender, 0, null);
        }

        public static UniversalEventArgs<T, int, R> ExecuteForResult<T, R>(UniversalEventHandler<T, int, R> handler,
                                                                           object sender, T data){
            return ExecuteForResult(handler, sender, 0, data);
        }

        public static UniversalEventArgs<T, int, R> ExecuteForResult<T, R>(UniversalEventHandler<T, int, R> handler,
                                                                           object sender, int eventType, T data){
            return ExecuteForResult<T, int, R>(handler, sender, eventType, data);
        }

        public static UniversalEventArgs<T, C, R> ExecuteForResult<T, C, R>(UniversalEventHandler<T, C, R> handler,
                                                                            object sender, C eventType, T data)
            where C : struct{
            return Execute(handler, data, eventType, sender, false, true, true);
        }

        public static object GetResult(UniversalEventHandler<object, int, object> handler, object sender){
            return GetResult<object, int, object>(handler, sender, 0, null, null);
        }

        public static object GetResult(UniversalEventHandler<object, int, object> handler, object sender,
                                       object defaultValue){
            return GetResult<object, int, object>(handler, sender, 0, null, defaultValue);
        }

        public static R GetResult<R>(UniversalEventHandler<object, int, R> handler, object sender){
            return GetResult<object, int, R>(handler, sender, 0, null, default(R));
        }

        public static R GetResult<R>(UniversalEventHandler<object, int, R> handler, object sender, R defaultValue){
            return GetResult<object, int, R>(handler, sender, 0, null, defaultValue);
        }

        public static R GetResult<T, R>(UniversalEventHandler<T, int, R> handler, object sender, T data, R defaultValue){
            return GetResult<T, int, R>(handler, sender, 0, data, defaultValue);
        }

        public static R GetResult<T, R>(UniversalEventHandler<T, int, R> handler, object sender, int eventType, T data,
                                        R defaultValue){
            return GetResult<T, int, R>(handler, sender, eventType, data, defaultValue);
        }

        public static R GetResult<T, C, R>(UniversalEventHandler<T, C, R> handler, object sender, C eventType, T data,
                                           R defaultValue) where C : struct{
            var result = ExecuteForResult(handler, sender, eventType, data);
            if (result == null) return defaultValue;
            if (result.Handled) return result.ReturnValue;
            return defaultValue;
        }

        public static bool IsCancel(UniversalEventHandler<object, int, object> handler, object sender){
            return IsCancel<object, int, object>(handler, sender, 0, null);
        }

        public static bool IsCancel<T>(UniversalEventHandler<T, int, object> handler, object sender, T data){
            return IsCancel<T, int, object>(handler, sender, 0, data);
        }

        public static bool IsCancel<T, C>(UniversalEventHandler<T, C, object> handler, object sender, C eventType,
                                          T data) where C : struct{
            return IsCancel<T, C, object>(handler, sender, eventType, data);
        }

        public static bool IsCancel<T, C, R>(UniversalEventHandler<T, C, R> handler, object sender, C eventType, T data)
            where C : struct{
            var result = Execute(handler, data, eventType, sender, true, false, false);
            if (result == null) return false;
            return result.Cancel;
        }

        public static bool Handled(UniversalEventHandler<object, int, object> handler, object sender){
            return Handled<object, int, object>(handler, sender, 0, null);
        }

        public static bool Handled<T>(UniversalEventHandler<T, int, object> handler, object sender, T data){
            return Handled<T, int, object>(handler, sender, 0, data);
        }

        public static bool Handled<T, C>(UniversalEventHandler<T, C, object> handler, object sender, C eventType, T data)
            where C : struct{
            return Handled<T, C, object>(handler, sender, eventType, data);
        }

        public static bool Handled<T, C, R>(UniversalEventHandler<T, C, R> handler, object sender, C eventType, T data)
            where C : struct{
            var result = Execute(handler, data, eventType, sender, false, true, false);
            if (result == null) return false;
            return result.Handled;
        }
    }
}