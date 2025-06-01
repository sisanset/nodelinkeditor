using MQTTnet;
using Newtonsoft.Json;
using static NodeLinkEditor.ViewModels.AMRViewModel;

namespace NodeLinkEditor.Others
{
    public class AMRMqttClient
    {
        private IMqttClient _mqttClient;
        public string BrokerAddress { get; set; } = "localhost";
        public int BrokerPort { get; set; } = 1883;
        public string ClientId { get; set; } = Guid.NewGuid().ToString();
        public string Topic { get; set; } = "amr/location";
        public bool IsConnected { get; private set; }

        private event Action<double, double, double>? MessageReceivedEvent;
        private event Action? ConnectedEvent;
        private event Action? DisconnectedEvent;
        public void SetMessageReceivedEvent(Action<double, double, double> action) => MessageReceivedEvent += action;
        public void SetConnectedEvent(Action action) => ConnectedEvent += action;
        public void SetDisconnectedEvent(Action action) => DisconnectedEvent += action;

        public AMRMqttClient()
        {
            var mqttFactory = new MqttClientFactory();
            _mqttClient = mqttFactory.CreateMqttClient();

            _mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                string payload = e.ApplicationMessage.ConvertPayloadToString();
                var amrData = JsonConvert.DeserializeObject<AmrData>(payload);
                if (amrData == null)
                {
                    Console.WriteLine("### COULD NOT DESERIALIZE AMR DATA ###");
                    return Task.CompletedTask;
                }
                MessageReceivedEvent?.Invoke(amrData.X, amrData.Y, amrData.Yaw);
                return Task.CompletedTask;
            };

            _mqttClient.ConnectedAsync += e =>
            {
                Console.WriteLine("### CONNECTED TO BROKER ###");
                IsConnected = true;
                ConnectedEvent?.Invoke();
                return Task.CompletedTask;
            };
            _mqttClient.DisconnectedAsync += e =>
            {
                Console.WriteLine("### DISCONNECTED FROM BROKER ###");
                IsConnected = false;
                DisconnectedEvent?.Invoke();
                return Task.CompletedTask;
            };
        }

        public async Task Connect()
        {
            var mqttOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(BrokerAddress, BrokerPort)
                .WithClientId(ClientId)
                .Build();

            try
            {
                await _mqttClient.ConnectAsync(mqttOptions, CancellationToken.None);
                IsConnected = true;
                Console.WriteLine("### CONNECTED TO BROKER ###");
            }
            catch (Exception e)
            {
                Console.WriteLine("### COULD NOT CONNECT TO BROKER ###" + e);
            }
        }

        public async Task Disconnect()
        {
            try
            {
                await _mqttClient.DisconnectAsync();
                IsConnected = false;
                Console.WriteLine("### DISCONNECTED FROM BROKER ###");
            }
            catch (Exception e)
            {
                Console.WriteLine("### COULD NOT DISCONNECT FROM BROKER ###" + e);
            }
        }

        public async Task Subscribe()
        {
            var mqttSubscribeOptions = new MqttClientFactory().CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => { f.WithTopic(Topic); })
                .Build();

            try
            {
                await _mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
                Console.WriteLine("### SUBSCRIBED ###");
            }
            catch (Exception e)
            {
                Console.WriteLine("### COULD NOT SUBSCRIBE ###" + e);
            }
        }

        public async Task Unsubscribe()
        {
            var mqttUnsubscribeOptions = new MqttClientFactory().CreateUnsubscribeOptionsBuilder()
                .WithTopicFilter(Topic)
                .Build();

            try
            {
                await _mqttClient.UnsubscribeAsync(mqttUnsubscribeOptions, CancellationToken.None);
                Console.WriteLine("### UNSUBSCRIBED ###");
            }
            catch (Exception e)
            {
                Console.WriteLine("### COULD NOT UNSUBSCRIBE ###" + e);
            }
        }

        public async Task Publish(string message)
        {
            var mqttApplicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(Topic)
                .WithPayload(message)
                .Build();

            try
            {
                await _mqttClient.PublishAsync(mqttApplicationMessage, CancellationToken.None);
                Console.WriteLine("### PUBLISHED ###");
            }
            catch (Exception e)
            {
                Console.WriteLine("### COULD NOT PUBLISH ###" + e);
            }
        }
    }
}
