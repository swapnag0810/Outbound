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
    public class EventTypeBL
    {
        private readonly IUnitOfWork<CommunicationQContext> _unitOfWork;
        public EventTypeBL(IUnitOfWork<CommunicationQContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<EventType> FetchOutboundEventTypes()
        {
            var list = new List<EventType>();
            var ctx = _unitOfWork.GetContext();
            var rep = new GenericRepository<CommunicationQContext>(ctx);
            list = rep.Find<EventType>(a => (a.Type == "Outbound" || a.Type == "outbound")).OrderBy(a=> a.EventTypeId).ToList();
            _unitOfWork.Complete();
            return list;
        }
    }
}
