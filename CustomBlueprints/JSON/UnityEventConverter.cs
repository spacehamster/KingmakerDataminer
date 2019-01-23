using Harmony12;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.Events;

namespace CustomBlueprints
{
    public class UnityEventConverter : JsonConverter
    {

        public UnityEventConverter() { }

        static void AddRuntimeCalls(object o, JObject j)
        {
            var callArray = new JArray();
            var m_PersistentCalls = Traverse.Create(o).Field("m_Calls").Field("m_PersistentCalls").GetValue<IList>();
            var m_RuntimeCalls = Traverse.Create(o).Field("m_Calls").Field("m_RuntimeCalls").GetValue<IList>();
            //System.Collections.Generic.List`1[[UnityEngine.Events.BaseInvokableCall, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]
            foreach (var call in m_PersistentCalls)
            {
                callArray.Add(call.GetType().FullName);
            }
            foreach (var call in m_RuntimeCalls)
            {
                callArray.Add(call.GetType().FullName);
            }
            j.Add("m_Calls", callArray);
        }
        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {
            var unityEvent = o as UnityEventBase;
            var eventType = o.GetType();
            var j = new JObject();
            j.Add("$type", unityEvent.GetType().FullName);
            var persistantCallArray = new JArray();
            var type = unityEvent.GetType();
            Type genericArgument = null;
            while(type != null && genericArgument == null)
            {
                if(type.GenericTypeArguments.Length == 0)
                {
                    type = type.BaseType;
                }
                else
                {
                    genericArgument = type.GenericTypeArguments.First();
                }
            }
            for(int i = 0; i < unityEvent.GetPersistentEventCount(); i++)
            {
                var method = unityEvent.GetPersistentMethodName(i);
                var target = unityEvent.GetPersistentTarget(i);
                var targetName = target == null ? "" : $"{target.GetType().FullName}.";
                persistantCallArray.Add($"{targetName}{method}({genericArgument?.Name ?? "null"})");
            }
            j.Add("m_PersistentCalls", persistantCallArray);
            j.WriteTo(w);
        }

        public override object ReadJson(JsonReader reader, Type type, object existing, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        public override bool CanConvert(Type type)
        {
            return typeof(UnityEventBase).IsAssignableFrom(type);
        }
    }
}