using System;
using System.Linq;
using Comdiv.Application;
using Comdiv.Conversations;
using Comdiv.Extensions;
using Comdiv.Inversion;
using Comdiv.IO;
using Comdiv.Logging;
using Comdiv.Model;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;
using Comdiv.Security;
using Comdiv.Security.Acl;

namespace Comdiv.MVC.Report{
    public abstract class ReportProcessorBaseWithExecutionPipe : ReportProcessorBaseWithLoad{
        public override void Execute(IReportRequest request){
            try{
                innerExecute(request);
            }
            catch (ReportException){
                throw;
            }
            catch (Exception ex){
                throw new ReportException("Error occured while report processing", ex, request);
            }
        }

        protected virtual void innerExecute(IReportRequest request){
            checkReportSecurity(request);
            refactorRequest(request);
            var production = generateProduction(request);
            outputProduction(production);
        }

        protected abstract void outputProduction(IReportProduction production);

        private IReportProduction generateProduction(IReportRequest request){
            IReportProduction result = null;
            if (request is IMultiProductionReportRequest){
                result = generateMultipleProduction((IMultiProductionReportRequest) request);
            }
            else{
                var preProduction = generatePreProduction(request);
                result = generateProduction(request, preProduction);
            }
            result.ParentRequest = request;
            return result;
        }

        private IReportProduction generateMultipleProduction(IMultiProductionReportRequest request){
            var result = new MultipleProdcution();

            foreach (var reportRequest in request.Requests){
                result.ProductionList.Add(generateProduction(reportRequest));
            }
            return result;
        }


        protected abstract IPreProduction generatePreProduction(IReportRequest request);
        protected abstract IReportProduction generateProduction(IReportRequest request, IPreProduction preProduction);
        protected abstract void refactorRequest(IReportRequest request);
        protected abstract void checkReportSecurity(IReportRequest request);
    }
}