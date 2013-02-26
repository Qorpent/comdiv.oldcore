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

namespace Comdiv.Patching{
    /// <summary>
    /// simple realization of IPackageIdentity
    /// </summary>
    public class SimplePackageIdentity : IPackageIdentity, IEquatable<SimplePackageIdentity>{
        /// <summary>
        /// Initializes a new instance of the <see cref="SimplePackageIdentity"/> class.
        /// </summary>
        public SimplePackageIdentity() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="SimplePackageIdentity"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public SimplePackageIdentity(string name){
            Name = name;
        }

        #region IEquatable<SimplePackageIdentity> Members

        /// <summary>
        /// Equalses the specified simple package identity.
        /// </summary>
        /// <param name="simplePackageIdentity">The simple package identity.</param>
        /// <returns></returns>
        public bool Equals(SimplePackageIdentity simplePackageIdentity){
            if (simplePackageIdentity == null){
                return false;
            }
            return Equals(Name, simplePackageIdentity.Name);
        }

        #endregion

        #region IPackageIdentity Members

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        #endregion

        ///<summary>
        ///</summary>
        ///<param name="simplePackageIdentity1"></param>
        ///<param name="simplePackageIdentity2"></param>
        ///<returns></returns>
        public static bool operator !=(
            SimplePackageIdentity simplePackageIdentity1, SimplePackageIdentity simplePackageIdentity2){
            return !Equals(simplePackageIdentity1, simplePackageIdentity2);
        }

        ///<summary>
        ///</summary>
        ///<param name="simplePackageIdentity1"></param>
        ///<param name="simplePackageIdentity2"></param>
        ///<returns></returns>
        public static bool operator ==(
            SimplePackageIdentity simplePackageIdentity1, SimplePackageIdentity simplePackageIdentity2){
            return Equals(simplePackageIdentity1, simplePackageIdentity2);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
        public override bool Equals(object obj){
            if (ReferenceEquals(this, obj)){
                return true;
            }
            return Equals(obj as SimplePackageIdentity);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode(){
            return Name.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString(){
            return "package: " + Name;
        }
    }
}