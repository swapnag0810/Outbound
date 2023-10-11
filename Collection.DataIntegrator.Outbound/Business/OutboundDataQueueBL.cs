using Collection.DataIntegrator.Outbound.DAL;
using Collection.DataIntegrator.Outbound.DAL.Interface;
using Collection.DataIntegrator.Outbound.DAL.Repository;
using Collection.DataIntegrator.Outbound.DAL.UnitOfWork;
using Collection.DataIntegrator.Outbound.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Collection.DataIntegrator.Outbound.Business
{
    public class OutboundDataQueueBL
    {
        private readonly IUnitOfWork<CommunicationQContext> _unitOfWork;
        public OutboundDataQueueBL(IUnitOfWork<CommunicationQContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<OutboundDataQueue> FetchPendingOutboundQueue(int outboundEventTypeId)
        {
            var list = new List<OutboundDataQueue>();
            var ctx = _unitOfWork.GetContext();
            var rep = new GenericRepository<CommunicationQContext>(ctx);
            list = rep.Find<OutboundDataQueue>(a => a.Status == false && a.EventTypeId == outboundEventTypeId).OrderBy(a=> a.OutboundDataQueueId).ToList();
            _unitOfWork.Complete();
            return list;
        }

        public OutboundDataQueueLog CreateOutboundDataQueueLog(OutboundDataQueueLog dataQueueLog)
        {
            if (dataQueueLog == null)
            {
                return dataQueueLog;
            }
            var ctx = _unitOfWork.GetContext();
            var rep = new GenericRepository<CommunicationQContext>(ctx);
            rep.Add(dataQueueLog);
            _unitOfWork.Complete();
            return dataQueueLog;

        }

        public bool Save(List<OutboundDataQueue> outboundQueueList)
        {
            var ctx = _unitOfWork.GetContext();
            var rep = new GenericRepository<CommunicationQContext>(ctx);
            rep.Add(outboundQueueList);
            _unitOfWork.Complete();
            return true;

        }

        public OutboundDataQueue FetchOutboundQueueById(int Id)
        {
            var queueData = new OutboundDataQueue();
            var ctx = _unitOfWork.GetContext();
            var rep = new GenericRepository<CommunicationQContext>(ctx);
            queueData = rep.Find<OutboundDataQueue>(a => a.OutboundDataQueueId == Id).FirstOrDefault();
            _unitOfWork.Complete();
            return queueData;
        }
       
        public OutboundDataQueue UpdateOutboundQueueStatus(OutboundDataQueue dataQueue)
        {
            if (dataQueue == null)
            {
                return dataQueue;
            }
            var ctx = _unitOfWork.GetContext();
            var rep = new GenericRepository<CommunicationQContext>(ctx);
            rep.Update(dataQueue);
            _unitOfWork.Complete();
            return dataQueue;

        }
    }
}
