using System;
using APIService.Models;
using APIService.Repository;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using DataFormatConverter;

using APIService.Services;

namespace APIService.Handlers
{
  public class XmlToCsvHandler : IMessageHandler
  {
    private ConnectionFactory _connectionFactory;
    private ILogger<XmlToCsvHandler> _logger; 
    private IQueueProducerService _queueProducerService;
    private Converter _converter;

    public XmlToCsvHandler(IQueueProducerService queueProducerService, ConnectionFactory rabbitConnection, ILoggerFactory loggerFactory)
    {
      _logger = loggerFactory.CreateLogger<XmlToCsvHandler>();
      _queueProducerService = queueProducerService;
      _connectionFactory = rabbitConnection; 
      _queueProducerService.QueueName = "test-queue-out";
      _queueProducerService.ExchangeName ="ExchangeName";
      _queueProducerService.ExchangeType = "direct";
      _queueProducerService.RoutingKeyName = "csv";
      _queueProducerService.Connect(_connectionFactory);
      _converter = new Converter();
    }

    public bool Handle(string message)
    {
        _logger.LogInformation($"XmlToCsv Message Handler: {message}");
        var result = _converter.XML_to_CSV(message);  
        _logger.LogInformation($"XmlToCsv Message Handler: {result}");

        if (! string.IsNullOrEmpty(result))
        {
            _queueProducerService.WriteToQueue( _queueProducerService.ExchangeName,
                                                _queueProducerService.QueueName,
                                                _queueProducerService.RoutingKeyName,
                                                result );
            _logger.LogInformation("XmlToCsv Message Handler written to queue");
            return true;
        } else {
            _logger.LogInformation($"Error Message: {message}");
            return false;
        }
    }
  }
}
