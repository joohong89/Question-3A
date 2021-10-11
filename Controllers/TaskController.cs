using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace api_consumer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaskController : ControllerBase
    {
       
        private readonly ILogger<TaskController> _logger;

        public TaskController(ILogger<TaskController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<string>>> Get()
        {


            List<string> queueItems = new List<string>();
            var factory = new ConnectionFactory() {
                // HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                // Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT"))
                 HostName = "localhost", 
                 Port= 31672
            };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                
                channel.QueueDeclare(queue: "TaskQueue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    queueItems.Add(message);
                    Console.WriteLine(" [x] Received {0}", message);
                };
                channel.BasicConsume(queue: "TaskQueue",
                                     autoAck: true,
                                     consumer: consumer);

            
            }

            if (queueItems.Count == 0) {
                return NoContent();
            }

            return queueItems;
        }
    }
}
