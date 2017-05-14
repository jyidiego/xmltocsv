using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

using APIService.Services;
using Microsoft.Extensions.Options;

using DataFormatConverter;

namespace APIService.Handlers
{
	public class XmlToCsvHandler : IMessageHandler
	{
		private ConnectionFactory _connectionFactory;
		private ILogger<XmlToCsvHandler> _logger;
		private IQueueProducerService _queueProducerService;
		private Converter _converter;

		public XmlToCsvHandler(IQueueProducerService queueProducerService, ConnectionFactory rabbitConnection, ILoggerFactory loggerFactory, IOptions<DataFlowServiceConfig> config)
		{
			_logger = loggerFactory.CreateLogger<XmlToCsvHandler>();
			_queueProducerService = queueProducerService;
			_connectionFactory = rabbitConnection;
			_queueProducerService.QueueName = config.Value.OutQueueName;
			_queueProducerService.ExchangeName = config.Value.ExchangeName;
			_queueProducerService.ExchangeType = config.Value.ExchangeType;
			_queueProducerService.RoutingKeyName = config.Value.OutRoutingKeyName;
			_queueProducerService.Connect(_connectionFactory);
			_converter = new Converter();
		}

		public bool Handle(string message)
		{
			_logger.LogInformation("XmlToCsv Message Handler: Input");
			var result = _converter.XML_to_CSV(message);
			_logger.LogInformation("XmlToCsv Message Handler: Output");

			if (!string.IsNullOrEmpty(result))
			{
				_queueProducerService.WriteToQueue(_queueProducerService.ExchangeName,
								    _queueProducerService.QueueName,
								    _queueProducerService.RoutingKeyName,
								    result);
				_logger.LogInformation("XmlToCsv Message Handler written to queue");
				return true;
			}
			else
			{
				_logger.LogInformation($"Error Message: {message}");
				return false;
			}
		}
	}
}
