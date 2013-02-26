using System;
using System.Collections.Generic;
using System.Linq;

namespace Comdiv.Rules.TextAnalyzer{
    public class ExtractionDescription{
        private IList<ExtractorOperation> operations;
        public ExtractionDescription(){
            ResultNodeName = "default";
            ResultNodeType = "default";          
        }

       

        public string ResultNodeName { get; set; }

        public IList<ExtractorOperation> Operations{
            get { return operations ?? (operations = new List<ExtractorOperation>()); }
            set { operations = value; }
        }

        public bool CanBeDelted{
            get{
                foreach (ExtractorOperation operation in operations){
                    if (operation.Activated &&
                        (NodeApplyMode.DeltaOnly == operation.Mode || NodeApplyMode.CreateOrDelta == operation.Mode)){
                        return true;
                    }
                }
                return false;
            }
        }

        public bool CanBeCreated{
            get{
                foreach (ExtractorOperation operation in operations){
                    if (operation.Activated &&
                        (NodeApplyMode.CreateOnly == operation.Mode || NodeApplyMode.CreateOrDelta == operation.Mode)){
                        return true;
                    }
                }
                return false;
            }
        }

        public double AsDeltaValue{
            get{
                if (!CanBeDelted){
                    return 0;
                }
                double current = 0;

                foreach (ExtractorOperation operation in operations){
                    if (operation.Activated &&
                        (NodeApplyMode.DeltaOnly == operation.Mode || NodeApplyMode.CreateOrDelta == operation.Mode)){
                        current += operation.Delta;
                    }
                }
                return current;
            }
        }

        public double AsOnlyDelta{
            get{
                if (!CanBeDelted){
                    return 0;
                }
                double current = 0;

                foreach (ExtractorOperation operation in operations){
                    if (operation.Activated && NodeApplyMode.DeltaOnly == operation.Mode){
                        current += operation.Delta;
                    }
                }
                return current;
            }
        }

        public double AsCreateValue{
            get{
                if (!CanBeCreated){
                    return 0;
                }
                double current = 0;

                foreach (ExtractorOperation operation in operations){
                    if (operation.Activated &&
                        (NodeApplyMode.CreateOnly == operation.Mode || NodeApplyMode.CreateOrDelta == operation.Mode)){
                        current = Math.Max(current, operation.Delta);
                    }
                }
                return current;
            }
        }

        public string ResultNodeType { get; set; }

        public void Apply(string text){
          
            Reset();
            foreach (ExtractorOperation operation in Operations){
                if (!operation.Activated){
                    operation.Apply(text);
                }
            }
        }

        public void Reset(){
            foreach (ExtractorOperation operation in operations){
                operation.Reset();
            }
        }
    }
}