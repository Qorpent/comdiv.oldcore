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
namespace Comdiv.Collections{
    public interface IMap<F, T>{
        /// <summary>
        /// ���������� ������ ����� � ����� ����������
        /// </summary>
        /// <param name="from">��������</param>
        /// <returns>������ �����</returns>
        T[] this[F from] { get; set; }

        /// <summary>
        /// ������� � ��������� ����� ������� �����
        /// </summary>
        /// <param name="from">��������</param>
        /// <param name="to">����</param>
        /// <returns>������� �����</returns>
        IMapItem<F, T> Add(F from, T to);

        /// <summary>
        /// ���������� ������ ���������� ��� ����
        /// </summary>
        /// <param name="to">����</param>
        /// <returns>������ ����������</returns>
        F[] Reverse(T to);
    }
}