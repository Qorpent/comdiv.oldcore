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
using Comdiv.Design;
using Comdiv.Model;
using Comdiv.Model.Interfaces;

namespace Comdiv.Model{
    [NoCover]
    public class Entity : IWithId, IWithCode, IWithName, IWithComment, IWithControllerDefinition,IWithIdx,IOldWithTags, IEquatable<Entity>,IEntityDataPattern{
        public object Tag { get; set; }
        public Entity(){}
        public  Entity(string code,string name){
            this.Code = code;
            this.Name = name;
        }
        #region IWithCode Members

        public string Code { get; set; }

        #endregion

        #region IWithComment Members

        public string Comment { get; set; }

        #endregion

        #region IWithControllerDefinition Members

        public string Action { get; set; }

        public string Controller { get; set; }

        public string Area { get; set; }

        #endregion

        #region IWithId Members

        public int Id { get; set; }

        #endregion

        #region IWithName Members

        public string Name { get; set; }

        public int Idx { get; set; }
        public string Tags { get; set; }

        public DateTime Date { get; set; }

        public string Evidence { get; set; }

        public string Type { get; set; }

        public string System { get; set; }

        #endregion

        public bool Equals(Entity entity){
            if (entity == null){
                return false;
            }
            if (!Equals(Tag, entity.Tag)){
                return false;
            }
            if (!Equals(Code, entity.Code)){
                return false;
            }
            if (!Equals(Comment, entity.Comment)){
                return false;
            }
            if (!Equals(Action, entity.Action)){
                return false;
            }
            if (!Equals(Controller, entity.Controller)){
                return false;
            }
            if (!Equals(Area, entity.Area)){
                return false;
            }
            if (Id != entity.Id){
                return false;
            }
            if (!Equals(Name, entity.Name)){
                return false;
            }
            return true;
        }

        public override bool Equals(object obj){
            if (ReferenceEquals(this, obj)){
                return true;
            }
            return Equals(obj as Entity);
        }

        public override int GetHashCode(){
            int result = Tag != null ? Tag.GetHashCode() : 0;
            result = 29*result + (Code != null ? Code.GetHashCode() : 0);
            result = 29*result + (Comment != null ? Comment.GetHashCode() : 0);
            result = 29*result + (Action != null ? Action.GetHashCode() : 0);
            result = 29*result + (Controller != null ? Controller.GetHashCode() : 0);
            result = 29*result + (Area != null ? Area.GetHashCode() : 0);
            result = 29*result + Id;
            result = 29*result + (Name != null ? Name.GetHashCode() : 0);
            return result;
        }

    	public DateTime Version { get; private set; }
    	string IWithNewTags.Tag { get; set; }
    }
}