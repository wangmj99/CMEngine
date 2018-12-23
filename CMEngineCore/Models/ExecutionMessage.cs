using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMEngineCore.Models
{
    class ExecutionMessage
    {
        public int ReqId { get; set; }
        public Contract Contract { get; set; }
        public Execution Execution { get; set; }

        public ExecutionMessage(int reqId, Contract contract, Execution execution)
        {
            this.ReqId = reqId;
            this.Contract = contract;
            this.Execution = execution;
        }
    }
}
