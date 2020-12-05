using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Laparo.Sim.ZeroMQ;
using Multiplayer.API.System;
using UnityEngine;
using ZeroMQ;

namespace Multiplayer.API
{
    public class UnityDebugger : ILog
    {
        public void LogError(Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
        }

        public void LogError(string msg)
        {
            UnityEngine.Debug.LogError(msg);
        }

        public void LogWarning(string msg)
        {
            UnityEngine.Debug.LogWarning(msg);
        }
    }

    public class ZeroMQApi : MonoBehaviour, ICommandsHandler
    {
        public NetworkMode Mode => mode;

        public event Action UpdateEvent;
        public event Action<string> ReceiveEvent;

        public ILog Logger => logger;
        private ILog logger = new UnityDebugger();

        [SerializeField]
        private NetworkMode mode;
        [SerializeField]
        private string zmqTopic = "";

        [SerializeField]
        private bool verbose = false;

        [SerializeField]
        private string subscriberIp = $"tcp://127.0.0.1:47343";
        [SerializeField]
        private string publisherIp = $"tcp://127.0.0.1:47344";

        private ZeroMQSubscriber subscriber;
        private ZeroMQPublisher publisher;

        public void Send(string message)
        {
            if (verbose)
            {
                Debug.Log($"ZeroMQApi Sending {publisherIp}: {message}");
            }

            publisher.Send(message);
        }

        private void Receive(string message)
        {
            if (verbose)
            {
                Debug.Log($"ZeroMQApi Receiving {subscriberIp}: {message}");
            }
            ReceiveEvent?.Invoke(message);
        }

        private void Awake()
        {
            subscriber = new ZeroMQSubscriber(subscriberIp, zmqTopic, out var success);
            if (!success)
            {
                enabled = false;
            }
            publisher = new ZeroMQPublisher(publisherIp, out success);
            if (!success)
            {
                enabled = false;
            }
            NetworkHandler.Instance.RegisterCommandsHandler(this);
        }

        private void Update()
        {
            UpdateEvent?.Invoke();
            subscriber.OnReceive(Receive);
        }

        private void OnDestroy()
        {
            if (publisher != null)
            {
                publisher.Dispose();
            }
            if (subscriber != null)
            {
                subscriber.Dispose();
            }
        }
    }
}
/*
    /// <summary>
    /// Class responsible for gathering requests from <see cref="ArgumentsContainer.Instance.Arguments.CommunicationUrlRequest"/>
    /// converting them to Commands and invoking all subscribed Actions to ReceiveEvent which then can 
    /// call Send method responding to the request on <see cref="ArgumentsContainer.Instance.Arguments.CommunicationUrlRespond"/>
    /// </summary>
    [DisallowMultipleComponent]
    public class ZeroMQApi : MonoBehaviour
    {
        [SerializeField]
        private string zmqTopic = "";

        [SerializeField]
        private bool verbose = false;

        private string subscriberIp => ArgumentsContainer.Instance.Arguments.CommunicationUrlRequest;
        private string publisherIp => ArgumentsContainer.Instance.Arguments.CommunicationUrlRespond;

        private ZeroMQSubscriber subscriber;
        private ZeroMQPublisher publisher;

        private Dictionary<string, Func<string, Command>> registeredControllers = new Dictionary<string, Func<string, Command>>();

        public void Register(string tag, Func<string, Command> func)
        {
            if (!registeredControllers.ContainsKey(tag))
            {
                registeredControllers.Add(tag, func);
            }
        }

        public void Send(Command command)
        {
            string message = JsonConvert.SerializeObject(command, Formatting.None);
            if (verbose)
            {
                Debug.Log($"API Sending: {message}");
            }

            publisher.Send(message);
        }

        private void Receive(string message)
        {
            if (verbose)
            {
                Debug.Log($"API Receiving: {message}");
            }
            try
            {
                var commandJson = JObject.Parse(message);
                var tag = commandJson["Tag"].ToString();
                var payload = commandJson["Payload"].ToString();

                if (registeredControllers.TryGetValue(tag, out var func))
                {
                    Send(func(payload));
                }
                else
                {
                    Send(new Command("API", Reply.InvalidRequest));
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Send(new Command("API", Reply.ParsingError));
            }
        }

        private void Start()
        {
            subscriber = new ZeroMQSubscriber(subscriberIp, zmqTopic, out var success);
            if (!success)
            {
                enabled = false;
            }
            publisher = new ZeroMQPublisher(publisherIp, out success);
            if (!success)
            {
                enabled = false;
            }
        }

        private void Update()
        {
            subscriber.OnReceive(Receive);
        }

        private void OnDestroy()
        {
            if (publisher != null)
            {
                publisher.Dispose();
            }
            if (subscriber != null)
            {
                subscriber.Dispose();
            }
        }
    }

 * */
