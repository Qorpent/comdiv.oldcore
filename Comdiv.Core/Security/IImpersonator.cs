// Copyright 2007-2011 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
//  Supported by Media Technology LTD 
//   
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  
// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System.Security.Principal;
namespace Comdiv.Security{
    /// <summary>
    /// Impersonator interface - allows to redirect thread principal 
    /// to another, while keepeing source principal
    /// </summary>
    public interface IImpersonator{
        /// <summary>
        /// Optimization property - true if impersonator have active impersonations,
        /// due to that fact that Impersonator is not used offen
        /// </summary>
        bool Active { get; }
        /// <summary>
        /// Set that some principal must be treat as another
        /// </summary>
        /// <param name="principal">principal to impersonate</param>
        /// <param name="userName">username of another user</param>
        void Impersonate(IPrincipal principal, string userName);
        /// <summary>
        /// Revoke Impersonator of user
        /// </summary>
        /// <param name="principal">principal that must not be impersonated further</param>
        void DeImpersonate(IPrincipal principal);
        /// <summary>
        /// input principal if not impersonaqted, impersonated user otherwise
        /// </summary>
        /// <param name="user">principal to check</param>
        /// <returns></returns>
        IPrincipal Resolve(IPrincipal user);
        /// <summary>
        /// Determines if given principal is impersonated or not
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        bool IsImpersonated(IPrincipal principal);
    }
}