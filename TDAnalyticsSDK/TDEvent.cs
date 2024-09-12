using System;
using System.Collections.Generic;

namespace ThinkingData.Analytics
{
    public class TDEventModel
    {
        public string eventName = string.Empty;
        public Dictionary<string, object> properties = new Dictionary<string, object>();

        public TDEventModel(string eventName, Dictionary<string, object> properties)
        {
            this.eventName = eventName;
            this.properties = properties;
        }

        public virtual void Check()
        {
            properties = properties ?? new Dictionary<string, object>();
            eventName = eventName ?? string.Empty;
        }
    }

    public class TDFirstEventModel : TDEventModel
    {
        public string firstCheckId;
        public TDFirstEventModel(string eventName, Dictionary<string, object> properties, string firstCheckId = "") : base(eventName, properties)
        {
            this.firstCheckId = firstCheckId;
        }

        public override void Check()
        {
            firstCheckId = firstCheckId ?? string.Empty;
        }
    }

    public class TDUpdatableEventModel : TDEventModel
    {
        public string eventId;
        public TDUpdatableEventModel(string eventName, Dictionary<string, object> properties, string eventId) : base(eventName, properties)
        {
            this.eventId = eventId;
        }

        public override void Check()
        {
            eventId = eventId ?? string.Empty;
        }
    }

    public class TDOverwritableEventModel : TDEventModel
    {
        public string eventId;
        public TDOverwritableEventModel(string eventName, Dictionary<string, object> properties, string eventId) : base(eventName, properties)
        {
            this.eventId = eventId;
        }

        public override void Check()
        {
            eventId = eventId ?? string.Empty;
        }
    }
}

