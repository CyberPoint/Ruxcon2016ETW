using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ETW_IE_InfoLeak_Demo_Parser
{
    public class IE_Demo_Parser
    {
        static Guid WinINetProviderId = new Guid("43D1A55C-76D6-4F7E-995C-64C711E5CAFE");
        static string sessionName = "ETW_IE_InfoLeak_Demo";
        TraceEventSession session;
        Dictionary<Guid, Dictionary<string, string>> httpRequests = new Dictionary<Guid, Dictionary<string, string>>();
        ETWTraceEventSource source;
        HashSet<string> missedEvents = new HashSet<string>();
        private int numMissedEvents = 0;
        private int numParsedEvents = 0;

        public const int COOKIE_STORED = 1;
        public const int URL_ACCESSED = 2;
        public const int REQUEST_MADE = 3;

        public delegate void EventCallbackHandler(EventData data);
        private GrowableArray<EventCallbackHandler> eventCallbacks;

        public delegate void EventMissedCallbackHandler(int numMissedEvents);
        private GrowableArray<EventMissedCallbackHandler> eventMissedCallbacks;

        public delegate void ExtractedDataCallbackHandler(JObject data, int type);
        private GrowableArray<ExtractedDataCallbackHandler> extractedDataCallbacks;

        public int NumParsedEvents { get { return numParsedEvents; } }
        public int NumMissedEvents { get { return numMissedEvents; } }

        public event EventCallbackHandler EventCallback
        {
            add
            {
                eventCallbacks.Add(value);
            }
            remove
            {
                for (int i = 0; i < eventCallbacks.Count; i++)
                {
                    if (Delegate.Equals(eventCallbacks[i], value))
                    {
                        eventCallbacks.RemoveRange(i, 1);
                    }
                }
            }
        }

        public event EventMissedCallbackHandler EventMissedCallback
        {
            add
            {
                eventMissedCallbacks.Add(value);
            }
            remove
            {
                for (int i = 0; i < eventMissedCallbacks.Count; i++)
                {
                    if (Delegate.Equals(eventMissedCallbacks[i], value))
                    {
                        eventMissedCallbacks.RemoveRange(i, 1);
                    }
                }
            }
        }

        public event ExtractedDataCallbackHandler ExtractedDataCallback
        {
            add
            {
                extractedDataCallbacks.Add(value);
            }
            remove
            {
                for (int i = 0; i < extractedDataCallbacks.Count; i++)
                {
                    if (Delegate.Equals(extractedDataCallbacks[i], value))
                    {
                        extractedDataCallbacks.RemoveRange(i, 1);
                    }
                }
            }
        }


        public IE_Demo_Parser()
        {

        }

        public void CreateSession(string fileName = null, bool setupSource = true)
        {
            if (fileName == null)
                session = new TraceEventSession(sessionName);
            else
                session = new TraceEventSession(sessionName, fileName);

            if(setupSource)
            {
                source = session.Source;
                SetupSource();
            }
        }
        public void SetupSource()
        {
            source.Dynamic.All += delegate (TraceEvent data)
            {
                numParsedEvents++;
                EventData eventData = new EventData(data);
                foreach (EventCallbackHandler h in eventCallbacks)
                {
                    h(eventData);
                }

                Guid activityID = data.ActivityID;
                Dictionary<string, string> requestDataDictionary;
                switch (data.EventName)
                {
                    case "WININET_COOKIE_STORED":
                        JObject cookieData = JObject.FromObject(eventData.Properties);
                        cookieData["Timestamp"] = eventData.TimeStamp.ToString();
                        JObject cookieDataBase = new JObject();
                        cookieDataBase["type"] = "COOKIE_STORED";
                        cookieDataBase["label"] = eventData.FormattedMessage;
                        cookieDataBase["data"] = cookieData;
                        foreach(ExtractedDataCallbackHandler h in extractedDataCallbacks)
                        {
                            h(cookieDataBase, COOKIE_STORED);
                        }
                        break;
                    case "WININET_HTTP_REQUEST_HANDLE_CREATED":
                        requestDataDictionary = null;
                        string[] requestHandleNames = { "Verb", "ObjectName", "Version", "Referrer", "AcceptTypes", "ServerName", "ServerPort", "Service" };
                        if (!httpRequests.TryGetValue(activityID, out requestDataDictionary))
                        {
                            requestDataDictionary = new Dictionary<string, string>();
                            httpRequests.Add(activityID, requestDataDictionary);
                            //Debug.WriteLine("activityID " + activityID + " not found for " + data.FormattedMessage);
                        }
                        foreach (var name in data.PayloadNames)
                        {
                            if (requestHandleNames.Contains(name))
                            {
                                requestDataDictionary[name] = data.PayloadStringByName(name);
                            }
                        }
                        break;
                    case "WININET_REQUEST_HEADER":
                        requestDataDictionary = null;
                        if (!httpRequests.TryGetValue(activityID, out requestDataDictionary))
                        {
                            requestDataDictionary = new Dictionary<string, string>();
                            httpRequests.Add(activityID, requestDataDictionary);
                            Debug.WriteLine("activityID " + activityID + " not found for " + data.FormattedMessage);
                        }

                        string headers;
                        if (!requestDataDictionary.TryGetValue("Headers", out headers))
                        {
                            headers = "";
                        }

                        requestDataDictionary["Headers"] = headers + data.PayloadStringByName("Headers");
                        requestDataDictionary["Request Timestamp"] = data.TimeStamp.ToString();
                        break;
                    case "WININET_REQUEST_HEADER_OPTIONAL":
                        requestDataDictionary = null;
                        if (httpRequests.TryGetValue(activityID, out requestDataDictionary))
                        {
                            string optionalHeaders;
                            if (!requestDataDictionary.TryGetValue("WININET_REQUEST_HEADER_OPTIONAL", out optionalHeaders))
                            {
                                optionalHeaders = "";
                            }
                            requestDataDictionary["WININET_REQUEST_HEADER_OPTIONAL"] = optionalHeaders + data.PayloadStringByName("Headers");
                        }
                        else
                        {
                            //Debug.WriteLine("activityID " + activityID + " not found for " + data.FormattedMessage);
                            numMissedEvents++;
                            foreach (EventMissedCallbackHandler h in eventMissedCallbacks)
                            {
                                h(numMissedEvents);
                            }
#if DEBUG
                            throw new InvalidOperationException("activityID " + activityID + " not found for " + data.FormattedMessage);
#endif
                        }
                        break;
                    case "WININET_RESPONSE_HEADER":
                        requestDataDictionary = null;

                        if (httpRequests.TryGetValue(activityID, out requestDataDictionary))
                        {
                            string responseHeader;
                            if (!requestDataDictionary.TryGetValue("Response Headers", out responseHeader))
                            {
                                responseHeader = "";
                            }
                            requestDataDictionary["Response Headers"] = responseHeader + data.PayloadStringByName("Headers");
                            requestDataDictionary["Response Timestamp"] = data.TimeStamp.ToString();
                        }
                        else
                        {
                            //Debug.WriteLine("activityID " + activityID + " not found for " + data.FormattedMessage);
                            numMissedEvents++;
                            foreach (EventMissedCallbackHandler h in eventMissedCallbacks)
                            {
                                h(numMissedEvents);
                            }
#if DEBUG
                            throw new InvalidOperationException("activityID " + activityID + " not found for " + data.FormattedMessage);
#endif
                        }
                        break;
                    case "WININET_HANDLE_CLOSED":
                        requestDataDictionary = null;

                        if (httpRequests.TryGetValue(activityID, out requestDataDictionary))
                        {
                            JObject requestObj = new JObject();
                            JObject requestObjBase = new JObject();
                            try
                            {
                                requestObjBase["type"] = "REQUEST_MADE";
                                requestObjBase["label"] = "Server: " + requestDataDictionary["ServerName"] + "; \r\n" + requestDataDictionary["Headers"];
                                requestObjBase["data"] = requestObj;
                                foreach (KeyValuePair<string, string> kvp in requestDataDictionary)
                                {
                                    if (kvp.Key == "WININET_REQUEST_HEADER_OPTIONAL" && requestDataDictionary["Verb"] == "POST" && requestDataDictionary["Headers"].Contains("Content-Type: application/x-www-form-urlencoded"))
                                    {
                                        string postParamsStr = kvp.Value;
                                        string[] postParams = postParamsStr.Split('&');
                                        JObject postParamsObj = new JObject();
                                        postParamsObj["raw"] = postParamsStr;
                                        JArray parsedParamsArr = new JArray();
                                        JObject parsedParamsObj = new JObject();

                                        foreach (string postParam in postParams)
                                        {
                                            string[] param = postParam.Split("=".ToCharArray(), 2);
                                            if (param.Length > 1)
                                            {
                                                parsedParamsArr.Add(new JObject(new JProperty(param[0], param[1])));
                                                parsedParamsObj[param[0]] = param[1];
                                            }
                                            else
                                            {
                                                parsedParamsArr.Add(param[0]);
                                            }
                                        }
                                        postParamsObj["parsedArray"] = parsedParamsArr;
                                        postParamsObj["parsedObject"] = parsedParamsObj;

                                        JToken postParamStoreArr;
                                        if (!requestObj.TryGetValue("POST Parameters", out postParamStoreArr))
                                        {
                                            postParamStoreArr = new JArray();
                                        }
                                        ((JArray)postParamStoreArr).Add(postParamsObj);
                                        requestObj["POST Parameters"] = postParamStoreArr;
                                    }
                                    else if (kvp.Key == "WININET_REQUEST_HEADER_OPTIONAL" && requestDataDictionary["Verb"] == "POST")
                                    {
                                        //NOT URL encoded, need to handle differently, for now just store as text

                                        JToken postParamStoreArr;
                                        if (!requestObj.TryGetValue("POST Parameters (raw)", out postParamStoreArr))
                                        {
                                            postParamStoreArr = new JArray();
                                        }
                                        ((JArray)postParamStoreArr).Add(kvp.Value);
                                        requestObj["POST Parameters (raw)"] = postParamStoreArr;

                                    }
                                    else if (kvp.Key == "WININET_REQUEST_HEADER_OPTIONAL" && requestDataDictionary["Verb"] != "POST")
                                    {
                                        //Not a post, add to "Other Headers"
                                        JToken otherHeaderStoreArr;
                                        if (!requestObj.TryGetValue("Other Headers", out otherHeaderStoreArr))
                                        {
                                            otherHeaderStoreArr = new JArray();
                                        }
                                        ((JArray)otherHeaderStoreArr).Add(kvp.Value);
                                        requestObj["Other Headers"] = otherHeaderStoreArr;

                                        Debug.WriteLine("event with WININET_REQUEST_HEADER_OPTIONAL was not a post, debug! " + kvp.Value);
#if DEBUG
                                        throw new InvalidOperationException("event with WININET_REQUEST_HEADER_OPTIONAL was not a post, debug! " + kvp.Value);
#endif
                                    }
                                    else if (kvp.Key == "Headers" || kvp.Key == "Response Headers")
                                    {
                                        string[] splitValue = { "\r\n" };
                                        string[] headerParams = kvp.Value.Split(splitValue, StringSplitOptions.RemoveEmptyEntries);
                                        JObject headerParamsObj = new JObject();
                                        headerParamsObj["raw"] = kvp.Value;
                                        JArray parsedParamsArr = new JArray();
                                        JObject parsedParamsObj = new JObject();

                                        foreach (string headerParam in headerParams)
                                        {
                                            string[] param = headerParam.Split(":".ToCharArray(), 2);
                                            if (param.Length > 1)
                                            {
                                                parsedParamsArr.Add(new JObject(new JProperty(param[0], param[1])));
                                                parsedParamsObj[param[0]] = param[1];
                                            }
                                            else
                                            {
                                                parsedParamsArr.Add(param[0]);
                                            }
                                        }
                                        headerParamsObj["parsedArray"] = parsedParamsArr;
                                        headerParamsObj["parsedObject"] = parsedParamsObj;
                                        requestObj[kvp.Key] = headerParamsObj;
                                    }
                                    else
                                    {
                                        requestObj[kvp.Key] = kvp.Value;
                                    }
                                }
                                foreach (ExtractedDataCallbackHandler h in extractedDataCallbacks)
                                {
                                    h(requestObjBase, REQUEST_MADE);
                                }

                            }
                            catch (Exception ex)
                            {
                                //Debug.WriteLine("activityID " + activityID + " not found for " + data.FormattedMessage);
                                numMissedEvents++;
                                foreach (EventMissedCallbackHandler h in eventMissedCallbacks)
                                {
                                    h(numMissedEvents);
                                }
#if DEBUG
                                throw ex;
#endif
                            }
                        }
                        else
                        {
                            //Debug.WriteLine("activityID " + activityID + " not found for " + data.FormattedMessage);
                            //This event fires for more than just http request handles, so ignore if not found.
                        }


                        httpRequests.Remove(activityID);
                        break;
                    case "Wininet_UsageLogRequest":
                        JObject urlData = JObject.FromObject(eventData.Properties);
                        urlData["Timestamp"] = data.TimeStamp.ToString();
                        JObject urlDataBase = new JObject();
                        urlDataBase["type"] = "URL_ACCESSED";
                        urlDataBase["label"] = eventData.FormattedMessage;
                        urlDataBase["data"] = urlData;
                        foreach (ExtractedDataCallbackHandler h in extractedDataCallbacks)
                        {
                            h(urlDataBase, URL_ACCESSED);
                        }
                        break;
                }

            };

        }

        public void CreateSource(string fileName)
        {
            source = new ETWTraceEventSource(fileName);
            SetupSource();
        }

        public bool EnableProvider(TraceEventLevel level = TraceEventLevel.Verbose, ulong matchAnyKeywords = ulong.MaxValue, TraceEventProviderOptions options = null)
        {
            return session.EnableProvider(WinINetProviderId, TraceEventLevel.Verbose, matchAnyKeywords, options);
        }

        public bool Stop()
        {
            return session.Stop();
        }

        public bool Process()
        {
            return source.Process();
        }
        public int EventsLost
        {
            get
            {
                if (session != null)
                    return session.EventsLost;
                return source.EventsLost;
            }
        }
        public class EventData
        {
            [JsonIgnore]
            private TraceEvent evnt;
            private string infoStr;
            private Dictionary<string, string> properties;

            public EventData(TraceEvent data) {
                evnt = data;
                infoStr = "";
                properties = new Dictionary<string, string>();

                infoStr += "Event Name: " + data.EventName + "\r\n";
                infoStr += "Event Message: " + data.FormattedMessage + "\r\n";

                foreach (var name in data.PayloadNames)
                {
                    infoStr += name + " - " + data.PayloadStringByName(name) + "\r\n";
                    properties[name] = data.PayloadStringByName(name);
                }
            }
            public string InfoString { get { return infoStr; } }
            public TraceEvent Event { get { return evnt; } }
            public Dictionary<string, string> Properties { get { return properties; } }

            public string EventName { get { return evnt.EventName; } }
            public string FormattedMessage { get { return evnt.FormattedMessage; } }
            public DateTime TimeStamp { get { return evnt.TimeStamp; } }

            public override string ToString() { return infoStr; }
        }
    }
}
