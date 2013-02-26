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
using System;
using System.Collections;
using Comdiv.Persistence;

namespace Comdiv.Inversion{
	/// <summary>
	/// Интерфейс простого локатора
	/// </summary>
	public interface ITypeLocator {
		
		/// <summary>
		/// Разрешение объекта по типу
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		object Resolve(Type type);
		/// <summary>
		/// Разрешение объекта по имени
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		object Resolve(string name);

		/// <summary>
		/// Разрешение всех объектов по типу
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		object[] ResolveAll(Type type);

		/// <summary>
		/// Разрешение одного объекта
		/// </summary>
		/// <param name="type"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		object Resolve(Type type, IDictionary parameters);
	}

	///<summary>
    ///</summary>
    public interface IInversionContainer:IDisposable, ITypeLocator {
		bool Exists(Type type);
        bool Exists(string name);
        IInversionContainer AddTransient(string name, Type type);
        IInversionContainer AddSingleton(string name, Type type, object typeOrInstance);
        IInversionContainer Remove(string name);
        IInversionContainer Remove(Type type);
        void Clear();
        bool Disposed { get; }
		
    }
}