using System;
using System.Collections.Generic;

namespace ThinkingData.Analytics
{
	public class TDEvent
    {
        public string EventName = string.Empty;
        public Dictionary<string, object> Properties = new Dictionary<string, object>();
        public string ExtraId = string.Empty;

        public TDEvent(string eventName, Dictionary<string, object> properties, string extraId)
        {
            EventName = eventName;
            Properties = properties;
            ExtraId = extraId;
        }

        public void Check()
        {
            ExtraId = ExtraId ?? string.Empty;
            Properties = Properties ?? new Dictionary<string, object>();
            EventName = EventName ?? string.Empty;
        }
    }

    public class TDFirstEvent: TDEvent
    {
        public TDFirstEvent(string eventName, Dictionary<string, object> properties, string extraId = ""): base(eventName, properties, extraId)
        {
            
        }
    }

    public class TDUpdatableEvent : TDEvent
    {
        public TDUpdatableEvent(string eventName, Dictionary<string, object> properties, string extraId) : base(eventName, properties, extraId)
        {

        }
    }

    public class TDOverWritableEvent : TDEvent
    {
        public TDOverWritableEvent(string eventName, Dictionary<string, object> properties, string extraId) : base(eventName, properties, extraId)
        {

        }
    }
}

